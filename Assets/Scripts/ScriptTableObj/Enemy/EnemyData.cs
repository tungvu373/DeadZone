using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "TD/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("Info")]
    public string enemyName = "Slime";

    [Header("Stats")]
    public float speed = 3f;
    public float maxHealth = 100f;

    [Header("Economy")]
    public int moneyReward = 10;      // tiền nhận khi giết
    public int damageToBase = 1;      // máu base mất khi quái đến đích
}