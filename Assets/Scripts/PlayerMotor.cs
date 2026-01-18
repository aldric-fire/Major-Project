using UnityEngine;
using Alteruna;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Alteruna.Avatar))]
public class PlayerMotor : MonoBehaviour
{
    private CharacterController controller;
    private Vector3 playerVelocity;
    private bool isGrounded;
    private Alteruna.Avatar _avatar;

    [Header("Movement Settings")]
    public float speed = 5f;
    public float gravity = -9.8f;
    public float jumpHeight = 3f;

    void Start()
    {
        _avatar = GetComponent<Alteruna.Avatar>();

        if (!_avatar.IsMe)
        {
            enabled = false;
            return;
        }

        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        // Skip remote players
        if (!_avatar.IsMe)
            return;

        isGrounded = controller.isGrounded;

        // Apply gravity
        playerVelocity.y += gravity * Time.deltaTime;
        if (isGrounded && playerVelocity.y < 0)
            playerVelocity.y = -2f;

        controller.Move(playerVelocity * Time.deltaTime);
    }

    // Called from InputManager
    public void ProcessMove(Vector2 input)
    {
        if (!_avatar.IsMe)
            return;

        Vector3 move = new Vector3(input.x, 0, input.y);
        move = transform.TransformDirection(move) * speed * Time.deltaTime;
        controller.Move(move);
    }

    public void Jump()
    {
        if (!_avatar.IsMe)
            return;

        if (isGrounded)
        {
            playerVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }
}
