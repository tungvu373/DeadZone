using UnityEngine;

[CreateAssetMenu(fileName = "TankerData", menuName = "TD/Tanker Data")]
public class TankerData : EnemyData
{
    [Header("Attack Tower")]
    public float attackDamage = 15f;
    public float attackRate = 1f;         // số đòn / giây
    public float attackRange = 1.2f;      // tầm phát hiện & đánh tower

    [Header("Shield")]
    [Range(0f, 1f)]
    public float shieldBlockPercent = 0.5f;   // chặn 50% damage khi giơ khiên
}