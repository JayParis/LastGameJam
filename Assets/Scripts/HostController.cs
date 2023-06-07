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

    public Transform thrownItemsTrans;

    void Start()
    {
        
    }

    void Update()
    {
        if(periodicTableUpdate > 0) {
            periodicTableUpdate -= Time.deltaTime;
        } else {
            NM.RPC.SyncTable(NM.tableOut.rotation, NM.tableMid.rotation, NM.tableIn.rotation);
            periodicTableUpdate = 3f;
        }
    }

    public void CalculateAllScores() {
        foreach (ActiveThrowable AT in thrownItemsTrans.GetComponentsInChildren<ActiveThrowable>()) {

        }
    }
}
