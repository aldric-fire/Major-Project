using UnityEngine;
using Alteruna;

[RequireComponent(typeof(Alteruna.Avatar))]
public class RemoteSmoothing : MonoBehaviour
{
    public float positionSmoothTime = 0.1f;
    public float rotationSmoothTime = 0.1f;

    private Alteruna.Avatar avatar;
    private Vector3 positionVelocity;
    private Vector3 rotationVelocity; // used for SmoothDampAngle

    private Vector3 lastNetworkPosition;
    private Vector3 lastNetworkEuler;

    void Start()
    {
        avatar = GetComponent<Alteruna.Avatar>();

        if (avatar.IsMe)
        {
            enabled = false;
            return;
        }

        lastNetworkPosition = transform.position;
        lastNetworkEuler = transform.eulerAngles;
    }

    void LateUpdate()
    {
        if (avatar.IsMe) return;

        // Target is the networked position (already applied by TransformSynchronizable)
        Vector3 networkPos = transform.position;
        Vector3 networkEuler = transform.eulerAngles;

        // Smooth position
        transform.position = Vector3.SmoothDamp(
            transform.position,
            networkPos,
            ref positionVelocity,
            positionSmoothTime
        );

        // Smooth rotation
        Vector3 smoothedEuler = new Vector3(
            Mathf.SmoothDampAngle(transform.eulerAngles.x, networkEuler.x, ref rotationVelocity.x, rotationSmoothTime),
            Mathf.SmoothDampAngle(transform.eulerAngles.y, networkEuler.y, ref rotationVelocity.y, rotationSmoothTime),
            Mathf.SmoothDampAngle(transform.eulerAngles.z, networkEuler.z, ref rotationVelocity.z, rotationSmoothTime)
        );

        transform.rotation = Quaternion.Euler(smoothedEuler);
    }
}
