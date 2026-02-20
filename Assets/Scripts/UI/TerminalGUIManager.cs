using UnityEngine;
using UnityEngine.InputSystem;

public class TerminalGUIManager : MonoBehaviour
{
    [Header("UI Canvases")]
    public Canvas hoverCanvas; // InteractionPromptCanvas
    public Canvas terminalCanvas; // TerminalGUI

    [Header("Terminal Elements (Optional)")]
    public GameObject folder1;
    public GameObject folder2;
    public GameObject filePopup1;
    public GameObject fileClean;
    public GameObject fileInfected;
    public GameObject quarantineSlot;
    public GameObject feedbackAlert;

    private bool isTerminalOpen = false;

    void Start()
    {
        Debug.Log("[TerminalGUIManager] Starting on " + gameObject.name);
        
        // Ensure terminal is hidden at start
        if (terminalCanvas != null)
        {
            terminalCanvas.gameObject.SetActive(false);
            Debug.Log("[TerminalGUIManager] Terminal canvas hidden at start");
        }
        else
        {
            Debug.LogError("[TerminalGUIManager] Terminal Canvas is not assigned!");
        }
        
        if (hoverCanvas == null)
        {
            Debug.LogError("[TerminalGUIManager] Hover Canvas is not assigned!");
        }

        // Hide optional popup elements
        if (filePopup1 != null) filePopup1.SetActive(false);
        if (fileInfected != null) fileInfected.SetActive(false);
        if (feedbackAlert != null) feedbackAlert.SetActive(false);
    }

    void Update()
    {
        // Allow closing terminal with E or Escape when terminal is open
        if (isTerminalOpen && Keyboard.current != null && 
            (Keyboard.current.eKey.wasPressedThisFrame || Keyboard.current.escapeKey.wasPressedThisFrame))
        {
            CloseTerminal();
        }
    }

    public void OpenTerminal()
    {
        Debug.Log("[TerminalGUIManager] OpenTerminal() called");
        
        // Hide hover canvas (E prompt)
        if (hoverCanvas != null)
        {
            hoverCanvas.gameObject.SetActive(false);
            Debug.Log("[TerminalGUIManager] Hover canvas hidden");
        }

        // Show terminal GUI
        if (terminalCanvas != null)
        {
            terminalCanvas.gameObject.SetActive(true);
            isTerminalOpen = true;
            Debug.Log("[TerminalGUIManager] Terminal canvas opened");
        }

        // Unlock cursor for terminal interaction
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Debug.Log("[TerminalGUIManager] Cursor unlocked");
    }

    public void CloseTerminal()
    {
        Debug.Log("[TerminalGUIManager] CloseTerminal() called");
        
        // Hide terminal GUI
        if (terminalCanvas != null)
        {
            terminalCanvas.gameObject.SetActive(false);
            isTerminalOpen = false;
            Debug.Log("[TerminalGUIManager] Terminal canvas closed");
        }

        // Lock cursor back for FPS movement
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Debug.Log("[TerminalGUIManager] Cursor locked");

        // Note: Hover canvas will reappear automatically when player looks at laptop again
    }

    // Helper methods for showing/hiding popups
    public void ShowFilePopup(GameObject popup)
    {
        if (popup != null)
        {
            popup.SetActive(true);
        }
    }

    public void HideFilePopup(GameObject popup)
    {
        if (popup != null)
        {
            popup.SetActive(false);
        }
    }

    public void ShowFeedbackAlert(string message)
    {
        if (feedbackAlert != null)
        {
            feedbackAlert.SetActive(true);
            // You can add TextMeshPro component to show message
        }
    }

    public void HideFeedbackAlert()
    {
        if (feedbackAlert != null)
        {
            feedbackAlert.SetActive(false);
        }
    }
}
