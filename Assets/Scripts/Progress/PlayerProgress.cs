using System.Collections.Generic;
using UnityEngine;
using Alteruna;

/// <summary>
/// Syncs challenge completion state across all players via Alteruna RPCs.
/// When a player completes a challenge, it broadcasts the result to all clients.
/// Attach to the same GameObject as ChallengeManager.
/// </summary>
[RequireComponent(typeof(ChallengeManager))]
public class PlayerProgress : AttributesSync
{
    private ChallengeManager challengeManager;

    /// <summary>
    /// Local cache of all synced results. Updated via RPCs.
    /// </summary>
    private Dictionary<string, ChallengeResult> syncedResults = new Dictionary<string, ChallengeResult>();

    void Start()
    {
        challengeManager = GetComponent<ChallengeManager>();

        if (challengeManager != null)
        {
            challengeManager.OnChallengeCompleted.AddListener(OnLocalChallengeCompleted);
        }
    }

    /// <summary>
    /// Called when the local player completes a challenge.
    /// Broadcasts the result to all other players.
    /// </summary>
    private void OnLocalChallengeCompleted(ChallengeData data, bool passed)
    {
        // Broadcast to all clients
        BroadcastRemoteMethod(nameof(RPC_SyncChallengeResult), data.challengeId, passed ? 1 : 0);
    }

    /// <summary>
    /// RPC received by all clients when any player completes a challenge.
    /// </summary>
    [SynchronizableMethod]
    private void RPC_SyncChallengeResult(string challengeId, int passedInt)
    {
        bool passed = passedInt == 1;

        // Store in synced cache
        syncedResults[challengeId] = new ChallengeResult(challengeId, passed, -1, "Remote");

        // Notify HUD
        HUDController hud = HUDController.Instance;
        if (hud != null)
        {
            string status = passed ? "<color=green>PASSED</color>" : "<color=red>FAILED</color>";
            hud.ShowNotification($"A team member completed: {challengeId} — {status}", 4f);
        }

        // Trigger consequence for all players if failed
        if (!passed)
        {
            ChallengeData data = FindChallengeData(challengeId);
            if (data != null)
            {
                ConsequenceManager consequence = ConsequenceManager.Instance;
                if (consequence != null)
                {
                    consequence.TriggerConsequence(data);
                }
            }
        }

        Debug.Log($"PlayerProgress: Synced result for '{challengeId}' — passed: {passed}");
    }

    /// <summary>
    /// Finds a ChallengeData by ID from the ChallengeManager registry.
    /// </summary>
    private ChallengeData FindChallengeData(string challengeId)
    {
        if (challengeManager == null) return null;

        foreach (var data in challengeManager.allChallenges)
        {
            if (data.challengeId == challengeId) return data;
        }
        return null;
    }

    /// <summary>
    /// Returns all synced results (from other players).
    /// </summary>
    public Dictionary<string, ChallengeResult> GetSyncedResults()
    {
        return new Dictionary<string, ChallengeResult>(syncedResults);
    }
}
