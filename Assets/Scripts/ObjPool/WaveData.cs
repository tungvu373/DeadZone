using UnityEngine;
using System.Collections.Generic;

// ─────────────────────────────────────────────────────────────────────────────
// Một entry trong wave: loại quái + số lượng + timing
// ─────────────────────────────────────────────────────────────────────────────
[System.Serializable]
public class EnemySpawnEntry
{
    [Tooltip("Kéo EnemyData asset vào đây — không cần gõ tag, không typo.")]
    public EnemyData enemyData;

    [Min(1)]
    [Tooltip("Số lượng quái loại này trong entry.")]
    public int count = 5;

    [Min(0f)]
    [Tooltip("Thời gian giữa mỗi lần spawn trong entry này (giây).")]
    public float spawnInterval = 0.8f;

    [Min(0f)]
    [Tooltip("Nghỉ trước khi entry này bắt đầu spawn (giây). Dùng để tạo khoảng cách giữa các nhóm.")]
    public float delayBefore = 0f;
}

// ─────────────────────────────────────────────────────────────────────────────
// Thứ tự spawn các entry trong 1 wave
// ─────────────────────────────────────────────────────────────────────────────
public enum SpawnMode
{
    /// <summary>Spawn hết entry 1 rồi mới qua entry 2.</summary>
    Sequential,

    /// <summary>Xen kẽ: 1 quái entry 1 → 1 quái entry 2 → ... lặp lại.</summary>
    Interleaved
}

// ─────────────────────────────────────────────────────────────────────────────
// WaveData — ScriptableObject cấu hình 1 wave
// Tạo từ menu: Create → DeadZone → Wave Data
// ─────────────────────────────────────────────────────────────────────────────
[CreateAssetMenu(fileName = "WaveData", menuName = "DeadZone/Wave Data")]
public class WaveData : ScriptableObject
{
    [Tooltip("Tên hiển thị cho dễ nhận ra trong Inspector.")]
    public string waveName = "Wave";

    [Tooltip("Danh sách các nhóm quái trong wave này.")]
    public List<EnemySpawnEntry> entries = new List<EnemySpawnEntry>();

    [Tooltip("Sequential: spawn hết entry 1 rồi entry 2.\nInterleaved: xen kẽ từng con.")]
    public SpawnMode mode = SpawnMode.Sequential;

    [Tooltip("Boss xuất hiện cuối wave này. Để trống nếu không có boss.")]
    public EnemyData bossData;   // dùng EnemyData thông thường cho đến khi BossData được tạo (Bước 6)

    [Min(0)]
    [Tooltip("Tiền thưởng thêm khi clear wave này (ngoài tiền từ giết quái).")]
    public int bonusMoneyOnClear = 0;

    /// <summary>Tổng số quái của wave này (không tính boss).</summary>
    public int TotalEnemyCount()
    {
        int total = 0;
        foreach (var entry in entries)
            if (entry.enemyData != null) total += entry.count;
        return total;
    }
}
