using UnityEngine;

/// <summary>
/// Interactable pickup: USB drive found on the ground.
/// Place a USB model with a collider in the hallway/parking area.
/// Assign a ChallengeData asset of type USBDrop.
/// </summary>
public class USBPickup : MonoBehaviour, IInteractable
{
    [Header("Challenge")]
    [Tooltip("The ChallengeData ScriptableObject for this USB drop scenario.")]
    public ChallengeData challengeData;

    [Header("Visual Settings")]
    [Tooltip("Optional glow/highlight effect to draw attention.")]
    public GameObject highlightEffect;

    [Tooltip("If true, the USB model is hidden after the challenge is completed.")]
    public bool hideAfterCompleted = true;

    private bool isCompleted = false;

    void Start()
    {
        if (highlightEffect != null)
            highlightEffect.SetActive(true);
    }

    void Update()
    {
        if (isCompleted) return;

        bool completed = ChallengeManager.Instance?.IsChallengeCompleted(challengeData?.challengeId) ?? false;
        if (completed)
        {
            isCompleted = true;
            if (highlightEffect != null)
                highlightEffect.SetActive(false);

            if (hideAfterCompleted)
            {
                // Disable the visual mesh but keep the script alive for status
                MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
                if (meshRenderer != null) meshRenderer.enabled = false;

                // Also disable child renderers
                foreach (var renderer in GetComponentsInChildren<MeshRenderer>())
                    renderer.enabled = false;
            }
        }
    }

    public void Interact(GameObject player)
    {
        InteractionController controller = player.GetComponent<InteractionController>();
        if (controller == null) return;

        ChallengeManager.Instance?.StartChallenge(challengeData, controller);
    }

    public string GetPromptText()
    {
        return "Press E to Pick Up USB Drive";
    }

    public bool CanInteract()
    {
        return challengeData != null;
    }
}
