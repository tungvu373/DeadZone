using UnityEngine;
using System.Collections;

/// <summary>
/// Lightning VFX — đường chớp giữa 2 điểm, tự động fade và về pool.
/// Gắn script này lên prefab có LineRenderer.
/// </summary>
[RequireComponent(typeof(LineRenderer))]
public class LightningVFX : MonoBehaviour, IPoolable
{
    [Header("Fade")]
    [Min(0.01f)]
    public float fadeDuration = 0.15f;

    private LineRenderer lr;
    private Coroutine fadeCoroutine;
    private bool isReturned;

    void Awake()
    {
        lr = GetComponent<LineRenderer>();
        lr.positionCount = 2;
    }

    // ─────────────────── IPOOLABLE ───────────────────────────────────

    public void OnSpawnFromPool()
    {
        isReturned = false;
        if (lr != null)
        {
            Color c = lr.startColor;
            c.a = 1f;
            lr.startColor = c;
            lr.endColor   = c;
        }
    }

    public void OnReturnToPool()
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
            fadeCoroutine = null;
        }
    }

    // ─────────────────────────── API ─────────────────────────────────

    /// <summary>LightningTower gọi ngay sau khi SpawnFromPool.</summary>
    public void Setup(Vector3 from, Vector3 to)
    {
        if (lr == null) return;
        lr.SetPosition(0, from);
        lr.SetPosition(1, to);

        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeAndReturn());
    }

    // ─────────────────────────── PRIVATE ─────────────────────────────

    IEnumerator FadeAndReturn()
    {
        float elapsed = 0f;
        Color startColor = lr.startColor;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);

            Color c = startColor;
            c.a = alpha;
            lr.startColor = c;
            lr.endColor   = c;

            yield return null;
        }

        fadeCoroutine = null;
        ReturnSelf();
    }

    void ReturnSelf()
    {
        if (isReturned) return;
        isReturned = true;
        ObjectPool.Instance.ReturnToPool("LightningVFX", gameObject);
    }
}
