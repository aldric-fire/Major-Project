using UnityEngine;
using Alteruna;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(TransformSynchronizable))]
public class NetworkedProp : MonoBehaviour
{
    private Rigidbody rb;

    // Which player currently owns physics
    private Alteruna.Avatar owner;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Start()
    {
        // Nobody owns it at start → physics OFF everywhere
        rb.isKinematic = true;
    }

    void OnCollisionEnter(Collision collision)
    {
        Alteruna.Avatar avatar =
            collision.gameObject.GetComponentInParent<Alteruna.Avatar>();

        if (avatar == null)
            return;

        // Give ownership to the player who touched it
        owner = avatar;

        // Only the owner simulates physics
        rb.isKinematic = !owner.IsMe;
    }

    void FixedUpdate()
    {
        if (owner == null)
            return;

        // Ensure only owner runs physics
        rb.isKinematic = !owner.IsMe;
    }
}
