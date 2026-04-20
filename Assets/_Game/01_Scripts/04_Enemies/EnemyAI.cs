using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// EnemyData의 actionPattern을 읽어 턴마다 행동을 실행합니다.
/// TurnManager의 EnemyPhase 시작 시 ExecuteAction()이 호출됩니다.
/// </summary>
[RequireComponent(typeof(Unit))]
public class EnemyAI : MonoBehaviour
{
    [Header("데이터")]
    [SerializeField] private EnemyData data;

    // ───────────────────────────────────────────
    // 상태
    // ───────────────────────────────────────────

    private Unit unit;
    private int  patternIndex = 0;          // 현재 패턴 위치
    private EnemyAction nextAction;         // 이번 턴 예고 행동
    private bool actionCancelled = false;   // 행동 취소 (방어 카드 등)

    // ───────────────────────────────────────────
    // 이벤트 — UI가 구독해 예고 아이콘 표시에 사용
    // ───────────────────────────────────────────

    public System.Action<EnemyAction> OnActionPreviewed;

    // ───────────────────────────────────────────
    // Unity 생명주기
    // ───────────────────────────────────────────

    private void Awake()
    {
        unit = GetComponent<Unit>();
    }

    private void Start()
    {
        if (data != null)
        {
            unit.Team  = TeamType.Enemy;
            unit.MaxHP = data.maxHP;

            SubscribeToTurnManager();
        }
        else
        {
            Debug.LogWarning($"[EnemyAI] {name}에 EnemyData가 할당되지 않았습니다.");
        }
    }

    private void OnDestroy()
    {
        UnsubscribeFromTurnManager();
    }

    // ───────────────────────────────────────────
    // TurnManager 연결
    // ───────────────────────────────────────────

    private void SubscribeToTurnManager()
    {
        if (TurnManager.Instance == null) return;
        TurnManager.Instance.OnRoundStarted += OnRoundStarted;
    }

    private void UnsubscribeFromTurnManager()
    {
        if (TurnManager.Instance == null) return;
        TurnManager.Instance.OnRoundStarted -= OnRoundStarted;
    }

    /// <summary>라운드 시작 시 다음 행동을 결정하고 UI에 예고합니다.</summary>
    private void OnRoundStarted(int _)
    {
        actionCancelled = false;
        PreviewNextAction();
    }

    // ───────────────────────────────────────────
    // 행동 예고
    // ───────────────────────────────────────────

    private void PreviewNextAction()
    {
        if (data == null || data.actionPattern == null || data.actionPattern.Length == 0)
        {
            nextAction = null;
            return;
        }

        nextAction = data.actionPattern[patternIndex % data.actionPattern.Length];
        OnActionPreviewed?.Invoke(nextAction);
        Debug.Log($"[EnemyAI] {data.enemyName} 예고: {nextAction.actionType} ({nextAction.previewDescription})");
    }

    // ───────────────────────────────────────────
    // 행동 실행 (TurnManager의 EnemyPhase 코루틴에서 호출)
    // ───────────────────────────────────────────

    /// <summary>예고된 행동을 실행합니다.</summary>
    public IEnumerator ExecuteAction()
    {
        if (actionCancelled || nextAction == null)
        {
            Debug.Log($"[EnemyAI] {data?.enemyName} 행동 취소됨");
            yield break;
        }

        switch (nextAction.actionType)
        {
            case EnemyActionType.Move:
                yield return StartCoroutine(DoMove(nextAction));
                break;

            case EnemyActionType.Attack:
                yield return StartCoroutine(DoAttack(nextAction));
                break;

            case EnemyActionType.Defend:
                yield return StartCoroutine(DoDefend(nextAction));
                break;

            case EnemyActionType.Buff:
                yield return StartCoroutine(DoBuff(nextAction));
                break;

            case EnemyActionType.Skip:
                Debug.Log($"[EnemyAI] {data.enemyName} 행동 스킵");
                break;
        }

        // 패턴 인덱스 진행
        patternIndex++;
        yield return null;
    }

    // ───────────────────────────────────────────
    // 행동 구현
    // ───────────────────────────────────────────

