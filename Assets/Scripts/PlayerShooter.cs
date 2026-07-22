using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class PlayerShooter : MonoBehaviour
{
    [SerializeField] float fireRate = 0.3f;
    [SerializeField] int bulletDamage = 25;
    [SerializeField] float shootRange = 20f;
    [SerializeField] Camera arCamera;
    [SerializeField] ParticleSystem muzzleFlash;

    float _nextFireTime;

    void Awake()
    {
        if (arCamera == null)
            arCamera = Camera.main;
    }

    void Update()
    {
        if (GameManager.Instance != null && !GameManager.Instance.IsPlaying)
            return;

        // Mobile touch
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            Vector2 touchPosition = Touchscreen.current.primaryTouch.position.ReadValue();
            if (!IsPointerOverUI())
                TryShoot(touchPosition);
        }

        // Editor mouse click
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            if (!IsPointerOverUI())
                TryShoot(mousePosition);
        }
    }

    void TryShoot(Vector2 screenPosition)
    {
        if (Time.time < _nextFireTime) return;
        _nextFireTime = Time.time + fireRate;

        if (muzzleFlash != null) muzzleFlash.Play();
        AudioManager.Instance?.PlayShoot();

        if (arCamera == null)
            arCamera = Camera.main;

        if (arCamera == null)
            return;

        Ray ray = arCamera.ScreenPointToRay(screenPosition);
        if (Physics.Raycast(ray, out RaycastHit hit, shootRange))
        {
            EnemyBase enemy = hit.collider.GetComponentInParent<EnemyBase>()
                              ?? hit.collider.GetComponentInChildren<EnemyBase>();

            if (enemy != null)
                enemy.TakeDamage(bulletDamage);
        }
    }

    bool IsPointerOverUI()
    {
        if (EventSystem.current == null)
            return false;

        if (Touchscreen.current != null &&
            Touchscreen.current.primaryTouch.press.isPressed)
        {
            int touchId = Touchscreen.current.primaryTouch.touchId.ReadValue();
            return EventSystem.current.IsPointerOverGameObject(touchId);
        }

        return EventSystem.current.IsPointerOverGameObject();
    }
}
