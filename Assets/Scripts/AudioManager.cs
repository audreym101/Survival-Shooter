using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField] AudioSource sfxSource;

    [Header("Clips")]
    [SerializeField] AudioClip shootClip;
    [SerializeField] AudioClip playerDeathClip;
    [SerializeField] AudioClip enemySpawnClip;
    [SerializeField] AudioClip enemyShootClip;
    [SerializeField] AudioClip enemyDamageClip;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void PlayShoot() => Play(shootClip);
    public void PlayPlayerDeath() => Play(playerDeathClip);
    public void PlayEnemySpawn() => Play(enemySpawnClip);
    public void PlayEnemyShoot() => Play(enemyShootClip);
    public void PlayEnemyDamage() => Play(enemyDamageClip);

    void Play(AudioClip clip)
    {
        if (clip != null) sfxSource.PlayOneShot(clip);
    }
}
