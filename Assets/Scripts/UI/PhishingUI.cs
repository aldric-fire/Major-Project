using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI panel for Phishing Email challenges.
/// Displays a mock email client with multiple emails; the player must flag phishing ones.
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

    [Tooltip("Title text at top.")]
    public TextMeshProUGUI titleText;

    [Tooltip("Description / instruction text.")]
    public TextMeshProUGUI descriptionText;

    private List<EmailEntryData> emailEntries = new List<EmailEntryData>();
    private int selectedEmailIndex = -1;

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

    protected override void PopulateUI(ChallengeData data)
    {
        // Clear previous entries
        ClearEmailList();

        if (titleText != null) titleText.text = data.title;
        if (descriptionText != null) descriptionText.text = data.description;
        if (feedbackText != null) feedbackText.text = "";
        if (emailDetailPanel != null) emailDetailPanel.SetActive(false);

        // Create email rows
        for (int i = 0; i < data.emails.Count; i++)
        {
            EmailData email = data.emails[i];
            EmailEntryData entry = new EmailEntryData
            {
                data = email,
                flaggedAsPhishing = false
            };

            if (emailEntryPrefab != null && emailListContainer != null)
            {
                GameObject row = Instantiate(emailEntryPrefab, emailListContainer);
                entry.uiRow = row;

                // Set up the row text
                TextMeshProUGUI rowText = row.GetComponentInChildren<TextMeshProUGUI>();
                if (rowText != null)
                    rowText.text = $"<b>{email.senderName}</b>  —  {email.subject}";

                // Set up the flag toggle
                Toggle toggle = row.GetComponentInChildren<Toggle>();
                if (toggle != null)
                {
                    entry.flagToggle = toggle;
                    toggle.isOn = false;
                    int index = i; // capture for closure
                    toggle.onValueChanged.AddListener((isOn) =>
                    {
                        emailEntries[index].flaggedAsPhishing = isOn;
                    });
                }

                // Set up click to view detail
                Button rowButton = row.GetComponent<Button>();
                if (rowButton != null)
                {
                    int index = i;
                    rowButton.onClick.AddListener(() => ShowEmailDetail(index));
                }
            }

            emailEntries.Add(entry);
        }

        // Set up submit button
        if (submitButton != null)
        {
            submitButton.onClick.RemoveAllListeners();
            submitButton.onClick.AddListener(OnSubmitClicked);
            submitButton.interactable = true;
        }
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

        // Evaluate: did the player correctly flag ALL phishing emails
        // and NOT flag any legitimate ones?
        bool allCorrect = true;
        int correctFlags = 0;
        int totalPhishing = 0;

        for (int i = 0; i < emailEntries.Count; i++)
        {
            bool isPhishing = emailEntries[i].data.isPhishing;
            bool flagged = emailEntries[i].flaggedAsPhishing;

            if (isPhishing) totalPhishing++;

            if (isPhishing && flagged)
                correctFlags++;
            else if (isPhishing && !flagged)
                allCorrect = false;
            else if (!isPhishing && flagged)
                allCorrect = false;
        }

        // Map to a choice index:
        // We use index 0 = correct (all flagged properly), index 1 = incorrect
        // The ChallengeData should have option[0] as correct and option[1] as incorrect
        int choiceIndex = allCorrect ? 0 : 1;

        // Show visual feedback on entries
        for (int i = 0; i < emailEntries.Count; i++)
        {
            if (emailEntries[i].uiRow != null)
            {
                Image bg = emailEntries[i].uiRow.GetComponent<Image>();
                if (bg != null)
                {
                    bool isPhishing = emailEntries[i].data.isPhishing;
                    bool flagged = emailEntries[i].flaggedAsPhishing;

                    if (isPhishing && flagged)
                        bg.color = new Color(0.2f, 0.8f, 0.2f, 0.5f); // green - correct flag
                    else if (isPhishing && !flagged)
                        bg.color = new Color(0.8f, 0.2f, 0.2f, 0.5f); // red - missed phishing
                    else if (!isPhishing && flagged)
                        bg.color = new Color(0.8f, 0.6f, 0.2f, 0.5f); // orange - false flag
                    else
                        bg.color = new Color(0.2f, 0.8f, 0.2f, 0.3f); // light green - correctly left
                }
            }
        }

        // Show feedback text
        if (feedbackText != null)
        {
            if (allCorrect)
                feedbackText.text = $"<color=green>Excellent!</color> You correctly identified all {totalPhishing} phishing emails.";
            else
                feedbackText.text = $"<color=red>Incorrect.</color> You flagged {correctFlags}/{totalPhishing} phishing emails correctly.";
        }

        // Disable submit button
        if (submitButton != null) submitButton.interactable = false;

        // Submit to ChallengeManager
        SubmitChoice(choiceIndex);
    }

    protected override void ShowFeedback(string feedbackText, bool wasCorrect)
    {
        // Keep the panel open longer so the player can review the highlighted emails
        float delay = wasCorrect ? 3f : 1f; // shorter for failure since consequence takes over
        StartCoroutine(DelayedCloseRoutine(delay));
    }

    private System.Collections.IEnumerator DelayedCloseRoutine(float delay)
    {
        yield return new WaitForSeconds(delay);
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
