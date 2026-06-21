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

        // Don't shoot if clicking UI buttons
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        // Mobile touch
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
            TryShoot();

        // Editor mouse click
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            TryShoot();
    }

    void TryShoot()
    {
        if (Time.time < _nextFireTime) return;
        _nextFireTime = Time.time + fireRate;

        if (muzzleFlash != null) muzzleFlash.Play();
        AudioManager.Instance?.PlayShoot();

        Ray ray = arCamera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f));
        if (Physics.Raycast(ray, out RaycastHit hit, shootRange))
        {
            EnemyBase enemy = hit.collider.GetComponentInParent<EnemyBase>()
                              ?? hit.collider.GetComponentInChildren<EnemyBase>();

            if (enemy != null)
                enemy.TakeDamage(bulletDamage);
        }
    }
}
