using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DamageFeedback : MonoBehaviour
{
    [SerializeField] Image damageOverlay;
    [SerializeField] float flashDuration = 0.2f;

    Renderer[] _renderers;
    Color[] _originalColors;

    void Awake()
    {
        _renderers = GetComponentsInChildren<Renderer>();
        _originalColors = new Color[_renderers.Length];
        for (int i = 0; i < _renderers.Length; i++)
            _originalColors[i] = _renderers[i].material.color;
    }

    // Called when player takes damage
    public void ShowPlayerDamage()
    {
        if (damageOverlay != null)
            StartCoroutine(FlashOverlay());
    }

    // Called when enemy takes damage
    public void ShowEnemyHit()
    {
        StartCoroutine(FlashEnemyRed());
    }

    IEnumerator FlashOverlay()
    {
        Color c = damageOverlay.color;
        c.a = 0.4f;
        damageOverlay.color = c;
        yield return new WaitForSeconds(flashDuration);
        while (c.a > 0)
        {
            c.a -= Time.deltaTime * 3f;
            damageOverlay.color = c;
            yield return null;
        }
    }

    IEnumerator FlashEnemyRed()
    {
        for (int i = 0; i < _renderers.Length; i++)
            _renderers[i].material.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        for (int i = 0; i < _renderers.Length; i++)
            _renderers[i].material.color = _originalColors[i];
    }
}
