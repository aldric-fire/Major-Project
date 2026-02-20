using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace UI.PhishingGame
{
    /// <summary>
    /// Detects mouse hover over the phishing email and displays a popup at a specific position.
    /// Logs mouse coordinates to help determine the popup placement.
    /// </summary>
    public class EmailHoverPopup : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("Popup Settings")]
        [SerializeField] private GameObject hoverPopup;
        
        [Header("Popup Position")]
        [Tooltip("Position of the popup in Canvas local coordinates (set this after testing)")]
        [SerializeField] private Vector2 popupPosition = new Vector2(400f, 300f);
        
        [Header("Debug Settings")]
        [SerializeField] private bool logMousePosition = true;
        [SerializeField] private float logInterval = 0.1f; // Log every 0.1 seconds
        
        private Canvas canvas;
        private RectTransform canvasRectTransform;
        private RectTransform popupRectTransform;
        private RectTransform emailRectTransform;
        private Image emailImage;
        private bool isHovering = false;
        private float lastLogTime = 0f;

        private void Awake()
        {
            // Get the canvas (searches up the hierarchy)
            canvas = GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                canvasRectTransform = canvas.GetComponent<RectTransform>();
            }

            // Get the email's RectTransform (this GameObject)
            emailRectTransform = GetComponent<RectTransform>();
            emailImage = GetComponent<Image>();

            // CRITICAL: Enable raycast target on the email image
            if (emailImage != null)
            {
                if (!emailImage.raycastTarget)
                {
                    Debug.LogWarning("EmailHoverPopup: Raycast Target was disabled! Enabling it now.");
                    emailImage.raycastTarget = true;
                }
            }
            else
            {
                Debug.LogError("EmailHoverPopup: No Image component found on this GameObject!");
            }

            // Get popup's RectTransform if assigned
            if (hoverPopup != null)
            {
                popupRectTransform = hoverPopup.GetComponent<RectTransform>();
                hoverPopup.SetActive(false); // Start hidden
                Debug.Log("EmailHoverPopup: Initialized. Popup hidden by default.");
            }
            else
            {
                Debug.LogWarning("EmailHoverPopup: Hover popup GameObject is not assigned!");
            }

            // Check for EventSystem
            if (FindObjectOfType<EventSystem>() == null)
            {
                Debug.LogError("EmailHoverPopup: No EventSystem found in scene! Hover events will not work!");
            }
        }

        private void Start()
        {
            Debug.Log($"EmailHoverPopup: Script active on {gameObject.name}. Waiting for hover events...");
        }

        private void Update()
        {
            // Continuously log mouse position while hovering
            if (isHovering && logMousePosition && Time.time - lastLogTime >= logInterval)
            {
                LogMouseCoordinates();
                lastLogTime = Time.time;
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            Debug.Log("=== MOUSE ENTERED EMAIL ===");
            isHovering = true;
            
            if (hoverPopup != null)
            {
                // Show the popup
                hoverPopup.SetActive(true);
                Debug.Log("EmailHoverPopup: Popup shown.");
                
                // Position the popup at the specified location
                if (popupRectTransform != null)
                {
                    popupRectTransform.anchoredPosition = popupPosition;
                }
            }

            if (logMousePosition)
            {
                LogMouseCoordinates();
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            Debug.Log("=== MOUSE EXITED EMAIL ===");
            isHovering = false;
            
            if (hoverPopup != null)
            {
                // Hide the popup
                hoverPopup.SetActive(false);
                Debug.Log("EmailHoverPopup: Popup hidden.");
            }
        }

        private void LogMouseCoordinates()
        {
            Vector2 screenPos = Vector2.zero;
#if ENABLE_INPUT_SYSTEM
            if (Mouse.current != null)
            {
                screenPos = Mouse.current.position.ReadValue();
            }
            else
            {
                // Fallback if Input System is enabled but no mouse device present
                screenPos = Vector2.zero;
            }
#else
            screenPos = Input.mousePosition;
#endif
            
            // Convert screen position to canvas local position
            Vector2 canvasLocalPos = Vector2.zero;
            if (canvasRectTransform != null)
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvasRectTransform,
                    screenPos,
                    canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
                    out canvasLocalPos
                );
            }

            // Convert screen position to email local position
            Vector2 emailLocalPos = Vector2.zero;
            if (emailRectTransform != null)
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    emailRectTransform,
                    screenPos,
                    canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
                    out emailLocalPos
                );
            }

            Debug.Log($"Mouse Position - Screen: {screenPos} | Canvas Local: {canvasLocalPos} | Email Local: {emailLocalPos}");
        }

        /// <summary>
        /// Update the popup position at runtime (useful for testing different positions)
        /// </summary>
        public void SetPopupPosition(Vector2 newPosition)
        {
            popupPosition = newPosition;
            if (hoverPopup != null && hoverPopup.activeSelf && popupRectTransform != null)
            {
                popupRectTransform.anchoredPosition = popupPosition;
            }
        }
    }
}
