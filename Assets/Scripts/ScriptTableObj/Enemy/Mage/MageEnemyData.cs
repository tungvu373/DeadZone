using UnityEngine;

[CreateAssetMenu(fileName = "MageEnemyData", menuName = "Game/Enemy/Mage Enemy")]
public class MageEnemyData : EnemyData
{
    [Header("Attack")]
    public float attackDamage = 10f;
    public float attackRange = 2.5f;
    public float attackCooldown = 1.5f;

    [Header("Projectile")]
    public GameObject projectilePrefab;
    public float projectileSpeed = 6f;

    [Header("Animation")]
    public float attackDelay = 0.5f;
}