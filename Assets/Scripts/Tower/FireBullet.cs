using UnityEngine;

public class FireBullet : MonoBehaviour
{
    [Header("Stats")]
    public float speed = 12f;
    public float hitDistance = 0.15f;

    [Header("Fire")]
    public float splashRadius = 1.5f;

    private EnemyMovement target;
    private float damage;
    private bool isReturned;

    void OnEnable()
    {
        isReturned = false;
    }

    public void Seek(EnemyMovement newTarget, float dmg)
    {
        target = newTarget;
        damage = dmg;
    }

    void Update()
    {
        if (target == null || !target.gameObject.activeInHierarchy)
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

    void HitTarget()
    {
        Collider2D[] hits =
            Physics2D.OverlapCircleAll(transform.position, splashRadius);

        foreach (Collider2D hit in hits)
        {
            EnemyMovement enemy = hit.GetComponent<EnemyMovement>();

            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }
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