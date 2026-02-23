using UnityEngine;

/// <summary>
/// Represents a single selectable option within a challenge.
/// Used inside ChallengeData ScriptableObjects.
/// </summary>
[System.Serializable]
public class ChallengeOption
{
    [Tooltip("The text displayed for this option.")]
    public string text;

    [Tooltip("Whether this is the correct answer.")]
    public bool isCorrect;

    [Tooltip("Narrative feedback shown after selecting this option.")]
    [TextArea(2, 4)]
    public string feedbackNarrative;
}
