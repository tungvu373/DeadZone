using UnityEngine;
using System.Collections.Generic;
public class Tower : MonoBehaviour
{
    public static List<Tower> ActiveTowers = new List<Tower>();

    void OnEnable() { ActiveTowers.Add(this); }
    void OnDisable() { ActiveTowers.Remove(this); }
    private float health;
    [Header("Data")]
    public TowerData data;
    [Header("Setup")]
    public Transform rotatePart;
    public Transform firePoint;
    [Header("UI")]
    public HealthBar healthBar;
    [Header("Range Indicator")]
    public GameObject rangeIndicator;
    [Header("Visual")]
    public SpriteRenderer bodyRenderer;      // ✅ SpriteRenderer của thân tower (Base)
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
        health = stats.maxHealth;
        if (healthBar != null)
            healthBar.SetHealth(health, stats.maxHealth);
        // ✅ Đổi sprite theo level
        if (bodyRenderer != null && stats.levelSprite != null)
            bodyRenderer.sprite = stats.levelSprite;
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

    void Shoot()
    {
        GameObject bulletObj = ObjectPool.Instance.SpawnFromPool
            ("Bullet", firePoint.position, firePoint.rotation);

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
        if (rangeIndicator != null && rangeIndicator.activeSelf)
            ShowRange(true);
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
    public void TakeDamage(float amount)
    {
        health -= amount;
        if (healthBar != null)
            healthBar.SetHealth(health, data.levels[Level - 1].maxHealth);
        if (health <= 0) Die();
    }
    public void ShowRange(bool show)
    {
        if (rangeIndicator == null) return;

        if (show)
        {
            // Sprite tròn mặc định đường kính 1 unit → scale = range * 2
            rangeIndicator.transform.localScale = Vector3.one * range * 2f;
        }
        rangeIndicator.SetActive(show);
    }
    void Die()
    {
        Destroy(gameObject);
    }
}