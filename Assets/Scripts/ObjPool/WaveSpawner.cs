using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class WaveSpawner : MonoBehaviour
{
    // ─────────────────────────── TRẠNG THÁI ──────────────────────────
    public enum WaveState
    {
        WaitingToStart,
        Spawning,
        WaitingForClear,
        DelayBetweenWaves
    }

    public WaveState State { get; private set; } = WaveState.WaitingToStart;

    // ───────────────────────────── SETUP ─────────────────────────────
    [Header("Setup")]
    public Transform spawnPoint;

    // ──────────────────────── WAVE SETTINGS ──────────────────────────
    [Header("Wave Data")]
    [Tooltip("Kéo các WaveData asset vào đây theo thứ tự.")]
    public WaveData[] waves;

    [Header("Time Settings")]
    [Min(0f)] public float delayBeforeFirst   = 15f;
    [Min(0f)] public float delayBetweenWaves  = 20f;

    // ─────────────────────────────── UI ──────────────────────────────
    [Header("Countdown UI")]
    public TextMeshProUGUI countdownText;
    public Button          readyButton;
    public TextMeshProUGUI readyButtonText;

    // ──────────────────────────── NỘI BỘ ────────────────────────────
    private int   waveIndex;       // index wave hiện tại (0-based → dùng waves[waveIndex])
    private float timer;
    private bool  spawnFinished;
    private Coroutine spawnCoroutine;

    public int TotalWaves => waves != null ? waves.Length : 0;

    // ─────────────────────────── AWAKE ───────────────────────────────
    private void Awake()
    {
        if (readyButton != null)
        {
            readyButton.onClick.RemoveListener(SkipCountdown);
            readyButton.onClick.AddListener(SkipCountdown);
        }
    }

    // ──────────────────────────── START ──────────────────────────────
    private void Start()
    {
        if (spawnPoint == null)
        {
            Debug.LogError("WaveSpawner: Chưa gán Spawn Point."); enabled = false; return;
        }
        if (TotalWaves <= 0)
        {
            Debug.LogError("WaveSpawner: Chưa có WaveData nào."); enabled = false; return;
        }

        waveIndex    = 0;
        timer        = delayBeforeFirst;
        spawnFinished = false;
        State        = WaveState.WaitingToStart;

        UpdateWaveUI();
        ShowWaitingUI("Ready: ");
    }

    // ─────────────────────────── UPDATE ──────────────────────────────
    private void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.IsGameOver)
        {
            HideWaitingUI(); return;
        }

        switch (State)
        {
            case WaveState.WaitingToStart:    HandleWaitingBeforeFirst();  break;
            case WaveState.Spawning:          /* Coroutine quản lý */      break;
            case WaveState.WaitingForClear:   HandleWaitingForClear();     break;
            case WaveState.DelayBetweenWaves: HandleDelayBetweenWaves();   break;
        }
    }

    // ──────────────── CHỜ TRƯỚC WAVE ĐẦU TIÊN ───────────────────────
    private void HandleWaitingBeforeFirst()
    {
        timer -= Time.deltaTime;
        UpdateWaitingUI("Ready: ", "Bắt đầu Wave 1");
        if (timer <= 0f) StartNextWave();
    }

    // ────────────── CHỜ QUÁI BỊ TIÊU DIỆT HẾT ──────────────────────
    private void HandleWaitingForClear()
    {
        // Điều kiện: spawn xong VÀ không còn quái nào sống
        // Minion boss cũng nằm trong ActiveEnemies → tự đếm đúng
        if (!spawnFinished) return;
        if (EnemyMovement.ActiveEnemies.Count > 0) return;

        // Cộng bonus money cuối wave
        WaveData finishedWave = waves[waveIndex - 1];
        if (finishedWave.bonusMoneyOnClear > 0)
            GameManager.Instance.AddMoney(finishedWave.bonusMoneyOnClear);

        // Kiểm tra đã hết tất cả wave chưa
        if (waveIndex >= TotalWaves)
        {
            HideWaitingUI();
            GameManager.Instance.Win();
            return;
        }

        // Bắt đầu đếm ngược cho wave tiếp theo
        timer = delayBetweenWaves;
        State = WaveState.DelayBetweenWaves;
        ShowWaitingUI("Next Wave: ");
    }

    // ──────────────── CHỜ GIỮA CÁC WAVE ────────────────────────────
    private void HandleDelayBetweenWaves()
    {
        timer -= Time.deltaTime;
        int nextWave = waveIndex + 1;
        UpdateWaitingUI("Next Wave: ", $"Bắt đầu Wave {nextWave}");
        if (timer <= 0f) StartNextWave();
    }

    // ──────────────── NÚT BỎ QUA ĐẾM NGƯỢC ─────────────────────────
    public void SkipCountdown()
    {
        if (GameManager.Instance == null || GameManager.Instance.IsGameOver) return;

        bool canSkip = State == WaveState.WaitingToStart
                    || State == WaveState.DelayBetweenWaves;
        if (!canSkip) return;

        timer = 0f;
        StartNextWave();
    }

    // ──────────────────── BẮT ĐẦU WAVE ─────────────────────────────
    private void StartNextWave()
    {
        if (State == WaveState.Spawning || State == WaveState.WaitingForClear) return;
        if (waveIndex >= TotalWaves)
        {
            HideWaitingUI(); GameManager.Instance.Win(); return;
        }

        HideWaitingUI();

        WaveData currentWave = waves[waveIndex];
        waveIndex++;

        GameManager.Instance.SetWaveText($"Wave: {waveIndex}/{TotalWaves}");
        State = WaveState.Spawning;
        spawnFinished = false;

        if (spawnCoroutine != null) StopCoroutine(spawnCoroutine);
        spawnCoroutine = StartCoroutine(SpawnWave(currentWave));
    }

    // ──────────────────── SPAWN QUÁI ────────────────────────────────
    private IEnumerator SpawnWave(WaveData wave)
    {
        if (wave.mode == SpawnMode.Sequential)
            yield return StartCoroutine(SpawnSequential(wave));
        else
            yield return StartCoroutine(SpawnInterleaved(wave));

        spawnFinished = true;
        spawnCoroutine = null;
        State = WaveState.WaitingForClear;
    }

    // Spawn hết entry 1 → entry 2 → ...
    private IEnumerator SpawnSequential(WaveData wave)
    {
        foreach (var entry in wave.entries)
        {
            if (!IsValidEntry(entry)) continue;

            if (entry.delayBefore > 0f)
                yield return new WaitForSeconds(entry.delayBefore);

            for (int i = 0; i < entry.count; i++)
            {
                if (GameManager.Instance.IsGameOver) yield break;

                SpawnEnemy(entry.enemyData.poolTag);

                if (i < entry.count - 1 && entry.spawnInterval > 0f)
                    yield return new WaitForSeconds(entry.spawnInterval);
            }
        }
    }

    // Xen kẽ: 1 quái entry[0], 1 quái entry[1], ...
    private IEnumerator SpawnInterleaved(WaveData wave)
    {
        // Tính số lượng từng entry
        List<int> remaining = new List<int>();
        foreach (var entry in wave.entries)
            remaining.Add(IsValidEntry(entry) ? entry.count : 0);

        float interval = 0.8f;
        if (wave.entries.Count > 0) interval = wave.entries[0].spawnInterval;

        bool anyLeft = true;
        while (anyLeft)
        {
            anyLeft = false;
            for (int e = 0; e < wave.entries.Count; e++)
            {
                if (remaining[e] <= 0) continue;
                anyLeft = true;

                if (GameManager.Instance.IsGameOver) yield break;

                SpawnEnemy(wave.entries[e].enemyData.poolTag);
                remaining[e]--;

                if (interval > 0f)
                    yield return new WaitForSeconds(interval);
            }
        }
    }

    // Spawn 1 quái từ pool theo tag
    private void SpawnEnemy(string poolTag)
    {
        ObjectPool.Instance.SpawnFromPool(poolTag, spawnPoint.position, Quaternion.identity);
    }

    private bool IsValidEntry(EnemySpawnEntry entry)
    {
        if (entry == null || entry.enemyData == null)
        {
            Debug.LogWarning("WaveSpawner: EnemySpawnEntry thiếu EnemyData — bỏ qua.");
            return false;
        }
        return entry.count > 0;
    }

    // ───────────────────────────── UI ────────────────────────────────
    private void ShowWaitingUI(string prefix)
    {
        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(true);
            countdownText.text = $"{prefix}{Mathf.Max(0, Mathf.CeilToInt(timer))}s";
        }
        if (readyButton != null)
        {
            readyButton.gameObject.SetActive(true);
            readyButton.interactable = true;
        }
        UpdateReadyButtonText();
    }

    private void UpdateWaitingUI(string prefix, string buttonContent)
    {
        int seconds = Mathf.Max(0, Mathf.CeilToInt(timer));

        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(true);
            countdownText.text = $"{prefix}{seconds}s";
        }
        if (readyButton != null)
        {
            readyButton.gameObject.SetActive(true);
            readyButton.interactable = true;
        }
        if (readyButtonText != null)
            readyButtonText.text = $"{buttonContent} ngay ({seconds}s)";
    }

    private void UpdateReadyButtonText()
    {
        if (readyButtonText == null) return;
        int nextWave = Mathf.Clamp(waveIndex + 1, 1, TotalWaves);
        int seconds  = Mathf.Max(0, Mathf.CeilToInt(timer));
        readyButtonText.text = $"Bắt đầu Wave {nextWave} ngay ({seconds}s)";
    }

    private void HideWaitingUI()
    {
        if (countdownText != null) countdownText.gameObject.SetActive(false);
        if (readyButton   != null) readyButton.gameObject.SetActive(false);
    }

    private void UpdateWaveUI()
    {
        GameManager.Instance?.SetWaveText($"Wave: 0/{TotalWaves}");
    }

    private void OnDestroy()
    {
        if (readyButton != null)
            readyButton.onClick.RemoveListener(SkipCountdown);
    }
}