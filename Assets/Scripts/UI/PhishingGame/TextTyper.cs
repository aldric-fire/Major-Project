using System.Collections;
using UnityEngine;
using TMPro;

namespace PhishingGame
{
    public class TextTyper : MonoBehaviour
    {
        [Header("Typing Settings")]
        [SerializeField] private float typingSpeed = 0.05f;

        private TextMeshProUGUI textComponent;
        private string fullText = "";
        private Coroutine typingCoroutine;

        private void Awake()
        {
            textComponent = GetComponent<TextMeshProUGUI>();
        }

        public void StartTyping(string text)
        {
            fullText = text;
            
            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
            }

            typingCoroutine = StartCoroutine(TypeText());
        }

        private IEnumerator TypeText()
        {
            textComponent.text = "";
            
            foreach (char c in fullText)
            {
                textComponent.text += c;
                yield return new WaitForSeconds(typingSpeed);
            }

            typingCoroutine = null;
        }

        public void ClearText()
        {
            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
                typingCoroutine = null;
            }
            textComponent.text = "";
            fullText = "";
        }
    }
}
