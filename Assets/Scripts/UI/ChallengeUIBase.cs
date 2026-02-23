using UnityEngine;

/// <summary>
/// Abstract base class for all challenge UI panels.
/// Each challenge type (Phishing, Password, etc.) has a concrete subclass.
/// Attach to a UI panel GameObject that starts disabled.
/// </summary>
public abstract class ChallengeUIBase : MonoBehaviour
{
    /// <summary>
    /// Which ChallengeType this UI handles. Override in subclasses.
    /// </summary>
    public abstract ChallengeType HandledType { get; }

    /// <summary>
    /// The currently loaded challenge data.
    /// </summary>
    protected ChallengeData currentData;

    /// <summary>
    /// Opens the challenge UI panel and populates it with the given data.
    /// </summary>
    public virtual void Open(ChallengeData data)
    {
        currentData = data;
        gameObject.SetActive(true);
        PopulateUI(data);
    }

    /// <summary>
    /// Closes the challenge UI panel and notifies ChallengeManager.
    /// </summary>
    public virtual void Close()
    {
        gameObject.SetActive(false);
        currentData = null;

        ChallengeManager.Instance?.OnChallengeUIClosed();
    }

    /// <summary>
    /// Override to populate the UI with challenge-specific content.
    /// </summary>
    protected abstract void PopulateUI(ChallengeData data);

    /// <summary>
    /// Call this from the Submit button to evaluate the player's answer.
    /// </summary>
    protected void SubmitChoice(int choiceIndex)
    {
        if (currentData == null) return;

        ChallengeManager.Instance?.SubmitAnswer(currentData.challengeId, choiceIndex);

        // Show per-option feedback before closing
        if (choiceIndex >= 0 && choiceIndex < currentData.options.Count)
        {
            string feedback = currentData.options[choiceIndex].feedbackNarrative;
            ShowFeedback(feedback, currentData.options[choiceIndex].isCorrect);
        }
    }

    /// <summary>
    /// Override to show in-panel feedback text after submission.
    /// Default implementation closes after a delay.
    /// </summary>
    protected virtual void ShowFeedback(string feedbackText, bool wasCorrect)
    {
        // Subclasses should override this to display feedback in their UI.
        // After feedback is acknowledged, call Close().
        StartCoroutine(DelayedClose(wasCorrect ? 2f : 0.5f));
    }

    private System.Collections.IEnumerator DelayedClose(float delay)
    {
        yield return new WaitForSeconds(delay);
        Close();
    }
}
