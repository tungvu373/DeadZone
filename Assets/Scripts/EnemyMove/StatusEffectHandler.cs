using UnityEngine;

/// <summary>
/// Quản lý mọi debuff tốc độ trên quái.
/// Nguyên tắc: speed gốc (EnemyData.speed) là bất biến — chỉ đọc, không ghi.
/// Mọi thứ thay đổi tốc độ đi qua SpeedMultiplier.
///
/// Quy tắc slow (đã chốt trong design):
///   - Strongest-wins: slow mạnh hơn thay thế slow yếu hơn.
///   - Slow yếu KHÔNG ghi đè slow mạnh dù dài hơn — đây là FEATURE, không phải bug.
///   - Global cap: không bao giờ slow quá 60% dù resistance và percent ra sao.
/// </summary>
public class StatusEffectHandler : MonoBehaviour
{
    // Cap toàn cục — ngăn degenerate strategy "xây toàn Ice Tower"
    private const float MaxSlowPercent = 0.6f;

    // Tint màu khi bị slow
    private static readonly Color SlowTintColor = new Color(0.5f, 0.83f, 1f); // #7FD4FF

    private SpriteRenderer spriteRend;
    private Color originalColor;

    private float slowTimer;
    public float CurrentSlowPercent { get; private set; }

    /// <summary>Nhân với speed gốc để ra tốc độ thực tế. 1.0 = không bị slow.</summary>
    public float SpeedMultiplier => 1f - CurrentSlowPercent;

    void Awake()
    {
        // Tìm SpriteRenderer trên chính object hoặc con đầu tiên
        spriteRend = GetComponentInChildren<SpriteRenderer>();
        if (spriteRend != null) originalColor = spriteRend.color;
    }

    /// <summary>
    /// Áp dụng slow. Gọi từ IceBullet khi trúng quái.
    /// </summary>
    /// <param name="percent">Phần trăm giảm tốc (0→1). Ví dụ: 0.4 = chậm 40%.</param>
    /// <param name="duration">Thời gian hiệu lực (giây).</param>
    /// <param name="resistance">Kháng slow của quái (0=không kháng, 1=miễn nhiễm).</param>
    public void ApplySlow(float percent, float duration, float resistance = 0f)
    {
        // Áp dụng kháng slow và global cap
        float effective = Mathf.Min(percent * (1f - resistance), MaxSlowPercent);

        if (effective <= 0f) return; // boss miễn nhiễm hoàn toàn

        // Strongest-wins: chỉ thay thế nếu slow mới mạnh hơn hoặc chưa bị slow
        if (effective >= CurrentSlowPercent)
        {
            CurrentSlowPercent = effective;
            slowTimer = duration;
            ApplyTint();
        }
        // else: slow yếu hơn → bỏ qua (không cộng dồn, không refresh)
    }

    void Update()
    {
        if (CurrentSlowPercent > 0f)
        {
            slowTimer -= Time.deltaTime;
            if (slowTimer <= 0f) ClearSlow();
        }
    }

    /// <summary>Gọi khi quái về pool hoặc reset — PHẢI clear tint để không dính sang lần spawn tiếp.</summary>
    public void ResetAll()
    {
        CurrentSlowPercent = 0f;
        slowTimer = 0f;
        ClearTint();
    }

    // ────────────────────────── PRIVATE ──────────────────────────────

    void ClearSlow()
    {
        CurrentSlowPercent = 0f;
        ClearTint();
    }

    void ApplyTint()
    {
        if (spriteRend != null) spriteRend.color = SlowTintColor;
    }

    void ClearTint()
    {
        if (spriteRend != null) spriteRend.color = originalColor;
    }
}
