using UnityEngine;
using System;
using System.Collections.Generic;

public class EnemyMovement : MonoBehaviour, IPoolable
{
    // ─────────────────────────── DATA ────────────────────────────────
    [Header("Data")]
    public EnemyData data;

    // ─────────────────────── ACTIVE LIST ─────────────────────────────
    /// <summary>
    /// Danh sách quái đang hoạt động. Add/Remove qua OnEnable/OnDisable.
    /// QUAN TRỌNG: khi iterate và có thể deal damage (kill), phải dùng .ToList() trước.
    /// </summary>
    public static List<EnemyMovement> ActiveEnemies = new List<EnemyMovement>();

    // ─────────────────────────── STATE ───────────────────────────────
    protected float health;
    protected Transform target;
    protected int waypointIndex;
    protected bool isReturned;

    /// <summary>
    /// Slow resistance: 0 = không kháng, 1 = miễn nhiễm hoàn toàn.
    /// Boss sẽ set = 0.7f trong OnEnable override.
    /// </summary>
    [HideInInspector] public float slowResistance = 0f;

    /// <summary>
    /// Multiplier tốc độ theo phase (Boss phase 2 set = 1.5f).
    /// data.speed bất biến — không bao giờ ghi đè field này.
    /// </summary>
    protected float phaseMultiplier = 1f;

    // ─────────────────── GENERATION STAMP ────────────────────────────
    /// <summary>
    /// Tăng mỗi lần OnEnable. Bullet so sánh giá trị này để phát hiện
    /// "quái tái chế từ pool" — tránh đuổi theo quái mới chưa bị nhắm.
    /// </summary>
    public int SpawnGeneration { get; private set; }

    // ──────────────────────── COMPONENTS ─────────────────────────────
    [Header("UI")]
    public HealthBar healthBar;

    [SerializeField] protected StatusEffectHandler statusHandler;

    // ─────────────────── EVENT (cho BossHealthBar) ────────────────────
    /// <summary>Raise trong TakeDamage. BossHealthBar subscribe vào đây.</summary>
    public event Action<float, float> OnHealthChanged;

    // ─────────────────────── LIFECYCLE ───────────────────────────────

    /// <summary>
    /// Chạy khi object được kích hoạt — cả pool (SetActive) lẫn Instantiate/Destroy (Boss).
    /// Đây là lifecycle chính. Không dùng Start() ở lớp derived.
    /// </summary>
    protected virtual void OnEnable()
    {
        if (data == null)
        {
            Debug.LogError($"[EnemyMovement] Chưa gán EnemyData vào '{gameObject.name}'!", gameObject);
            return;
        }

        InitStats();
        ActiveEnemies.Add(this);
        healthBar?.SetHealth(health, data.maxHealth);
    }

    /// <summary>
    /// Chạy khi object bị tắt — cả pool (SetActive) lẫn Destroy (Boss, scene unload).
    /// Tự động dọn ActiveEnemies kể cả khi scene unload.
    /// </summary>
    protected virtual void OnDisable()
    {
        ActiveEnemies.Remove(this);
        StopAllCoroutines();
        statusHandler?.ResetAll();
    }

    // ──────────────────── IPOOL ABLE ─────────────────────────────────

    /// <summary>
    /// Chạy TRƯỚC OnEnable (ObjectPool gọi trước SetActive).
    /// Chỉ reset state đặc thù pool. KHÔNG Add ActiveEnemies.
    /// KHÔNG gán position (WaveSpawner/caller truyền qua SpawnFromPool).
    /// </summary>
    public virtual void OnSpawnFromPool()
    {
        isReturned = false;
    }

    /// <summary>
    /// Chạy TRƯỚC OnDisable (ObjectPool gọi trước SetActive false).
    /// Explicit cleanup — OnDisable cũng lo nhưng ghi rõ cho clarity.
    /// </summary>
    public virtual void OnReturnToPool()
    {
        StopAllCoroutines();
        statusHandler?.ResetAll();
    }

    // ─────────────────────────── UPDATE ──────────────────────────────

    protected virtual void Update()
    {
        MoveAlongPath();
    }

