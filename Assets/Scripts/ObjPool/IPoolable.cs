/// <summary>
/// Contract bắt buộc cho mọi object dùng ObjectPool.
/// OnSpawnFromPool  → chạy TRƯỚC SetActive(true)  → reset state đặc thù pool.
/// OnReturnToPool   → chạy TRƯỚC SetActive(false) → hủy coroutine, tắt VFX.
///
/// KHÔNG Add/Remove ActiveEnemies ở đây — việc đó do OnEnable/OnDisable đảm nhiệm.
/// </summary>
public interface IPoolable
{
    void OnSpawnFromPool();
    void OnReturnToPool();
}
