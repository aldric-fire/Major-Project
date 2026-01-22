using UnityEngine;
using Alteruna;

public class UIManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject StartScreenPanel;
    public GameObject RoomMenuPanel;
    public GameObject RoomMenuBackground;

    private bool hasJoinedRoom = false;
    private Multiplayer multiplayer;

    void Start()
    {
        if (StartScreenPanel != null) StartScreenPanel.SetActive(true);
        if (RoomMenuPanel != null) RoomMenuPanel.SetActive(false);
        if (RoomMenuBackground != null) RoomMenuBackground.SetActive(false);

        multiplayer = FindObjectOfType<Multiplayer>();
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
    }

    public void LeaveRoomAndShowLobby()
    {
        if (RoomMenuPanel != null)
            RoomMenuPanel.SetActive(true);

        if (RoomMenuBackground != null)
            RoomMenuBackground.SetActive(true);

        if (StartScreenPanel != null)
            StartScreenPanel.SetActive(false);
    }
}
