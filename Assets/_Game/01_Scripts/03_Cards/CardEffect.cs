using System;
using UnityEngine;

/// <summary>
/// 카드 한 장에 포함될 수 있는 단일 효과를 정의합니다.
/// CardData의 effects 배열 안에 여러 개 넣어 복합 효과를 표현합니다.
/// </summary>
[Serializable]
public class CardEffect
{
    [Tooltip("어떤 효과인지")]
    public EffectType effectType;

    [Tooltip("효과 수치 (피해량, 보호막량, 이동 거리 등)")]
    public int value;

    [Tooltip("지속 턴수 (상태이상, 강화에 사용)")]
    public int duration;

    [Tooltip("단일 셀 공격 시 플레이어 기준 오프셋 (col, row)")]
    public Vector2Int targetOffset;

    [Tooltip("범위 공격 시 오프셋 목록")]
    public Vector2Int[] areaOffsets;

    [Tooltip("이동·밀치기 방향 (예: 위 = (0,1), 오른쪽 = (1,0))")]
    public Vector2Int direction;

    // ── 콤보 조건 ──────────────────────────────
    [Tooltip("콤보 카드 전용: 이 조건이 충족되면 comboValue를 사용합니다")]
    public ComboCondition comboCondition;

    [Tooltip("콤보 조건 충족 시 대체 수치")]
    public int comboValue;
}

/// <summary>
/// 콤보 카드의 발동 조건
/// </summary>
public enum ComboCondition
{
    None,               // 조건 없음 (콤보 카드 아님)
    MovedThisTurn,      // 이번 턴에 이동 카드를 사용했을 때
    AttackedThisTurn,   // 이번 턴에 공격 카드를 사용했을 때
    ShieldActive,       // 보호막이 남아있을 때
    EnemyAdjacent,      // 인접 칸에 적이 있을 때
}