    /// <summary>플레이어 방향으로 direction칸 이동합니다.</summary>
    private IEnumerator DoMove(EnemyAction action)
    {
        Vector2Int dir = GetDirectionTowardsPlayer();
        if (dir == Vector2Int.zero) dir = action.direction; // 플레이어를 못 찾으면 기본 방향 사용

        for (int i = 0; i < action.range; i++)
        {
            Vector2Int next = unit.GridPosition + dir;
            if (!GridManager.Instance.MoveUnit(unit, next)) break;
            yield return new WaitForSeconds(0.15f);
        }
        Debug.Log($"[EnemyAI] {data.enemyName} 이동 → {unit.GridPosition}");
    }

    /// <summary>방향 기준으로 공격 범위 내 플레이어 유닛에 피해를 줍니다.</summary>
    private IEnumerator DoAttack(EnemyAction action)
    {
        Vector2Int dir = GetDirectionTowardsPlayer();
        if (dir == Vector2Int.zero) dir = action.direction;

        List<Unit> hit = GetUnitsInLine(unit.GridPosition, dir, action.range, TeamType.Player);
        foreach (var target in hit)
        {
            target.TakeDamage(action.value);
            Debug.Log($"[EnemyAI] {data.enemyName} 공격 → {target.name} {action.value} 피해");
        }
        yield return new WaitForSeconds(0.2f);
    }

    /// <summary>보호막을 얻습니다.</summary>
    private IEnumerator DoDefend(EnemyAction action)
    {
        var status = GetComponent<StatusEffectHandler>();
        if (status != null)
        {
            status.AddShield(action.value);
            Debug.Log($"[EnemyAI] {data.enemyName} 방어 — 보호막 +{action.value}");
        }
        yield return null;
    }

    /// <summary>자신을 강화합니다 (Strength 부여).</summary>
    private IEnumerator DoBuff(EnemyAction action)
    {
        var status = GetComponent<StatusEffectHandler>();
        if (status != null)
        {
            status.Apply(StatusEffectType.Strength, action.value, action.range); // range = duration
            Debug.Log($"[EnemyAI] {data.enemyName} 강화 — Strength +{action.value}");
        }
        yield return null;
    }

    // ───────────────────────────────────────────
    // 행동 취소 (플레이어 카드 효과로 호출)
    // ───────────────────────────────────────────

    public void CancelNextAction()
    {
        actionCancelled = true;
        Debug.Log($"[EnemyAI] {data?.enemyName} 다음 행동 취소됨");
    }

    // ───────────────────────────────────────────
    // 유틸리티
    // ───────────────────────────────────────────

    /// <summary>가장 가까운 플레이어 유닛 방향을 반환합니다.</summary>
    private Vector2Int GetDirectionTowardsPlayer()
    {
        var players = GridManager.Instance.GetUnitsByTeam(TeamType.Player);
        if (players.Count == 0) return Vector2Int.zero;

        Vector2Int myPos     = unit.GridPosition;
        Vector2Int playerPos = players[0].GridPosition;
        Vector2Int diff      = playerPos - myPos;

        // 수평/수직 중 더 먼 방향으로 우선 이동
        if (Mathf.Abs(diff.x) >= Mathf.Abs(diff.y))
            return new Vector2Int((int)Mathf.Sign(diff.x), 0);
        else
            return new Vector2Int(0, (int)Mathf.Sign(diff.y));
    }

    /// <summary>시작점에서 direction 방향으로 maxRange칸 내 특정 팀 유닛을 반환합니다.</summary>
    private List<Unit> GetUnitsInLine(Vector2Int origin, Vector2Int direction, int maxRange, TeamType targetTeam)
    {
        var result  = new List<Unit>();
        var gm      = GridManager.Instance;
        Vector2Int current = origin + direction;

        for (int i = 0; i < maxRange; i++)
        {
            if (!gm.IsValidCell(current)) break;
            Unit u = gm.GetUnitAt(current);
            if (u != null && u.Team == targetTeam)
                result.Add(u);
            current += direction;
        }
        return result;
    }

    public EnemyData Data => data;
}
