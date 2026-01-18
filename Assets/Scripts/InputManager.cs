using UnityEngine;
using UnityEngine.InputSystem;
using Alteruna;

[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerLook))]
[RequireComponent(typeof(Alteruna.Avatar))]
public class InputManager : MonoBehaviour
{
    private PlayerInput playerInput;
    private PlayerInput.OnFootActions onFoot;

    private PlayerMovement movement;
    private PlayerLook look;
    private Alteruna.Avatar avatar;

    void Awake()
    {
        movement = GetComponent<PlayerMovement>();
        look = GetComponent<PlayerLook>();
        avatar = GetComponent<Alteruna.Avatar>();

        playerInput = new PlayerInput();
        onFoot = playerInput.OnFoot;

        // Jump
        onFoot.Jump.performed += _ =>
        {
            if (avatar.IsMe)
                movement.Jump();
        };

        // Shift-lock toggle
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

        Vector2 input = onFoot.Movement.ReadValue<Vector2>();
        movement.SetMoveInput(input);
    }

    void LateUpdate()
    {
        if (!avatar.IsMe) return;

        look.ProcessLook(onFoot.Look.ReadValue<Vector2>());
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
