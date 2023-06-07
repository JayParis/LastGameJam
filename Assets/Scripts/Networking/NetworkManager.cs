using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using TMPro;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    public bool forceMobile = false;
    public bool useTestServer = false;
    public static bool isHost = true;
    public TextMeshProUGUI outputString;

    public List<Transform> spawns;
    public List<Transform> thrownItems;


    public PlayerController PC;
    public HostController HC;
    public GameRPCs RPC;

    public static int myPlayerID = 0;

    int gameState = 0;



    public static bool isTeam_1 = true;
    public static int seed = 0;
    public static int mapID = 0;
    public static int colourID = 0;
    public static Color team_1_Colour;
    public static Color team_2_Colour;

    //Menus
    public Gradient menuGrad;
    float gradSamplePoint = 0f;

    public RawImage TTS_BG;
    public TextMeshProUGUI TTC_TMP;
    public Transform TTC_TargetRot;
    public Transform TTC_CentrePos;
    public Transform TTC_TeamPos;

    public Transform TL;
    public Transform BR;

    public List<Color> teamColours;
    public GameObject TTC_TeamButtons;

    float playTransitionTime = 0f;
    bool hasSetupPC = false;

    //Table

    public Transform tableOut;
    public Transform tableMid;
    public Transform tableIn;
    public Transform tiltPivot;

    //Score Bar
    public RawImage scoreBar;


    void Start()
    {
        if (forceMobile)
            isHost = false;
        else
            isHost = SystemInfo.deviceType == DeviceType.Desktop;
        
        outputString.text += "isHost: " + isHost.ToString() + '\n';
        //ConnectToGame();

        gradSamplePoint = Random.Range(0f,0.99f);
        TTS_BG.material.SetFloat("_Lightness", 1f);
        TTS_BG.material.SetFloat("_TeamSelect", 0f);
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

            int rndSeed = Random.Range(1,1870);
            int rndMap = Random.Range(0,3);
            int rndColourID = Random.Range(0,3);

            seed = rndSeed;
            mapID = rndMap;
            colourID = rndColourID;

            switch (colourID) {
                case 0:
                    team_1_Colour = teamColours[0];
                    team_2_Colour = teamColours[1];
                    break;
                case 1:
                    team_1_Colour = teamColours[2];
                    team_2_Colour = teamColours[3];
                    break;
                case 2:
                    team_1_Colour = teamColours[4];
                    team_2_Colour = teamColours[5];
                    break;
            }

            options.CustomRoomProperties = new Hashtable { { "Seed", rndSeed }, { "MapID", rndMap }, { "ColourID", rndColourID } };

            //PhotonNetwork.JoinOrCreateRoom("MainRoom", options, TypedLobby.Default);
            if (isHost) //|| forceMobile
                PhotonNetwork.JoinOrCreateRoom(useTestServer ? "TTS" : "MainRoom", options, TypedLobby.Default);
            else
                PhotonNetwork.JoinRoom(useTestServer ? "TTS" : "MainRoom");
            //PhotonNetwork.JoinRoom("MainRoom", options, TypedLobby.Default);
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
            HC.enabled = true;

            TTS_BG.gameObject.SetActive(false);
            TTC_TMP.gameObject.SetActive(false);

            gameState = 10;



        } else {
            //PC.enabled = true;
            myPlayerID = PhotonNetwork.LocalPlayer.ActorNumber;
            SetSpawn(myPlayerID - 1);

            gameState = 2;


            seed = (int)PhotonNetwork.CurrentRoom.CustomProperties["Seed"];
            mapID = (int)PhotonNetwork.CurrentRoom.CustomProperties["MapID"];
            colourID = (int)PhotonNetwork.CurrentRoom.CustomProperties["ColourID"];

            switch (colourID) {
                case 0:
                    team_1_Colour = teamColours[0];
                    team_2_Colour = teamColours[1];
                    break;
                case 1:
                    team_1_Colour = teamColours[2];
                    team_2_Colour = teamColours[3];
                    break;
                case 2:
                    team_1_Colour = teamColours[4];
                    team_2_Colour = teamColours[5];
                    break;
            }
            

            //outputString.text += "owning player: " + PhotonNetwork.LocalPlayer.ActorNumber.ToString() + this.ToString() + '\n';
        }

        outputString.text += "Seed param = " + PhotonNetwork.CurrentRoom.CustomProperties["Seed"].ToString() + '\n';
        outputString.text += "MapID param = " + PhotonNetwork.CurrentRoom.CustomProperties["MapID"].ToString() + '\n';
        outputString.text += "ColourID param = " + PhotonNetwork.CurrentRoom.CustomProperties["ColourID"].ToString() + '\n';
    }

    public override void OnCreateRoomFailed(short returnCode, string message) {
        outputString.text += "room failed " + message.ToString() + '\n';
    }

    void Update()
    {
        if(gameState == 0 || gameState == 1) {
            gradSamplePoint += Time.deltaTime * (gameState == 0 ? 0.06f : 0.35f);
            if (gradSamplePoint >= 1f)
                gradSamplePoint = 0f;
            TTS_BG.material.SetColor("_Colour_1", menuGrad.Evaluate(Mathf.Clamp(gradSamplePoint, 0f, 1f)));
        }

        if (gameState == 0) { //Tap to connect
            
            TTS_BG.material.SetFloat("_Tile", Mathf.Lerp(TTS_BG.material.GetFloat("_Tile"), 60f, Time.deltaTime * 10f));
            TTS_BG.material.SetFloat("_Rot", Mathf.Lerp(TTS_BG.material.GetFloat("_Rot"), 0.32f, Time.deltaTime * 2));
            TTS_BG.material.SetFloat("_Speed", Mathf.Lerp(TTS_BG.material.GetFloat("_Speed"), 0.46f, Time.deltaTime * 2));

            TTC_TMP.text = "Tap to\nConnect";
        } else if(gameState == 1) { //Connecting

            TTS_BG.material.SetFloat("_Tile", Mathf.Lerp(TTS_BG.material.GetFloat("_Tile"), 10f, Time.deltaTime * 4f));

            TTS_BG.material.SetFloat("_Rot", Mathf.Lerp(TTS_BG.material.GetFloat("_Rot"), 0.32f * 3f, Time.deltaTime * 2));
            TTS_BG.material.SetFloat("_Speed", Mathf.Lerp(TTS_BG.material.GetFloat("_Speed"), 0.46f * 2f, Time.deltaTime * 2));
            TTC_TMP.text = "Connecting";
        } else if(gameState == 2) { //Select team
            TTC_TeamButtons.SetActive(true);
            TTS_BG.transform.localRotation = Quaternion.Lerp(TTS_BG.transform.rotation, TTC_TargetRot.localRotation, Time.deltaTime * 16f);
            TTS_BG.transform.localScale = Vector3.Lerp(TTS_BG.transform.localScale, TTC_TargetRot.localScale, Time.deltaTime * 16f);
            //TTS_BG.material.SetFloat("_Lightness", Mathf.Lerp(TTS_BG.material.GetFloat("_Lightness"), 0.1f, Time.deltaTime * 7f));
            TTS_BG.material.SetFloat("_TeamSelect", Mathf.Lerp(TTS_BG.material.GetFloat("_TeamSelect"), 1f, Time.deltaTime * 7f));
            TTS_BG.material.SetColor("_Colour_1", menuGrad.Evaluate(Mathf.Clamp(0.32f, 0f, 1f)));
            TTS_BG.GetComponent<Button>().interactable = false;

            TTC_TMP.text = "Select a Team";

            TTC_TMP.transform.position = Vector3.Lerp(TTC_TMP.transform.position, TTC_TeamPos.position, Time.deltaTime * 15f);
            TTC_TeamPos.position = ((TL.position + BR.position) / 2f) + new Vector3(0, TL.position.y * 0.0633f, 0); //0.133f

            TTC_TMP.rectTransform.sizeDelta = new Vector2(0, TL.position.y * 0.083f); //0.073

            TTS_BG.material.SetColor("_Team_1", team_1_Colour);
            TTS_BG.material.SetColor("_Team_2", team_2_Colour);

            //TTC_TMP.transform.localScale = Vector3.Lerp(TTC_TMP.transform.localScale, TTC_TeamPos.localScale, Time.deltaTime * 7f);
        } else if (gameState == 3) { //Transition
            TTS_BG.transform.localScale = Vector3.Lerp(TTS_BG.transform.localScale, 
                new Vector3(TTC_TargetRot.localScale.x, TTC_TargetRot.localScale.y * 5f, TTC_TargetRot.localScale.z), Time.deltaTime * 36f);

            TTS_BG.transform.Translate(new Vector3(0, isTeam_1 ? -200f : 200f, 0) * Time.deltaTime * 188f);
            TTC_TMP.transform.Translate(new Vector3(isTeam_1 ? 200f : -200f, 0, 0) * Time.deltaTime * 188f * 0.6f);

            playTransitionTime += Time.deltaTime;

            if(playTransitionTime > 0.5f) {
                Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, 
                    PC.throwableStartPos.position + PC.transform.parent.TransformDirection(new Vector3(0, 3f, -3f)), Time.deltaTime * 3.6f);
                Camera.main.transform.rotation = Quaternion.Lerp(Camera.main.transform.rotation, PC.throwableCamPos.rotation, Time.deltaTime * 3.6f);
                RenderSettings.fogDensity = Mathf.Lerp(RenderSettings.fogDensity, 0f, Time.deltaTime * 5f);
                if(playTransitionTime > 3f && !hasSetupPC) {
                    PC.enabled = true;
                    RenderSettings.fogDensity = 0f;
                    hasSetupPC = true;
                }
            }
        }


        //Debug ----

        if (Input.GetKeyDown(KeyCode.E)) {
            outputString.text += "clients: " + PhotonNetwork.CurrentRoom.PlayerCount.ToString() + '\n';

        }
        if (Input.GetKeyDown(KeyCode.R)) {
            ConnectToGame();
        }
        if (Input.GetKeyDown(KeyCode.Escape) && gameState == 1) {
            gameState = 0;
        }
        if (Input.GetKeyDown(KeyCode.Q)) {
            gameState = 2;
            //Vector3 pos = TTC_TMP.transform.TransformPoint(TTC_TMP.mesh.vertices[0]);
            //GameObject.Find("DBPOS").transform.position = pos;
        }
        if (Input.GetKey(KeyCode.D)) {
            GameObject dbpiv = GameObject.Find("DBPiv");
            dbpiv.transform.Rotate(Vector3.forward * 150 * Time.deltaTime);
            Vector3 hotspot = dbpiv.transform.GetChild(0).position;

            float tintIntensity = 0.5f;
            tiltPivot.eulerAngles = new Vector3(hotspot.z, 0, hotspot.x * -1) * tintIntensity;
            Camera.main.fieldOfView = 110;
        }
        if (Input.GetKey(KeyCode.W)) {
            GameObject.Find("DBPiv").transform.GetChild(0).Translate(Vector3.forward * Time.deltaTime * 10f);
        }
        if (Input.GetKey(KeyCode.S)) {
            GameObject.Find("DBPiv").transform.GetChild(0).Translate(Vector3.back * Time.deltaTime * 10f);
        }
    }

        public void SyncTableRotations(Quaternion outRot, Quaternion midRot, Quaternion inRot) {
        tableOut.rotation = outRot;
        tableMid.rotation = midRot;
        tableIn.rotation = inRot;
        //GameObject.Find("Floor").GetComponent<Renderer>().material.color = Random.ColorHSV(0, 1, 1, 1, 1, 1);
    }

    public void SetSpawn(int playerID) {
        PC.transform.parent.eulerAngles = new Vector3(PC.transform.parent.eulerAngles.x, spawns[playerID].eulerAngles.y, PC.transform.parent.eulerAngles.z);
    }

    //UI ----

    public void UIConnectButton() {
        //ConnectToGame();
    }

    public void TTCButton() {
        gameState = 1;
        ConnectToGame();

    }

    public void Team1Button() {
        Debug.Log("JoinTeam_1");
        outputString.text += "Joined Team 1 " + '\n';

        TTC_TeamButtons.SetActive(false);
        gameState = 3;
        isTeam_1 = true;
    }
    public void Team2Button() {
        Debug.Log("JoinTeam_2");
        outputString.text += "Joined Team 2 " + '\n';
        TTC_TeamButtons.SetActive(false);
        gameState = 3;
        isTeam_1 = false;
    }
}
