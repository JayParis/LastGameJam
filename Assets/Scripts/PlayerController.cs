using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public class PlayerController : MonoBehaviour
{
    public NetworkManager NM;
    public GameRPCs RPC;

    public AnimationCurve throwRemap;
    public float lateralSensitivity = 1f;

    private InputManager inputManager;

    float touchDownDelay = 0.05f;
    float touchUpDelay = 0.05f;

    private Vector2 tapPos;
    private Vector2 dragPos;
    private Vector2 speedPos;

    private Vector2 dragDelta;

    public RawImage touchPosRI;
    public RawImage dragPosRI;
    public RawImage speedPosRI;

    float speedCheckInterval = 0f;
    float dragSpeed = 0;

    List<float> speeds = new List<float>();
    const int avgCapacity = 20;


    public TextMeshProUGUI outputText;

    public Rigidbody throwable;
    public Transform throwableStartPos;
    public Transform throwableCamPos;
    public Transform travelCamPos;
    public Transform lookCamPos;

    public GameObject nextItem;

    bool thrown = false;
    public bool camFollow = false;

    float zoomFoV = 60f;

    float camHeight = 3f;

    int bounceMarkID = 0;
    float bounceMarkDelay = 0f;
    public Transform bouncemark_1;
    public Transform bouncemark_2;
    public Transform bouncemark_3;

    public LayerMask collisionLayerMask;
    public GameObject dropshadow;

    private void Awake() {
        //Application.targetFrameRate = 60;
        inputManager = InputManager.Instance;
    }

    void Start()
    {
        speeds = new List<float>();
        for (int i = 0; i < avgCapacity; i++) {
            speeds.Add(0);
        }


        bouncemark_1.transform.parent = null;
        bouncemark_2.transform.parent = null;
        bouncemark_3.transform.parent = null;

        bouncemark_1.eulerAngles = Vector3.zero;
        bouncemark_2.eulerAngles = Vector3.zero;
        bouncemark_3.eulerAngles = Vector3.zero;
    }

    void Update()
    {
        if (speedCheckInterval > 0f)
            speedCheckInterval -= Time.deltaTime;
        else {

            Vector2 normDragPos = dragPos / new Vector2(Screen.width, Screen.height);
            Vector2 normTouchPos = tapPos / new Vector2(Screen.width, Screen.height);
            Vector2 normSpeedPos = speedPos / new Vector2(Screen.width, Screen.height);

            for (int i = 0; i < speeds.Count; i++) {
                if (speeds[i] == 0) {
                    speeds[i] = (normDragPos - normSpeedPos).magnitude * 40.995f;
                    break;
                }
            }

            //dragSpeed = (dragPos - speedPos).magnitude * 0.005f;
            outputText.text = "Mag: " + dragSpeed.ToString();
            speedPos = dragPosRI.transform.position;
            speedPosRI.transform.position = speedPos;
            speedCheckInterval = 0.025f / 2f;//0.05f
        }

        if (touchDownDelay > 0)
            touchDownDelay -= Time.deltaTime;
        if (touchUpDelay > 0)
            touchUpDelay -= Time.deltaTime;

        if(bounceMarkDelay > 0) {
            bounceMarkDelay -= Time.deltaTime;
        }

        if (camFollow) {
            Camera.main.transform.rotation = Quaternion.Lerp(Camera.main.transform.rotation, thrown ? travelCamPos.rotation : throwableCamPos.rotation, Time.deltaTime * 15f);
        } else {
            Camera.main.transform.rotation = Quaternion.Lerp(Camera.main.transform.rotation, thrown ? lookCamPos.rotation : throwableCamPos.rotation, Time.deltaTime * 5f);
        }

        //Debug---

        if (Input.GetKeyDown(KeyCode.Y)) {
            //SetSpawn(Random.Range(0,32));
        }

        lookCamPos.LookAt(throwable.transform.position);

        if (!camFollow && thrown)
            zoomFoV = Mathf.Lerp(zoomFoV, 32f, Time.deltaTime * 0.5f);
        else
            zoomFoV = 60f;

        Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, thrown ? zoomFoV : 60f, Time.deltaTime * 10f);
        camHeight = Mathf.Lerp(camHeight, thrown ? 1f : 3f, Time.deltaTime * 8f);
    }

    private void LateUpdate() {
        if (camFollow) {
            Camera.main.transform.position = throwable.position + transform.parent.TransformDirection(new Vector3(0, camHeight, -3f));
        } else {
            Camera.main.transform.position = throwableStartPos.position + transform.parent.TransformDirection(new Vector3(0, 3f, -3f));
        }
        //Vector3 targetPos = throwable.position + transform.parent.TransformDirection(new Vector3(0, 3f, -3f));
        //Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, targetPos, Time.deltaTime * 35f);
    }

    private void OnEnable() {

        inputManager.OnStartTouch += TouchDown;
        inputManager.OnEndTouch += TouchUp;
        inputManager.OnTouchPosition += TouchHeld;
    }
    private void OnDisable() {

        inputManager.OnEndTouch -= TouchDown;
        inputManager.OnEndTouch -= TouchUp;
        inputManager.OnTouchPosition -= TouchHeld;
    }


    public void TouchDown(Vector2 screenPosition, float time) {
        if (Application.isPlaying && touchDownDelay <= 0) {
            tapPos = screenPosition;
            touchPosRI.transform.position = screenPosition;
            dragPosRI.transform.position = screenPosition;
            speedPos = screenPosition;
            speedPosRI.transform.position = screenPosition;
            
            for (int i = 0; i < speeds.Count; i++) {
                speeds[i] = 0f;
            }

            if (thrown) {
                ResetThrowable();
            }

            

            touchDownDelay = 0.05f;
        }
    }

    public void TouchUp(Vector2 screenPosition, float time) {
        if (Application.isPlaying && touchUpDelay <= 0) {
            dragDelta = Vector2.zero;

            Vector2 normDragPos = dragPos / new Vector2(Screen.width, Screen.height);
            Vector2 normTouchPos = tapPos / new Vector2(Screen.width, Screen.height);
            Vector2 normSpeedPos = speedPos / new Vector2(Screen.width, Screen.height);

            float avgSpeed = 0f;
            for (int i = 0; i < speeds.Count; i++) {
                if(speeds[i] != 0)
                    avgSpeed += speeds[i];

            }
            float finalSpeed = avgSpeed / (float)avgCapacity;

            //float finalSpeed = (normDragPos - normSpeedPos).magnitude * 40.995f;


            if (finalSpeed > 0.2f) {

                //float xVarCalc = (tapPos.x - dragPos.x) / Screen.width;
                float xVarCalc = (normTouchPos.x - normDragPos.x);


                Throw(finalSpeed, xVarCalc);
            }

            touchUpDelay = 0.05f;
        }
    }

    public void TouchHeld(Vector2 screenPosition, float time) {
        if (Application.isPlaying) {
            //Debug.Log();
            dragPos = screenPosition;
            dragPosRI.transform.position = screenPosition;

            dragDelta = tapPos - dragPos;

            //moveData.text = "X: " + moveDir.x + '\n' + "Y: " + moveDir.y;
        }
    }

    public void Throw(float speed, float xVal) {

        foreach (Collider col in throwable.GetComponentsInChildren<Collider>()) {
            col.enabled = true;
        }

        //throwable.GetComponent<Collider>().enabled = true;
        throwable.constraints = RigidbodyConstraints.None;
        throwable.velocity = Camera.main.transform.TransformDirection(new Vector3(xVal * -0.59f * lateralSensitivity, 0.56f, 1f)) * 12f * throwRemap.Evaluate(speed);
        throwable.angularVelocity = Vector3.one * speed;
        thrown = true;

        RPC.FireShot(NetworkManager.myPlayerID, 0);
    }

    public void ResetThrowable() {

        bouncemark_1.position = Vector3.zero;
        bouncemark_2.position = Vector3.zero;
        bouncemark_3.position = Vector3.zero;
        bouncemark_1.parent = null;
        bouncemark_2.parent = null;
        bouncemark_3.parent = null;
        bounceMarkID = 0;

        throwable.GetComponent<ActiveThrowable>().Deactivate();


        GameObject newThrowable = PhotonNetwork.Instantiate("RugbyBall", throwableStartPos.position, throwableStartPos.rotation);
        //newThrowable.GetComponent<Rigidbody>();
        throwable = newThrowable.GetComponent<Rigidbody>();
        //newThrowable.transform.parent = NM.thrownItems[NetworkManager.myPlayerID - 1].transform;

        throwable.transform.position = throwableStartPos.position;
        throwable.transform.rotation = throwableStartPos.rotation;
        throwable.constraints = RigidbodyConstraints.FreezeAll;
        throwable.velocity = Vector3.zero;
        throwable.angularVelocity = Vector3.zero;
        thrown = false;

        throwable.GetComponent<ActiveThrowable>().PC = gameObject.GetComponent<PlayerController>();
        GameObject newDropshadow = Instantiate(dropshadow, Vector3.zero, dropshadow.transform.rotation, dropshadow.transform.parent);
        throwable.GetComponent<ActiveThrowable>().myDropshadow = newDropshadow.transform;
    }

    public void Bounce(Collision col) {
        if (bounceMarkDelay <= 0) {

            float heightOffset = 0.25f;
            switch (bounceMarkID) {
                case 0:
                    bouncemark_1.position = col.contacts[0].point + new Vector3(0,heightOffset, 0);
                    bouncemark_1.transform.parent = col.transform;
                    Debug.Log("Bounce " + Time.time);

                    break;
                case 1:
                    bouncemark_2.position = col.contacts[0].point + new Vector3(0, heightOffset, 0);
                    bouncemark_2.transform.parent = col.transform;
                    break;
                case 2:
                    bouncemark_3.position = col.contacts[0].point + new Vector3(0, heightOffset, 0);
                    bouncemark_3.transform.parent = col.transform;
                    break;
            }

            bounceMarkID++;
            bounceMarkDelay = 0.05f;

        }
    }

    //public void SetSpawn(int playerID) {
    //    transform.parent.eulerAngles = new Vector3(transform.parent.eulerAngles.x, NM.spawns[playerID].eulerAngles.y, transform.parent.eulerAngles.z);
    //}

    //UI ----

    public void ToggleCamFollow() {
        camFollow = !camFollow;
    }

}
