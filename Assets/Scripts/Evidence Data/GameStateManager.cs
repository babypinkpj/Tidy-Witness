using UnityEngine;
using System;

/// <summary>
/// Every macro state the game moves through, in narrative order.
/// Drives lighting, NPC positions, audio, and evidence availability
/// via SceneStateProfile assets and the various *StateController scripts.
/// </summary>public class GameStateManager : MonoBehaviour


public enum GameState
{
    PreEvent,
    GuestsArriving,
    EventPeak,
    MurderWindow,
    BodyFound,
    Investigation,
    FlashbackReady,
    Accusation,
    Resolution
}
/// <summary>
/// Central authority for the game's current state. Everything else
/// (lighting, NPCs, evidence unlocks, audio) subscribes to
/// <see cref="OnStateChanged"/> instead of polling this class.
/// Persists across scene loads.
/// </summary>

public class GameStateManager : MonoBehaviour
{
 public static GameStateManager Instance { get; private set; }
 
    /// <summary>Fired once whenever the state actually changes. Never fires for a no-op SetState call.</summary>
    public static event Action<GameState> OnStateChanged;
 
    [Tooltip("State the game starts in when this scene first loads.")]
    [SerializeField] private GameState _initialState = GameState.PreEvent;
 
    /// <summary>The state the game is in right now.</summary>
    public GameState Current { get; private set; }
 
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
 
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Current = _initialState;
    }
 
    /// <summary>
    /// Transitions the game to <paramref name="newState"/>. No-op if the
    /// game is already in that state — <see cref="OnStateChanged"/> will
    /// not fire in that case.
    /// </summary>
    public void SetState(GameState newState)
    {
        if (Current == newState) return;
 
        Current = newState;
        OnStateChanged?.Invoke(newState);
    }
 
    /// <summary>Convenience check so callers don't have to compare <see cref="Current"/> directly.</summary>
    public bool IsState(GameState state) => Current == state;
 
    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}
