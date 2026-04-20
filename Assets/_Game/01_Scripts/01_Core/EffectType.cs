/// <summary>
/// 카드 효과 종류
/// </summary>
public enum EffectType
{
    // 이동
    MoveInDirection,    // 방향 지정 이동 (direction, distance)
    MoveToCell,         // 특정 셀로 이동 (상호작용 UI 필요)

    // 공격
    DamageSingleCell,   // 단일 셀 피해 (offset 기준)
    DamageArea,         // 범위 피해 (offsets 목록)
    DamageLinear,       // 직선 관통 피해

    // 밀치기 / 당기기
    Push,               // 대상 밀치기 (direction, distance)
    Pull,               // 대상 당기기 (direction, distance)

    // 방어
    GainShield,         // 보호막 획득 (amount)
    CancelEnemyAction,  // 적 행동 취소

    // 상태이상
    ApplyPoison,        // 독 부여 (duration)
    ApplyStun,          // 기절 부여 (duration)
    ApplyWeaken,        // 약화 부여 (duration, damageReducePercent)

    // 강화
    GainStrength,       // 공격력 증가 (amount, duration)
    DrawCards,          // 카드 드로우 (amount)
    GainEnergy,         // 에너지 획득 (amount)
}
