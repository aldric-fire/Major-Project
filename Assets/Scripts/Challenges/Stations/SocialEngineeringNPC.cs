using UnityEngine;

/// <summary>
/// Interactable NPC: social engineering encounter.
/// Place on an NPC character model with a collider.
/// Assign a ChallengeData asset of type SocialEngineering.
/// </summary>
public class SocialEngineeringNPC : MonoBehaviour, IInteractable
{
    [Header("Challenge")]
    [Tooltip("The ChallengeData ScriptableObject for this social engineering scenario.")]
    public ChallengeData challengeData;

    [Header("NPC Settings")]
    [Tooltip("NPC display name shown in dialogue.")]
    public string npcName = "Unknown Visitor";

    [Header("Visual Feedback")]
    [Tooltip("Optional floating indicator above NPC (e.g., exclamation mark).")]
    public GameObject interactionIndicator;

    public void Interact(GameObject player)
    {
        InteractionController controller = player.GetComponent<InteractionController>();
        if (controller == null) return;

        ChallengeManager.Instance?.StartChallenge(challengeData, controller);
    }

    public string GetPromptText()
    {
        return $"Press E to Talk to {npcName}";
    }

    public bool CanInteract()
    {
        return challengeData != null;
    }

    void Update()
    {
        // Show/hide indicator based on completion state
        if (interactionIndicator != null && challengeData != null)
        {
            bool completed = ChallengeManager.Instance?.IsChallengeCompleted(challengeData.challengeId) ?? false;
            interactionIndicator.SetActive(!completed);
        }
    }
}
