using System.Collections;
using UnityEngine;
using TMPro;

namespace PhishingGame
{
    public class EmailTutorialOverlay : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject overlayPanel;
        [SerializeField] private TextMeshProUGUI tutorialText;
        [SerializeField] private TextTyper textTyper;
        [SerializeField] private CanvasGroup overlayCanvasGroup;

        [Header("Messages")]
        [SerializeField] private string firstMessage = "The wording feels urgent… maybe too urgent.";
        [SerializeField] private string secondMessage = "What do you do?";

        [Header("Timing")]
        [SerializeField] private float delayBetweenMessages = 3f;
        [SerializeField] private float typingSpeed = 0.05f;
        [SerializeField] private float displayTimeAfterSecondMessage = 2f;
        [SerializeField] private float fadeOutDuration = 1f;

        private void Start()
        {
            if (overlayPanel != null)
            {
                overlayPanel.SetActive(false);
            }

            // Setup CanvasGroup if not assigned
            if (overlayCanvasGroup == null && overlayPanel != null)
            {
                overlayCanvasGroup = overlayPanel.GetComponent<CanvasGroup>();
                if (overlayCanvasGroup == null)
                {
                    overlayCanvasGroup = overlayPanel.AddComponent<CanvasGroup>();
                }
            }

            if (textTyper == null && tutorialText != null)
            {
                textTyper = tutorialText.gameObject.GetComponent<TextTyper>();
                if (textTyper == null)
                {
                    textTyper = tutorialText.gameObject.AddComponent<TextTyper>();
                }
            }
        }

        public void ShowTutorial()
        {
            if (overlayPanel != null)
            {
                overlayPanel.SetActive(true);
                
                // Reset alpha to fully visible
                if (overlayCanvasGroup != null)
                {
                    overlayCanvasGroup.alpha = 1f;
                }
            }

            StartCoroutine(PlayTutorialSequence());
        }

        public void HideTutorial()
        {
            if (overlayPanel != null)
            {
                overlayPanel.SetActive(false);
            }

            if (textTyper != null)
            {
                textTyper.ClearText();
            }
        }

        private IEnumerator PlayTutorialSequence()
        {
            if (textTyper != null)
            {
                textTyper.StartTyping(firstMessage);
            }


            // Wait for second message to finish typing + display time
            yield return new WaitForSeconds(secondMessage.Length * typingSpeed + displayTimeAfterSecondMessage);

            // Fade out animation
            yield return StartCoroutine(FadeOut());

            // Hide the overlay after fade
            if (overlayPanel != null)
            {
                overlayPanel.SetActive(false);
            }
        }

        private IEnumerator FadeOut()
        {
            if (overlayCanvasGroup == null) yield break;

            float elapsedTime = 0f;
            float startAlpha = overlayCanvasGroup.alpha;

            while (elapsedTime < fadeOutDuration)
            {
                elapsedTime += Time.deltaTime;
                overlayCanvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsedTime / fadeOutDuration);
                yield return null;
            }

            overlayCanvasGroup.alpha = 0f;
            yield return new WaitForSeconds(firstMessage.Length * typingSpeed + delayBetweenMessages);

            if (textTyper != null)
            {
                textTyper.StartTyping(secondMessage);
            }
        }
    }
}
