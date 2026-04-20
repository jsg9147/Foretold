using UnityEngine;

/// <summary>
/// 카드 한 장의 모든 데이터를 담는 ScriptableObject입니다.
/// Assets/02_ScriptableObjects/01_Cards 에 인스턴스를 생성하세요.
/// </summary>
[CreateAssetMenu(fileName = "NewCard", menuName = "Foretold/Card Data")]
public class CardData : ScriptableObject
{
    // ───────────────────────────────────────────
    // 기본 정보
    // ───────────────────────────────────────────

    [Header("기본 정보")]
    [Tooltip("카드 이름 (UI 표시용)")]
    public string cardName = "새 카드";

    [Tooltip("카드 분류")]
    public CardType cardType;

    [Tooltip("에너지 비용 (0~3)")]
    [Range(0, 3)]
    public int energyCost;

    [Tooltip("카드 설명 (UI 표시용)")]
    [TextArea(2, 4)]
    public string description;

    [Tooltip("카드 일러스트")]
    public Sprite artwork;

    // ───────────────────────────────────────────
    // 효과
    // ───────────────────────────────────────────

    [Header("효과 목록 (복합 효과 가능)")]
    public CardEffect[] effects;

    // ───────────────────────────────────────────
    // 업그레이드 (추후 구현)
    // ───────────────────────────────────────────

    [Header("업그레이드")]
    [Tooltip("업그레이드 버전 카드. 없으면 비워두세요.")]
    public CardData upgradedVersion;

    // ───────────────────────────────────────────
    // 유틸리티
    // ───────────────────────────────────────────

    /// <summary>콤보 카드인지 여부</summary>
    public bool IsCombo => cardType == CardType.Combo;

    /// <summary>업그레이드 가능한지 여부</summary>
    public bool IsUpgradable => upgradedVersion != null;

#if UNITY_EDITOR
    private void OnValidate()
    {
        // 에너지 비용 0~3 강제
        energyCost = Mathf.Clamp(energyCost, 0, 3);
    }
#endif
}
