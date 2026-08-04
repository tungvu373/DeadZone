using UnityEngine;

public class MageEnemy : EnemyMovement
{
    [Header("Mage")]
    public MageEnemyData mageData;
    public Animator animator;

    private Tower targetTower;
    private float attackTimer;

    // ─────────────────── IPOOL ABLE override ─────────────────────────

    public override void OnSpawnFromPool()
    {
        base.OnSpawnFromPool();
        targetTower = null;
        attackTimer = 0f;
    }

    public override void OnReturnToPool()
    {
        base.OnReturnToPool();
        targetTower = null;
        if (animator != null) animator.SetBool("Attack", false);
    }

    // ─────────────────────── LIFECYCLE ───────────────────────────────

    protected override void OnEnable()
    {
        base.OnEnable();
        targetTower = null;
        attackTimer = 0f;
    }

    protected override void OnDisable()
    {
        targetTower = null;
        if (animator != null) animator.SetBool("Attack", false);
        base.OnDisable();
    }

    // ─────────────────────────── UPDATE ──────────────────────────────

    protected override void Update()
    {
        SearchTower();

        if (targetTower != null)
            AttackTower();
        else
        {
            if (animator != null) animator.SetBool("Attack", false);
            base.Update(); // di chuyển bình thường
        }
    }

    // ─────────────────────────── PRIVATE ─────────────────────────────

    void SearchTower()
    {
        targetTower = null;
        float nearest = Mathf.Infinity;

        foreach (Tower tower in Tower.ActiveTowers)
        {
            if (tower == null) continue;
            float dis = Vector2.Distance(transform.position, tower.transform.position);
            if (dis <= mageData.attackRange && dis < nearest)
            {
                nearest = dis;
                targetTower = tower;
            }
        }
    }

    void AttackTower()
    {
        attackTimer -= Time.deltaTime;
        if (animator != null) animator.SetBool("Attack", true);

        if (attackTimer <= 0f)
        {
            attackTimer = mageData.attackCooldown;
            if (targetTower != null)
                targetTower.TakeDamage(mageData.attackDamage);
        }
    }
}