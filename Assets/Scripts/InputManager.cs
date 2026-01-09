using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    private PlayerInput playerInput;
    private PlayerInput.OnFootActions onFoot;

    private Vector2 movementInput;

    private PlayerMotor motor;
    private PlayerLook look;

    void Awake()
    {
        playerInput = new PlayerInput();
        onFoot = playerInput.OnFoot;

        motor = GetComponent<PlayerMotor>();
        look = GetComponent<PlayerLook>();

        // Subscribe to jump input
        onFoot.Jump.performed += ctx => motor.Jump();
    }

    // Update is called once per frame
    void Update()
    {
        // Movement (CharacterController → Update)
        movementInput = onFoot.Movement.ReadValue<Vector2>();
        motor.ProcessMove(movementInput);
    }

    void LateUpdate()
    {
        // Camera look
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
