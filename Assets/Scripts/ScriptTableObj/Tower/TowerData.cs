using UnityEngine;

[System.Serializable]
public class TowerLevelStats
{
    public float damage = 25f;
    public float range = 3f;
    public float fireRate = 1f;
    public int upgradeCost = 50;     // giá để NÂNG LÊN level này (level 1 không dùng)
}

[CreateAssetMenu(fileName = "TowerData", menuName = "TD/Tower Data")]
public class TowerData : ScriptableObject
{
    [Header("Info")]
    public string towerName = "Archer Tower";

    [Header("Economy")]
    public int buildCost = 100;
    [Range(0f, 1f)]
    public float sellRefundPercent = 0.7f;   // bán hoàn lại 70% tổng tiền đã đầu tư

    [Header("Levels (phần tử 0 = level 1)")]
    public TowerLevelStats[] levels;

    public int MaxLevel => levels.Length;
}