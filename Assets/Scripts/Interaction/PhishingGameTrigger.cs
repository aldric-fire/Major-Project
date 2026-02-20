using UnityEngine;
using PhishingGame;

/// <summary>
/// Attached to laptop GameObject to trigger the phishing mini-game.
/// When player interacts with the laptop, it starts the phishing game.
/// </summary>
public class PhishingGameTrigger : MonoBehaviour
{
    [Header("Phishing Game Reference")]
    [SerializeField] private GameStateManager phishingGameManager;

    [Header("Player Control")]
    [SerializeField] private bool lockPlayerMovement = true;
    [SerializeField] private bool showCursor = true;

    private void Start()
    {
        Debug.Log("[PhishingGameTrigger] Initialized on " + gameObject.name);

        // Auto-find GameStateManager if not assigned
        if (phishingGameManager == null)
        {
            phishingGameManager = FindObjectOfType<GameStateManager>();
            if (phishingGameManager == null)
            {
                Debug.LogError("[PhishingGameTrigger] GameStateManager not found in scene! Phishing game won't start.");
            }
            else
            {
                Debug.Log("[PhishingGameTrigger] Auto-found GameStateManager");
            }
        }
    }

    /// <summary>
    /// Called by InteractionPrompt when player presses E while looking at laptop
    /// </summary>
    public void StartPhishingGame()
    {
        Debug.Log("[PhishingGameTrigger] Player interacted with laptop - starting phishing game");

        if (phishingGameManager == null)
        {
            Debug.LogError("[PhishingGameTrigger] Cannot start game - GameStateManager is null!");
            return;
        }

        // Start the phishing game
        phishingGameManager.StartPhishingGame();

        // Handle player controls
        if (lockPlayerMovement)
        {
            DisablePlayerMovement();
        }

        if (showCursor)
        {
            UnlockCursor();
        }
    }

    /// <summary>
    /// Disables player movement (finds PlayerMovement component)
    /// </summary>
    private void DisablePlayerMovement()
    {
        PlayerMovement playerMovement = FindObjectOfType<PlayerMovement>();
        if (playerMovement != null)
        {
            playerMovement.enabled = false;
            Debug.Log("[PhishingGameTrigger] Player movement disabled");
        }
    }

    /// <summary>
    /// Unlocks and shows the cursor for UI interaction
    /// </summary>
    private void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Debug.Log("[PhishingGameTrigger] Cursor unlocked");
    }

    /// <summary>
    /// Public method to end the phishing game session (for future use)
    /// </summary>
    public void EndPhishingGame()
    {
        Debug.Log("[PhishingGameTrigger] Ending phishing game session");

        // Re-enable player movement
        PlayerMovement playerMovement = FindObjectOfType<PlayerMovement>();
        if (playerMovement != null)
        {
            playerMovement.enabled = true;
        }

        // Lock cursor back
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Reset game state
        if (phishingGameManager != null)
        {
            phishingGameManager.SetState(GameStateManager.GameState.Idle);
        }
    }
}
