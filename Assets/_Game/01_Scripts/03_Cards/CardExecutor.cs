using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// CardData의 effects 배열을 읽어 실제 게임 로직에 적용합니다.
/// HandManager.PlayCard() 성공 후 이 클래스의 Execute()를 호출하세요.
/// </summary>
public class CardExecutor : MonoBehaviour
{
    public static CardExecutor Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    // ───────────────────────────────────────────
    // 진입점
    // ───────────────────────────────────────────

    /// <summary>
    /// 카드의 모든 효과를 실행합니다.
    /// caster  : 카드를 사용하는 유닛 (플레이어)
    /// target  : 주요 대상 유닛 (단일 공격 등에 사용, 없으면 null)
    /// </summary>
    public void Execute(CardData card, Unit caster, Unit target = null)
    {
        if (card == null || card.effects == null) return;

        foreach (CardEffect effect in card.effects)
        {
            int finalValue = ResolveValue(effect);
            ApplyEffect(effect, finalValue, caster, target);
        }
    }

    // ───────────────────────────────────────────
    // 콤보 수치 결정
    // ───────────────────────────────────────────

    /// <summary>콤보 조건을 확인하고 최종 수치를 반환합니다.</summary>
    private int ResolveValue(CardEffect effect)
    {
        if (effect.comboCondition == ComboCondition.None)
            return effect.value;

        bool conditionMet = CheckComboCondition(effect.comboCondition);
        if (conditionMet)
        {
            Debug.Log($"[CardExecutor] 콤보 달성! ({effect.comboCondition}) → {effect.comboValue}");
            return effect.comboValue;
        }
        return effect.value;
    }

    private bool CheckComboCondition(ComboCondition condition)
    {
        var hm = HandManager.Instance;
        if (hm == null) return false;

        return condition switch
        {
            ComboCondition.MovedThisTurn    => hm.MovedThisTurn,
            ComboCondition.AttackedThisTurn => hm.AttackedThisTurn,
            ComboCondition.ShieldActive     => GetPlayerShield() > 0,
            ComboCondition.EnemyAdjacent    => IsEnemyAdjacentToPlayer(),
            _                               => false
        };
    }

    // ───────────────────────────────────────────
    // 효과 분기
    // ───────────────────────────────────────────

    private void ApplyEffect(CardEffect effect, int value, Unit caster, Unit target)
    {
        switch (effect.effectType)
        {
            // ── 이동 ──────────────────────────────
            case EffectType.MoveInDirection:
                MoveUnit(caster, effect.direction, value);
                break;

            case EffectType.MoveToCell:
                // 런타임에 플레이어 입력으로 목적지를 받아야 함 (추후 UI 연동)
                Debug.Log("[CardExecutor] MoveToCell: 목적지 입력 대기 (미구현)");
                break;

            // ── 공격 ──────────────────────────────
            case EffectType.DamageSingleCell:
                DamageSingleCell(caster, effect.targetOffset, value);
                break;

            case EffectType.DamageArea:
                DamageArea(caster, effect.areaOffsets, value);
                break;

            case EffectType.DamageLinear:
                DamageLinear(caster, effect.direction, value);
                break;

            // ── 밀치기 / 당기기 ───────────────────
            case EffectType.Push:
                if (target != null)
                    GridManager.Instance.PushUnit(target, effect.direction, value);
                break;

            case EffectType.Pull:
                if (target != null)
                    GridManager.Instance.PullUnit(target, effect.direction, value);
                break;

            // ── 방어 ──────────────────────────────
            case EffectType.GainShield:
                ApplyShield(caster, value);
                break;

            case EffectType.CancelEnemyAction:
                CancelEnemyAction(target);
                break;

            // ── 상태이상 ──────────────────────────
            case EffectType.ApplyPoison:
                ApplyStatusEffect(target, StatusEffectType.Poison, value, effect.duration);
                break;

            case EffectType.ApplyStun:
                ApplyStatusEffect(target, StatusEffectType.Stun, value, effect.duration);
                break;

            case EffectType.ApplyWeaken:
                ApplyStatusEffect(target, StatusEffectType.Weaken, value, effect.duration);
                break;

            // ── 강화 ──────────────────────────────
            case EffectType.GainStrength:
                ApplyStatusEffect(caster, StatusEffectType.Strength, value, effect.duration);
                break;

            case EffectType.DrawCards:
                HandManager.Instance?.DrawCards(value);
                break;

            case EffectType.GainEnergy:
                HandManager.Instance?.GainEnergy(value);
                break;

            default:
                Debug.LogWarning($"[CardExecutor] 미처리 EffectType: {effect.effectType}");
                break;
        }
    }

    // ───────────────────────────────────────────
    // 이동
    // ───────────────────────────────────────────

    private void MoveUnit(Unit unit, Vector2Int direction, int distance)
    {
        var gm = GridManager.Instance;
        for (int i = 0; i < distance; i++)
        {
            Vector2Int next = unit.GridPosition + direction;
            if (!gm.MoveUnit(unit, next)) break;
        }
        Debug.Log($"[CardExecutor] {unit.name} 이동 → {unit.GridPosition}");
    }

    // ───────────────────────────────────────────
    // 공격
    // ───────────────────────────────────────────

