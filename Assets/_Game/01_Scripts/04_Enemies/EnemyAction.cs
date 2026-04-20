using System;
using UnityEngine;

/// <summary>
/// 적이 한 턴에 수행할 단일 행동 데이터.
/// EnemyData의 actionPattern 배열 안에 나열해 패턴을 구성합니다.
/// </summary>
[Serializable]
public class EnemyAction
{
    [Tooltip("행동 종류")]
    public EnemyActionType actionType;

    [Tooltip("이동 방향 또는 공격 방향 (그리드 기준)")]
    public Vector2Int direction;

    [Tooltip("이동 거리 또는 공격 사거리")]
    public int range = 1;

    [Tooltip("공격 피해량 또는 방어 수치")]
    public int value = 5;

    [Tooltip("UI에 표시할 행동 설명")]
    public string previewDescription;
}
