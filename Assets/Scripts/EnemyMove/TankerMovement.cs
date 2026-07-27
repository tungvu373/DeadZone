using System.Security.Cryptography;
using UnityEngine;

public class TankerMovement : EnemyMovement
{
    [Header("Shield Visual")]
    public GameObject shieldVisual;       // sprite khiên (child), có thể để trống

    private TankerData TData => (TankerData)data;

    private Tower towerTarget;
    private float attackCountdown;
    private float searchCountdown;
    private const float searchInterval = 0.3f;
    private bool shieldUp;

    protected override void OnEnable()
    {
        base.OnEnable();                  // reset máu, waypoint... như quái thường

        towerTarget = null;
        attackCountdown = 0f;
        searchCountdown = 0f;
        SetShield(false);
    }

    protected override void Update()
    {
        // ----- Tìm tower trong tầm (định kỳ) -----
        if (towerTarget == null)          // tower sập → Unity null → tự tìm mục tiêu mới
        {
            SetShield(false);

            searchCountdown -= Time.deltaTime;
            if (searchCountdown <= 0f)
            {
                FindTowerInRange();
                searchCountdown = searchInterval;
            }
        }

        if (towerTarget != null)
        {
            // ----- CHẾ ĐỘ TẤN CÔNG: đứng yên + giơ khiên + đánh tower -----
            SetShield(true);
            AttackTower();
        }
        else
        {
            // ----- Không có tower gần → đi tiếp như quái thường -----
            MoveAlongPath();
        }
    }

    void FindTowerInRange()
    {
        float shortest = Mathf.Infinity;
        Tower nearest = null;

        foreach (Tower tower in Tower.ActiveTowers)
        {
            float dist = Vector2.Distance(transform.position, tower.transform.position);
            if (dist <= TData.attackRange && dist < shortest)
            {
                shortest = dist;
                nearest = tower;
            }
        }
        towerTarget = nearest;
    }

    void AttackTower()
    {
        // Quay mặt về tower
        Vector3 dir = towerTarget.transform.position - transform.position;
        if (dir != Vector3.zero)
        {
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        attackCountdown -= Time.deltaTime;
        if (attackCountdown <= 0f)
        {
            towerTarget.TakeDamage(TData.attackDamage);
            attackCountdown = 1f / TData.attackRate;
        }
    }

    // ✅ Override: đang giơ khiên → chặn bớt damage
    public override void TakeDamage(float amount)
    {
        if (shieldUp)
            amount *= (1f - TData.shieldBlockPercent);   // 0.5 → chỉ nhận 50%

        base.TakeDamage(amount);
    }

    void SetShield(bool up)
    {
        shieldUp = up;
        if (shieldVisual != null)
            shieldVisual.SetActive(up);
    }

    // Vẽ tầm đánh trong Scene view
    void OnDrawGizmosSelected()
    {
        if (data is TankerData td)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, td.attackRange);
        }
    }
}