using UnityEngine;
using UnityEngine.InputSystem;
using Alteruna;

[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerLook))]
[RequireComponent(typeof(InteractionController))]
[RequireComponent(typeof(Alteruna.Avatar))]
public class InputManager : MonoBehaviour
{
    private PlayerInput input;
    private PlayerInput.OnFootActions onFoot;

    private PlayerMovement movement;
    private PlayerLook look;
    private InteractionController interaction;
    private Alteruna.Avatar avatar;

    void Awake()
    {
        movement = GetComponent<PlayerMovement>();
        look = GetComponent<PlayerLook>();
        interaction = GetComponent<InteractionController>();
        avatar = GetComponent<Alteruna.Avatar>();

        input = new PlayerInput();
        onFoot = input.OnFoot;

        onFoot.Jump.performed += _ =>
        {
            if (avatar.IsMe && !interaction.IsInChallengeUI)
                movement.Jump();
        };

        onFoot.ShiftLock.performed += _ =>
        {
            if (!avatar.IsMe || interaction.IsInChallengeUI) return;

            look.shiftLocked = !look.shiftLocked;
            if (look.shiftLocked) look.LockCursor();
            else look.UnlockCursor();
        };

        onFoot.Interact.performed += _ =>
        {
            if (avatar.IsMe)
                interaction.TryInteract();
        };
    }

    void Update()
    {
        if (!avatar.IsMe) return;

        // Suppress movement/look input when challenge UI is open
        if (interaction != null && interaction.IsInChallengeUI)
        {
            movement.SetMoveInput(Vector2.zero);
            return;
        }

        Vector2 moveInput = onFoot.Movement.ReadValue<Vector2>();
        movement.SetMoveInput(moveInput);

        Vector2 lookInput = onFoot.Look.ReadValue<Vector2>();
        look.Look(lookInput);
    }

    private void OnEnable()
    {
        onFoot.Enable();
    }

    private void OnDisable()
    {
        onFoot.Disable();
    }
}
