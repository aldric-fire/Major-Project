using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI panel for Phishing Email challenges.
/// Intro sequence: Title screen → email appears → description appears → description fades → email + options remain.
/// </summary>
public class PhishingUI : ChallengeUIBase
{
    public override ChallengeType HandledType => ChallengeType.Phishing;

    [Header("Phishing UI Elements")]
    [Tooltip("Parent transform where email entries are instantiated.")]
    public Transform emailListContainer;

    [Tooltip("Prefab for a single email entry row.")]
    public GameObject emailEntryPrefab;

    [Tooltip("Panel showing the selected email's full content.")]
    public GameObject emailDetailPanel;

    [Tooltip("Text fields for the email detail view.")]
    public TextMeshProUGUI detailSenderText;
    public TextMeshProUGUI detailSubjectText;
    public TextMeshProUGUI detailBodyText;

    [Tooltip("Submit button.")]
    public Button submitButton;

    [Tooltip("Feedback text shown after submission.")]
    public TextMeshProUGUI feedbackText;

    [Tooltip("Title text at top of the intro screen.")]
    public TextMeshProUGUI titleText;

    [Tooltip("Description / instruction text shown briefly after the email appears.")]
    public TextMeshProUGUI descriptionText;

    [Header("Title Screen Overlay")]
    [Tooltip("Full-screen semi-transparent dark panel shown during the title screen. Assign a UI Image with ~0.6 alpha black colour.")]
    public GameObject titleScreenOverlay;

    [Header("Option Navigator")]
    [Tooltip("The root GameObject of the entire option navigator (the container with the black background). This whole panel is hidden during the intro.")]
    public GameObject optionNavigatorContainer;

    [Tooltip("Left arrow button (<) to go to previous option.")]
    public Button prevOptionButton;

    [Tooltip("Right arrow button (>) to go to next option.")]
    public Button nextOptionButton;

    [Tooltip("Text showing the current option.")]
    public TextMeshProUGUI optionDisplayText;

    [Tooltip("Text showing e.g. '2 / 3' option counter.")]
    public TextMeshProUGUI optionCounterText;

    [Header("Feedback Screen")]
    [Tooltip("Full-screen semi-transparent overlay shown during the feedback screen. Can be the same as titleScreenOverlay or a separate one styled differently.")]
    public GameObject feedbackScreenOverlay;

    [Tooltip("How long the feedback screen stays up before closing (seconds).")]
    public float feedbackDuration = 3.5f;

    [Header("Intro Timing")]
    [Tooltip("How long the title screen stays up (seconds).")]
    public float titleDuration = 2.5f;

    [Tooltip("Pause between email appearing and description appearing (seconds).")]
    public float emailToDescriptionDelay = 0.5f;

    [Tooltip("How long the description text stays visible (seconds).")]
    public float descriptionDuration = 2.5f;

    private List<EmailEntryData> emailEntries = new List<EmailEntryData>();
    private int selectedEmailIndex = -1;
    private int currentOptionIndex = 0;

    // Groups of UI that are shown/hidden at each stage
    private GameObject[] emailGroup;
    private GameObject[] optionsGroup;

    /// <summary>
    /// Runtime data for each email row, tracking the player's flag state.
    /// </summary>
    private class EmailEntryData
    {
        public EmailData data;
        public bool flaggedAsPhishing;
        public GameObject uiRow;
        public Toggle flagToggle;
    }

    /// <summary>
    /// Override Open so we can run the intro sequence as a coroutine.
    /// </summary>
    public override void Open(ChallengeData data)
    {
        currentData = data;
        gameObject.SetActive(true);
        BuildUI(data);
        StartCoroutine(IntroSequence());
    }

    /// <summary>
    /// Builds all email rows and wires up buttons, but does NOT show them yet.
    /// </summary>
    private void BuildUI(ChallengeData data)
    {
        ClearEmailList();

        // Populate text content
        if (titleText != null) titleText.text = data.title;
        if (descriptionText != null) descriptionText.text = data.description;
        if (feedbackText != null) { feedbackText.text = ""; feedbackText.gameObject.SetActive(false); }
        if (feedbackScreenOverlay != null) feedbackScreenOverlay.SetActive(false);
        if (emailDetailPanel != null) emailDetailPanel.SetActive(false);

        // Create email rows
        for (int i = 0; i < data.emails.Count; i++)
        {
            EmailData email = data.emails[i];
            EmailEntryData entry = new EmailEntryData { data = email, flaggedAsPhishing = false };

            if (emailEntryPrefab != null && emailListContainer != null)
            {
                GameObject row = Instantiate(emailEntryPrefab, emailListContainer);
                entry.uiRow = row;

                TextMeshProUGUI rowText = row.GetComponentInChildren<TextMeshProUGUI>();
                if (rowText != null)
                    rowText.text = $"<b>{email.senderName}</b>  —  {email.subject}";

                Toggle toggle = row.GetComponentInChildren<Toggle>();
                if (toggle != null)
                {
                    entry.flagToggle = toggle;
                    toggle.isOn = false;
                    int index = i;
                    toggle.onValueChanged.AddListener((isOn) => emailEntries[index].flaggedAsPhishing = isOn);
                }

                Button rowButton = row.GetComponent<Button>();
                if (rowButton != null)
                {
                    int index = i;
                    rowButton.onClick.AddListener(() => ShowEmailDetail(index));
                }
            }
            emailEntries.Add(entry);
        }

        // Wire up submit
        if (submitButton != null)
        {
            submitButton.onClick.RemoveAllListeners();
            submitButton.onClick.AddListener(OnSubmitClicked);
        }

        // Wire up option navigator
        currentOptionIndex = 0;
        RefreshOptionNavigator(data);

        if (prevOptionButton != null)
        {
            prevOptionButton.onClick.RemoveAllListeners();
            prevOptionButton.onClick.AddListener(() =>
            {
                currentOptionIndex = (currentOptionIndex - 1 + currentData.options.Count) % currentData.options.Count;
                RefreshOptionNavigator(currentData);
            });
        }

        if (nextOptionButton != null)
        {
            nextOptionButton.onClick.RemoveAllListeners();
            nextOptionButton.onClick.AddListener(() =>
            {
                currentOptionIndex = (currentOptionIndex + 1) % currentData.options.Count;
                RefreshOptionNavigator(currentData);
            });
        }

        // Cache groups for toggling
        emailGroup = new GameObject[]
        {
            emailListContainer != null ? emailListContainer.gameObject : null,
            emailDetailPanel
        };

        optionsGroup = new GameObject[]
        {
            submitButton != null ? submitButton.gameObject : null,
            optionNavigatorContainer
        };
    }

