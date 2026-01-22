using UnityEngine;
using Alteruna;

public class PlayerLook : MonoBehaviour
{
    public Camera cam;

    public float xSensitivity = 30f;
    public float ySensitivity = 30f;

    public bool shiftLocked = true;

    private float xRotation = 0f;
    private Alteruna.Avatar avatar;

    void Start()
    {
        avatar = GetComponent<Alteruna.Avatar>();

        if (!avatar.IsMe)
        {
            if (cam != null) cam.gameObject.SetActive(false);
            enabled = false;
            return;
        }

        LockCursor();
    }

    public void Look(Vector2 input)
    {
        if (!shiftLocked || !avatar.IsMe) return;

        float mouseX = input.x * xSensitivity * Time.deltaTime;
        float mouseY = input.y * ySensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -80f, 80f);
        if (cam != null)
            cam.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    public void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
