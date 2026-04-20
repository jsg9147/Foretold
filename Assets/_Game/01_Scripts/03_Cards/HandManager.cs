using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 덱 / 손패 / 무덤 사이클과 에너지를 관리합니다.
/// TurnManager의 이벤트를 구독해 자동으로 드로우·정리를 수행합니다.
/// </summary>
public class HandManager : MonoBehaviour
{
    public static HandManager Instance { get; private set; }

    // ───────────────────────────────────────────
    // 설정값 (GDD 기준 초기값)
    // ───────────────────────────────────────────

    [Header("Settings")]
    [SerializeField] private int drawPerRound   = 5;
    [SerializeField] private int maxEnergy      = 3;
    [SerializeField] private int maxHandSize    = 10;

    // ───────────────────────────────────────────
    // 상태
    // ───────────────────────────────────────────

    private List<CardData> deck    = new List<CardData>();
    private List<CardData> hand    = new List<CardData>();
    private List<CardData> discard = new List<CardData>();

    public IReadOnlyList<CardData> Hand    => hand;
    public IReadOnlyList<CardData> Deck    => deck;
    public IReadOnlyList<CardData> Discard => discard;

    public int CurrentEnergy { get; private set; }
    public int MaxEnergy => maxEnergy;

    // 콤보 추적 (TurnManager·CardExecutor에서 참조)
    public bool MovedThisTurn    { get; private set; }
    public bool AttackedThisTurn { get; private set; }

    // ───────────────────────────────────────────
    // 이벤트
    // ───────────────────────────────────────────

