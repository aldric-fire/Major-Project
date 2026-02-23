using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Top-level game manager. Handles overall game flow:
/// spawning → free-roam → all challenges done → summary screen.
/// Place on a persistent GameObject in the scene.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Summary Screen")]
    [Tooltip("Root panel for the end-of-game summary.")]
    public GameObject summaryPanel;

    [Tooltip("Title text for the summary screen.")]
    public TextMeshProUGUI summaryTitleText;

    [Tooltip("Container for individual challenge result entries.")]
    public Transform summaryResultsContainer;

    [Tooltip("Prefab for a single result row (text + icon).")]
    public GameObject summaryResultPrefab;

    [Tooltip("Overall grade text (e.g., 'Security Score: A').")]
    public TextMeshProUGUI gradeText;

    [Tooltip("Button to replay all challenges.")]
    public Button replayButton;

    [Tooltip("Button to return to lobby.")]
    public Button returnToLobbyButton;

    private ChallengeManager challengeManager;
    private bool summaryShown = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (summaryPanel != null)
            summaryPanel.SetActive(false);
    }

    void Start()
    {
        challengeManager = ChallengeManager.Instance;

        if (challengeManager != null)
        {
            challengeManager.OnAllChallengesCompleted.AddListener(OnAllChallengesCompleted);
        }

        if (replayButton != null)
            replayButton.onClick.AddListener(OnReplayClicked);

        if (returnToLobbyButton != null)
            returnToLobbyButton.onClick.AddListener(OnReturnToLobbyClicked);
    }

    private void OnAllChallengesCompleted()
    {
        if (summaryShown) return;
        summaryShown = true;

        // Small delay so the last consequence/debrief can finish
        Invoke(nameof(ShowSummary), 2f);
    }

    /// <summary>
    /// Displays the summary screen with results for all challenges.
    /// </summary>
    public void ShowSummary()
    {
        if (summaryPanel == null || challengeManager == null) return;

        summaryPanel.SetActive(true);

        // Unlock cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (summaryTitleText != null)
            summaryTitleText.text = "Training Complete!";

        // Clear previous results
        if (summaryResultsContainer != null)
        {
            for (int i = summaryResultsContainer.childCount - 1; i >= 0; i--)
                Destroy(summaryResultsContainer.GetChild(i).gameObject);
        }

        // Populate results
        Dictionary<string, ChallengeResult> results = challengeManager.GetAllResults();
        int passed = 0;
        int total = challengeManager.TotalCount;

        foreach (var challenge in challengeManager.allChallenges)
        {
            results.TryGetValue(challenge.challengeId, out ChallengeResult result);

            if (summaryResultPrefab != null && summaryResultsContainer != null)
            {
                GameObject row = Instantiate(summaryResultPrefab, summaryResultsContainer);
                TextMeshProUGUI rowText = row.GetComponentInChildren<TextMeshProUGUI>();
                if (rowText != null)
                {
                    string status;
                    if (result == null)
                        status = "<color=gray>Not Attempted</color>";
                    else if (result.passed)
                    {
                        status = "<color=green>PASSED</color>";
                        passed++;
                    }
                    else
                        status = "<color=red>FAILED</color>";

                    rowText.text = $"{challenge.title}  —  {status}";
                }
            }
            else if (result != null && result.passed)
            {
                passed++;
            }
        }

        // Calculate grade
        if (gradeText != null)
        {
            float percentage = total > 0 ? (float)passed / total * 100f : 0f;
            string grade = CalculateGrade(percentage);
            gradeText.text = $"Security Score: {grade} ({passed}/{total})";
        }
    }

    private string CalculateGrade(float percentage)
    {
        if (percentage >= 100f) return "A+";
        if (percentage >= 75f) return "A";
        if (percentage >= 50f) return "B";
        if (percentage >= 25f) return "C";
        return "F";
    }

    private void OnReplayClicked()
    {
        if (challengeManager != null)
            challengeManager.ResetAllProgress();

        summaryShown = false;

        if (summaryPanel != null)
            summaryPanel.SetActive(false);

        // Re-lock cursor for gameplay
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Notify HUD
        HUDController hud = HUDController.Instance;
        if (hud != null)
            hud.ShowNotification("All challenges reset — try again!", 3f);
    }

    private void OnReturnToLobbyClicked()
    {
        // Use UIManager to handle lobby return
        UIManager uiManager = FindFirstObjectByType<UIManager>();
        if (uiManager != null)
        {
            uiManager.LeaveRoomAndShowLobby();
        }

        if (summaryPanel != null)
            summaryPanel.SetActive(false);

        summaryShown = false;
    }
}
