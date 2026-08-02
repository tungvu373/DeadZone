using UnityEngine;

public class TankerMovement : EnemyMovement
{
    [Header("Shield Visual")]
    public GameObject shieldVisual;       // sprite khiên (child)

    [Header("Animation")]
    public Animator animator;             // ✅ kéo Animator vào Inspector

    [Header("Shield Timing")]
    public float shieldDuration = 2f;     // ✅ giơ khiên trong 2s
    public float shieldCooldown = 5f;     // ✅ khóa khiên trong 5s


    private TankerData TData => (TankerData)data;

    private Tower towerTarget;
    private float attackCountdown;
    private float searchCountdown;
    private const float searchInterval = 0.3f;

    private bool shieldUp;
    private float shieldTimer;             // đếm thời gian khi đang giơ khiên
    private float cooldownTimer;           // đếm thời gian khóa khiên

    // Animation state hiện tại (tránh gọi Play lặp lại)
    private string currentAnim = "";

    protected override void OnEnable()
    {
        base.OnEnable();

        towerTarget = null;
        attackCountdown = 0f;
        searchCountdown = 0f;

        // reset shield
        shieldTimer = 0f;
        cooldownTimer = 0f;
        SetShield(false);
    }

    protected override void Update()
    {
        // ----- Tìm tower trong tầm (định kỳ) -----
        if (towerTarget == null)
        {
            searchCountdown -= Time.deltaTime;
            if (searchCountdown <= 0f)
            {
                FindTowerInRange();
                searchCountdown = searchInterval;
            }
        }

        if (towerTarget != null)
        {
            // ----- Có tower → xử lý chu kỳ khiên / tấn công -----
            HandleShieldCycle();
        }
        else
        {
            // ----- Không có tower gần → đi tiếp như quái thường -----
            SetShield(false);
            shieldTimer = 0f;
            cooldownTimer = 0f;
            MoveAlongPath();
        }
    }

    void HandleShieldCycle()
    {
        if (shieldUp)
        {
            // ----- ĐANG GIƠ KHIÊN: không tấn công, đếm ngược 2s -----
            shieldTimer -= Time.deltaTime;
            if (shieldTimer <= 0f)
            {
                // hết thời gian giơ khiên → chuyển sang khóa khiên (cooldown)
                SetShield(false);
                cooldownTimer = shieldCooldown;
            }
        }
        else
        {
            // ----- ĐANG KHÓA KHIÊN: được phép tấn công -----
            AttackTower();

            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0f)
            {
                // hết cooldown → giơ khiên trở lại
                SetShield(true);
                shieldTimer = shieldDuration;
            }
        }
    }

    void FindTowerInRange()
    {
        float shortest = Mathf.Infinity;
        Tower nearest = null;

        foreach (Tower tower in Tower.ActiveTowers)
        {
            float dist = Vector2.Distance(transform.position, tower.transform.position);
            if (dist <= TData.attackRange && dist < shortest)
            {
                shortest = dist;
                nearest = tower;
            }
        }
        towerTarget = nearest;

        // ✅ vừa tìm thấy tower → bắt đầu bằng trạng thái GIƠ KHIÊN
        if (towerTarget != null && !shieldUp && cooldownTimer <= 0f && shieldTimer <= 0f)
        {
            SetShield(true);
            shieldTimer = shieldDuration;
        }
    }

    void AttackTower()
    {
        if (towerTarget == null) return;

        attackCountdown -= Time.deltaTime;
        if (attackCountdown <= 0f)
        {
            towerTarget.TakeDamage(TData.attackDamage);
            attackCountdown = 1f / TData.attackRate;
        }
    }

    // ✅ Override: đang giơ khiên → chặn bớt damage
    public override void TakeDamage(float amount)
    {
        if (shieldUp)
            amount *= (1f - TData.shieldBlockPercent);

        base.TakeDamage(amount);
    }

    void SetShield(bool up)
    {
        shieldUp = up;

        if (shieldVisual != null)
            shieldVisual.SetActive(up);

        // ✅ đổi animation
        if (up)
            PlayAnim("Tanker_khien");
        else
            PlayAnim("Tanker_Attack");
    }

    // Tránh gọi Play lặp lại mỗi frame
    void PlayAnim(string animName)
    {
        if (animator == null) return;
        if (currentAnim == animName) return;

        currentAnim = animName;
        animator.Play(animName);
    }

    // Vẽ tầm đánh trong Scene view
    void OnDrawGizmosSelected()
    {
        if (data is TankerData td)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, td.attackRange);
        }
    }
}