using UnityEngine;
using UnityEngine.UI;

namespace PhishingGame
{
    /// <summary>
    /// Helper component for setting up UI buttons with click handlers.
    /// Attach this to any clickable Image to make it interactive.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class UIButtonHandler : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameStateManager gameStateManager;

        [Header("Button Type")]
        [SerializeField] private ButtonType buttonType;

        /// <summary>
        /// Enum for different button types in the phishing game
        /// </summary>
        public enum ButtonType
        {
            ChromeIcon,         // Desktop Chrome icon - opens Gmail
            PhishingEmail,      // Email preview in inbox - opens full email
            EmailLink,          // Link inside email (future use)
            EmailAttachment,    // Attachment button (future use)
            DeleteEmail,        // Delete button (future use)
            ReportPhishing      // Report button (future use)
        }

        private Button button;

        private void Awake()
        {
            button = GetComponent<Button>();

            // Auto-find GameStateManager if not assigned
            if (gameStateManager == null)
            {
                gameStateManager = FindObjectOfType<GameStateManager>();
                if (gameStateManager == null)
                {
                    Debug.LogError($"[UIButtonHandler] GameStateManager not found in scene! Button on {gameObject.name} won't work.");
                }
            }

            // Add click listener
            button.onClick.AddListener(OnButtonClick);
        }

        /// <summary>
        /// Handles button click based on button type
        /// </summary>
        private void OnButtonClick()
        {
            Debug.Log($"[UIButtonHandler] Button clicked: {buttonType} on {gameObject.name}");

            if (gameStateManager == null)
            {
                Debug.LogError("[UIButtonHandler] Cannot handle click - GameStateManager is null!");
                return;
            }

            // Route to appropriate handler based on button type
            switch (buttonType)
            {
                case ButtonType.ChromeIcon:
                    gameStateManager.OnChromeClicked();
                    break;

                case ButtonType.PhishingEmail:
                    gameStateManager.OnPhishingEmailClicked();
                    break;

                case ButtonType.EmailLink:
                    gameStateManager.OnClickLinkInEmail();
                    break;

                case ButtonType.EmailAttachment:
                    gameStateManager.OnDownloadAttachment();
                    break;

                case ButtonType.DeleteEmail:
                    gameStateManager.OnDeleteEmail();
                    break;

                case ButtonType.ReportPhishing:
                    gameStateManager.OnReportPhishing();
                    break;

                default:
                    Debug.LogWarning($"[UIButtonHandler] Unhandled button type: {buttonType}");
                    break;
            }
        }

        private void OnDestroy()
        {
            // Clean up listener
            if (button != null)
            {
                button.onClick.RemoveListener(OnButtonClick);
            }
        }
    }
}
