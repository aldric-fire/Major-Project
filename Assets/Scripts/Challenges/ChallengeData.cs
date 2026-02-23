using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ScriptableObject that defines a single cybersecurity challenge.
/// Create instances via Assets > Create > Cybersecurity > Challenge Data.
/// </summary>
[CreateAssetMenu(fileName = "NewChallenge", menuName = "Cybersecurity/Challenge Data")]
public class ChallengeData : ScriptableObject
{
    [Header("Identity")]
    [Tooltip("Unique identifier for this challenge (e.g., 'phishing_01').")]
    public string challengeId;

    [Tooltip("Display title shown in the challenge UI.")]
    public string title;

    [Tooltip("Brief description or scenario setup text.")]
    [TextArea(3, 6)]
    public string description;

    [Header("Challenge Configuration")]
    public ChallengeType challengeType;

    [Tooltip("Difficulty tier: 1 = Easy, 2 = Medium, 3 = Hard.")]
    [Range(1, 3)]
    public int difficultyTier = 1;

    [Header("Options")]
    [Tooltip("The choices the player can make.")]
    public List<ChallengeOption> options = new List<ChallengeOption>();

    [Header("Phishing-Specific (optional)")]
    [Tooltip("For phishing challenges: list of email data to display.")]
    public List<EmailData> emails = new List<EmailData>();

    [Header("Consequence")]
    [Tooltip("Narrative text shown when the player fails this challenge.")]
    [TextArea(3, 6)]
    public string consequenceNarrative;

    [Tooltip("The type of environmental consequence triggered on failure.")]
    public ConsequenceType consequenceType = ConsequenceType.GenericAlert;

    [Header("Debrief / Education")]
    [Tooltip("Educational text explaining why the correct answer matters.")]
    [TextArea(3, 8)]
    public string debriefText;

    [Tooltip("Real-world statistic to reinforce the lesson.")]
    public string realWorldStat;

    /// <summary>
    /// Returns the index of the first correct option, or -1 if none.
    /// </summary>
    public int GetCorrectOptionIndex()
    {
        for (int i = 0; i < options.Count; i++)
        {
            if (options[i].isCorrect) return i;
        }
        return -1;
    }

    /// <summary>
    /// Checks whether the given choice index is a correct answer.
    /// </summary>
    public bool IsCorrectChoice(int index)
    {
        if (index < 0 || index >= options.Count) return false;
        return options[index].isCorrect;
    }
}

/// <summary>
/// Data for a single email displayed in a Phishing challenge.
/// </summary>
[System.Serializable]
public class EmailData
{
    public string senderName;
    public string senderAddress;
    public string subject;

    [TextArea(3, 8)]
    public string body;

    [Tooltip("True if this email is a phishing attempt.")]
    public bool isPhishing;

    [Tooltip("Hints the player could spot (shown in debrief).")]
    [TextArea(2, 4)]
    public string phishingClues;
}

/// <summary>
/// Types of environmental consequences triggered on challenge failure.
/// </summary>
public enum ConsequenceType
{
    GenericAlert,       // Red screen flash + alarm
    RansomwareScreen,   // All office monitors change to ransom texture
    DataBreach,         // News ticker on office TV
    PhysicalBreach,     // Security camera feed popup
    NetworkDown         // Lights flicker, monitors go dark briefly
}
