using UnityEngine;
using TMPro;

/// <summary>
/// Displays challenge completion results on a whiteboard in the office.
/// Updates in real-time as challenges are completed.
/// Attach to a whiteboard/bulletin board object with a TextMeshPro component.
/// </summary>
public class ScoreboardDisplay : MonoBehaviour
{
    [Header("Display")]
    [Tooltip("Text component on the whiteboard.")]
    public TextMeshPro boardText;

    [Tooltip("Update interval in seconds.")]
    public float updateInterval = 1f;

    private float nextUpdateTime;

    void Update()
    {
        if (Time.time < nextUpdateTime) return;
        nextUpdateTime = Time.time + updateInterval;

        UpdateBoard();
    }

    private void UpdateBoard()
    {
        ChallengeManager cm = ChallengeManager.Instance;
        if (cm == null || boardText == null) return;

        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine("<b>SECURITY TRAINING PROGRESS</b>");
        sb.AppendLine("─────────────────────");

        foreach (var challenge in cm.allChallenges)
        {
            ChallengeResult result = cm.GetResult(challenge.challengeId);

            string icon;
            if (result == null)
                icon = "○"; // not attempted
            else if (result.passed)
                icon = "<color=green>●</color>"; // passed
            else
                icon = "<color=red>●</color>"; // failed

            sb.AppendLine($"  {icon}  {challenge.title}");
        }

        sb.AppendLine("─────────────────────");
        sb.AppendLine($"  Completed: {cm.CompletedCount}/{cm.TotalCount}");
        sb.AppendLine($"  Passed: {cm.PassedCount}/{cm.TotalCount}");

        boardText.text = sb.ToString();
    }
}
