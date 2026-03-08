using UnityEngine;
using Alteruna;

public class UIManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject StartScreenPanel;
    public GameObject RoomMenuPanel;
    public GameObject RoomMenuBackground;

    [Header("Challenge UI Panels (assign in Inspector)")]
    [Tooltip("Root panel for the Phishing challenge UI.")]
    public GameObject phishingUIPanel;

    [Tooltip("Root panel for the Password challenge UI.")]
    public GameObject passwordUIPanel;

    [Tooltip("Root panel for the Social Engineering dialogue UI.")]
    public GameObject dialogueUIPanel;

    [Tooltip("Root panel for the USB Drop choice UI.")]
    public GameObject choiceUIPanel;

    [Tooltip("Root panel for the Consequence overlay.")]
    public GameObject consequenceOverlayPanel;

    [Tooltip("Root panel for the Debrief panel.")]
    public GameObject debriefPanel;

    [Tooltip("Root panel for the Summary screen.")]
    public GameObject summaryPanel;

    private bool hasJoinedRoom = false;
    private Multiplayer multiplayer;

    void Start()
    {
        if (StartScreenPanel != null) StartScreenPanel.SetActive(true);
        if (RoomMenuPanel != null) RoomMenuPanel.SetActive(false);
        if (RoomMenuBackground != null) RoomMenuBackground.SetActive(false);

        // Ensure all gameplay UI starts hidden
        SetGameplayPanelsActive(false);

        multiplayer = FindFirstObjectByType<Multiplayer>();
        if (multiplayer == null)
        {
            Debug.LogError("UIManager: No Multiplayer object found in scene!");
        }
    }

    void Update()
    {
        if (!hasJoinedRoom && multiplayer != null && multiplayer.IsConnected && multiplayer.InRoom)
        {
            HandleRoomJoined();
            hasJoinedRoom = true;
        }
    }

    /// <summary>
    /// Called by Play Now button
    /// </summary>
    public void OnPlayNowClicked()
    {
        if (StartScreenPanel != null) StartScreenPanel.SetActive(false);
        if (RoomMenuPanel != null) RoomMenuPanel.SetActive(true);
        if (RoomMenuBackground != null) RoomMenuBackground.SetActive(true);
    }

    private void HandleRoomJoined()
    {
        if (RoomMenuPanel != null) RoomMenuPanel.SetActive(false);
        if (RoomMenuBackground != null) RoomMenuBackground.SetActive(false);

        // Show hearts now that the player is in the game
        HeartsSystem.Instance?.Show();
    }

    public void LeaveRoomAndShowLobby()
    {
        if (RoomMenuPanel != null)
            RoomMenuPanel.SetActive(true);

        if (RoomMenuBackground != null)
            RoomMenuBackground.SetActive(true);

        if (StartScreenPanel != null)
            StartScreenPanel.SetActive(false);

        // Hide gameplay panels when returning to lobby
        SetGameplayPanelsActive(false);
        HeartsSystem.Instance?.Hide();
        hasJoinedRoom = false;
    }

    /// <summary>
    /// Called when the player runs out of hearts. Hides all gameplay UI and returns to the Start Screen.
    /// </summary>
    public void GameOver()
    {
        SetGameplayPanelsActive(false);
        HeartsSystem.Instance?.Hide();

        if (RoomMenuPanel != null) RoomMenuPanel.SetActive(false);
        if (RoomMenuBackground != null) RoomMenuBackground.SetActive(false);
        if (StartScreenPanel != null) StartScreenPanel.SetActive(true);

        hasJoinedRoom = false;

        // Unlock cursor so the player can interact with the start screen
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    /// <summary>
    /// Hides or shows all gameplay-related UI panels.
    /// </summary>  
    private void SetGameplayPanelsActive(bool active)
    {
        if (phishingUIPanel != null) phishingUIPanel.SetActive(active);
        if (passwordUIPanel != null) passwordUIPanel.SetActive(active);
        if (dialogueUIPanel != null) dialogueUIPanel.SetActive(active);
        if (choiceUIPanel != null) choiceUIPanel.SetActive(active);
        if (consequenceOverlayPanel != null) consequenceOverlayPanel.SetActive(active);
        if (debriefPanel != null) debriefPanel.SetActive(active);
        if (summaryPanel != null) summaryPanel.SetActive(active);
    }
}
