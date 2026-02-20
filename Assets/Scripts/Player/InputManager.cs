using UnityEngine;
using UnityEngine.InputSystem;
using Alteruna;

[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerLook))]
[RequireComponent(typeof(Alteruna.Avatar))]
public class InputManager : MonoBehaviour
{
    private PlayerInput input;
    private PlayerInput.OnFootActions onFoot;

    private PlayerMovement movement;
    private PlayerLook look;
    private Alteruna.Avatar avatar;

    void Awake()
    {
        movement = GetComponent<PlayerMovement>();
        look = GetComponent<PlayerLook>();
        avatar = GetComponent<Alteruna.Avatar>();

        input = new PlayerInput();
        onFoot = input.OnFoot;

        onFoot.Jump.performed += _ =>
        {
            if (avatar.IsMe)
                movement.Jump();
        };

        onFoot.ShiftLock.performed += _ =>
        {
            if (!avatar.IsMe) return;

            look.shiftLocked = !look.shiftLocked;
            if (look.shiftLocked) look.LockCursor();
            else look.UnlockCursor();
        };
    }

    void Update()
    {
        if (!avatar.IsMe) return;

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
