using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class WaveSpawner : MonoBehaviour
{
    // ───────────────────── CẤU HÌNH TỪNG WAVE ─────────────────────
    [System.Serializable]
    public class WaveSettings
    {
        [Min(0)]
        [Tooltip("Tổng số quái trong wave này.")]
        public int enemyCount = 5;
    }

    // ───────────────────────── TRẠNG THÁI ─────────────────────────
    public enum WaveState
    {
        WaitingToStart,
        Spawning,
        WaitingForClear,
        DelayBetweenWaves
    }

    public WaveState State { get; private set; }
        = WaveState.WaitingToStart;

    // ─────────────────────────── SETUP ─────────────────────────────
    [Header("Setup")]
    public Transform spawnPoint;

    // ─────────────────────── WAVE SETTINGS ─────────────────────────
    [Header("Wave Settings")]
    public WaveSettings[] waves =
    {
        new WaveSettings { enemyCount = 5 },
        new WaveSettings { enemyCount = 8 },
        new WaveSettings { enemyCount = 12 },
        new WaveSettings { enemyCount = 16 },
        new WaveSettings { enemyCount = 20 }
    };

    [Header("Time Settings")]
    [Min(0f)]
    [Tooltip("Thời gian chuẩn bị trước wave đầu tiên.")]
    public float delayBeforeFirst = 15f;

    [Min(0f)]
    [Tooltip("Thời gian chuẩn bị giữa các wave.")]
    public float delayBetweenWaves = 20f;

    [Min(0f)]
    [Tooltip("Khoảng cách thời gian giữa mỗi lần spawn quái.")]
    public float spawnRate = 0.5f;

    // ───────────────────────────── UI ──────────────────────────────
    [Header("Countdown UI")]
    public TextMeshProUGUI countdownText;

    [Tooltip("Nút dùng để bỏ qua đếm ngược và bắt đầu wave ngay.")]
    public Button readyButton;

    [Tooltip("Text nằm bên trong nút Ready.")]
    public TextMeshProUGUI readyButtonText;

    // ─────────────────────────── NỘI BỘ ────────────────────────────
    private int waveIndex;
    private float timer;
    private bool isSpawning;
    private Coroutine spawnCoroutine;

    // Số wave tự động lấy từ kích thước mảng waves.
    public int TotalWaves => waves != null ? waves.Length : 0;

    // ──────────────────────────────────────────────────────────────
    private void Awake()
    {
        // Tự động đăng ký sự kiện cho nút.
        if (readyButton != null)
        {
            readyButton.onClick.RemoveListener(SkipCountdown);
            readyButton.onClick.AddListener(SkipCountdown);
        }
    }

    private void Start()
    {
        if (spawnPoint == null)
        {
            Debug.LogError("WaveSpawner: Chưa gán Spawn Point.");
            enabled = false;
            return;
        }

        if (TotalWaves <= 0)
        {
            Debug.LogError("WaveSpawner: Chưa thiết lập wave nào.");
            enabled = false;
            return;
        }

        waveIndex = 0;
        timer = delayBeforeFirst;
        isSpawning = false;

        State = WaveState.WaitingToStart;

        UpdateWaveUI();
        ShowWaitingUI("Ready: ");

        /*
         * Không thay đổi Time.timeScale tại đây.
         * Vì vậy người chơi vẫn có thể xây, nâng cấp, bán tháp
         * và di chuyển camera trong thời gian đếm ngược.
         */
    }

    private void Update()
    {
        if (GameManager.Instance == null ||
            GameManager.Instance.IsGameOver)
        {
            HideWaitingUI();
            return;
        }

        switch (State)
        {
            case WaveState.WaitingToStart:
                HandleWaitingBeforeFirstWave();
                break;

            case WaveState.Spawning:
                // Coroutine SpawnWave quản lý trạng thái này.
                break;

            case WaveState.WaitingForClear:
                HandleWaitingForClear();
                break;

            case WaveState.DelayBetweenWaves:
                HandleDelayBetweenWaves();
                break;
        }
    }

    // ───────────────── CHỜ TRƯỚC WAVE ĐẦU TIÊN ───────────────────
    private void HandleWaitingBeforeFirstWave()
    {
        timer -= Time.deltaTime;

        UpdateWaitingUI(
            "Ready: ",
            "Bắt đầu Wave 1"
        );

        if (timer <= 0f)
        {
            StartNextWave();
        }
    }

    // ───────────────── CHỜ QUÁI BỊ TIÊU DIỆT HẾT ─────────────────
    private void HandleWaitingForClear()
    {
        if (isSpawning)
            return;

        if (EnemyMovement.ActiveEnemies.Count > 0)
            return;

        // Hoàn thành wave cuối cùng.
        if (waveIndex >= TotalWaves)
        {
            HideWaitingUI();
            GameManager.Instance.Win();
            return;
        }

        // Bắt đầu đếm ngược cho wave tiếp theo.
        timer = delayBetweenWaves;
        State = WaveState.DelayBetweenWaves;

        ShowWaitingUI("Next Wave: ");
    }

    // ─────────────────── CHỜ GIỮA CÁC WAVE ───────────────────────
    private void HandleDelayBetweenWaves()
    {
        timer -= Time.deltaTime;

        int nextWave = waveIndex + 1;

        UpdateWaitingUI(
            "Next Wave: ",
            $"Bắt đầu Wave {nextWave}"
        );

        if (timer <= 0f)
        {
            StartNextWave();
        }
    }

    // ─────────────────── NÚT BỎ QUA ĐẾM NGƯỢC ────────────────────
    public void SkipCountdown()
    {
        if (GameManager.Instance == null ||
            GameManager.Instance.IsGameOver)
        {
            return;
        }

        // Chỉ được bỏ qua khi đang ở một trong hai trạng thái chờ.
        bool canSkip =
            State == WaveState.WaitingToStart ||
            State == WaveState.DelayBetweenWaves;

        if (!canSkip)
            return;

        timer = 0f;
        StartNextWave();
    }

    // ───────────────────── BẮT ĐẦU WAVE ───────────────────────────
    private void StartNextWave()
    {
        // Chống việc nhấn nút nhiều lần.
        if (State == WaveState.Spawning ||
            State == WaveState.WaitingForClear)
        {
            return;
        }

        if (waveIndex >= TotalWaves)
        {
            HideWaitingUI();
            GameManager.Instance.Win();
            return;
        }

        HideWaitingUI();

        waveIndex++;
        State = WaveState.Spawning;

        GameManager.Instance.SetWaveText(
            $"Wave: {waveIndex}/{TotalWaves}"
        );

        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
        }

        spawnCoroutine = StartCoroutine(SpawnWave());
    }

    // ─────────────────────── SPAWN QUÁI ───────────────────────────
    private IEnumerator SpawnWave()
    {
        isSpawning = true;

        // waveIndex bắt đầu từ 1 nhưng mảng bắt đầu từ 0.
        WaveSettings currentWave = waves[waveIndex - 1];
        int enemyCount = Mathf.Max(0, currentWave.enemyCount);

        for (int i = 0; i < enemyCount; i++)
        {
            if (GameManager.Instance.IsGameOver)
            {
                isSpawning = false;
                spawnCoroutine = null;
                yield break;
            }

            // Từ wave 2 trở đi, cứ con thứ 4 là Tanker.
            bool isTanker =
                waveIndex >= 2 &&
                (i + 1) % 4 == 0;

            string poolTag = isTanker ? "Tanker" : "Enemy";

            ObjectPool.Instance.SpawnFromPool(
                poolTag,
                spawnPoint.position,
                Quaternion.identity
            );

            // Không cần chờ sau khi spawn con cuối cùng.
            if (i < enemyCount - 1 && spawnRate > 0f)
            {
                yield return new WaitForSeconds(spawnRate);
            }
        }

        isSpawning = false;
        spawnCoroutine = null;
        State = WaveState.WaitingForClear;
    }

    // ────────────────────────── UI ─────────────────────────────────
    private void ShowWaitingUI(string prefix)
    {
        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(true);
            countdownText.text =
                $"{prefix}{Mathf.Max(0, Mathf.CeilToInt(timer))}s";
        }

        if (readyButton != null)
        {
            readyButton.gameObject.SetActive(true);
            readyButton.interactable = true;
        }

        UpdateReadyButtonText();
    }

    private void UpdateWaitingUI(
        string prefix,
        string buttonContent)
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
        {
            readyButtonText.text =
                $"{buttonContent} ngay ({seconds}s)";
        }
    }

    private void UpdateReadyButtonText()
    {
        if (readyButtonText == null)
            return;

        int nextWave = Mathf.Clamp(
            waveIndex + 1,
            1,
            TotalWaves
        );

        int seconds = Mathf.Max(0, Mathf.CeilToInt(timer));

        readyButtonText.text =
            $"Bắt đầu Wave {nextWave} ngay ({seconds}s)";
    }

    private void HideWaitingUI()
    {
        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(false);
        }

        if (readyButton != null)
        {
            readyButton.gameObject.SetActive(false);
        }
    }

    private void UpdateWaveUI()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetWaveText(
                $"Wave: 0/{TotalWaves}"
            );
        }
    }

    private void OnDestroy()
    {
        if (readyButton != null)
        {
            readyButton.onClick.RemoveListener(SkipCountdown);
        }
    }
}