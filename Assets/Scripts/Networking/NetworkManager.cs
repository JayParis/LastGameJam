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
    public static string myPlayerName = "Player 1";

    int gameState = 0;



    public static bool isTeam_1 = true;
    public static int seed = 0;
    public static int mapID = 0;
    public static int colourID = 0;
    public static Color team_1_Colour;
    public static Color team_2_Colour;
    public static int team_1_ScoreStatic = 0;
    public static int team_2_ScoreStatic = 0;

    public List<string> playerNames;

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
    bool hasSetupFirstThrowable = false;

    public TextMeshProUGUI playerNameTMP;

    //Table

    public Transform tableOut;
    public Transform tableMid;
    public Transform tableIn;
    public Transform tiltPivot;

    public Vector3 hotspot;
    public Vector3 targetHotspot;
    public float tiltPower = 0f;
    public float targetTiltPower = 0f;

    //Score Bar
    public RawImage scoreBar_Top;
    public RawImage scoreBar_Mid;
    public RawImage scoreBar_Bottom;

    public Transform barTilt_L;
    public Transform barTilt_R;

    float testBarVal = 0f;

    float currentBarVal = 0.5f;
    public Transform homeAwayPivot;


    public bool gameStarted = false;
    public float gameTimer = 20f;//120f
    public TextMeshProUGUI timerTPM;

    public bool hasEndedGame = false;
    bool team_1_Wins = false;

    public GameObject backOutButton;
    public GameObject nameNotch;

    public Transform spectatePos;

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
        RPC.SyncTime(gameTimer);

        

        if (isHost) {
            HC.enabled = true;

            TTS_BG.gameObject.SetActive(false);
            TTC_TMP.gameObject.SetActive(false);

            gameState = 10;

            gameStarted = true;

        } else {
            //PC.enabled = true;
            myPlayerID = PhotonNetwork.LocalPlayer.ActorNumber;

            SetSpawn(myPlayerID - 1);

            gameState = 2;


            seed = (int)PhotonNetwork.CurrentRoom.CustomProperties["Seed"];
            mapID = (int)PhotonNetwork.CurrentRoom.CustomProperties["MapID"];
            colourID = (int)PhotonNetwork.CurrentRoom.CustomProperties["ColourID"];

            nameNotch.SetActive(true);

            myPlayerName = GetPlayerName(myPlayerID - 1);
            playerNameTMP.text = myPlayerName;


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

            outputString.text += "Player connected" + '\n';


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
        backOutButton.SetActive(gameState == 1);


        if (gameState == 0 || gameState == 1) {
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
            TTS_BG.transform.localRotation = Quaternion.Lerp(TTS_BG.transform.rotation, TTC_TargetRot.localRotation, Time.deltaTime * 4.6f);//16f
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
                RenderSettings.fogDensity = Mathf.Lerp(RenderSettings.fogDensity, 0f, Time.deltaTime * 2.5f);

                if(playTransitionTime > 1f && !hasSetupFirstThrowable) {
                    PC.ResetThrowable();
                    hasSetupFirstThrowable = true;
                }

                if(playTransitionTime > 2.33f && !hasSetupPC) {
                    PC.enabled = true;
                    RenderSettings.fogDensity = 0f;
                    ScaleScoreBar();
                    gameStarted = true;
                    hasSetupPC = true;
                }
            }

            //Bar UI
        }

        Vector3 finalBarPos = new Vector3(Screen.width * (1 - currentBarVal), scoreBar_Mid.transform.position.y, scoreBar_Mid.transform.position.z);
        homeAwayPivot.position = finalBarPos;

        if ((team_1_ScoreStatic + team_2_ScoreStatic) > 5 && scoreBar_Top.gameObject.activeSelf) {
            float biggest = 0;
            float smallest = 0;
            bool smallestIsTeam_1 = false;
            if (team_1_ScoreStatic > team_2_ScoreStatic) {
                biggest = team_1_ScoreStatic;
                smallest = team_2_ScoreStatic;
                smallestIsTeam_1 = false;
            } else if (team_2_ScoreStatic > team_1_ScoreStatic) {
                biggest = team_2_ScoreStatic;
                smallest = team_1_ScoreStatic;
                smallestIsTeam_1 = true;
            } else if(team_2_ScoreStatic == team_1_ScoreStatic) {
                biggest = team_1_ScoreStatic;
                smallest = team_2_ScoreStatic;
                smallestIsTeam_1 = false;
            }

            float tilt_t = 0.95f; //1f = full team 1 win

            if(smallestIsTeam_1)
                tilt_t =  smallest / biggest;
            else
                tilt_t = 1 - (smallest / biggest);


            currentBarVal = Mathf.Lerp(currentBarVal, tilt_t, Time.deltaTime * 4f);
            //Debug.Log(testBarVal);
            //float tilt_t = Mathf.Clamp01(testBarVal);

            //float tilt_t = (Mathf.Sin(Time.time * 0.5f) + 1) / 2f       ;

            Quaternion tilt_q_1 = Quaternion.Lerp(barTilt_L.rotation, barTilt_R.rotation, currentBarVal);
            Quaternion tilt_q_2 = Quaternion.Lerp(barTilt_L.rotation, barTilt_R.rotation, 1- currentBarVal);

            scoreBar_Top.transform.rotation = Quaternion.Lerp(scoreBar_Top.transform.rotation, tilt_q_1, Time.deltaTime * 15f);
            scoreBar_Bottom.transform.rotation = Quaternion.Lerp(scoreBar_Bottom.transform.rotation, tilt_q_2, Time.deltaTime * 15f);

            scoreBar_Top.transform.GetChild(0).rotation = TTC_CentrePos.rotation;
            scoreBar_Mid.transform.GetChild(0).rotation = TTC_CentrePos.rotation;
            scoreBar_Bottom.transform.GetChild(0).rotation = TTC_CentrePos.rotation;

            scoreBar_Top.transform.GetChild(0).position = finalBarPos;
            scoreBar_Mid.transform.GetChild(0).position = finalBarPos;
            scoreBar_Bottom.transform.GetChild(0).position = finalBarPos;


            
        }

        hotspot = Vector3.Lerp(hotspot, targetHotspot, Time.deltaTime * 2f);
        tiltPower = Mathf.Lerp(tiltPower, targetTiltPower, Time.deltaTime * 2f);

        tiltPivot.eulerAngles = new Vector3(hotspot.z, 0, hotspot.x * -1) * tiltPower;

        if (gameStarted) {
            timerTPM.enabled = true;
            if(!hasEndedGame)
                gameTimer -= Time.deltaTime;  //time is a float
            int seconds = ((int)gameTimer % 60);
            int minutes = ((int)gameTimer / 60);
            timerTPM.text = string.Format("{0:00}:{1:00}", minutes, seconds);

            if(gameTimer <= 0 && !hasEndedGame && isHost) {
                RPC.GameFinished();


                hasEndedGame = true;
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
            //Vector3 pos = TTC_TMP.transform.TransformPoint(TTC_TMP.mesh.vertices[0]);
            //GameObject.Find("DBPOS").transform.position = pos;

            
        }
        if (Input.GetKey(KeyCode.D)) {
            
        }
        if (Input.GetKey(KeyCode.W)) {
            GameObject.Find("DBPiv").transform.GetChild(0).Translate(Vector3.forward * Time.deltaTime * 10f);
        }
        if (Input.GetKey(KeyCode.S)) {
            GameObject.Find("DBPiv").transform.GetChild(0).Translate(Vector3.back * Time.deltaTime * 10f);
        }


        if (hasEndedGame) {
            Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, spectatePos.position, Time.deltaTime * 5f);
            Camera.main.transform.rotation = Quaternion.Lerp(Camera.main.transform.rotation, spectatePos.rotation, Time.deltaTime * 5f);
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
        if (gameState == 0) {
            ConnectToGame();
            gameState = 1;
        }
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

    public void ScaleScoreBar() {
        scoreBar_Top.gameObject.SetActive(true);
        scoreBar_Mid.gameObject.SetActive(true);
        scoreBar_Bottom.gameObject.SetActive(true);
        homeAwayPivot.gameObject.SetActive(true);

        float barDist = barTilt_L.transform.position.x - barTilt_R.transform.position.x;
        float screenDist = TL.position.x - BR.position.x;

        float mod = screenDist / barDist;
        scoreBar_Top.transform.parent.localScale = Vector3.one * mod;

        scoreBar_Top.transform.parent.localPosition = new Vector3(0, Screen.height * -0.113f, 0);

        scoreBar_Top.transform.GetChild(0).GetComponent<RawImage>().color = team_1_Colour;
        scoreBar_Mid.transform.GetChild(0).GetComponent<RawImage>().color = team_1_Colour;
        scoreBar_Bottom.transform.GetChild(0).GetComponent<RawImage>().color = team_1_Colour;

        scoreBar_Top.color = team_2_Colour;
        scoreBar_Mid.color = team_2_Colour;
        scoreBar_Bottom.color = team_2_Colour;

        HC.team_1_ScoreTMP_BG.color = team_1_Colour;
        HC.team_2_ScoreTMP_BG.color = team_2_Colour;
    }

    public void UpdateTilt(Vector3 targetCoG, float targetIntensity) {
        //GameObject dbpiv = GameObject.Find("DBPiv");
        //dbpiv.transform.Rotate(Vector3.forward * 150 * Time.deltaTime);

        targetHotspot = targetCoG;
        targetTiltPower = targetIntensity;
        Debug.Log("Tilt_" + targetIntensity);
    }

    public void GameFinished() {
        team_1_Wins = team_1_ScoreStatic >= team_2_ScoreStatic;
        hasEndedGame = true;

        if (isHost) {

        } else {
            PC.enabled = false;
        }
    }

    public string GetPlayerName(int PlayerID) {
        string resultName = "";
        resultName = playerNames[(seed + PlayerID) % 32];
        return resultName;
    }

    public void BackOutButton() {
        if(PhotonNetwork.IsConnected)
            PhotonNetwork.Disconnect();
        gameState = 0;
    }
}
