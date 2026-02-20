using UnityEngine;
using Alteruna;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Alteruna.Avatar))]
public class PlayerMovement : MonoBehaviour
{
    private Rigidbody rb;
    private Alteruna.Avatar avatar;

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

        // Handle local vs remote
        if (avatar.IsMe)
        {
            rb.isKinematic = false; // local player moves via Rigidbody
        }
        else
        {
            rb.isKinematic = true;  // remote players follow network only
        }
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
        Vector3 targetPos = rb.position + move * speed * Time.fixedDeltaTime;
        rb.MovePosition(targetPos); // horizontal movemen
    }

    public void Jump()
    {
        if (!avatar.IsMe || !isGrounded) return;

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
