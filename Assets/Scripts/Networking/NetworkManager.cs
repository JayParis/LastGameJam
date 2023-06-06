using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using TMPro;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    public bool forceMobile = false;
    public static bool isHost = true;
    public TextMeshProUGUI outputString;

    public List<Transform> spawns;

    public PlayerController PC;

    void Start()
    {
        if (forceMobile)
            isHost = false;
        else
            isHost = SystemInfo.deviceType == DeviceType.Desktop;
        
        outputString.text += "isHost: " + isHost.ToString() + '\n';
        ConnectToGame();
    }

    void ConnectToGame() {
        outputString.text += "connecting to Server..." + '\n';
        PhotonNetwork.GameVersion = "0.0.1";
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster() {
        //Debug.Log("Connected to Server");
        outputString.text += "connected to server" + '\n';
        if (PhotonNetwork.IsConnected) {
            RoomOptions options = new RoomOptions();
            options.MaxPlayers = 20;
            PhotonNetwork.JoinOrCreateRoom("MainRoom", options, TypedLobby.Default);
        }
    }

    public override void OnDisconnected(DisconnectCause cause) {
        //Debug.Log("Disconnected from Server for reason " + cause.ToString());
        outputString.text += "disconnected from server for reason " + cause.ToString() + '\n';
    }

    public override void OnCreatedRoom() {
        outputString.text += "room created " + this.ToString() + '\n';
    }
    public override void OnJoinedRoom() {
        outputString.text += "room joined " + this.ToString() + '\n';

        if (isHost) {

        } else {
            PC.enabled = true;
            outputString.text += "owning player: " + PhotonNetwork.LocalPlayer.ActorNumber.ToString() + this.ToString() + '\n';

        }
    }

    public override void OnCreateRoomFailed(short returnCode, string message) {
        outputString.text += "room failed " + message.ToString() + '\n';
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E)) {
            outputString.text += "clients: " + PhotonNetwork.CurrentRoom.PlayerCount.ToString() + '\n';
        }
        if (Input.GetKeyDown(KeyCode.R)) {
            ConnectToGame();
        }
    }
}
