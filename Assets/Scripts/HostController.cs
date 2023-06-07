using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using TMPro;


public class HostController : MonoBehaviour
{
    public NetworkManager NM;

    float periodicTableUpdate = 1f;
    float periodicPointsUpdate = 2f;

    public Transform thrownItemsTrans;

    public TextMeshProUGUI team_1_ScoreTMP;
    public TextMeshProUGUI team_2_ScoreTMP;


    void Start()
    {
        
    }

    void Update()
    {
        if(periodicTableUpdate > 0) {
            periodicTableUpdate -= Time.deltaTime;
        } else {
            NM.RPC.SyncTable(NM.tableOut.rotation, NM.tableMid.rotation, NM.tableIn.rotation);
            CalculateWeight();
            periodicTableUpdate = 3f;
        }

        if (periodicPointsUpdate > 0) {
            periodicPointsUpdate -= Time.deltaTime;
        } else {
            CalculateAllScores();
            periodicPointsUpdate = 2f;
        }
    }

    public void CalculateAllScores() {
        int Team_1_Score = 0;
        int Team_2_Score = 0;

        foreach (ActiveThrowable AT in thrownItemsTrans.GetComponentsInChildren<ActiveThrowable>()) {
            RaycastHit hit;
            if (Physics.Raycast(AT.transform.position + Vector3.up, Vector3.down, out hit, 99, NM.PC.collisionLayerMask)) {
                if (hit.collider.GetComponent<ScoreVal>() != null) {
                    //hit.collider.GetComponent<ScoreVal>()
                    if(AT.isTeam_1)
                        Team_1_Score += hit.collider.GetComponent<ScoreVal>().points;
                    else
                        Team_2_Score += hit.collider.GetComponent<ScoreVal>().points;

                }
            }
        }
        NM.RPC.SyncScore(Team_1_Score, Team_2_Score);

        //Debug.Log("Calc points: " + Team_1_Score.ToString() + " | " + Team_2_Score.ToString());

        team_1_ScoreTMP.text = Team_1_Score.ToString();
        team_2_ScoreTMP.text = Team_2_Score.ToString();
    }

    public void CalculateWeight() {
        //List<Vector3> weightPositions = new List<Vector3>();

        Vector3 avgPos = Vector3.zero;
        int counts = 0;

        foreach (ActiveThrowable AT in thrownItemsTrans.GetComponentsInChildren<ActiveThrowable>()) {
            if(AT.transform.position.y > 5f && AT.transform.position.y < 15) {
                //weightPositions.Add(AT.transform.position);
                avgPos += AT.transform.position;
                counts++;
            }
        }

        if (counts > 2) {
            Vector3 finalPos = avgPos / counts;

            NM.RPC.SyncTilt(finalPos, counts * 0.25f); //2 is too much
        } else {
            NM.RPC.SyncTilt(Vector3.zero, 0);
        }
        //GameObject.Find("_AVG").transform.position = finalPos;

    }
}
