using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;

public class GameRPCs : MonoBehaviour
{
    public NetworkManager NM;

    public void FireShot(int playerID, int throwableID) {

        PhotonView photonView = PhotonView.Get(this);
        photonView.RPC("FireShotRPC", RpcTarget.All, playerID, throwableID);

    }

    public void SyncTable(Quaternion outRot, Quaternion midRot, Quaternion inRot) {

        PhotonView photonView = PhotonView.Get(this);
        photonView.RPC("SyncTableRPC", RpcTarget.All, outRot, midRot, inRot);

    }

    public void SyncScore(int T1_Score, int T2_Score) {

        PhotonView photonView = PhotonView.Get(this);
        photonView.RPC("SyncScoreRPC", RpcTarget.All, T1_Score, T2_Score);

    }

    public void SyncTilt(Vector3 newHotspot, float newIntensity) {

        PhotonView photonView = PhotonView.Get(this);
        photonView.RPC("SyncTiltRPC", RpcTarget.All, newHotspot, newIntensity);

    }

    public void SyncTime(float newTime) {

        PhotonView photonView = PhotonView.Get(this);
        photonView.RPC("SyncTimeRPC", RpcTarget.All, newTime);

    }

    public void GameFinished() {

        PhotonView photonView = PhotonView.Get(this);
        photonView.RPC("GameFinishedRPC", RpcTarget.All);

    }

    public void ResetAllPlayers() {
        PhotonView photonView = PhotonView.Get(this);
        photonView.RPC("ResetAllPlayersRPC", RpcTarget.All);

    }


    [PunRPC]
    void FireShotRPC(int playerID, int throwableID) {
        if (playerID != NetworkManager.myPlayerID) {
            //GameObject.Find("Floor").GetComponent<Renderer>().material.color = Random.ColorHSV(0,1,1,1,1,1);


        }
    }

    [PunRPC]
    void SyncTableRPC(Quaternion outRot, Quaternion midRot, Quaternion inRot) {
        if (!NetworkManager.isHost) {
            NM.SyncTableRotations(outRot,midRot,inRot);
        }
        //Debug.Log("SyncTable");
    }

    [PunRPC]
    void SyncScoreRPC(int T1_Score,int T2_Score) {
        NetworkManager.team_1_ScoreStatic = T1_Score;
        NetworkManager.team_2_ScoreStatic = T2_Score;
        //Debug.Log("SyncTable");

        NM.HC.team_1_ScoreTMP.text = T1_Score.ToString();
        NM.HC.team_1_ScoreTMP_BG.text = T1_Score.ToString();
        NM.HC.team_2_ScoreTMP.text = T2_Score.ToString();
        NM.HC.team_2_ScoreTMP_BG.text = T2_Score.ToString();
    }


    [PunRPC]
    void SyncTiltRPC(Vector3 newHotspot, float newIntensity) {
        NM.UpdateTilt(newHotspot, newIntensity);
    }

    [PunRPC]
    void SyncTimeRPC(float newTime) {
        if(!NetworkManager.isHost)
            NM.gameTimer = newTime;
        NM.gameStarted = true;
    }

    //
    [PunRPC]
    void GameFinishedRPC() {
        NM.GameFinished();
    }

    [PunRPC]
    void ResetAllPlayersRPC() {
        PhotonNetwork.Disconnect();
        SceneManager.LoadScene(0);
    }
}
