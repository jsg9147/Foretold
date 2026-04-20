using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 전투 턴 흐름을 관리합니다.
/// RoundStart → PlayerPhase → EnemyPhase → RoundEnd 사이클을 반복합니다.
/// </summary>
public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance { get; private set; }

    // ───────────────────────────────────────────
    // 상태 정의
    // ───────────────────────────────────────────

    public enum BattleState
    {
        Idle,           // 전투 시작 전
        RoundStart,     // 라운드 시작 (드로우, 에너지 회복, 적 행동 예고)
        PlayerPhase,    // 플레이어 카드 사용 중
        EnemyPhase,     // 적 행동 실행 중
        RoundEnd,       // 라운드 정리 (카드 무덤 처리)
        Victory,        // 승리
        Defeat          // 패배
    }

    // ───────────────────────────────────────────
    // 공개 상태 / 이벤트
    // ───────────────────────────────────────────

    public BattleState CurrentState { get; private set; } = BattleState.Idle;
    public int RoundNumber { get; private set; } = 0;

    /// <summary>상태가 바뀔 때 UI 등에서 구독할 수 있는 이벤트</summary>
    public event Action<BattleState> OnStateChanged;
    public event Action<int> OnRoundStarted;   // 라운드 번호 전달
    public event Action OnPlayerPhaseStarted;
    public event Action OnEnemyPhaseStarted;
    public event Action OnVictory;
    public event Action OnDefeat;

    // ───────────────────────────────────────────
    // 내부 참조
    // ───────────────────────────────────────────

    [Header("Settings")]
    [SerializeField] private float enemyActionDelay = 0.6f; // 적 행동 사이 딜레이(초)

    // ───────────────────────────────────────────
    // Unity 생명주기
    // ───────────────────────────────────────────

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // ───────────────────────────────────────────
    // 전투 시작 / 종료
    // ───────────────────────────────────────────

    /// <summary>전투를 시작합니다. 씬 준비 완료 후 호출하세요.</summary>
    public void StartBattle()
    {
        RoundNumber = 0;
        ChangeState(BattleState.RoundStart);
    }

    // ───────────────────────────────────────────
    // 상태 전환
    // ───────────────────────────────────────────

    private void ChangeState(BattleState newState)
    {
        CurrentState = newState;
        OnStateChanged?.Invoke(newState);
        Debug.Log($"[TurnManager] 상태 변경 → {newState}");

        switch (newState)
        {
            case BattleState.RoundStart:    HandleRoundStart();   break;
            case BattleState.PlayerPhase:   HandlePlayerPhase();  break;
            case BattleState.EnemyPhase:    StartCoroutine(HandleEnemyPhase()); break;
            case BattleState.RoundEnd:      HandleRoundEnd();     break;
            case BattleState.Victory:       HandleVictory();      break;
            case BattleState.Defeat:        HandleDefeat();       break;
        }
    }

    // ───────────────────────────────────────────
    // 각 단계 처리
    // ───────────────────────────────────────────

    /// <summary>라운드 시작: 라운드 번호 증가, 드로우·에너지 회복, 적 행동 예고</summary>
    private void HandleRoundStart()
    {
        RoundNumber++;
        Debug.Log($"[TurnManager] ── 라운드 {RoundNumber} 시작 ──");
        OnRoundStarted?.Invoke(RoundNumber);

        // 드로우 및 에너지 회복은 추후 HandManager에서 구독하여 처리
        // 적 행동 예고는 추후 EnemyManager에서 구독하여 처리

        // 예고 완료 후 플레이어 페이즈로 전환
        ChangeState(BattleState.PlayerPhase);
    }

    /// <summary>플레이어 페이즈 시작: 플레이어가 카드를 사용할 수 있는 상태</summary>
    private void HandlePlayerPhase()
    {
        OnPlayerPhaseStarted?.Invoke();
        Debug.Log("[TurnManager] 플레이어 페이즈 — 카드를 사용하세요");
        // 플레이어 입력은 UI/InputManager에서 처리하고
        // 완료 시 EndPlayerPhase()를 호출합니다
    }

    /// <summary>BattleManager가 모든 적 행동 완료 후 호출합니다.</summary>
    public void NotifyEnemyPhaseComplete()
    {
        if (CurrentState != BattleState.EnemyPhase)
        {
            Debug.LogWarning("[TurnManager] EnemyPhase가 아닌 상태에서 NotifyEnemyPhaseComplete 호출됨");
            return;
        }

        if (CheckVictory())
            ChangeState(BattleState.Victory);
        else if (CheckDefeat())
            ChangeState(BattleState.Defeat);
        else
            ChangeState(BattleState.RoundEnd);
    }

    /// <summary>플레이어가 턴 종료를 선택하거나 에너지가 소진됐을 때 호출합니다.</summary>
    public void EndPlayerPhase()
    {
        if (CurrentState != BattleState.PlayerPhase)
        {
            Debug.LogWarning("[TurnManager] 플레이어 페이즈가 아닌 상태에서 EndPlayerPhase 호출됨");
            return;
        }
        ChangeState(BattleState.EnemyPhase);
    }

    /// <summary>
    /// 적 페이즈: BattleManager에 이벤트를 발행하고 완료 대기합니다.
    /// 실제 적 행동 실행은 BattleManager.RunEnemyPhase()가 담당하며,
    /// 완료 시 NotifyEnemyPhaseComplete()를 호출합니다.
    /// </summary>
    private IEnumerator HandleEnemyPhase()
    {
        OnEnemyPhaseStarted?.Invoke();
        Debug.Log("[TurnManager] 적 페이즈 시작 — BattleManager에 위임");
        // 이후 흐름은 BattleManager.RunEnemyPhase() → NotifyEnemyPhaseComplete()로 이어짐
        yield return null;
    }

    /// <summary>라운드 종료: 사용한 카드를 무덤으로 이동하고 다음 라운드를 시작합니다.</summary>
    private void HandleRoundEnd()
    {
        Debug.Log("[TurnManager] 라운드 종료 — 카드 정리");
        // 추후 HandManager.Instance.DiscardHand() 호출 예정
        ChangeState(BattleState.RoundStart);
    }

    // ───────────────────────────────────────────
    // 승패 판정
    // ───────────────────────────────────────────

    /// <summary>모든 적이 사망했으면 true</summary>
    private bool CheckVictory()
    {
        return GridManager.Instance.GetUnitsByTeam(TeamType.Enemy).Count == 0;
    }

    /// <summary>플레이어 유닛이 모두 사망했으면 true</summary>
    private bool CheckDefeat()
    {
        return GridManager.Instance.GetUnitsByTeam(TeamType.Player).Count == 0;
    }

    private void HandleVictory()
    {
        Debug.Log("[TurnManager] ★ 승리!");
        OnVictory?.Invoke();
    }

    private void HandleDefeat()
    {
        Debug.Log("[TurnManager] ✕ 패배");
        OnDefeat?.Invoke();
    }
}
