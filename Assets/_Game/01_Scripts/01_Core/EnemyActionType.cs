/// <summary>
/// 적이 예고할 수 있는 행동 종류 (GDD 5-1)
/// </summary>
public enum EnemyActionType
{
    Move,       // 이동 — 방향 화살표 표시
    Attack,     // 공격 — 공격 범위 셀 하이라이트 (빨강)
    Defend,     // 방어 — 보호막 획득
    Buff,       // 강화 — 자신 강화 💪
    Summon,     // 소환 ✨
    Skip,       // 아무것도 안 함
}
