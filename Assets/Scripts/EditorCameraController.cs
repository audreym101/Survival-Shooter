using UnityEngine;
using UnityEngine.InputSystem;

public class EditorCameraController : MonoBehaviour
{
    [SerializeField] float moveSpeed = 3f;
    [SerializeField] float lookSpeed = 2f;
    [SerializeField] float buttonStepDistance = 0.5f;
    [SerializeField] Transform movementRoot;

    float _rotX;
    float _rotY;
    float _buttonForwardInput;

    void Awake() => movementRoot ??= transform.parent != null ? transform.parent : transform;

    void Update()
    {
        if (Keyboard.current != null && Mouse.current != null && Mouse.current.rightButton.isPressed)
        {
            Vector2 mouseDelta = Mouse.current.delta.ReadValue();
            _rotY += mouseDelta.x * lookSpeed;
            _rotX -= mouseDelta.y * lookSpeed;
            _rotX = Mathf.Clamp(_rotX, -80f, 80f);
            transform.rotation = Quaternion.Euler(_rotX, _rotY, 0);
        }

        Vector3 move = Vector3.zero;
        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) move += transform.forward;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) move -= transform.forward;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) move -= transform.right;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) move += transform.right;
            if (Keyboard.current.eKey.isPressed) move += Vector3.up;
            if (Keyboard.current.qKey.isPressed) move -= Vector3.up;
        }

        if (!Mathf.Approximately(_buttonForwardInput, 0f))
            move += transform.forward * _buttonForwardInput;

        if (move.sqrMagnitude > 1f)
            move.Normalize();

        movementRoot.position += move * moveSpeed * Time.deltaTime;
    }

    public void StartMovingForward() => _buttonForwardInput = 1f;

    public void StartMovingBackward() => _buttonForwardInput = -1f;

    public void StopMoving() => _buttonForwardInput = 0f;

    public void MoveForwardStep() => MoveStep(1f);

    public void MoveBackwardStep() => MoveStep(-1f);

    void MoveStep(float direction)
    {
        movementRoot.position += transform.forward * direction * buttonStepDistance;
    }
}
