using UnityEngine;
using UnityEngine.InputSystem;

namespace PhishingGame
{
    /// <summary>
    /// Manages the game states for the phishing awareness mini-game.
    /// Controls UI visibility and transitions between different screens.
    /// </summary>
    public class GameStateManager : MonoBehaviour
    {
        [Header("UI Groups")]
        [SerializeField] private GameObject windowsUI;      // Windows Desktop UI group
        [SerializeField] private GameObject gmailUI;        // Gmail Inbox UI group
        [SerializeField] private GameObject emailUI;        // Open Email UI group

        [Header("State Display (Debug)")]
        [SerializeField] private GameState currentState = GameState.Idle;

        /// <summary>
        /// Enum representing all possible game states
        /// </summary>
        public enum GameState
        {
            Idle,               // Waiting for player to press E
            WindowsDesktop,     // Showing Windows desktop with Chrome icon
            GmailInbox,         // Showing Gmail inbox with phishing email
            OpenEmail           // Showing the full phishing email (decision moment)
        }

        private void Start()
        {
            Debug.Log("[PhishingGame] GameStateManager initialized");
            
            // Start in Idle state - everything hidden
            SetState(GameState.Idle);
        }

        // Removed Update() - E key now handled by laptop interaction
        // Game starts when player interacts with laptop

        /// <summary>
        /// Changes the current game state and updates UI visibility accordingly
        /// </summary>
        public void SetState(GameState newState)
        {
            Debug.Log($"[PhishingGame] State transition: {currentState} → {newState}");
            currentState = newState;

            // Hide all UI groups first
            HideAllUI();

            // Show the appropriate UI for the current state
            switch (currentState)
            {
                case GameState.Idle:
                    // Nothing visible, waiting for player input
                    break;

                case GameState.WindowsDesktop:
                    ShowWindowsDesktop();
                    break;

                case GameState.GmailInbox:
                    ShowGmailInbox();
                    break;

                case GameState.OpenEmail:
                    ShowOpenEmail();
                    break;
            }
        }

        /// <summary>
        /// Hides all UI groups
        /// </summary>
        private void HideAllUI()
        {
            if (windowsUI != null) windowsUI.SetActive(false);
            if (gmailUI != null) gmailUI.SetActive(false);
            if (emailUI != null) emailUI.SetActive(false);
        }

        /// <summary>
        /// Shows the Windows Desktop UI (icons on desktop background)
        /// </summary>
        private void ShowWindowsDesktop()
        {
            if (windowsUI != null)
            {
                windowsUI.SetActive(true);
                Debug.Log("[PhishingGame] Windows Desktop UI shown");
            }
            else
            {
                Debug.LogError("[PhishingGame] WindowsUI is not assigned!");
            }
        }

        /// <summary>
        /// Shows the Gmail Inbox UI (inbox with phishing email preview)
        /// </summary>
        private void ShowGmailInbox()
        {
            if (gmailUI != null)
            {
                gmailUI.SetActive(true);
                Debug.Log("[PhishingGame] Gmail Inbox UI shown");
            }
            else
            {
                Debug.LogError("[PhishingGame] GmailUI is not assigned!");
            }
        }

        /// <summary>
        /// Shows the Open Email UI (full phishing email - decision moment)
        /// </summary>
        private void ShowOpenEmail()
        {
            if (emailUI != null)
            {
                emailUI.SetActive(true);
                Debug.Log("[PhishingGame] Open Email UI shown (decision moment)");
            }
            else
            {
                Debug.LogError("[PhishingGame] EmailUI is not assigned!");
            }
        }

        /// <summary>
        /// Public method to start the phishing game (called by laptop interaction)
        /// </summary>
        public void StartPhishingGame()
        {
            Debug.Log("[PhishingGame] Starting phishing game from laptop interaction");
            SetState(GameState.WindowsDesktop);
        }

        /// <summary>
        /// Public method to transition to Gmail Inbox (called by Chrome icon button)
        /// </summary>
        public void OnChromeClicked()
        {
            Debug.Log("[PhishingGame] Chrome icon clicked");
            SetState(GameState.GmailInbox);
        }

        /// <summary>
        /// Public method to open phishing email (called by email preview button)
        /// </summary>
        public void OnPhishingEmailClicked()
        {
            Debug.Log("[PhishingGame] Phishing email clicked - opening full email");
            SetState(GameState.OpenEmail);
            
            // Show tutorial overlay with typing effect
            EmailTutorialOverlay tutorial = emailUI.GetComponent<EmailTutorialOverlay>();
            if (tutorial != null)
            {
                tutorial.ShowTutorial();
            }
        }

        /// <summary>
        /// Future-proofing: Handle clicking link in email (bad outcome)
        /// </summary>
        public void OnClickLinkInEmail()
        {
            Debug.LogWarning("[PhishingGame] Player clicked malicious link! (not implemented yet)");
            // TODO: Trigger infection state, show virus overlay, start timer
        }

        /// <summary>
        /// Future-proofing: Handle downloading attachment (bad outcome)
        /// </summary>
        public void OnDownloadAttachment()
        {
            Debug.LogWarning("[PhishingGame] Player downloaded malicious attachment! (not implemented yet)");
            // TODO: Trigger infection state, show virus overlay, start timer
        }

        /// <summary>
        /// Future-proofing: Handle deleting email (good outcome)
        /// </summary>
        public void OnDeleteEmail()
        {
            Debug.Log("[PhishingGame] Player deleted phishing email (good outcome)");
            // TODO: Show positive feedback, return to inbox
        }

        /// <summary>
        /// Future-proofing: Handle reporting phishing (best outcome)
        /// </summary>
        public void OnReportPhishing()
        {
            Debug.Log("[PhishingGame] Player reported phishing email (best outcome!)");
            // TODO: Show excellent feedback, educational message, return to inbox
        }

        /// <summary>
        /// Public getter for current state (useful for other scripts)
        /// </summary>
        public GameState GetCurrentState()
        {
            return currentState;
        }
    }
}
