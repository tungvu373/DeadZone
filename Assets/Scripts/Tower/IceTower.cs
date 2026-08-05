using UnityEngine;

/// <summary>
/// Ice Tower — bắn đạn lạnh làm chậm quái.
/// Kế thừa Tower (HP, targeting, upgrade, sell đều hoạt động tự động).
/// Mage enemy vẫn tấn công được tower này qua TakeDamage() từ Tower base.
/// </summary>
public class IceTower : Tower
{
    [Header("Ice Settings")]
    [Range(0f, 0.6f)]
    [Tooltip("Phần trăm giảm tốc (0→0.6). Global cap trong StatusEffectHandler = 60%.")]
    public float slowPercent = 0.4f;

    [Min(0.1f)]
    [Tooltip("Thời gian slow (giây).")]
    public float slowDuration = 2f;

    /// <summary>
    /// Override Shoot() của Tower base — bắn IceBullet thay vì Bullet thường.
    /// Tất cả logic khác (FindTarget, fireRate, upgrade, sell) giữ nguyên từ base.
    /// </summary>
    protected override void Shoot()
    {
        if (target == null) return;

        GameObject bulletObj = ObjectPool.Instance.SpawnFromPool(
            "IceBullet", firePoint.position, firePoint.rotation);

        if (bulletObj != null)
        {
            IceBullet iceBullet = bulletObj.GetComponent<IceBullet>();
            if (iceBullet != null)
                iceBullet.Init(target, damage, slowPercent, slowDuration);
        }
    }
}
