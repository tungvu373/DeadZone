using UnityEngine;

public class Tower : MonoBehaviour
{
    [Header("Data")]
    public TowerData data;                 // ✅ toàn bộ chỉ số nằm trong asset

    [Header("Setup")]
    public Transform rotatePart;
    public Transform firePoint;

    public int Level { get; private set; } = 1;
    public int TotalInvested { get; private set; }   // tổng tiền đã bỏ vào (để tính hoàn khi bán)

    // Chỉ số hiện tại (lấy từ data theo level)
    private float damage, range, fireRate;

    private EnemyMovement target;
    private float fireCountdown;
    private float searchCountdown;
    private const float searchInterval = 0.3f;

    void Start()
    {
        Level = 1;
        TotalInvested = data.buildCost;
        ApplyStats();
    }

    void ApplyStats()
    {
        TowerLevelStats stats = data.levels[Level - 1];
        damage = stats.damage;
        range = stats.range;
        fireRate = stats.fireRate;
    }

    void Update()
    {
        searchCountdown -= Time.deltaTime;
        if (searchCountdown <= 0f)
        {
            FindTarget();
            searchCountdown = searchInterval;
        }

        if (target != null && !IsTargetValid(target))
            target = null;

        fireCountdown -= Time.deltaTime;
        if (target == null) return;

        RotateToTarget();

        if (fireCountdown <= 0f)
        {
            Shoot();
            fireCountdown = 1f / fireRate;
        }
    }

    bool IsTargetValid(EnemyMovement enemy)
    {
        return enemy.gameObject.activeInHierarchy &&
               Vector2.Distance(transform.position, enemy.transform.position) <= range;
    }

    void FindTarget()
    {
        float shortestDist = Mathf.Infinity;
        EnemyMovement nearest = null;

        foreach (EnemyMovement enemy in EnemyMovement.ActiveEnemies)
        {
            float dist = Vector2.Distance(transform.position, enemy.transform.position);
            if (dist <= range && dist < shortestDist)
            {
                shortestDist = dist;
                nearest = enemy;
            }
        }
        target = nearest;
    }

    void RotateToTarget()
    {
        if (rotatePart == null) return;
        Vector3 dir = target.transform.position - rotatePart.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        rotatePart.rotation = Quaternion.Euler(0, 0, angle);
    }

    void Shoot()
    {
        GameObject bulletObj = ObjectPool.Instance.SpawnFromPool(
            "Bullet", firePoint.position, firePoint.rotation);

        if (bulletObj != null)
            bulletObj.GetComponent<Bullet>().Seek(target, damage);
    }

    // ================== UPGRADE / SELL ==================

    public bool CanUpgrade() => Level < data.MaxLevel;

    // Giá nâng lên level kế tiếp
    public int GetUpgradeCost() => CanUpgrade() ? data.levels[Level].upgradeCost : 0;

    public void Upgrade()
    {
        if (!CanUpgrade()) return;
        TotalInvested += GetUpgradeCost();
        Level++;
        ApplyStats();
        transform.localScale *= 1.1f;   // placeholder, Phase 6 đổi sprite theo level
    }

    public int GetSellValue()
    {
        return Mathf.RoundToInt(TotalInvested * data.sellRefundPercent);
    }

    void OnDrawGizmosSelected()
    {
        float r = (data != null && data.levels.Length > 0)
            ? data.levels[Mathf.Clamp(Level, 1, data.MaxLevel) - 1].range : 3f;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, r);
    }
}