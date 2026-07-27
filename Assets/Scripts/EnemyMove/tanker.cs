using System.Collections.Generic;
using UnityEngine;

public class tanker : MonoBehaviour
{
    [Header("Stats")]
    public float speed = 1.5f;      // Tanker đi chậm
    public float maxHealth = 300f;  // Máu nhiều

    // Danh sách Tanker đang sống
    public static List<tanker> ActiveEnemies = new List<tanker>();

    private float health;
    private Transform target;
    private int waypointIndex;
    private bool isReturned;

    void OnEnable()
    {
        // Pool đang khởi tạo lúc game start, Waypoints chưa sẵn sàng → bỏ qua
        if (Waypoints.points == null || Waypoints.points.Length == 0)
            return;

        health = maxHealth;
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

        // Di chuyển đến waypoint hiện tại
        Vector3 dir = target.position - transform.position;
        transform.Translate(dir.normalized * speed * Time.deltaTime, Space.World);

        // Xoay theo hướng di chuyển
        if (dir != Vector3.zero)
        {
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        // Đến waypoint
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
        ReturnSelf();
    }

    public void TakeDamage(float amount)
    {
        health -= amount;

        if (health <= 0)
        {
            ReturnSelf();
        }
    }

    void ReturnSelf()
    {
        if (isReturned) return;

        isReturned = true;

        // Trả về pool của Tanker
        ObjectPool.Instance.ReturnToPool("Tanker", gameObject);
    }
}