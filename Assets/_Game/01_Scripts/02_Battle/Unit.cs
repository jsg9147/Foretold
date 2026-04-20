using UnityEngine;

/// <summary>
/// 그리드 위에 존재하는 모든 유닛(플레이어, 적)의 기반 클래스
/// </summary>
public class Unit : MonoBehaviour
{
    [Header("Unit Info")]
    public TeamType Team;
    public int MaxHP = 10;
    public int CurrentHP;

    /// <summary>현재 그리드 좌표 (GridManager가 관리)</summary>
    public Vector2Int GridPosition { get; set; }

    protected virtual void Awake()
    {
        CurrentHP = MaxHP;
    }

    /// <summary>피해를 받습니다. HP가 0 이하가 되면 Die()를 호출합니다.</summary>
    public virtual void TakeDamage(int amount)
    {
        CurrentHP -= amount;
        Debug.Log($"[Unit] {name} {amount} 피해 → 잔여 HP: {CurrentHP}");
        if (CurrentHP <= 0) Die();
    }

    /// <summary>유닛 사망 처리. 하위 클래스에서 오버라이드 가능.</summary>
    protected virtual void Die()
    {
        Debug.Log($"[Unit] {name} 사망");
        GridManager.Instance.RemoveUnit(this);
        Destroy(gameObject);
    }
}
