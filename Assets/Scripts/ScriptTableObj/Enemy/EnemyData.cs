using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "TD/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("Info")]
    public string enemyName = "Slime";
    public string poolTag = "Enemy";

    [Header("Stats")]
    public float speed = 3f;
    public float maxHealth = 100f;

    [Header("Economy")]
    public int moneyReward = 10;
    public int damageToBase = 1;
}