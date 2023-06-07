using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

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
    }
}
