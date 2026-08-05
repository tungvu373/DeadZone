using UnityEngine;

public class IceBullet : MonoBehaviour, IPoolable
{
    [Header("Stats")]
    public float speed = 10f;
    public float hitDistance = 0.15f;

    private EnemyMovement target;
    private int targetGen;          // generation stamp — phát hiện "quái tái chế"
    private float damage;
    private float slowPercent;
    private float slowDuration;
    private bool isReturned;

    // ─────────────────── IPOOLABLE ───────────────────────────────────

    public void OnSpawnFromPool()
    {
        isReturned = false;
        target     = null;
    }

    public void OnReturnToPool()
    {
        target = null;
    }

    // ─────────────────────────── API ─────────────────────────────────

    /// <summary>IceTower gọi ngay sau khi spawn từ pool.</summary>
    public void Init(EnemyMovement newTarget, float dmg, float slowPct, float slowDur)
    {
        target       = newTarget;
        targetGen    = newTarget.SpawnGeneration;
        damage       = dmg;
        slowPercent  = slowPct;
        slowDuration = slowDur;
    }

    // ─────────────────────────── UPDATE ──────────────────────────────

    void Update()
    {
        // Quái đã chết HOẶC tái chế từ pool (SpawnGeneration tăng) → tự hủy
        if (target == null
            || !target.gameObject.activeSelf
            || target.SpawnGeneration != targetGen)
        {
            ReturnSelf();
            return;
        }

        Vector3 dir = target.transform.position - transform.position;
        float distThisFrame = speed * Time.deltaTime;

        if (dir.magnitude <= distThisFrame + hitDistance)
        {
            HitTarget();
            return;
        }

        transform.Translate(dir.normalized * distThisFrame, Space.World);

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    // ─────────────────────────── PRIVATE ─────────────────────────────

    void HitTarget()
    {
        // Gây damage
        target.TakeDamage(damage);

        // Áp dụng slow qua StatusEffectHandler — tự xử lý resistance và cap
        StatusEffectHandler handler = target.GetComponent<StatusEffectHandler>();
        if (handler != null)
            handler.ApplySlow(slowPercent, slowDuration, target.slowResistance);
        else
            Debug.LogWarning($"[IceBullet] {target.name} thiếu StatusEffectHandler!");

        ReturnSelf();
    }

    void ReturnSelf()
    {
        if (isReturned) return;
        isReturned = true;
        target = null;
        ObjectPool.Instance.ReturnToPool("IceBullet", gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.5f, 0.83f, 1f, 0.5f);
        Gizmos.DrawWireSphere(transform.position, hitDistance);
    }
}
