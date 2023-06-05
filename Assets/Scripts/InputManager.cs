using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;

[DefaultExecutionOrder(-1)]
public class InputManager : Singleton<InputManager>
{
    public delegate void StartTouchEvent(Vector2 position, float time);
    public event StartTouchEvent OnStartTouch;
    public delegate void EndTouchEvent(Vector2 position, float time);
    public event EndTouchEvent OnEndTouch;
    public delegate void TouchPositionEvent(Vector2 position, float time);
    public event TouchPositionEvent OnTouchPosition;

    private TouchControls touchControls;

    private void Awake() {
        touchControls = new TouchControls();
    }

    private void OnEnable() {
        touchControls.Enable();
        TouchSimulation.Enable();

        UnityEngine.InputSystem.EnhancedTouch.EnhancedTouchSupport.Enable();
        UnityEngine.InputSystem.EnhancedTouch.Touch.onFingerDown += FingerDown;
        UnityEngine.InputSystem.EnhancedTouch.Touch.onFingerUp += FingerUp;
    }
    private void OnDisable() {
        touchControls.Disable();
        TouchSimulation.Disable();

        UnityEngine.InputSystem.EnhancedTouch.EnhancedTouchSupport.Disable();
        UnityEngine.InputSystem.EnhancedTouch.Touch.onFingerDown -= FingerDown;
        UnityEngine.InputSystem.EnhancedTouch.Touch.onFingerUp -= FingerUp;

    }
    private void Start() {
        touchControls.Touch.TouchPress.started += ctx => StartTouch(ctx);
        touchControls.Touch.TouchPress.canceled += ctx => EndTouch(ctx);
        //touchControls.Touch.TouchRelease.started += ctx => EndTouch(ctx);
        touchControls.Touch.TouchPosition.performed += ctx => TouchPosition(ctx);

    }

    private void StartTouch(InputAction.CallbackContext context) {
        //Debug.Log("Touch Started " + touchControls.Touch.TouchPosition.ReadValue<Vector2>());
        if (OnStartTouch != null) OnStartTouch(touchControls.Touch.TouchPosition.ReadValue<Vector2>(), (float)context.startTime);
    }
    private void EndTouch(InputAction.CallbackContext context) {
        //Debug.Log("Touch Ended");
        if (OnEndTouch != null) OnEndTouch(touchControls.Touch.TouchPosition.ReadValue<Vector2>(), (float)context.time);
    }
    private void TouchPosition(InputAction.CallbackContext context) { 
        if(OnTouchPosition != null) OnTouchPosition(touchControls.Touch.TouchPosition.ReadValue<Vector2>(), (float)context.time);
    }

    private void FingerDown(Finger finger) {
        if (OnStartTouch != null) OnStartTouch(finger.screenPosition, Time.time);
    }

    private void FingerUp(Finger finger) {
        //Debug.Log("Touch Ended");
        if (OnEndTouch != null) OnEndTouch(finger.screenPosition, Time.time);
    }
}
