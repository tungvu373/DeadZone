using UnityEngine;

public class FireBullet : MonoBehaviour, IPoolable
{
    [Header("Stats")]
    public float speed = 12f;
    public float hitDistance = 0.15f;

    [Header("Fire")]
    public float splashRadius = 1.5f;

    private EnemyMovement target;
    private int targetGen;          // generation stamp
    private float damage;
    private bool isReturned;

    // Fire Tower bắn ~1 lần/giây — OverlapCircleAll đơn giản và đáng tin cậy hơn
    // ContactFilter2D có thể bỏ sót Trigger collider nếu không config đúng

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

    public void Seek(EnemyMovement newTarget, float dmg)
    {
        target    = newTarget;
        targetGen = newTarget.SpawnGeneration;
        damage    = dmg;
    }

    // ─────────────────────────── UPDATE ──────────────────────────────

    void Update()
    {
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
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, splashRadius);

        for (int i = 0; i < hits.Length; i++)
        {
            EnemyMovement enemy = hits[i].GetComponent<EnemyMovement>();
            if (enemy == null) continue;

            // FlyingEnemy miễn nhiễm hoàn toàn với Fire splash (đã chốt trong Interaction Matrix)
            // Bước 5 sẽ tạo class FlyingEnemy — filter sẽ tự hoạt động khi đó
            // if (enemy is FlyingEnemy) continue;  ← uncomment sau Bước 5

            enemy.TakeDamage(damage);
        }

        ReturnSelf();
    }

    void ReturnSelf()
    {
        if (isReturned) return;
        isReturned = true;
        target = null;
        ObjectPool.Instance.ReturnToPool("FireBullet", gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, splashRadius);
    }
}