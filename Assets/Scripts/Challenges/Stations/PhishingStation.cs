using UnityEngine;

/// <summary>
/// Interactable station: office computer with phishing emails.
/// Place on a desk/computer object with a collider.
/// Assign a ChallengeData asset of type Phishing.
/// </summary>
public class PhishingStation : MonoBehaviour, IInteractable
{
    [Header("Challenge")]
    [Tooltip("The ChallengeData ScriptableObject for this phishing scenario.")]
    public ChallengeData challengeData;

    [Header("Visual Feedback")]
    [Tooltip("Renderer to change emissive color for completion state.")]
    public Renderer statusIndicator;

    [Tooltip("Color when untouched.")]
    public Color neutralColor = Color.white;

    [Tooltip("Color when passed.")]
    public Color passedColor = Color.green;

    [Tooltip("Color when failed.")]
    public Color failedColor = Color.red;

    private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");
    private MaterialPropertyBlock propBlock;

    void Start()
    {
        propBlock = new MaterialPropertyBlock();
        UpdateVisualState();
    }

    void Update()
    {
        // Keep visual state updated in case result comes from network sync
        UpdateVisualState();
    }

    public void Interact(GameObject player)
    {
        InteractionController controller = player.GetComponent<InteractionController>();
        if (controller == null) return;

        ChallengeManager.Instance?.StartChallenge(challengeData, controller);
    }

    public string GetPromptText()
    {
        return "Press E to Inspect Computer";
    }

    public bool CanInteract()
    {
        if (challengeData == null) return false;

        // Can't interact if already completed
        return !ChallengeManager.Instance?.IsChallengeCompleted(challengeData.challengeId) ?? true;
    }

    private void UpdateVisualState()
    {
        if (statusIndicator == null || challengeData == null || propBlock == null) return;

        ChallengeResult result = ChallengeManager.Instance?.GetResult(challengeData.challengeId);

        Color color;
        if (result == null)
            color = neutralColor;
        else if (result.passed)
            color = passedColor;
        else
            color = failedColor;

        statusIndicator.GetPropertyBlock(propBlock);
        propBlock.SetColor(EmissionColor, color);
        statusIndicator.SetPropertyBlock(propBlock);
    }
}
