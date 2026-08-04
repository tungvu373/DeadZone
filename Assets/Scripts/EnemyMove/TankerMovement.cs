using UnityEngine;

public class TankerMovement : EnemyMovement
{
    [Header("Shield Visual")]
    public GameObject shieldVisual;

    [Header("Animation")]
    public Animator animator;

    [Header("Shield Timing")]
    public float shieldDuration = 2f;
    public float shieldCooldown = 5f;

    private TankerData TData => (TankerData)data;

    private Tower towerTarget;
    private float attackCountdown;
    private float searchCountdown;
    private const float searchInterval = 0.3f;

    /// <summary>
    /// Expose để LightningTower kiểm tra — chain dừng tại Tanker đang giơ khiên.
    /// </summary>
    public bool ShieldUp { get; private set; }

    private float shieldTimer;
    private float cooldownTimer;
    private string currentAnim = "";

    // ─────────────────── IPOOL ABLE override ─────────────────────────

    public override void OnSpawnFromPool()
    {
        base.OnSpawnFromPool();
        towerTarget = null;
        attackCountdown = 0f;
        searchCountdown = 0f;
        shieldTimer = 0f;
        cooldownTimer = 0f;
        SetShield(false);
        currentAnim = "";
    }

    public override void OnReturnToPool()
    {
        base.OnReturnToPool();
        towerTarget = null;
        SetShield(false);
    }

    // ─────────────────────── LIFECYCLE ───────────────────────────────

    protected override void OnEnable()
    {
        base.OnEnable();
        // State reset bổ sung (base đã gọi InitStats + Add ActiveEnemies)
        towerTarget = null;
        attackCountdown = 0f;
        searchCountdown = 0f;
        shieldTimer = 0f;
        cooldownTimer = 0f;
        SetShield(false);
        currentAnim = "";
    }

    protected override void OnDisable()
    {
        SetShield(false);
        towerTarget = null;
        base.OnDisable();
    }

    // ─────────────────────────── UPDATE ──────────────────────────────

    protected override void Update()
    {
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
            HandleShieldCycle();
        else
        {
            SetShield(false);
            shieldTimer = 0f;
            cooldownTimer = 0f;
            MoveAlongPath();
        }
    }

    // ─────────────────────────── SHIELD ──────────────────────────────

    void HandleShieldCycle()
    {
        if (ShieldUp)
        {
            shieldTimer -= Time.deltaTime;
            if (shieldTimer <= 0f)
            {
                SetShield(false);
                cooldownTimer = shieldCooldown;
            }
        }
        else
        {
            AttackTower();
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0f)
            {
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

        if (towerTarget != null && !ShieldUp && cooldownTimer <= 0f && shieldTimer <= 0f)
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

    public override void TakeDamage(float amount)
    {
        if (ShieldUp) amount *= (1f - TData.shieldBlockPercent);
        base.TakeDamage(amount);
    }

    void SetShield(bool up)
    {
        ShieldUp = up;
        if (shieldVisual != null) shieldVisual.SetActive(up);
        PlayAnim(up ? "Tanker_khien" : "Tanker_Attack");
    }

    void PlayAnim(string animName)
    {
        if (animator == null) return;
        if (!gameObject.activeInHierarchy) return; // ← guard: không Play khi object inactive
        if (currentAnim == animName) return;
        currentAnim = animName;
        animator.Play(animName);
    }

    void OnDrawGizmosSelected()
    {
        if (data is TankerData td)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, td.attackRange);
        }
    }
}