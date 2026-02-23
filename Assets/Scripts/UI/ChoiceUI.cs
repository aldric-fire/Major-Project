using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI panel for simple choice-based challenges (e.g., USB drop attack).
/// Displays a scenario description and multiple-choice options.
/// </summary>
public class ChoiceUI : ChallengeUIBase
{
    public override ChallengeType HandledType => ChallengeType.USBDrop;

    [Header("Choice UI Elements")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI feedbackText;

    [Tooltip("Container for dynamically created option buttons.")]
    public Transform optionsContainer;

    [Tooltip("Prefab for a single option button.")]
    public GameObject optionButtonPrefab;

    private int selectedIndex = -1;

    protected override void PopulateUI(ChallengeData data)
    {
        if (titleText != null) titleText.text = data.title;
        if (descriptionText != null) descriptionText.text = data.description;
        if (feedbackText != null)
        {
            feedbackText.text = "";
            feedbackText.gameObject.SetActive(false);
        }

        selectedIndex = -1;
        ClearOptions();

        // Create option buttons
        for (int i = 0; i < data.options.Count; i++)
        {
            if (optionButtonPrefab != null && optionsContainer != null)
            {
                GameObject btnObj = Instantiate(optionButtonPrefab, optionsContainer);
                TextMeshProUGUI btnText = btnObj.GetComponentInChildren<TextMeshProUGUI>();
                if (btnText != null)
                    btnText.text = data.options[i].text;

                Button btn = btnObj.GetComponent<Button>();
                if (btn != null)
                {
                    int index = i;
                    btn.onClick.AddListener(() => OnOptionSelected(index));
                }
            }
        }
    }

    private void OnOptionSelected(int index)
    {
        if (currentData == null || index < 0 || index >= currentData.options.Count) return;

        selectedIndex = index;
        bool correct = currentData.IsCorrectChoice(index);

        // Show feedback
        if (feedbackText != null)
        {
            feedbackText.gameObject.SetActive(true);
            feedbackText.text = correct
                ? $"<color=green>Smart choice!</color>\n{currentData.options[index].feedbackNarrative}"
                : $"<color=red>Bad decision!</color>\n{currentData.options[index].feedbackNarrative}";
        }

        // Disable all buttons
        DisableAllButtons();

        // Highlight correct/incorrect
        HighlightAnswers();

        SubmitChoice(index);
    }

    private void HighlightAnswers()
    {
        if (optionsContainer == null || currentData == null) return;

        for (int i = 0; i < optionsContainer.childCount && i < currentData.options.Count; i++)
        {
            Image bg = optionsContainer.GetChild(i).GetComponent<Image>();
            if (bg != null)
            {
                if (currentData.options[i].isCorrect)
                    bg.color = new Color(0.2f, 0.8f, 0.2f, 0.5f); // green
                else if (i == selectedIndex)
                    bg.color = new Color(0.8f, 0.2f, 0.2f, 0.5f); // red
            }
        }
    }

    protected override void ShowFeedback(string feedbackText, bool wasCorrect)
    {
        float delay = wasCorrect ? 3f : 1f;
        StartCoroutine(DelayedCloseRoutine(delay));
    }

    private IEnumerator DelayedCloseRoutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        Close();
    }

    private void DisableAllButtons()
    {
        if (optionsContainer == null) return;

        Button[] buttons = optionsContainer.GetComponentsInChildren<Button>();
        foreach (var btn in buttons)
        {
            btn.interactable = false;
        }
    }

    private void ClearOptions()
    {
        if (optionsContainer == null) return;

        for (int i = optionsContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(optionsContainer.GetChild(i).gameObject);
        }
    }

    public override void Close()
    {
        ClearOptions();
        base.Close();
    }
}
