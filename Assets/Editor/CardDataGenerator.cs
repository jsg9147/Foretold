using UnityEditor;
using UnityEngine;

/// <summary>
/// GDD 예시 카드 5종을 ScriptableObject로 자동 생성합니다.
/// 메뉴: Foretold > Generate Starter Cards
/// </summary>
public static class CardDataGenerator
{
    private const string SavePath = "Assets/_Game/02_ScriptableObjects/01_Cards";

    [MenuItem("Foretold/Generate Starter Cards")]
    public static void GenerateStarterCards()
    {
        CreateDash();
        CreateSlash();
        CreateShield();
        CreatePoison();
        CreateComboStrike();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[CardDataGenerator] 스타터 카드 5종 생성 완료!");
        EditorUtility.DisplayDialog("완료", "스타터 카드 5종이 생성되었습니다.\n경로: " + SavePath, "확인");
    }

    // ─────────────────────────────────────────────
    // 1. 대시 — 이동, 비용 0
    // ─────────────────────────────────────────────
    private static void CreateDash()
    {
        var card = ScriptableObject.CreateInstance<CardData>();
        card.cardName    = "대시";
        card.cardType    = CardType.Move;
        card.energyCost  = 0;
        card.description = "지정한 방향으로 1칸 이동한다.";
        card.effects     = new CardEffect[]
        {
            new CardEffect
            {
                effectType = EffectType.MoveInDirection,
                value      = 1,   // 이동 거리
                direction  = Vector2Int.up  // 기본값; 런타임에 플레이어 입력으로 교체
            }
        };
        Save(card, "Card_Dash");
    }

    // ─────────────────────────────────────────────
    // 2. 베기 — 공격, 비용 1
    // ─────────────────────────────────────────────
    private static void CreateSlash()
    {
        var card = ScriptableObject.CreateInstance<CardData>();
        card.cardName    = "베기";
        card.cardType    = CardType.Attack;
        card.energyCost  = 1;
        card.description = "전방 1칸의 적에게 6 피해를 준다.";
        card.effects     = new CardEffect[]
        {
            new CardEffect
            {
                effectType   = EffectType.DamageSingleCell,
                value        = 6,
                targetOffset = Vector2Int.up   // 플레이어 위쪽 칸
            }
        };
        Save(card, "Card_Slash");
    }

    // ─────────────────────────────────────────────
    // 3. 방패 — 방어, 비용 1
    // ─────────────────────────────────────────────
    private static void CreateShield()
    {
        var card = ScriptableObject.CreateInstance<CardData>();
        card.cardName    = "방패";
        card.cardType    = CardType.Defense;
        card.energyCost  = 1;
        card.description = "보호막 5를 얻는다.";
        card.effects     = new CardEffect[]
        {
            new CardEffect
            {
                effectType = EffectType.GainShield,
                value      = 5
            }
        };
        Save(card, "Card_Shield");
    }

    // ─────────────────────────────────────────────
    // 4. 독 — 스킬, 비용 1
    // ─────────────────────────────────────────────
    private static void CreatePoison()
    {
        var card = ScriptableObject.CreateInstance<CardData>();
        card.cardName    = "독";
        card.cardType    = CardType.Skill;
        card.energyCost  = 1;
        card.description = "전방 1칸의 적에게 독을 부여한다. 3턴 동안 매 턴 2 피해.";
        card.effects     = new CardEffect[]
        {
            new CardEffect
            {
                effectType   = EffectType.ApplyPoison,
                value        = 2,       // 턴당 피해
                duration     = 3,
                targetOffset = Vector2Int.up
            }
        };
        Save(card, "Card_Poison");
    }

    // ─────────────────────────────────────────────
    // 5. 연격 — 콤보, 비용 1
    // ─────────────────────────────────────────────
    private static void CreateComboStrike()
    {
        var card = ScriptableObject.CreateInstance<CardData>();
        card.cardName    = "연격";
        card.cardType    = CardType.Combo;
        card.energyCost  = 1;
        card.description = "전방 1칸의 적에게 4 피해를 준다.\n이번 턴에 이동했다면 8 피해로 증가.";
        card.effects     = new CardEffect[]
        {
            new CardEffect
            {
                effectType     = EffectType.DamageSingleCell,
                value          = 4,             // 기본 피해
                comboCondition = ComboCondition.MovedThisTurn,
                comboValue     = 8,             // 콤보 달성 시 피해
                targetOffset   = Vector2Int.up
            }
        };
        Save(card, "Card_ComboStrike");
    }

    // ─────────────────────────────────────────────
    // 공통 저장
    // ─────────────────────────────────────────────
    private static void Save(CardData card, string fileName)
    {
        string path = $"{SavePath}/{fileName}.asset";
        AssetDatabase.CreateAsset(card, path);
        Debug.Log($"[CardDataGenerator] 생성: {path}");
    }
}
