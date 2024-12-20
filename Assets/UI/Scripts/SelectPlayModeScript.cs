using System.Collections.Generic;
using DataTransfer;
using NetworkThread;
using NetworkThread.Multiplayer;
using NetworkThread.Multiplayer.Packets;
using RoomEnum;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SelectPlayModeScript : MonoBehaviour
{
    public RoomData roomData;
    public RoomsData roomsData;
    public Button ShowRoomsButton;
    public Button ReloadRoomListButton;
    
    [Header("Room List Panel")]
    public GameObject RoomListPanel;
    private bool getRoomList = true;
    public RectTransform position;
    public GameObject roomInfoPrefab;
    private List<GameObject> _roomInfos = new List<GameObject>();
    
    
    [Header("Filter")]
    public Button FindButton;
    public TMPro.TMP_Dropdown RoomModeDropdown;
    public TMPro.TMP_Dropdown RoomTypeDropdown;
    private void Awake()
    {
        NetworkStaticManager.ClientHandle.SetUiScripts(this);
        ShowRoomsButton.onClick.AddListener(ShowRoomList);
        ReloadRoomListButton.onClick.AddListener(ShowRoomList);
        RoomTypeDropdown.onValueChanged.AddListener(UpdateRoomList);
        RoomModeDropdown.onValueChanged.AddListener(UpdateRoomList);
    }
    private void ShowRoomList()
    {
        if (!getRoomList) return;
        foreach (var r in _roomInfos)
        {
            Destroy(r);
        }
        _roomInfos.Clear();
        NetworkStaticManager.ClientHandle.SendRoomListPacket();
        getRoomList = false;
    }

    public void ParseRoomList(RoomListPacket roomListPacket)
    {
        getRoomList = true;
        foreach (var r in roomListPacket.rooms)
        {
            var newButton = Instantiate(roomInfoPrefab, position);
            var buttonScript = newButton.GetComponent<RoomInfoPrefabScript>();
            buttonScript.InitializeRoom(
                roomIdInt: r.Id,
                roomNameText: r.Name,
                mode: r.roomMode,
                type: r.roomType,
                currentPlayers: r.PlayerNumber,
                rStatus: r.roomStatus);
            
            Button uiButton = newButton.GetComponent<Button>();
            uiButton.onClick.AddListener(buttonScript.SendJoinRoomPacket);
            newButton.SetActive(true);
            _roomInfos.Add(newButton);
        }
        
        UpdateRoomList(0);
        RoomListPanel.SetActive(true);
    }

    private void UpdateRoomList(int value)
    {
        int tmp;
        if (RoomTypeDropdown.value == 0)
        {
            tmp = 0;
        } else if (RoomTypeDropdown.value == 1)
        {
            tmp = 2;
        } else if (RoomTypeDropdown.value == 2)
        {
            tmp = 4;
        }
        else
        {
            tmp = 8;
        }
        foreach (var r in _roomInfos)
        {
            r.SetActive(false);
            var rs = r.GetComponent<RoomInfoPrefabScript>();
            if (RoomModeDropdown.value == 0 && RoomTypeDropdown.value == 0)
            {
                r.SetActive(true);
            }
            else if (RoomModeDropdown.value != 0 && RoomTypeDropdown.value != 0)
            {
                r.SetActive(rs.GetRoomMode() == (RoomMode)RoomModeDropdown.value
                            && (int)rs.GetRoomType() == tmp);
            }
            else if (RoomModeDropdown.value == 0)
            {
                r.SetActive((int)rs.GetRoomType() == tmp);
            }
            else
            {
                r.SetActive(rs.GetRoomMode() == (RoomMode)RoomModeDropdown.value);
            }
        }
    }

    public void BackToMainMenu()
    {
        SceneManager.LoadScene("Main Menu");
    }

    public void NormalModeSelected()
    {
        NetworkStaticManager.ClientHandle.SendJoinRoomPacket(RoomMode.Normal, RoomType.TwoVsTwo);
    }

    public void ParseRoomInfoData(RoomPacket roomPacket)
    {
        roomData.RoomPacket = roomPacket;
    }

    public void ParsePlayerInRoomData(JoinRoomPacketToAll packet)
    {
        roomData.PlayersInRoom = packet.Players;
        SceneManager.LoadScene("Waiting Room");
    }
}
