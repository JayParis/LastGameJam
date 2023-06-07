using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActiveThrowable : MonoBehaviour
{
    public PlayerController PC;
    bool isActive = true;
    public bool isTeam_1 = false;

    public Transform myDropshadow;
    public Rigidbody rb;

    bool firstContact = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if(PC != null) {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, Vector3.down, out hit, 99, PC.collisionLayerMask)) {
                if(myDropshadow != null) {
                    myDropshadow.position = hit.point + Vector3.up * 0.0556f;
                }
            }
        }
    }

    private void FixedUpdate() {
        if (firstContact) {
            rb.AddForce(Vector3.down * 500f * Time.fixedDeltaTime);
        }
    }


    private void OnCollisionEnter(Collision collision) {
        if(PC != null && isActive) {
            PC.Bounce(collision);
            firstContact = true;
        }
    }

    public void Deactivate() {
        isActive = false;
    }

    public void SetThrownParent(int id) {
        transform.parent = GameObject.Find("_GLOBAL").GetComponent<NetworkManager>().thrownItems[id - 1].transform;
        //this.enabled = false;
        isActive = false;
    }

    public void SetIsTeam_1(bool itemIsTeam_1) {
        isTeam_1 = itemIsTeam_1;
        foreach (Renderer Rend in GetComponentsInChildren<Renderer>()) {
            Rend.material.color = isTeam_1 ? NetworkManager.team_1_Colour : NetworkManager.team_2_Colour;
            Rend.material.SetColor("_EmissionColor", isTeam_1 ? NetworkManager.team_1_Colour : NetworkManager.team_2_Colour);
        }
    }
}
