using UnityEngine;
using System.Collections.Generic;

public class EnemyMovement : MonoBehaviour
{
    [Header("Data")]
    public EnemyData data;                 // ✅ toàn bộ chỉ số nằm trong asset

    public static List<EnemyMovement> ActiveEnemies = new List<EnemyMovement>();

    private float health;
    private Transform target;
    private int waypointIndex;
    private bool isReturned;

    void OnEnable()
    {
        if (Waypoints.points == null || Waypoints.points.Length == 0)
            return;

        health = data.maxHealth;           // ✅ từ data
        waypointIndex = 0;
        isReturned = false;
        target = Waypoints.points[0];
        transform.position = Waypoints.points[0].position;

        ActiveEnemies.Add(this);
    }

    void OnDisable()
    {
        ActiveEnemies.Remove(this);
    }

    void Update()
    {
        if (target == null) return;

        Vector3 dir = target.position - transform.position;
        transform.Translate(dir.normalized * data.speed * Time.deltaTime, Space.World);  // ✅ từ data

        if (dir != Vector3.zero)
        {
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }

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
        GameManager.Instance.TakeDamage(data.damageToBase);   // ✅ trừ máu base
        ReturnSelf();
    }

    public void TakeDamage(float amount)
    {
        health -= amount;
        if (health <= 0)
        {
            GameManager.Instance.AddMoney(data.moneyReward);  // ✅ cộng tiền
            ReturnSelf();
        }
    }

    void ReturnSelf()
    {
        if (isReturned) return;
        isReturned = true;
        ObjectPool.Instance.ReturnToPool("Enemy", gameObject);
    }
}