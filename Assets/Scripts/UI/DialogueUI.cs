using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI panel for Social Engineering dialogue challenges.
/// Displays NPC dialogue lines and player response options in a conversation flow.
/// </summary>
public class DialogueUI : ChallengeUIBase
{
    public override ChallengeType HandledType => ChallengeType.SocialEngineering;

    [Header("Dialogue UI Elements")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI npcDialogueText;
    public TextMeshProUGUI feedbackText;

    [Tooltip("Optional NPC portrait image.")]
    public Image npcPortrait;

    [Tooltip("Container for response option buttons.")]
    public Transform responseContainer;

    [Tooltip("Prefab for a single response option button.")]
    public GameObject responseButtonPrefab;

    private int selectedIndex = -1;

    protected override void PopulateUI(ChallengeData data)
    {
        if (titleText != null) titleText.text = data.title;
        if (npcDialogueText != null) npcDialogueText.text = data.description;
        if (feedbackText != null)
        {
            feedbackText.text = "";
            feedbackText.gameObject.SetActive(false);
        }

        selectedIndex = -1;
        ClearResponses();

        // Create response buttons
        for (int i = 0; i < data.options.Count; i++)
        {
            if (responseButtonPrefab != null && responseContainer != null)
            {
                GameObject btnObj = Instantiate(responseButtonPrefab, responseContainer);
                TextMeshProUGUI btnText = btnObj.GetComponentInChildren<TextMeshProUGUI>();
                if (btnText != null)
                    btnText.text = data.options[i].text;

                Button btn = btnObj.GetComponent<Button>();
                if (btn != null)
                {
                    int index = i;
                    btn.onClick.AddListener(() => OnResponseSelected(index));
                }
            }
        }
    }

    private void OnResponseSelected(int index)
    {
        if (currentData == null || index < 0 || index >= currentData.options.Count) return;

        selectedIndex = index;
        bool correct = currentData.IsCorrectChoice(index);

        // Show feedback inline
        if (feedbackText != null)
        {
            feedbackText.gameObject.SetActive(true);
            feedbackText.text = correct
                ? $"<color=green>Good response!</color>\n{currentData.options[index].feedbackNarrative}"
                : $"<color=red>Bad call...</color>\n{currentData.options[index].feedbackNarrative}";
        }

        // Disable all buttons after selection
        DisableAllButtons();

        // Change NPC dialogue to reaction text
        if (npcDialogueText != null)
        {
            if (correct)
                npcDialogueText.text = "\"Alright, fair enough. I'll go through proper channels.\"";
            else
                npcDialogueText.text = "\"Thanks! I really appreciate it.\" <i>(They walk away quickly...)</i>";
        }

        SubmitChoice(index);
    }

    protected override void ShowFeedback(string feedbackText, bool wasCorrect)
    {
        float delay = wasCorrect ? 3f : 1.5f;
        StartCoroutine(DelayedCloseRoutine(delay));
    }

    private IEnumerator DelayedCloseRoutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        Close();
    }

    private void DisableAllButtons()
    {
        if (responseContainer == null) return;

        Button[] buttons = responseContainer.GetComponentsInChildren<Button>();
        foreach (var btn in buttons)
        {
            btn.interactable = false;
        }
    }

    private void ClearResponses()
    {
        if (responseContainer == null) return;

        for (int i = responseContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(responseContainer.GetChild(i).gameObject);
        }
    }

    public override void Close()
    {
        ClearResponses();
        base.Close();
    }
}
