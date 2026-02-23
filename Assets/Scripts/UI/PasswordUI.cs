using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI panel for Password Strength challenges.
/// Displays password options and/or a sticky-note sub-challenge.
/// </summary>
public class PasswordUI : ChallengeUIBase
{
    public override ChallengeType HandledType => ChallengeType.Password;

    [Header("Password UI Elements")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI feedbackText;

    [Tooltip("Container for dynamically created option buttons.")]
    public Transform optionsContainer;

    [Tooltip("Prefab for a single option button.")]
    public GameObject optionButtonPrefab;

    [Tooltip("Submit button (optional — can auto-submit on selection).")]
    public Button submitButton;

    private int selectedIndex = -1;
    private Button[] optionButtons;

    protected override void PopulateUI(ChallengeData data)
    {
        if (titleText != null) titleText.text = data.title;
        if (descriptionText != null) descriptionText.text = data.description;
        if (feedbackText != null) feedbackText.text = "";

        selectedIndex = -1;
        ClearOptions();

        // Create option buttons
        optionButtons = new Button[data.options.Count];
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
                    btn.onClick.AddListener(() => SelectOption(index));
                    optionButtons[i] = btn;
                }
            }
        }

        // Set up submit button
        if (submitButton != null)
        {
            submitButton.onClick.RemoveAllListeners();
            submitButton.onClick.AddListener(OnSubmitClicked);
            submitButton.interactable = false; // enable when option selected
        }
    }

    private void SelectOption(int index)
    {
        selectedIndex = index;

        // Highlight selected, unhighlight others
        for (int i = 0; i < optionButtons.Length; i++)
        {
            if (optionButtons[i] == null) continue;

            ColorBlock colors = optionButtons[i].colors;
            colors.normalColor = (i == selectedIndex)
                ? new Color(0.3f, 0.5f, 0.9f, 1f) // highlighted blue
                : Color.white;
            optionButtons[i].colors = colors;
        }

        if (submitButton != null)
            submitButton.interactable = true;
    }

    private void OnSubmitClicked()
    {
        if (selectedIndex < 0 || currentData == null) return;

        bool correct = currentData.IsCorrectChoice(selectedIndex);

        // Show visual feedback
        for (int i = 0; i < optionButtons.Length; i++)
        {
            if (optionButtons[i] == null) continue;

            ColorBlock colors = optionButtons[i].colors;
            if (currentData.options[i].isCorrect)
                colors.normalColor = new Color(0.2f, 0.8f, 0.2f, 1f); // green for correct
            else if (i == selectedIndex && !correct)
                colors.normalColor = new Color(0.8f, 0.2f, 0.2f, 1f); // red for wrong selection
            optionButtons[i].colors = colors;
            optionButtons[i].interactable = false;
        }

        if (feedbackText != null)
        {
            feedbackText.text = correct
                ? "<color=green>Correct!</color> " + currentData.options[selectedIndex].feedbackNarrative
                : "<color=red>Wrong!</color> " + currentData.options[selectedIndex].feedbackNarrative;
        }

        if (submitButton != null)
            submitButton.interactable = false;

        SubmitChoice(selectedIndex);
    }

    protected override void ShowFeedback(string feedbackText, bool wasCorrect)
    {
        float delay = wasCorrect ? 3f : 1f;
        StartCoroutine(DelayedCloseRoutine(delay));
    }

    private System.Collections.IEnumerator DelayedCloseRoutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        Close();
    }

    private void ClearOptions()
    {
        if (optionsContainer == null) return;

        for (int i = optionsContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(optionsContainer.GetChild(i).gameObject);
        }
        optionButtons = null;
    }

    public override void Close()
    {
        ClearOptions();
        base.Close();
    }
}
