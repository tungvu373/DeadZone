using UnityEngine;

public class FireTower : Tower
{
    [Header("Fire Tower")]
    public float splashRadius = 1.8f;
    public float burnDamage = 5f;
    public float burnDuration = 3f;

    protected override void Shoot()
    {
        GameObject bullet = ObjectPool.Instance.SpawnFromPool(
            "FireBullet",
            firePoint.position,
            firePoint.rotation);

        if (bullet != null)
        {
            bullet.GetComponent<FireBullet>()
                  .Seek(target, damage);
        }
    }
}