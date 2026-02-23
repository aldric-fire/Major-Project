using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Educational debrief panel shown after a challenge consequence fades.
/// Explains what went wrong and presents real-world data.
/// </summary>
public class DebriefPanel : MonoBehaviour
{
    [Header("Debrief UI Elements")]
    public GameObject debriefPanelRoot;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI explanationText;
    public TextMeshProUGUI statText;
    public Button gotItButton;

    void Awake()
    {
        if (debriefPanelRoot != null)
            debriefPanelRoot.SetActive(false);

        if (gotItButton != null)
            gotItButton.onClick.AddListener(OnGotItClicked);
    }

    /// <summary>
    /// Shows the debrief panel with info from the given challenge data.
    /// </summary>
    public void Show(ChallengeData data)
    {
        if (debriefPanelRoot == null) return;

        debriefPanelRoot.SetActive(true);

        if (titleText != null)
            titleText.text = $"What Happened: {data.title}";

        if (explanationText != null)
            explanationText.text = data.debriefText;

        if (statText != null)
        {
            if (!string.IsNullOrEmpty(data.realWorldStat))
                statText.text = $"<b>Did you know?</b>\n{data.realWorldStat}";
            else
                statText.text = "";
        }
    }

    /// <summary>
    /// Called when the "Got it" button is pressed.
    /// Hides the panel and tells ChallengeManager to release the player.
    /// </summary>
    private void OnGotItClicked()
    {
        if (debriefPanelRoot != null)
            debriefPanelRoot.SetActive(false);

        ChallengeManager.Instance?.OnChallengeUIClosed();
    }
}
