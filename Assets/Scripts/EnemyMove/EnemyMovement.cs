using UnityEngine;
using System.Collections.Generic;

public class EnemyMovement : MonoBehaviour
{
    [Header("Stats")]
    public float speed = 3f;
    public float maxHealth = 100f;

    // Danh sách quái đang sống
    public static List<EnemyMovement> ActiveEnemies = new List<EnemyMovement>();

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
        ActiveEnemies.Remove(this); // Remove trên phần tử không tồn tại → an toàn, không lỗi
    }

    void Update()
    {
        if (target == null) return;
        // Di chuyển về waypoint hiện tại
        Vector3 dir = target.position - transform.position;
        transform.Translate(dir.normalized * speed * Time.deltaTime, Space.World);

        // Xoay sprite theo hướng đi (cần cho đường ngoằn ngoèo)
        if (dir != Vector3.zero)
        {
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        // Đến waypoint → chuyển sang điểm kế tiếp
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
        if (isReturned) return;   // chống return về pool 2 lần trong cùng frame
        isReturned = true;
        ObjectPool.Instance.ReturnToPool("Enemy", gameObject);
    }
}