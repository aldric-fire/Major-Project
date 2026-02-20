using UnityEngine;

public class TerminalUI : MonoBehaviour
{
    void Start()
    {
        // Ensure terminal is hidden when game starts
        gameObject.SetActive(false);
    }

    void Update()
    {
        // Allow closing terminal with E or Escape key
        if (gameObject.activeSelf && (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Escape)))
        {
            CloseTerminal();
        }
    }

    public void CloseTerminal()
    {
        gameObject.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
