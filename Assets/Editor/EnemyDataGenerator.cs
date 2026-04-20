using UnityEditor;
using UnityEngine;

/// <summary>
/// GDD 프로토타입 적 3종을 ScriptableObject로 자동 생성합니다.
/// 메뉴: Foretold > Generate Starter Enemies
/// </summary>
public static class EnemyDataGenerator
{
    private const string SavePath = "Assets/_Game/02_ScriptableObjects/02_Enemies";

    [MenuItem("Foretold/Generate Starter Enemies")]
    public static void GenerateStarterEnemies()
    {
        CreateCharger();
        CreateSniper();
        CreateShieldGuard();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[EnemyDataGenerator] 스타터 적 3종 생성 완료!");
        EditorUtility.DisplayDialog("완료", "스타터 적 3종이 생성되었습니다.\n경로: " + SavePath, "확인");
    }

    // ─────────────────────────────────────────────
    // 1. 돌격병 — 이동 → 공격 반복, HP 15
    // ─────────────────────────────────────────────
    private static void CreateCharger()
    {
        var enemy = ScriptableObject.CreateInstance<EnemyData>();
        enemy.enemyName    = "돌격병";
        enemy.maxHP        = 15;
        enemy.isChargeType = true;
        enemy.actionPattern = new EnemyAction[]
        {
            new EnemyAction
            {
                actionType          = EnemyActionType.Move,
                range               = 1,
                value               = 0,
                previewDescription  = "플레이어 방향으로 1칸 이동"
            },
            new EnemyAction
            {
                actionType          = EnemyActionType.Attack,
                range               = 1,
                value               = 6,
                previewDescription  = "인접 플레이어에게 6 피해"
            }
        };
        Save(enemy, "Enemy_Charger");
    }

    // ─────────────────────────────────────────────
    // 2. 저격수 — 제자리 + 직선 원거리 공격, HP 10
    // ─────────────────────────────────────────────
    private static void CreateSniper()
    {
        var enemy = ScriptableObject.CreateInstance<EnemyData>();
        enemy.enemyName    = "저격수";
        enemy.maxHP        = 10;
        enemy.isRangedType = true;
        enemy.actionPattern = new EnemyAction[]
        {
            new EnemyAction
            {
                actionType          = EnemyActionType.Attack,
                range               = 3,        // 직선 최대 3칸
                value               = 8,
                previewDescription  = "직선 방향 최대 3칸, 8 피해"
            }
        };
        Save(enemy, "Enemy_Sniper");
    }

    // ─────────────────────────────────────────────
    // 3. 방패병 — 방어 → 반격 반복, HP 25
    // ─────────────────────────────────────────────
    private static void CreateShieldGuard()
    {
        var enemy = ScriptableObject.CreateInstance<EnemyData>();
        enemy.enemyName = "방패병";
        enemy.maxHP     = 25;
        enemy.actionPattern = new EnemyAction[]
        {
            new EnemyAction
            {
                actionType          = EnemyActionType.Defend,
                range               = 0,
                value               = 8,        // 보호막 8
                previewDescription  = "보호막 8 획득"
            },
            new EnemyAction
            {
                actionType          = EnemyActionType.Attack,
                range               = 1,
                value               = 10,       // 반격 피해
                previewDescription  = "인접 플레이어에게 10 반격"
            }
        };
        Save(enemy, "Enemy_ShieldGuard");
    }

    private static void Save(EnemyData enemy, string fileName)
    {
        string path = $"{SavePath}/{fileName}.asset";
        AssetDatabase.CreateAsset(enemy, path);
        Debug.Log($"[EnemyDataGenerator] 생성: {path}");
    }
}
