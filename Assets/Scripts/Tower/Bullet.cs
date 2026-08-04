using UnityEngine;

public class Bullet : MonoBehaviour, IPoolable
{
    [Header("Stats")]
    public float speed = 12f;
    public float hitDistance = 0.15f;

    private EnemyMovement target;
    private int targetGen;          // generation stamp — phát hiện "quái tái chế"
    private float damage;
    private bool isReturned;

    // ─────────────────── IPOOL ABLE ──────────────────────────────────

    public void OnSpawnFromPool()
    {
        isReturned = false;
        target = null;
    }

    public void OnReturnToPool()
    {
        target = null;
    }

    // ─────────────────────────── API ─────────────────────────────────

    /// <summary>Tower gọi ngay sau khi spawn từ pool.</summary>
    public void Seek(EnemyMovement newTarget, float dmg)
    {
        target    = newTarget;
        targetGen = newTarget.SpawnGeneration; // ghi nhớ thế hệ hiện tại
        damage    = dmg;
    }

    // ─────────────────────────── UPDATE ──────────────────────────────

    void Update()
    {
        // Quái đã chết HOẶC đã được tái chế từ pool (SpawnGeneration tăng)
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
        target.TakeDamage(damage);
        ReturnSelf();
    }

    void ReturnSelf()
    {
        if (isReturned) return;
        isReturned = true;
        target = null;
        ObjectPool.Instance.ReturnToPool("Bullet", gameObject);
    }
}