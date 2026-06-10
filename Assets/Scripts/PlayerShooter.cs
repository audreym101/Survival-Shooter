using UnityEngine;

public class PlayerShooter : MonoBehaviour
{
    [SerializeField] float fireRate = 0.3f;
    [SerializeField] int bulletDamage = 25;
    [SerializeField] float shootRange = 20f;
    [SerializeField] Camera arCamera;
    [SerializeField] ParticleSystem muzzleFlash;

    float _nextFireTime;

    void Update()
    {
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            TryShoot();

        // Editor testing with mouse
        if (Application.isEditor && Input.GetMouseButtonDown(0))
            TryShoot();
    }

    void TryShoot()
    {
        if (Time.time < _nextFireTime) return;
        _nextFireTime = Time.time + fireRate;

        muzzleFlash?.Play();
        AudioManager.Instance?.PlayShoot();

        Ray ray = arCamera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f));
        if (Physics.Raycast(ray, out RaycastHit hit, shootRange))
        {
            if (hit.collider.TryGetComponent(out EnemyBase enemy))
                enemy.TakeDamage(bulletDamage);
        }
    }
}
