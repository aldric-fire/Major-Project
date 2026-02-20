using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class InteractionPrompt : MonoBehaviour
{
    [Header("References")]
    public Transform interactorSource;
    public float interactRange = 2.5f;
    
    [Header("World Space UI")]
    public Canvas worldSpaceCanvas;
    public Image promptImage;
    public float hoverHeight = 0.5f; // How high above the object to show the prompt
    
    private Sprite eKeySprite;
    private GameObject currentTarget;
    private TerminalGUIManager currentTerminalManager;
    private PhishingGameTrigger currentPhishingTrigger;

    void Start()
    {
        Debug.Log("[InteractionPrompt] Starting...");
        
        // Auto-find camera if not assigned - look for camera as child of this player
        if (interactorSource == null)
        {
            // First try to find camera in children (for multiplayer)
            Camera childCam = GetComponentInChildren<Camera>();
            if (childCam != null)
            {
                interactorSource = childCam.transform;
                Debug.Log("[InteractionPrompt] Found camera in children");
            }
            else
            {
                // Fallback to Camera.main
                interactorSource = Camera.main?.transform;
                if (interactorSource != null)
                {
                    Debug.Log("[InteractionPrompt] Auto-found Main Camera");
                }
                else
                {
                    Debug.LogError("[InteractionPrompt] Could not find Main Camera! Make sure camera has MainCamera tag.");
                    return;
                }
            }
        }
        
        // Find world space canvas and prompt image in scene if not assigned
        if (worldSpaceCanvas == null)
        {
            // Find all canvases including inactive ones
            Canvas[] allCanvases = FindObjectsOfType<Canvas>(true);
            foreach (Canvas canvas in allCanvases)
            {
                if (canvas.name == "InteractionPromptCanvas" && canvas.renderMode == RenderMode.WorldSpace)
                {
                    worldSpaceCanvas = canvas;
                    promptImage = worldSpaceCanvas.GetComponentInChildren<Image>(true);
                    Debug.Log("[InteractionPrompt] Found InteractionPromptCanvas!");
                    break;
                }
            }
            
            if (worldSpaceCanvas == null)
            {
                Debug.LogError("[InteractionPrompt] Could not find InteractionPromptCanvas in scene! Make sure it exists and is named correctly.");
            }
        }
        
        // Load the E key icon from Resources
        eKeySprite = Resources.Load<Sprite>("GUI/Icons/letter-e");
        
        if (eKeySprite == null)
        {
            Debug.LogError("[InteractionPrompt] Failed to load letter-e.png from Resources/GUI/Icons/");
        }
        
        if (promptImage != null && eKeySprite != null)
        {
            promptImage.sprite = eKeySprite;
            Debug.Log("[InteractionPrompt] E key sprite loaded and assigned");
        }
        
        if (worldSpaceCanvas != null)
        {
            worldSpaceCanvas.gameObject.SetActive(false);
            Debug.Log("[InteractionPrompt] World Space Canvas hidden at start");
        }
    }

    void Update()
    {
        if (worldSpaceCanvas == null)
        {
            Debug.LogError("[InteractionPrompt] worldSpaceCanvas is NULL in Update!");
            return;
        }
        
        if (interactorSource == null)
        {
            Debug.LogError("[InteractionPrompt] interactorSource is NULL in Update!");
            return;
        }

        // Check if player is looking at an interactable object
        Ray r = new Ray(interactorSource.position, interactorSource.forward);
        
        // Debug draw the ray
        Debug.DrawRay(interactorSource.position, interactorSource.forward * interactRange, Color.red);
        
        if (Time.frameCount % 120 == 0) // Log every 2 seconds
        {
            Debug.Log($"[InteractionPrompt] Update running. Camera pos: {interactorSource.position}, forward: {interactorSource.forward}, range: {interactRange}");
            
            // Test raycast in all directions to see if ANY collider is detected
            RaycastHit[] hits = Physics.RaycastAll(r, interactRange);
            Debug.Log($"[InteractionPrompt] RaycastAll found {hits.Length} objects");
            foreach (var hit in hits)
            {
                Debug.Log($"[InteractionPrompt]   - Hit: {hit.collider.gameObject.name} at distance {hit.distance}");
            }
        }
        
        if (Physics.Raycast(r, out RaycastHit hitInfo, interactRange))
        {
            Debug.Log($"[InteractionPrompt] Raycast hit: {hitInfo.collider.gameObject.name} at distance {hitInfo.distance}");
            
            // Check for PhishingGameTrigger first (new system)
            if (hitInfo.collider.gameObject.TryGetComponent(out PhishingGameTrigger phishingTrigger))
            {
                Debug.Log("[InteractionPrompt] Found PhishingGameTrigger! Showing E prompt");
                
                // Show the prompt above the laptop
                currentTarget = hitInfo.collider.gameObject;
                currentPhishingTrigger = phishingTrigger;
                worldSpaceCanvas.gameObject.SetActive(true);
                
                // Position the canvas at the hit point, slightly above
                Vector3 targetPosition = hitInfo.point;
                targetPosition.y += hoverHeight;
                worldSpaceCanvas.transform.position = targetPosition;
                
                // Make canvas face the camera (billboard effect)
                Vector3 directionToCamera = interactorSource.position - worldSpaceCanvas.transform.position;
                worldSpaceCanvas.transform.rotation = Quaternion.LookRotation(-directionToCamera);
                
                // Make sure canvas scale is appropriate
                worldSpaceCanvas.transform.localScale = Vector3.one * 0.01f;
                
                Debug.Log($"[InteractionPrompt] Showing E prompt at {targetPosition}, canvas active: {worldSpaceCanvas.gameObject.activeSelf}, image: {promptImage?.gameObject.activeSelf}");
                
                // Check for E key press
                if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
                {
                    Debug.Log("[InteractionPrompt] E key pressed! Starting phishing game...");
                    currentPhishingTrigger.StartPhishingGame();
                    // Hide prompt after interaction
                    worldSpaceCanvas.gameObject.SetActive(false);
                }
                
                return;
            }
            // Fallback to old TerminalGUIManager system (if still used elsewhere)
            else if (hitInfo.collider.gameObject.TryGetComponent(out TerminalGUIManager terminalManager))
            {
                Debug.Log("[InteractionPrompt] Found TerminalGUIManager! Showing E prompt");
                
                // Show the prompt above the laptop
                currentTarget = hitInfo.collider.gameObject;
                currentTerminalManager = terminalManager;
                worldSpaceCanvas.gameObject.SetActive(true);
                
                // Position the canvas at the hit point, slightly above
                Vector3 targetPosition = hitInfo.point;
                targetPosition.y += hoverHeight;
                worldSpaceCanvas.transform.position = targetPosition;
                
                // Make canvas face the camera (billboard effect)
                Vector3 directionToCamera = interactorSource.position - worldSpaceCanvas.transform.position;
                worldSpaceCanvas.transform.rotation = Quaternion.LookRotation(-directionToCamera);
                
                // Make sure canvas scale is appropriate
                worldSpaceCanvas.transform.localScale = Vector3.one * 0.01f;
                
                Debug.Log($"[InteractionPrompt] Showing E prompt at {targetPosition}, canvas active: {worldSpaceCanvas.gameObject.activeSelf}, image: {promptImage?.gameObject.activeSelf}");
                
                // Check for E key press
                if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
                {
                    Debug.Log("[InteractionPrompt] E key pressed! Opening terminal...");
                    currentTerminalManager.OpenTerminal();
                }
                
                return;
            }
        }
        
        // Hide the prompt if not looking at anything interactable
        worldSpaceCanvas.gameObject.SetActive(false);
        currentTarget = null;
        currentTerminalManager = null;
        currentPhishingTrigger = null;
    }
}