    private void DamageSingleCell(Unit caster, Vector2Int offset, int damage)
    {
        Vector2Int targetCell = caster.GridPosition + offset;
        Unit hit = GridManager.Instance.GetUnitAt(targetCell);
        if (hit != null && hit.Team != caster.Team)
        {
            int finalDamage = ApplyStrengthBonus(caster, damage);
            hit.TakeDamage(finalDamage);
            Debug.Log($"[CardExecutor] {caster.name} → {hit.name} {finalDamage} 피해");
        }
    }

    private void DamageArea(Unit caster, Vector2Int[] offsets, int damage)
    {
        if (offsets == null) return;
        foreach (var offset in offsets)
            DamageSingleCell(caster, offset, damage);
    }

    private void DamageLinear(Unit caster, Vector2Int direction, int damage)
    {
        var gm = GridManager.Instance;
        Vector2Int current = caster.GridPosition + direction;

        while (gm.IsValidCell(current))
        {
            Unit hit = gm.GetUnitAt(current);
            if (hit != null && hit.Team != caster.Team)
            {
                int finalDamage = ApplyStrengthBonus(caster, damage);
                hit.TakeDamage(finalDamage);
                Debug.Log($"[CardExecutor] 직선 관통 → {hit.name} {finalDamage} 피해");
            }
            current += direction;
        }
    }

    // ───────────────────────────────────────────
    // 방어 / 상태이상
    // ───────────────────────────────────────────

    private void ApplyShield(Unit unit, int amount)
    {
        var status = unit.GetComponent<StatusEffectHandler>();
        if (status != null)
            status.AddShield(amount);
        else
            Debug.LogWarning($"[CardExecutor] {unit.name}에 StatusEffectHandler 없음");
    }

    private void CancelEnemyAction(Unit target)
    {
        if (target == null) return;
        // 추후 EnemyAI.CancelNextAction() 연동
        Debug.Log($"[CardExecutor] {target.name} 행동 취소 (미구현)");
    }

    private void ApplyStatusEffect(Unit target, StatusEffectType type, int value, int duration)
    {
        if (target == null) return;
        var status = target.GetComponent<StatusEffectHandler>();
        if (status != null)
            status.Apply(type, value, duration);
        else
            Debug.LogWarning($"[CardExecutor] {target.name}에 StatusEffectHandler 없음");
    }

    // ───────────────────────────────────────────
    // 강화 보너스 계산
    // ───────────────────────────────────────────

    /// <summary>Strength 상태이상 보너스를 더한 최종 피해량을 반환합니다.</summary>
    private int ApplyStrengthBonus(Unit caster, int baseDamage)
    {
        var status = caster.GetComponent<StatusEffectHandler>();
        if (status == null) return baseDamage;
        int bonus = status.GetStrengthBonus();
        return baseDamage + bonus;
    }

    // ───────────────────────────────────────────
    // 콤보 보조 메서드
    // ───────────────────────────────────────────

    private int GetPlayerShield()
    {
        var players = GridManager.Instance.GetUnitsByTeam(TeamType.Player);
        if (players.Count == 0) return 0;
        var status = players[0].GetComponent<StatusEffectHandler>();
        return status != null ? status.Shield : 0;
    }

    private bool IsEnemyAdjacentToPlayer()
    {
        var players = GridManager.Instance.GetUnitsByTeam(TeamType.Player);
        if (players.Count == 0) return false;

        Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        foreach (var dir in dirs)
        {
            Unit neighbor = GridManager.Instance.GetUnitAt(players[0].GridPosition + dir);
            if (neighbor != null && neighbor.Team == TeamType.Enemy) return true;
        }
        return false;
    }
}

// ───────────────────────────────────────────────
// 상태이상 타입 (StatusEffectHandler 구현 전 스텁)
// ───────────────────────────────────────────────

public enum StatusEffectType
{
    Poison,     // 매 턴 피해
    Stun,       // 행동 불가
    Weaken,     // 피해 감소
    Strength,   // 공격력 증가
}

/// <summary>
/// 유닛의 보호막·상태이상을 관리하는 컴포넌트 스텁.
/// 추후 StatusEffectHandler.cs로 분리 구현 예정.
/// </summary>
public class StatusEffectHandler : MonoBehaviour
{
    public int Shield { get; private set; }

    private int strengthBonus;

    public void AddShield(int amount)
    {
        Shield += amount;
        Debug.Log($"[StatusEffectHandler] {name} 보호막 +{amount} → 총 {Shield}");
    }

    public void Apply(StatusEffectType type, int value, int duration)
    {
        // 추후 상태이상 스택 관리 구현
        Debug.Log($"[StatusEffectHandler] {name} {type} 적용 (값:{value} 지속:{duration}턴)");
        if (type == StatusEffectType.Strength) strengthBonus += value;
    }

    public int GetStrengthBonus() => strengthBonus;

    /// <summary>피해를 받을 때 보호막을 먼저 소모합니다. Unit.TakeDamage()에서 호출하세요.</summary>
    public int AbsorbDamage(int incomingDamage)
    {
        if (Shield <= 0) return incomingDamage;
        int absorbed = Mathf.Min(Shield, incomingDamage);
        Shield -= absorbed;
        Debug.Log($"[StatusEffectHandler] {name} 보호막 {absorbed} 흡수 → 잔여 {Shield}");
        return incomingDamage - absorbed;
    }
}
