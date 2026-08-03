using UnityEngine;

public class MageEnemy : EnemyMovement
{
    [Header("Mage")]
    public MageEnemyData mageData;

    public Animator animator;

    private Tower targetTower;
    private float attackTimer;

    protected override void Update()
    {
        SearchTower();

        if (targetTower != null)
        {
            AttackTower();
        }
        else
        {
            if (animator != null)
                animator.SetBool("Attack", false);

            // tiếp tục di chuyển
            base.Update();
        }
    }

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

        if (animator != null)
            animator.SetBool("Attack", true);

        if (attackTimer <= 0)
        {
            attackTimer = mageData.attackCooldown;

            if (targetTower != null)
            {
                targetTower.TakeDamage(mageData.attackDamage);
            }
        }
    }
}