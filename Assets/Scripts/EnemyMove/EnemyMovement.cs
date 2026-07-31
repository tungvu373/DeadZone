using UnityEngine;
using System.Collections.Generic;

public class EnemyMovement : MonoBehaviour
{
    [Header("Data")]
    public EnemyData data;

    public static List<EnemyMovement> ActiveEnemies = new List<EnemyMovement>();

    protected float health;                 // ✅ protected để Tanker dùng
    protected Transform target;
    protected int waypointIndex;
    protected bool isReturned;
    [Header("UI")]
    public HealthBar healthBar;

    protected virtual void OnEnable()       // ✅ virtual
    {
        if (Waypoints.points == null || Waypoints.points.Length == 0)
            return;

        if (data == null)
        {
            Debug.LogError($"[EnemyMovement] Chưa gán EnemyData vào '{gameObject.name}'!", gameObject);
            return;
        }
        if (healthBar != null)
            healthBar.SetHealth(health, data.maxHealth);

        health = data.maxHealth;
        waypointIndex = 0;
        isReturned = false;
        target = Waypoints.points[0];
        transform.position = Waypoints.points[0].position;

        ActiveEnemies.Add(this);
        if (healthBar != null)
            healthBar.SetHealth(health, data.maxHealth);
    }

    protected virtual void OnDisable()
    {
        ActiveEnemies.Remove(this);
    }

    protected virtual void Update()         // ✅ virtual
    {
        MoveAlongPath();
    }

    protected void MoveAlongPath()
    {
        if (target == null) return;

        Vector3 dir = target.position - transform.position;
        transform.Translate(dir.normalized * data.speed * Time.deltaTime, Space.World);

        if (Vector3.Distance(transform.position, target.position) <= 0.2f)
        {
            GetNextWaypoint();
        }
    }

    void GetNextWaypoint()
    {
        if (waypointIndex >= Waypoints.points.Length - 1)
        {
            ReachEnd();
            return;
        }
        waypointIndex++;
        target = Waypoints.points[waypointIndex];
    }

    void ReachEnd()
    {
        GameManager.Instance.TakeDamage(data.damageToBase);
        ReturnSelf();
    }

    public virtual void TakeDamage(float amount)   // ✅ virtual — Tanker sẽ override để chặn damage
    {
        health -= amount;
        if (healthBar != null)
            healthBar.SetHealth(health, data.maxHealth);

        if (health <= 0)
        {
            GameManager.Instance.AddMoney(data.moneyReward);
            ReturnSelf();
        }
    }

    protected void ReturnSelf()
    {
        if (isReturned) return;
        isReturned = true;
        ObjectPool.Instance.ReturnToPool(data.poolTag, gameObject);   // ✅ dùng poolTag từ data
    }
}