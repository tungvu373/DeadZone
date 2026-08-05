using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Lightning Tower — bắn sét dây chuyền (chain lightning).
///
/// Quy tắc chain (đã chốt trong Interaction Matrix):
///   - Chain nhảy đến quái gần nhất chưa bị hit trong chainRadius.
///   - Tanker đang giơ khiên (ShieldUp) = target HỢP LỆ → chain HIT và DỪNG tại đó.
///   - Boss: nhận damage nhưng không là SOURCE của chain (handled bởi hitSet).
///   - Damage giảm theo chainFalloff mỗi bước: full → ×0.7 → ×0.49 → ...
///
/// Mage enemy đánh được tower này qua TakeDamage() kế thừa từ Tower base.
/// </summary>
public class LightningTower : Tower
{
    [Header("Chain Settings")]
    [Min(1)]
    [Tooltip("Tổng số mục tiêu bị đánh (bao gồm cả mục tiêu đầu tiên).")]
    public int maxChains = 3;

    [Min(0.1f)]
    [Tooltip("Bán kính tìm mục tiêu tiếp theo từ mục tiêu hiện tại (unit).")]
    public float chainRadius = 2.5f;

    [Range(0f, 1f)]
    [Tooltip("Hệ số giảm damage mỗi bước chain. 0.7 = giảm 30% mỗi lần.")]
    public float chainFalloff = 0.7f;

    // Cache HashSet — không new mỗi phát bắn, không GC alloc
    private readonly HashSet<EnemyMovement> hitSet = new HashSet<EnemyMovement>();

    // ─────────────────────────── SHOOT ───────────────────────────────

    protected override void Shoot()
    {
        if (target == null) return;

        hitSet.Clear();

        // Phát đầu tiên — đánh mục tiêu chính của tower
        hitSet.Add(target);
        target.TakeDamage(damage);
        Debug.Log($"[Lightning] Hit: {target.name} | DMG: {damage:F1}");

        EnemyMovement current = target;
        float currentDmg = damage;

        // Chain tối đa maxChains - 1 lần nhảy tiếp
        for (int i = 1; i < maxChains; i++)
        {
            EnemyMovement next = FindChainTarget(current.transform.position);
            if (next == null) break; // không còn quái trong tầm → dừng

            currentDmg *= chainFalloff;

            // Spawn VFX đường chớp
            SpawnChainVFX(current.transform.position, next.transform.position);

            // ── INTERACTION MATRIX: Tanker shield = firewall ──
            // Tanker là target hợp lệ → hit → chain DỪNG tại đây
            if (next is TankerMovement tanker && tanker.ShieldUp)
            {
                next.TakeDamage(currentDmg);
                Debug.Log($"[Lightning] Chain DỪNG tại Tanker shield | DMG: {currentDmg:F1}");
                break; // không nhảy sang quái phía sau
            }

            next.TakeDamage(currentDmg);
            hitSet.Add(next);
            Debug.Log($"[Lightning] Chain [{i}]: {next.name} | DMG: {currentDmg:F1}");
            current = next;
        }
    }

    // ─────────────────────────── PRIVATE ─────────────────────────────

    /// <summary>
    /// Tìm mục tiêu chain tiếp theo — quái gần nhất trong chainRadius chưa bị hit.
    /// Không alloc: iterate ActiveEnemies trực tiếp (không .ToList()).
    /// Không deal damage trong hàm này — chỉ tìm kiếm.
    /// </summary>
    EnemyMovement FindChainTarget(Vector3 from)
    {
        EnemyMovement best = null;
        float bestDist = chainRadius;

        foreach (EnemyMovement enemy in EnemyMovement.ActiveEnemies)
        {
            if (hitSet.Contains(enemy)) continue; // đã bị hit → bỏ qua

            float dist = Vector2.Distance(from, enemy.transform.position);
            if (dist < bestDist)
            {
                bestDist = dist;
                best = enemy;
            }
        }
        return best;
    }

    void SpawnChainVFX(Vector3 from, Vector3 to)
    {
        GameObject vfxObj = ObjectPool.Instance.SpawnFromPool(
            "LightningVFX", from, Quaternion.identity);

        if (vfxObj != null)
        {
            LightningVFX vfx = vfxObj.GetComponent<LightningVFX>();
            vfx?.Setup(from, to);
        }
    }

    // ─────────────────────────── GIZMOS ──────────────────────────────

    void OnDrawGizmosSelected()
    {
        // Chain radius từ tower (vẽ dựa vào mục tiêu đầu tiên)
        Gizmos.color = Color.yellow;
        if (target != null)
            Gizmos.DrawWireSphere(target.transform.position, chainRadius);
    }
}