    public event Action<List<CardData>>  OnHandChanged;     // 손패 갱신
    public event Action<int, int>        OnEnergyChanged;   // (현재, 최대)
    public event Action                  OnDeckShuffled;

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
        SubscribeToTurnManager();
    }

    private void OnDestroy()
    {
        UnsubscribeFromTurnManager();
    }

    // ───────────────────────────────────────────
    // TurnManager 이벤트 연결
    // ───────────────────────────────────────────

    private void SubscribeToTurnManager()
    {
        if (TurnManager.Instance == null) return;
        TurnManager.Instance.OnRoundStarted     += OnRoundStarted;
        TurnManager.Instance.OnPlayerPhaseStarted += OnPlayerPhaseStarted;
        TurnManager.Instance.OnStateChanged     += OnStateChanged;
    }

    private void UnsubscribeFromTurnManager()
    {
        if (TurnManager.Instance == null) return;
        TurnManager.Instance.OnRoundStarted     -= OnRoundStarted;
        TurnManager.Instance.OnPlayerPhaseStarted -= OnPlayerPhaseStarted;
        TurnManager.Instance.OnStateChanged     -= OnStateChanged;
    }

    private void OnRoundStarted(int roundNumber)
    {
        RefillEnergy();
        DrawCards(drawPerRound);
        ResetComboFlags();
    }

    private void OnPlayerPhaseStarted()
    {
        // 현재는 드로우가 RoundStart에서 처리되므로 추가 작업 없음
        // 추후 특수 효과(라운드 시작 드로우 추가 등) 처리 가능
    }

    private void OnStateChanged(TurnManager.BattleState state)
    {
        if (state == TurnManager.BattleState.RoundEnd)
            DiscardHand();
    }

    // ───────────────────────────────────────────
    // 덱 초기화
    // ───────────────────────────────────────────

    /// <summary>덱을 초기 카드 목록으로 설정하고 셔플합니다.</summary>
    public void InitializeDeck(List<CardData> initialCards)
    {
        deck.Clear();
        hand.Clear();
        discard.Clear();

        deck.AddRange(initialCards);
        ShuffleDeck();

        Debug.Log($"[HandManager] 덱 초기화 완료 ({deck.Count}장)");
        NotifyHandChanged();
    }

    // ───────────────────────────────────────────
    // 드로우
    // ───────────────────────────────────────────

    /// <summary>덱에서 count장을 손패로 드로우합니다.</summary>
    public void DrawCards(int count)
    {
        for (int i = 0; i < count; i++)
        {
            if (hand.Count >= maxHandSize)
            {
                Debug.Log("[HandManager] 손패 최대치 도달, 드로우 중단");
                break;
            }

            if (deck.Count == 0)
            {
                if (discard.Count == 0)
                {
                    Debug.Log("[HandManager] 덱과 무덤 모두 비어있음");
                    break;
                }
                ReshuffleDeck();
            }

            CardData drawn = deck[deck.Count - 1];
            deck.RemoveAt(deck.Count - 1);
            hand.Add(drawn);
        }

        Debug.Log($"[HandManager] 드로우 후 — 손패:{hand.Count} 덱:{deck.Count} 무덤:{discard.Count}");
        NotifyHandChanged();
    }

    // ───────────────────────────────────────────
    // 카드 사용
    // ───────────────────────────────────────────

    /// <summary>
    /// 손패에서 카드를 사용합니다.
    /// 에너지가 부족하거나 손패에 없으면 false를 반환합니다.
    /// </summary>
    public bool PlayCard(CardData card)
    {
        if (!hand.Contains(card))
        {
            Debug.LogWarning($"[HandManager] '{card.cardName}'이 손패에 없음");
            return false;
        }
        if (CurrentEnergy < card.energyCost)
        {
            Debug.Log($"[HandManager] 에너지 부족 ({CurrentEnergy}/{card.energyCost})");
            return false;
        }

        SpendEnergy(card.energyCost);
        hand.Remove(card);
        discard.Add(card);

        UpdateComboFlags(card);

        Debug.Log($"[HandManager] '{card.cardName}' 사용 — 남은 에너지: {CurrentEnergy}");
        NotifyHandChanged();
        return true;
    }

    // ───────────────────────────────────────────
    // 손패 버리기
    // ───────────────────────────────────────────

    /// <summary>손패 전체를 무덤으로 이동합니다. (라운드 종료 시 자동 호출)</summary>
    public void DiscardHand()
    {
        discard.AddRange(hand);
        hand.Clear();
        Debug.Log($"[HandManager] 손패 전체 무덤으로 이동 — 무덤:{discard.Count}장");
        NotifyHandChanged();
    }

    // ───────────────────────────────────────────
    // 에너지
    // ───────────────────────────────────────────

    public void RefillEnergy()
    {
        CurrentEnergy = maxEnergy;
        OnEnergyChanged?.Invoke(CurrentEnergy, maxEnergy);
    }

    public void SpendEnergy(int amount)
    {
        CurrentEnergy = Mathf.Max(0, CurrentEnergy - amount);
        OnEnergyChanged?.Invoke(CurrentEnergy, maxEnergy);
    }

    public void GainEnergy(int amount)
    {
        CurrentEnergy = Mathf.Min(maxEnergy, CurrentEnergy + amount);
        OnEnergyChanged?.Invoke(CurrentEnergy, maxEnergy);
    }

    /// <summary>에너지가 남아있는지 여부</summary>
    public bool HasEnergy(int required = 1) => CurrentEnergy >= required;

    // ───────────────────────────────────────────
    // 덱 셔플
    // ───────────────────────────────────────────

    private void ShuffleDeck()
    {
        for (int i = deck.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            (deck[i], deck[j]) = (deck[j], deck[i]);
        }
        OnDeckShuffled?.Invoke();
    }

    /// <summary>무덤을 덱으로 되돌리고 셔플합니다.</summary>
    private void ReshuffleDeck()
    {
        deck.AddRange(discard);
        discard.Clear();
        ShuffleDeck();
        Debug.Log($"[HandManager] 무덤 → 덱 리셔플 ({deck.Count}장)");
    }

    // ───────────────────────────────────────────
    // 콤보 플래그
    // ───────────────────────────────────────────

    private void ResetComboFlags()
    {
        MovedThisTurn    = false;
        AttackedThisTurn = false;
    }

    private void UpdateComboFlags(CardData card)
    {
        if (card.cardType == CardType.Move)    MovedThisTurn    = true;
        if (card.cardType == CardType.Attack)  AttackedThisTurn = true;
    }

    // ───────────────────────────────────────────
    // 내부 유틸
    // ───────────────────────────────────────────

    private void NotifyHandChanged() => OnHandChanged?.Invoke(hand);
}