    /// <summary>
    /// Drives the intro sequence:
    /// 1. Title screen only (overlay + title)
    /// 2. Title vanishes → email appears
    /// 3. Description appears
    /// 4. Description disappears → options revealed
    /// </summary>
    private IEnumerator IntroSequence()
    {
        // --- Stage 1: Title screen (overlay + title + description together) ---
        SetGroupActive(emailGroup, false);
        SetGroupActive(optionsGroup, false);
        SetActive(feedbackText, false);
        SetActive(feedbackScreenOverlay, false);
        SetActive(titleScreenOverlay, true);
        SetActive(titleText, true);
        SetActive(descriptionText, true);

        yield return new WaitForSeconds(titleDuration);

        // --- Stage 2: Title + description vanish together, email appears ---
        SetActive(titleScreenOverlay, false);
        SetActive(titleText, false);
        SetActive(descriptionText, false);
        SetGroupActive(emailGroup, true);

        yield return new WaitForSeconds(emailToDescriptionDelay);

        // --- Stage 3: Options revealed ---
        SetGroupActive(optionsGroup, true);

        if (submitButton != null) submitButton.interactable = true;
        if (prevOptionButton != null) prevOptionButton.interactable = true;
        if (nextOptionButton != null) nextOptionButton.interactable = true;
    }

    // Not used directly anymore — intro is driven by Open() → BuildUI() + IntroSequence()
    protected override void PopulateUI(ChallengeData data) { }

    private void SetActive(GameObject obj, bool active)
    {
        if (obj != null) obj.SetActive(active);
    }

    private void SetActive(Component obj, bool active)
    {
        if (obj != null) obj.gameObject.SetActive(active);
    }

    private void SetGroupActive(GameObject[] group, bool active)
    {
        if (group == null) return;
        foreach (var go in group)
            if (go != null) go.SetActive(active);
    }

    private void RefreshOptionNavigator(ChallengeData data)
    {
        if (data == null || data.options.Count == 0) return;

        if (optionDisplayText != null)
            optionDisplayText.text = data.options[currentOptionIndex].text;

        if (optionCounterText != null)
            optionCounterText.text = $"{currentOptionIndex + 1} / {data.options.Count}";
    }

    private void ShowEmailDetail(int index)
    {
        if (index < 0 || index >= emailEntries.Count) return;

        selectedEmailIndex = index;
        EmailData email = emailEntries[index].data;

        if (emailDetailPanel != null) emailDetailPanel.SetActive(true);
        if (detailSenderText != null) detailSenderText.text = $"From: {email.senderName} <{email.senderAddress}>";
        if (detailSubjectText != null) detailSubjectText.text = $"Subject: {email.subject}";
        if (detailBodyText != null) detailBodyText.text = email.body;
    }

    private void OnSubmitClicked()
    {
        if (currentData == null) return;

        int choiceIndex = currentOptionIndex;
        bool isCorrect = currentData.options[choiceIndex].isCorrect;
        string optionFeedback = currentData.options[choiceIndex].feedbackNarrative;

        // Pre-fill feedback text so it's ready when the screen appears
        if (feedbackText != null)
        {
            feedbackText.text = isCorrect
                ? $"<color=green>Correct!</color>\n\n{optionFeedback}"
                : $"<color=red>Incorrect.</color>\n\n{optionFeedback}";
        }

        // Submit to ChallengeManager (this also triggers ShowFeedback)
        SubmitChoice(choiceIndex);
    }

    protected override void ShowFeedback(string feedback, bool wasCorrect)
    {
        StartCoroutine(FeedbackSequence());
    }

    /// <summary>
    /// Hides all gameplay UI, shows the feedback screen overlay + feedback text, then closes.
    /// </summary>
    private IEnumerator FeedbackSequence()
    {
        // Hide everything
        SetGroupActive(emailGroup, false);
        SetGroupActive(optionsGroup, false);
        SetActive(descriptionText, false);
        SetActive(titleText, false);
        SetActive(titleScreenOverlay, false);
        SetActive(emailDetailPanel, false);

        // Show feedback screen
        SetActive(feedbackScreenOverlay, true);
        SetActive(feedbackText, true);

        yield return new WaitForSeconds(feedbackDuration);

        Close();
    }

    private void ClearEmailList()
    {
        foreach (var entry in emailEntries)
        {
            if (entry.uiRow != null)
                Destroy(entry.uiRow);
        }
        emailEntries.Clear();
        selectedEmailIndex = -1;
    }

    public override void Close()
    {
        ClearEmailList();
        base.Close();
    }
}
