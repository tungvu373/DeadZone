using UnityEngine;

public class Tower : MonoBehaviour
{
    [Header("Level")]
    public int level = 1;
    public int maxLevel = 3;

    public bool CanUpgrade()
    {
        return level < maxLevel;
    }

    public void Upgrade()
    {
        if (!CanUpgrade()) return;
        level++;

        // Placeholder: phóng to để thấy sự khác biệt
        // Phase 4-5 sẽ thay bằng tăng damage / range / tốc độ bắn thật
        transform.localScale *= 1.2f;
    }
}