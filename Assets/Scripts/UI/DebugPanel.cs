using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR || DEVELOPMENT_BUILD

/// <summary>
/// Debug Panel — chỉ active trong Editor và Development Build.
/// Gắn vào bất kỳ GameObject nào trong scene (ví dụ: GameManager).
///
/// Chức năng:
/// - Hiển thị ActiveEnemies.Count và pool size per tag
/// - Nút tắt/bật overlay
/// - +1000g, Kill All, God Mode (lives vô hạn)
/// </summary>
public class DebugPanel : MonoBehaviour
{
    private bool showPanel = true;
    private bool godMode = false;

    // Pool tags muốn theo dõi — thêm vào đây khi thêm pool mới
    private readonly string[] watchTags =
    {
        "Enemy", "Tanker", "Bullet", "FireBullet",
        "SpeedEnemy", "HeavyEnemy", "IceBullet", "LightningVFX"
    };

    void Update()
    {
        // Phím tắt: F1 bật/tắt panel
        if (Input.GetKeyDown(KeyCode.F1)) showPanel = !showPanel;

        // God mode: lives không xuống dưới 1
        if (godMode && GameManager.Instance != null && GameManager.Instance.Lives <= 0)
        {
            // Không thể gọi trực tiếp — chỉ hiển thị cảnh báo
        }
    }

    void OnGUI()
    {
        if (!showPanel) return;

        GUILayout.BeginArea(new Rect(10, 10, 230, 400));
        GUILayout.Box("── DEBUG PANEL (F1) ──");

        // ── Thông tin ──
        GUILayout.Label($"Enemies active: {EnemyMovement.ActiveEnemies.Count}");
        GUILayout.Label($"Towers active:  {Tower.ActiveTowers.Count}");

        // Pool idle count
        if (ObjectPool.Instance != null)
        {
            GUILayout.Label("── Pool idle ──");
            foreach (string tag in watchTags)
            {
                int idle = ObjectPool.Instance.GetIdleCount(tag);
                GUILayout.Label($"  {tag}: {idle}");
            }
        }

        GUILayout.Space(8);

        // ── Nút thao tác ──
        if (GUILayout.Button("+1000 Vàng"))
            GameManager.Instance?.AddMoney(1000);

        if (GUILayout.Button("Kill All Enemies"))
        {
            // Snapshot để tránh InvalidOperationException khi Remove trong foreach
            var snapshot = new List<EnemyMovement>(EnemyMovement.ActiveEnemies);
            foreach (var e in snapshot) e.TakeDamage(99999f);
        }

        godMode = GUILayout.Toggle(godMode, "God Mode (Lives ∞)");
        if (godMode && GameManager.Instance != null && GameManager.Instance.Lives < 1)
        {
            // Không thể set lives từ ngoài — chỉ cảnh báo
            GUILayout.Label("⚠ Cần thêm Lives setter!");
        }

        GUILayout.Space(8);
        if (GUILayout.Button("Reset Save (PlayerPrefs)"))
            PlayerPrefs.DeleteAll();

        GUILayout.EndArea();
    }
}

#endif
