/// <summary>
/// 카드 분류 (GDD 4-2)
/// </summary>
public enum CardType
{
    Move,       // 이동 — 플레이어를 그리드 내 이동
    Attack,     // 공격 — 특정 셀 or 범위 공격
    Defense,    // 방어 — 보호막 / 적 행동 취소
    Skill,      // 스킬 — 상태이상, 강화 등 특수 효과
    Combo       // 콤보 — 조건 충족 시 강화 효과
}