    // ────────────────────────── MOVEMENT ─────────────────────────────

    /// <summary>
    /// Di chuyển dọc theo waypoint. virtual để FlyingEnemy override.
    /// Speed thực tế = data.speed × phaseMultiplier × statusHandler.SpeedMultiplier.
    /// </summary>
    protected virtual void MoveAlongPath()
    {
        if (target == null) return;

        float spd = data.speed * phaseMultiplier * (statusHandler != null ? statusHandler.SpeedMultiplier : 1f);
        transform.position = Vector3.MoveTowards(
            transform.position, target.position, spd * Time.deltaTime);

        if (Vector3.Distance(transform.position, target.position) <= 0.2f)
            GetNextWaypoint();
    }

    void GetNextWaypoint()
    {
        if (waypointIndex >= Waypoints.points.Length - 1)
        {
            ReachEnd();
            return;
        }
        waypointIndex++;
        target = Waypoints.points[waypointIndex];
    }

    void ReachEnd()
    {
        GameManager.Instance.TakeDamage(data.damageToBase);
        ReturnSelf();
    }

    // ────────────────────────── DAMAGE ───────────────────────────────

    public virtual void TakeDamage(float amount)
    {
        health -= amount;
        healthBar?.SetHealth(health, data.maxHealth);
        OnHealthChanged?.Invoke(health, data.maxHealth);

        if (health <= 0)
        {
            GameManager.Instance.AddMoney(data.moneyReward);
            ReturnSelf();
        }
    }

    // ────────────────────────── API ──────────────────────────────────

    /// <summary>
    /// Spawn minion boss tại đúng vị trí trên path — không về waypoint 0.
    /// </summary>
    public void InitAtWaypoint(int index, Vector3 spawnPos)
    {
        waypointIndex = Mathf.Clamp(index, 0, Waypoints.points.Length - 1);
        target = Waypoints.points[waypointIndex];
        transform.position = spawnPos; // nguồn ghi position duy nhất cho minion
    }

    /// <summary>
    /// Tiến độ trên path, normalize về [0, 1].
    /// Dùng cho tower targeting "quái đi xa nhất".
    /// FlyingEnemy override với công thức đường thẳng.
    /// </summary>
    public virtual float PathProgress01
    {
        get
        {
            if (Waypoints.points == null || Waypoints.points.Length <= 1) return 0f;

            int total = Waypoints.points.Length;
            // waypointIndex = index waypoint ĐANG HƯỚNG TỚI
            // from = waypoint vừa qua, to = waypoint đang hướng đến
            int from = Mathf.Max(0, waypointIndex - 1);
            int to   = Mathf.Min(waypointIndex, total - 1);

            float segLen = Vector3.Distance(
                Waypoints.points[from].position,
                Waypoints.points[to].position);

            float frac = segLen > 0f
                ? Mathf.Clamp01(1f - Vector3.Distance(transform.position,
                    Waypoints.points[to].position) / segLen)
                : 1f;

            return (from + frac) / (total - 1);
        }
    }

    // ────────────────────────── PRIVATE ──────────────────────────────

    void InitStats()
    {
        health = data.maxHealth;
        waypointIndex = 0;
        isReturned = false;
        phaseMultiplier = 1f;
        slowResistance = 0f;
        SpawnGeneration++;  // tăng mỗi lần spawn — bullet dùng để phát hiện tái chế

        // Waypoint 0 là điểm xuất phát
        if (Waypoints.points != null && Waypoints.points.Length > 0)
        {
            target = Waypoints.points[0];
            // KHÔNG gán position ở đây — WaveSpawner truyền position qua SpawnFromPool
        }
    }

    protected void ReturnSelf()
    {
        if (isReturned) return;
        isReturned = true;
        ObjectPool.Instance.ReturnToPool(data.poolTag, gameObject);
    }

    // ─────────────────────────── GIZMOS ──────────────────────────────

    void OnDrawGizmosSelected()
    {
        // Hiển thị PathProgress trên Scene view khi chọn object
        // (DebugPanel sẽ render text lên đầu quái trong PlayMode)
    }
}