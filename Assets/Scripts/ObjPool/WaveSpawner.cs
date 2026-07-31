using UnityEngine;
using System.Collections;
using TMPro;

public class WaveSpawner : MonoBehaviour
{
    // ──────────────────────── TRẠNG THÁI ────────────────────────
    public enum WaveState { WaitingToStart, Spawning, WaitingForClear, DelayBetweenWaves }
    public WaveState State { get; private set; } = WaveState.WaitingToStart;

    // ──────────────────────── INSPECTOR ─────────────────────────
    [Header("Setup")]
    public Transform spawnPoint;

    [Header("Wave Settings")]
    public int   totalWaves          = 5;
    public float delayBeforeFirst    = 3f;    // chờ trước wave 1
    public float delayBetweenWaves   = 10f;   // chờ sau khi quái chết hết
    public float spawnRate           = 0.5f;  // giãn cách spawn từng con

    [Header("UI")]
    public TextMeshProUGUI countdownText;     // (tuỳ chọn) hiển thị đếm ngược

    // ──────────────────────── NỘI BỘ ────────────────────────────
    private int   waveIndex   = 0;
    private float timer       = 0f;
    private bool  isSpawning  = false;

    // ─────────────────────────────────────────────────────────────
    void Start()
    {
        timer = delayBeforeFirst;
        State = WaveState.WaitingToStart;
        UpdateWaveUI();
    }

    void Update()
    {
        if (GameManager.Instance.IsGameOver) return;

        switch (State)
        {
            case WaveState.WaitingToStart:
                HandleWaiting(delayBeforeFirst, true);
                break;

            case WaveState.WaitingForClear:
                HandleWaitingForClear();
                break;

            case WaveState.DelayBetweenWaves:
                HandleDelay();
                break;

            // Spawning được quản lý bởi Coroutine — Update không cần xử lý
        }
    }

    // ────────── Chờ trước wave đầu tiên ──────────
    void HandleWaiting(float totalDelay, bool isFirst)
    {
        timer -= Time.deltaTime;
        ShowCountdown("Ready: ", timer);

        if (timer <= 0f)
        {
            HideCountdown();
            StartNextWave();
        }
    }

    // ────────── Chờ toàn bộ quái của wave này chết ──────────
    void HandleWaitingForClear()
    {
        if (isSpawning) return; // vẫn đang spawn → chưa kiểm tra

        if (EnemyMovement.ActiveEnemies.Count == 0)
        {
            // Quái sạch → kiểm tra thắng hoặc bắt đầu delay
            if (waveIndex >= totalWaves)
            {
                GameManager.Instance.Win();
            }
            else
            {
                timer = delayBetweenWaves;
                State = WaveState.DelayBetweenWaves;
            }
        }
    }

    // ────────── Delay 10 giây giữa các wave ──────────
    void HandleDelay()
    {
        timer -= Time.deltaTime;
        ShowCountdown($"Next Wave: ", timer);

        if (timer <= 0f)
        {
            HideCountdown();
            StartNextWave();
        }
    }

    // ────────── Bắt đầu wave kế tiếp ──────────
    void StartNextWave()
    {
        waveIndex++;
        State = WaveState.Spawning;
        GameManager.Instance.SetWaveText($"Wave: {waveIndex}/{totalWaves}");
        StartCoroutine(SpawnWave());
    }

    // ────────── Coroutine spawn từng con ──────────
    IEnumerator SpawnWave()
    {
        isSpawning = true;

        int enemyCount = waveIndex * 2 + 3;

        for (int i = 0; i < enemyCount; i++)
        {
            if (GameManager.Instance.IsGameOver) yield break;

            // Từ wave 2 trở đi: cứ 4 con thì con thứ 4 là Tanker
            bool isTanker = waveIndex >= 2 && (i % 4 == 3);
            string tag = isTanker ? "Tanker" : "Enemy";

            ObjectPool.Instance.SpawnFromPool(tag, spawnPoint.position, Quaternion.identity);
            yield return new WaitForSeconds(spawnRate);
        }

        isSpawning = false;
        State = WaveState.WaitingForClear; // spawn xong → chờ quái chết hết
    }

    // ────────── Helpers UI ──────────
    void ShowCountdown(string prefix, float time)
    {
        if (countdownText == null) return;
        countdownText.gameObject.SetActive(true);
        countdownText.text = $"{prefix}{Mathf.CeilToInt(time)}s";
    }

    void HideCountdown()
    {
        if (countdownText == null) return;
        countdownText.gameObject.SetActive(false);
    }

    void UpdateWaveUI()
    {
        GameManager.Instance.SetWaveText($"Wave: 0/{totalWaves}");
    }
}