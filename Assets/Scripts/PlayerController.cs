using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

public class PlayerController : MonoBehaviour
{
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

    public TextMeshProUGUI outputText;

    public Rigidbody throwable;
    public Transform throwableStartPos;
    public Transform throwableCamPos;
    public Transform travelCamPos;

    bool thrown = false;

    private void Awake() {
        //Application.targetFrameRate = 60;
        inputManager = InputManager.Instance;
    }

    void Start()
    {
        
    }

    void Update()
    {
        if (speedCheckInterval > 0f)
            speedCheckInterval -= Time.deltaTime;
        else {
            dragSpeed = (dragPos - speedPos).magnitude * 0.005f;
            outputText.text = "Mag: " + dragSpeed.ToString();
            speedPos = dragPosRI.transform.position;
            speedPosRI.transform.position = speedPos;
            speedCheckInterval = 0.025f;//0.05f
        }

        if (touchDownDelay > 0)
            touchDownDelay -= Time.deltaTime;
        if (touchUpDelay > 0)
            touchUpDelay -= Time.deltaTime;

        Camera.main.transform.rotation = Quaternion.Lerp(Camera.main.transform.rotation, thrown ? travelCamPos.rotation : throwableCamPos.rotation, Time.deltaTime * 15f);

        //Debug---

        if (Input.GetKeyDown(KeyCode.Y)) {
            ResetThrowable();
            Debug.Log("Debug");
        }

        Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, thrown ? 72f : 60f, Time.deltaTime * 10f);
    }

    private void LateUpdate() {
        Camera.main.transform.position = throwable.position + transform.parent.TransformDirection(new Vector3(0, 3f, -3f));
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

            if (thrown) {
                ResetThrowable();
            }

            touchDownDelay = 0.05f;
        }
    }

    public void TouchUp(Vector2 screenPosition, float time) {
        if (Application.isPlaying && touchUpDelay <= 0) {
            dragDelta = Vector2.zero;
            Debug.Log(dragSpeed);

            if (dragSpeed > 0.2f) {
                float xVarCalc = (tapPos.x - dragPos.x) / Screen.width;
                Debug.Log(xVarCalc);
                Throw(dragSpeed, xVarCalc);
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
        throwable.constraints = RigidbodyConstraints.None;
        throwable.velocity = Camera.main.transform.TransformDirection(new Vector3(xVal * -0.47f, 0.56f, 1f)) * 12f * speed;
        throwable.angularVelocity = Vector3.one * speed;
        thrown = true;
    }

    public void ResetThrowable() {
        throwable.transform.position = throwableStartPos.position;
        throwable.transform.rotation = throwableStartPos.rotation;
        throwable.constraints = RigidbodyConstraints.FreezeAll;
        throwable.velocity = Vector3.zero;
        throwable.angularVelocity = Vector3.zero;
        thrown = false;
    }
}
