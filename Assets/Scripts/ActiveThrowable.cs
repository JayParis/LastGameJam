using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActiveThrowable : MonoBehaviour
{
    public PlayerController PC;
    bool isActive = true;

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
                    myDropshadow.position = hit.point + Vector3.up * 0.256f;
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
}
