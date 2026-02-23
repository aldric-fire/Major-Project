using UnityEngine;
using Alteruna;

/// <summary>
/// Handles raycasting from the player camera to detect IInteractable objects,
/// displays HUD prompts, and triggers interactions on E press.
/// Attach to the Player prefab alongside InputManager.
/// </summary>
[RequireComponent(typeof(Alteruna.Avatar))]
public class InteractionController : MonoBehaviour
{
    [Header("Raycast Settings")]
    [Tooltip("Maximum distance to detect interactable objects.")]
    public float interactRange = 3f;

    [Tooltip("Layer mask for interactable objects. Set to 'Default' or a custom 'Interactable' layer.")]
    public LayerMask interactLayerMask = ~0; // all layers by default

    private Camera playerCamera;
    private Alteruna.Avatar avatar;
    private IInteractable currentTarget;
    private HUDController hud;

    /// <summary>
    /// True when a challenge UI is open and player input should be suppressed.
    /// </summary>
    public bool IsInChallengeUI { get; private set; }

    void Start()
    {
        avatar = GetComponent<Alteruna.Avatar>();

        if (!avatar.IsMe)
        {
            enabled = false;
            return;
        }

        // Find camera on this player (PlayerLook references it)
        PlayerLook look = GetComponent<PlayerLook>();
        if (look != null && look.cam != null)
            playerCamera = look.cam;
        else
            playerCamera = GetComponentInChildren<Camera>();

        // Find HUD in scene (singleton)
        hud = FindFirstObjectByType<HUDController>();
    }

    void Update()
    {
        if (!avatar.IsMe || IsInChallengeUI) return;

        // Lazy-find HUD if not yet available (spawned later)
        if (hud == null)
            hud = FindFirstObjectByType<HUDController>();

        PerformRaycast();
    }

    private void PerformRaycast()
    {
        if (playerCamera == null) return;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, interactRange, interactLayerMask))
        {
            // Check the hit object and its parents for IInteractable
            IInteractable interactable = hit.collider.GetComponentInParent<IInteractable>();

            if (interactable != null && interactable.CanInteract())
            {
                if (currentTarget != interactable)
                {
                    currentTarget = interactable;
                    if (hud != null)
                        hud.ShowInteractPrompt(currentTarget.GetPromptText());
                }
                return;
            }
        }

        // Nothing interactable found — clear prompt
        if (currentTarget != null)
        {
            currentTarget = null;
            if (hud != null)
                hud.HideInteractPrompt();
        }
    }

    /// <summary>
    /// Called by InputManager when the Interact action is performed (E key).
    /// </summary>
    public void TryInteract()
    {
        if (!avatar.IsMe || IsInChallengeUI) return;

        if (currentTarget != null && currentTarget.CanInteract())
        {
            currentTarget.Interact(gameObject);
        }
    }

    /// <summary>
    /// Call this when a challenge UI opens to freeze player movement and unlock cursor.
    /// </summary>
    public void EnterChallengeUI()
    {
        IsInChallengeUI = true;

        // Disable movement and look
        PlayerMovement movement = GetComponent<PlayerMovement>();
        PlayerLook look = GetComponent<PlayerLook>();

        if (movement != null) movement.SetMoveInput(Vector2.zero);
        if (look != null) look.shiftLocked = false;

        // Unlock cursor for UI interaction
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Hide interaction prompt
        if (hud != null)
            hud.HideInteractPrompt();
    }

    /// <summary>
    /// Call this when a challenge UI closes to restore player control.
    /// </summary>
    public void ExitChallengeUI()
    {
        IsInChallengeUI = false;

        // Re-enable look
        PlayerLook look = GetComponent<PlayerLook>();
        if (look != null)
        {
            look.shiftLocked = true;
            look.LockCursor();
        }
    }
}
