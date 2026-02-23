using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Central manager for all cybersecurity challenges.
/// Tracks challenge states, evaluates answers, and dispatches events.
/// Place on a persistent GameObject in the scene (e.g., "GameManager").
/// </summary>
public class ChallengeManager : MonoBehaviour
{
    public static ChallengeManager Instance { get; private set; }

    [Header("Challenge Registry")]
    [Tooltip("All ChallengeData assets available in this level.")]
    public List<ChallengeData> allChallenges = new List<ChallengeData>();

    /// <summary>
    /// Tracks results for completed challenges. Key = challengeId.
    /// </summary>
    private Dictionary<string, ChallengeResult> completedChallenges = new Dictionary<string, ChallengeResult>();

    [Header("Events")]
    public UnityEvent<ChallengeData, bool> OnChallengeCompleted; // data, passed
    public UnityEvent<ChallengeData> OnChallengeFailed;
    public UnityEvent OnAllChallengesCompleted;

    private ChallengeData activeChallengeData;
    private InteractionController activePlayerController;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// Returns true if a specific challenge has been completed (pass or fail).
    /// </summary>
    public bool IsChallengeCompleted(string challengeId)
    {
        return completedChallenges.ContainsKey(challengeId);
    }

    /// <summary>
    /// Returns the result of a completed challenge, or null if not yet completed.
    /// </summary>
    public ChallengeResult GetResult(string challengeId)
    {
        completedChallenges.TryGetValue(challengeId, out ChallengeResult result);
        return result;
    }

    /// <summary>
    /// Returns how many challenges have been completed so far.
    /// </summary>
    public int CompletedCount => completedChallenges.Count;

    /// <summary>
    /// Returns how many challenges the player passed.
    /// </summary>
    public int PassedCount
    {
        get
        {
            int count = 0;
            foreach (var kvp in completedChallenges)
            {
                if (kvp.Value.passed) count++;
            }
            return count;
        }
    }

    /// <summary>
    /// Returns the total number of registered challenges.
    /// </summary>
    public int TotalCount => allChallenges.Count;

    /// <summary>
    /// Returns true if all challenges have been completed.
    /// </summary>
    public bool AllCompleted => completedChallenges.Count >= allChallenges.Count;

    /// <summary>
    /// Begins a challenge: opens the appropriate UI panel and freezes player input.
    /// Called by interactable stations.
    /// </summary>
    public void StartChallenge(ChallengeData data, InteractionController playerController)
    {
        if (data == null || IsChallengeCompleted(data.challengeId))
        {
            Debug.LogWarning($"ChallengeManager: Challenge '{data?.challengeId}' is null or already completed.");
            return;
        }

        activeChallengeData = data;
        activePlayerController = playerController;

        // Freeze player
        playerController.EnterChallengeUI();

        // Open the correct UI panel based on challenge type
        ChallengeUIBase ui = FindUIForType(data.challengeType);
        if (ui != null)
        {
            ui.Open(data);
        }
        else
        {
            Debug.LogError($"ChallengeManager: No UI panel found for challenge type {data.challengeType}");
            playerController.ExitChallengeUI();
        }
    }

    /// <summary>
    /// Called by a challenge UI panel when the player submits their answer.
    /// </summary>
    public void SubmitAnswer(string challengeId, int choiceIndex)
    {
        ChallengeData data = GetChallengeById(challengeId);
        if (data == null)
        {
            Debug.LogError($"ChallengeManager: Unknown challenge ID '{challengeId}'");
            return;
        }

        bool passed = data.IsCorrectChoice(choiceIndex);
        string playerName = activePlayerController != null ? activePlayerController.gameObject.name : "Unknown";

        ChallengeResult result = new ChallengeResult(challengeId, passed, choiceIndex, playerName);
        completedChallenges[challengeId] = result;

        // Fire events
        OnChallengeCompleted?.Invoke(data, passed);

        if (!passed)
        {
            OnChallengeFailed?.Invoke(data);

            // Trigger consequence
            ConsequenceManager consequence = ConsequenceManager.Instance;
            if (consequence != null)
            {
                consequence.TriggerConsequence(data);
            }
        }

        Debug.Log($"ChallengeManager: Challenge '{challengeId}' completed. Passed: {passed}");

        // Check if all challenges are now completed
        if (AllCompleted)
        {
            OnAllChallengesCompleted?.Invoke();
        }
    }

    /// <summary>
    /// Called by a challenge UI panel when it closes (after showing feedback).
    /// Restores player control.
    /// </summary>
    public void OnChallengeUIClosed()
    {
        if (activePlayerController != null)
        {
            activePlayerController.ExitChallengeUI();
        }

        activeChallengeData = null;
        activePlayerController = null;
    }

    /// <summary>
    /// Resets all challenge progress (for replay).
    /// </summary>
    public void ResetAllProgress()
    {
        completedChallenges.Clear();
    }

    /// <summary>
    /// Returns a read-only view of all results.
    /// </summary>
    public Dictionary<string, ChallengeResult> GetAllResults()
    {
        return new Dictionary<string, ChallengeResult>(completedChallenges);
    }

    private ChallengeData GetChallengeById(string id)
    {
        foreach (var c in allChallenges)
        {
            if (c.challengeId == id) return c;
        }
        return null;
    }

    private ChallengeUIBase FindUIForType(ChallengeType type)
    {
        ChallengeUIBase[] allUIs = FindObjectsByType<ChallengeUIBase>(FindObjectsSortMode.None);
        foreach (var ui in allUIs)
        {
            if (ui.HandledType == type) return ui;
        }
        return null;
    }
}
