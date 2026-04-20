using UnityEngine;

/// <summary>
/// 적 한 종류의 모든 데이터를 담는 ScriptableObject.
/// Assets/02_ScriptableObjects/02_Enemies 에 인스턴스를 생성하세요.
/// </summary>
[CreateAssetMenu(fileName = "NewEnemy", menuName = "Foretold/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("기본 정보")]
    public string enemyName   = "새 적";
    public int    maxHP       = 10;
    public Sprite portrait;

    [Header("행동 패턴 (순서대로 반복)")]
    [Tooltip("턴마다 순환 실행. 1칸=1턴 행동.")]
    public EnemyAction[] actionPattern;

    [Header("특성")]
    [Tooltip("매 이동 후 공격하는 돌격 타입 여부")]
    public bool isChargeType  = false;

    [Tooltip("이동하지 않고 원거리 공격만 하는 타입 여부")]
    public bool isRangedType  = false;
}
