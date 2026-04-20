using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 전투 씬의 진입점. GridManager·TurnManager·HandManager·EnemyAI를 조율합니다.
/// 씬에 하나만 배치하세요.
/// </summary>
public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }

    // ───────────────────────────────────────────
    // 인스펙터 설정
    // ───────────────────────────────────────────

    [Header("플레이어")]
    [SerializeField] private GameObject  playerPrefab;
    [SerializeField] private Vector2Int  playerStartCell = new Vector2Int(1, 0); // 하단 중앙

    [Header("초기 덱")]
    [SerializeField] private List<CardData> starterDeck;

    [Header("적 스폰")]
    [SerializeField] private List<EnemySpawnEntry> enemySpawns;

    // ───────────────────────────────────────────
    // 내부 참조
    // ───────────────────────────────────────────

    private Unit playerUnit;
    private List<EnemyAI> spawnedEnemies = new List<EnemyAI>();

    // ───────────────────────────────────────────
    // Unity 생명주기
    // ───────────────────────────────────────────

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        StartCoroutine(InitializeBattle());
    }

    // ───────────────────────────────────────────
    // 전투 초기화
    // ───────────────────────────────────────────

    private IEnumerator InitializeBattle()
    {
        Debug.Log("[BattleManager] 전투 초기화 시작");

        // 1. 그리드 초기화
        GridManager.Instance.InitializeGrid();
        yield return null;

        // 2. 플레이어 스폰
        SpawnPlayer();
        yield return null;

        // 3. 적 스폰
        SpawnEnemies();
        yield return null;

        // 4. 덱 초기화
        if (starterDeck != null && starterDeck.Count > 0)
            HandManager.Instance.InitializeDeck(starterDeck);
        else
            Debug.LogWarning("[BattleManager] 스타터 덱이 비어있습니다.");

        yield return null;

        // 5. 이벤트 구독
        SubscribeToEvents();

        // 6. 전투 시작
        Debug.Log("[BattleManager] 전투 시작!");
        TurnManager.Instance.StartBattle();
    }

    // ───────────────────────────────────────────
    // 스폰
    // ───────────────────────────────────────────

    private void SpawnPlayer()
    {
        if (playerPrefab == null)
        {
            Debug.LogError("[BattleManager] playerPrefab이 없습니다.");
            return;
        }

        GameObject go = Instantiate(playerPrefab, GridManager.Instance.CellToWorld(playerStartCell), Quaternion.identity);
        go.name = "Player";
        playerUnit = go.GetComponent<Unit>();

        if (playerUnit == null)
        {
            Debug.LogError("[BattleManager] playerPrefab에 Unit 컴포넌트가 없습니다.");
            return;
        }

        playerUnit.Team = TeamType.Player;
        GridManager.Instance.PlaceUnit(playerUnit, playerStartCell);
        Debug.Log($"[BattleManager] 플레이어 배치 → {playerStartCell}");
    }

    private void SpawnEnemies()
    {
        spawnedEnemies.Clear();

        foreach (var entry in enemySpawns)
        {
            if (entry.prefab == null) continue;

            GameObject go = Instantiate(entry.prefab, GridManager.Instance.CellToWorld(entry.spawnCell), Quaternion.identity);
            go.name = entry.prefab.name;

            Unit unit = go.GetComponent<Unit>();
            if (unit == null) continue;

            if (GridManager.Instance.PlaceUnit(unit, entry.spawnCell))
            {
                EnemyAI ai = go.GetComponent<EnemyAI>();
                if (ai != null) spawnedEnemies.Add(ai);
                Debug.Log($"[BattleManager] 적 배치: {go.name} → {entry.spawnCell}");
            }
            else
            {
                Debug.LogWarning($"[BattleManager] {entry.spawnCell} 칸에 적 배치 실패");
                Destroy(go);
            }
        }
    }

    // ───────────────────────────────────────────
    // 이벤트 구독
    // ───────────────────────────────────────────

    private void SubscribeToEvents()
    {
        TurnManager.Instance.OnEnemyPhaseStarted += OnEnemyPhaseStarted;
        TurnManager.Instance.OnVictory            += OnVictory;
        TurnManager.Instance.OnDefeat             += OnDefeat;
    }

    private void OnDestroy()
    {
        if (TurnManager.Instance == null) return;
        TurnManager.Instance.OnEnemyPhaseStarted -= OnEnemyPhaseStarted;
        TurnManager.Instance.OnVictory            -= OnVictory;
        TurnManager.Instance.OnDefeat             -= OnDefeat;
    }

    // ───────────────────────────────────────────
    // 적 페이즈 실행
    // ───────────────────────────────────────────

    private void OnEnemyPhaseStarted()
    {
        StartCoroutine(RunEnemyPhase());
    }

    private IEnumerator RunEnemyPhase()
    {
        // 죽은 적 제거
        spawnedEnemies.RemoveAll(ai => ai == null);

        foreach (var ai in spawnedEnemies)
        {
            if (ai == null) continue;
            yield return StartCoroutine(ai.ExecuteAction());
        }

        // 모든 적 행동 완료 → TurnManager에 종료 알림
        TurnManager.Instance.NotifyEnemyPhaseComplete();
    }

    // ───────────────────────────────────────────
    // 승패 처리
    // ───────────────────────────────────────────

    private void OnVictory()
    {
        Debug.Log("[BattleManager] ★ 전투 승리!");
        // 추후: 보상 UI 표시, 다음 씬 로드 등
    }

    private void OnDefeat()
    {
        Debug.Log("[BattleManager] ✕ 전투 패배 — 런 종료");
        // 추후: 게임 오버 UI 표시, 타이틀 복귀 등
    }

    // ───────────────────────────────────────────
    // 외부에서 플레이어 카드 사용 호출
    // ───────────────────────────────────────────

    /// <summary>UI에서 카드를 클릭했을 때 호출합니다.</summary>
    public void OnCardPlayed(CardData card, Unit target = null)
    {
        if (TurnManager.Instance.CurrentState != TurnManager.BattleState.PlayerPhase)
        {
            Debug.Log("[BattleManager] 플레이어 페이즈가 아닙니다.");
            return;
        }

        if (HandManager.Instance.PlayCard(card))
            CardExecutor.Instance.Execute(card, playerUnit, target);
    }

    public Unit PlayerUnit => playerUnit;
}

// ───────────────────────────────────────────────
// 적 스폰 설정 (인스펙터용)
// ───────────────────────────────────────────────

[System.Serializable]
public class EnemySpawnEntry
{
    public GameObject prefab;
    public Vector2Int spawnCell;
}
