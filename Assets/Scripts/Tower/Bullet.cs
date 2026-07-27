using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Stats")]
    public float speed = 12f;
    public float hitDistance = 0.15f;   // khoảng cách tính là trúng

    private EnemyMovement target;
    private float damage;
    private bool isReturned;

    void OnEnable()
    {
        isReturned = false;
    }

    // Tower gọi hàm này ngay sau khi spawn
    public void Seek(EnemyMovement newTarget, float dmg)
    {
        target = newTarget;
        damage = dmg;
    }

    void Update()
    {
        // Mục tiêu đã chết / về pool giữa chừng → đạn tự hủy
        if (target == null || !target.gameObject.activeInHierarchy)
        {
            ReturnSelf();
            return;
        }

        Vector3 dir = target.transform.position - transform.position;
        float distThisFrame = speed * Time.deltaTime;

        // Trúng mục tiêu
        if (dir.magnitude <= distThisFrame + hitDistance)
        {
            HitTarget();
            return;
        }

        // Bay đuổi theo mục tiêu
        transform.Translate(dir.normalized * distThisFrame, Space.World);

        // Xoay đầu đạn theo hướng bay
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

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