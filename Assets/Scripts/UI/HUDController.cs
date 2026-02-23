using UnityEngine;
using TMPro;

/// <summary>
/// Controls the persistent in-game HUD: interaction prompts, objective tracker,
/// and consequence notification bar.
/// Attach to a Canvas GameObject in the scene.
/// </summary>
public class HUDController : MonoBehaviour
{
    public static HUDController Instance { get; private set; }

    [Header("Interaction Prompt")]
    [Tooltip("The panel/group containing the interact prompt text.")]
    public GameObject interactPromptPanel;

    [Tooltip("Text showing 'Press E to ...'")]
    public TextMeshProUGUI interactPromptText;

    [Header("Objective Tracker")]
    [Tooltip("Panel showing remaining challenges.")]
    public GameObject objectivePanel;

    [Tooltip("Text displaying objective progress.")]
    public TextMeshProUGUI objectiveText;

    [Header("Notification Bar")]
    [Tooltip("Top notification bar for consequence alerts.")]
    public GameObject notificationBar;

    [Tooltip("Text for the notification bar.")]
    public TextMeshProUGUI notificationText;

    private float notificationTimer;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Start with panels hidden
        if (interactPromptPanel != null) interactPromptPanel.SetActive(false);
        if (notificationBar != null) notificationBar.SetActive(false);
    }

    void Update()
    {
        // Auto-hide notification after timer
        if (notificationBar != null && notificationBar.activeSelf)
        {
            notificationTimer -= Time.deltaTime;
            if (notificationTimer <= 0f)
            {
                notificationBar.SetActive(false);
            }
        }

        // Update objective tracker
        UpdateObjectiveTracker();
    }

    /// <summary>
    /// Shows the interaction prompt with the given text.
    /// </summary>
    public void ShowInteractPrompt(string text)
    {
        if (interactPromptPanel != null)
        {
            interactPromptPanel.SetActive(true);
            if (interactPromptText != null)
                interactPromptText.text = text;
        }
    }

    /// <summary>
    /// Hides the interaction prompt.
    /// </summary>
    public void HideInteractPrompt()
    {
        if (interactPromptPanel != null)
            interactPromptPanel.SetActive(false);
    }

    /// <summary>
    /// Shows a notification at the top of the screen for a duration.
    /// </summary>
    public void ShowNotification(string message, float duration = 5f)
    {
        if (notificationBar != null)
        {
            notificationBar.SetActive(true);
            if (notificationText != null)
                notificationText.text = message;
            notificationTimer = duration;
        }
    }

    private void UpdateObjectiveTracker()
    {
        if (objectivePanel == null || objectiveText == null) return;

        ChallengeManager cm = ChallengeManager.Instance;
        if (cm == null)
        {
            objectivePanel.SetActive(false);
            return;
        }

        objectivePanel.SetActive(true);
        int completed = cm.CompletedCount;
        int total = cm.TotalCount;
        int passed = cm.PassedCount;

        objectiveText.text = $"Stations: {completed}/{total}\nPassed: {passed}/{total}";
    }
}
