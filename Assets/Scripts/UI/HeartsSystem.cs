using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Displays 3 pixel hearts in the bottom-left corner of the HUD.
/// Each wrong answer on any challenge drains one heart (full → empty).
/// Attach to a persistent HUD GameObject in the scene.
/// </summary>
public class HeartsSystem : MonoBehaviour
{
    public static HeartsSystem Instance { get; private set; }

    [Header("Heart Sprites")]
    [Tooltip("The full/filled heart sprite (Full-heart.png).")]
    public Sprite fullHeartSprite;

    [Tooltip("The empty heart sprite (Empty-heart.png).")]
    public Sprite emptyHeartSprite;

    [Header("Heart UI Images")]
    [Tooltip("Assign the 3 heart Image components left to right.")]
    public Image heart1;
    public Image heart2;
    public Image heart3;

    [Header("Settings")]
    [Tooltip("Maximum number of hearts (lives).")]
    public int maxHearts = 3;

    private int currentHearts;
    private Image[] hearts;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        hearts = new Image[] { heart1, heart2, heart3 };
        currentHearts = maxHearts;

        // Hide until the player spawns in
        gameObject.SetActive(false);

        RefreshHearts();

        // Hook into ChallengeManager's fail event
        if (ChallengeManager.Instance != null)
        {
            ChallengeManager.Instance.OnChallengeFailed.AddListener(OnChallengeFailed);
        }
        else
        {
            // ChallengeManager may not be ready yet — defer to next frame
            StartCoroutine(LateSubscribe());
        }
    }

    /// <summary>Show the hearts HUD (call when player spawns in).</summary>
    public void Show()
    {
        gameObject.SetActive(true);
    }

    /// <summary>Hide the hearts HUD (call on game over or return to lobby).</summary>
    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private System.Collections.IEnumerator LateSubscribe()
    {
        yield return null;
        if (ChallengeManager.Instance != null)
            ChallengeManager.Instance.OnChallengeFailed.AddListener(OnChallengeFailed);
    }

    private void OnChallengeFailed(ChallengeData data)
    {
        LoseHeart();
    }

    /// <summary>
    /// Remove one heart. Call this manually if needed elsewhere.
    /// </summary>
    public void LoseHeart()
    {
        if (currentHearts <= 0) return;
        currentHearts--;
        RefreshHearts();

        if (currentHearts <= 0)
        {
            StartCoroutine(GameOverSequence());
        }
    }

    private System.Collections.IEnumerator GameOverSequence()
    {
        // Small delay so the feedback screen can finish showing
        yield return new WaitForSeconds(2f);

        UIManager uiManager = FindFirstObjectByType<UIManager>();
        if (uiManager != null)
            uiManager.GameOver();
    }

    /// <summary>
    /// Restore all hearts (e.g. on replay).
    /// </summary>
    public void ResetHearts()
    {
        currentHearts = maxHearts;
        RefreshHearts();
    }

    private void RefreshHearts()
    {
        for (int i = 0; i < hearts.Length; i++)
        {
            if (hearts[i] == null) continue;
            hearts[i].sprite = i < currentHearts ? fullHeartSprite : emptyHeartSprite;
        }
    }

    void OnDestroy()
    {
        if (ChallengeManager.Instance != null)
            ChallengeManager.Instance.OnChallengeFailed.RemoveListener(OnChallengeFailed);
    }
}
