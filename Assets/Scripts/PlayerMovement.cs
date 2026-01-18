using UnityEngine;
using Alteruna;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Alteruna.Avatar))]
public class PlayerMovement : MonoBehaviour
{
    private Rigidbody rb;
    private Alteruna.Avatar avatar;

    [Header("Movement")]
    public float speed = 6f;
    public float jumpForce = 5f;

    private bool isGrounded;
    private Vector2 moveInput;

    void Start()
    {
        avatar = GetComponent<Alteruna.Avatar>();
        rb = GetComponent<Rigidbody>();

        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        if (!avatar.IsMe)
            enabled = false;
    }

    // Called by InputManager
    public void SetMoveInput(Vector2 input)
    {
        moveInput = input;
    }

    void FixedUpdate()
    {
        if (!avatar.IsMe) return;

        // Movement
        Vector3 move = (transform.right * moveInput.x + transform.forward * moveInput.y).normalized;
        rb.MovePosition(rb.position + move * speed * Time.fixedDeltaTime);
    }

    public void Jump()
    {
        if (!avatar.IsMe) return;
        if (!isGrounded) return;

        // reset vertical velocity for consistent jump
        Vector3 vel = rb.linearVelocity;
        vel.y = 0f;
        rb.linearVelocity = vel;

        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    void OnCollisionStay(Collision collision)
    {
        isGrounded = true;
    }

    void OnCollisionExit(Collision collision)
    {
        isGrounded = false;
    }
}
