using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Editor only - simulates camera movement for testing.
/// This script is automatically disabled on Android builds.
/// </summary>
public class EditorCameraController : MonoBehaviour
{
    [SerializeField] float moveSpeed = 3f;
    [SerializeField] float lookSpeed = 2f;

    float _rotX;
    float _rotY;

    void Awake()
    {
#if !UNITY_EDITOR
        enabled = false;
#endif
    }

    void Update()
    {
        if (Keyboard.current == null || Mouse.current == null) return;

        // Look with right mouse button held
        if (Mouse.current.rightButton.isPressed)
        {
            Vector2 mouseDelta = Mouse.current.delta.ReadValue();
            _rotY += mouseDelta.x * lookSpeed;
            _rotX -= mouseDelta.y * lookSpeed;
            _rotX = Mathf.Clamp(_rotX, -80f, 80f);
            transform.rotation = Quaternion.Euler(_rotX, _rotY, 0);
        }

        // Move with WASD
        Vector3 move = Vector3.zero;
        if (Keyboard.current.wKey.isPressed) move += transform.forward;
        if (Keyboard.current.sKey.isPressed) move -= transform.forward;
        if (Keyboard.current.aKey.isPressed) move -= transform.right;
        if (Keyboard.current.dKey.isPressed) move += transform.right;
        if (Keyboard.current.eKey.isPressed) move += Vector3.up;
        if (Keyboard.current.qKey.isPressed) move -= Vector3.up;

        transform.position += move * moveSpeed * Time.deltaTime;
    }
}
