/// <summary>
/// Tracks the result of a completed challenge.
/// </summary>
[System.Serializable]
public class ChallengeResult
{
    public string challengeId;
    public bool passed;
    public int choiceIndex;
    public string playerName;

    public ChallengeResult(string challengeId, bool passed, int choiceIndex, string playerName = "")
    {
        this.challengeId = challengeId;
        this.passed = passed;
        this.choiceIndex = choiceIndex;
        this.playerName = playerName;
    }
}
