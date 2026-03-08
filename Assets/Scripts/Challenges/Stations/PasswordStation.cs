using UnityEngine;

/// <summary>
/// Interactable station: computer terminal or sticky note for password challenges.
/// Place on a desk/terminal object with a collider.
/// Assign a ChallengeData asset of type Password.
/// </summary>
public class PasswordStation : MonoBehaviour, IInteractable
{
    [Header("Challenge")]
    [Tooltip("The ChallengeData ScriptableObject for this password scenario.")]
    public ChallengeData challengeData;

    [Header("Visual Feedback")]
    public Renderer statusIndicator;
    public Color neutralColor = Color.white;
    public Color passedColor = Color.green;
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
        return "Press E to Check Password Policy";
    }

    public bool CanInteract()
    {
        return challengeData != null;
    }

    private void UpdateVisualState()
    {
        if (statusIndicator == null || challengeData == null || propBlock == null) return;

        ChallengeResult result = ChallengeManager.Instance?.GetResult(challengeData.challengeId);

        Color color;
        if (result == null) color = neutralColor;
        else if (result.passed) color = passedColor;
        else color = failedColor;

        statusIndicator.GetPropertyBlock(propBlock);
        propBlock.SetColor(EmissionColor, color);
        statusIndicator.SetPropertyBlock(propBlock);
    }
}
