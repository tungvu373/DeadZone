using UnityEngine;
using TMPro;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Start Values")]
    public int startMoney = 300;
    public int startLives = 20;

    [Header("UI (TextMeshPro)")]
    public TextMeshProUGUI moneyText;
    public TextMeshProUGUI livesText;
    public TextMeshProUGUI waveText;
    public GameObject winPanel;
    public GameObject losePanel;

    [Header("Map")]
    // Gán qua Inspector — mỗi scene ứng với 1 MapData asset (1:1)
    // MapData sẽ được tạo ở Bước 7; để null nếu chưa có
    public ScriptableObject currentMapData;

    public int Money { get; private set; }
    public int Lives { get; private set; }
    public bool IsGameOver { get; private set; }

    void Awake()
    {
        Instance = this;
        Money = startMoney;
        Lives = startLives;
        IsGameOver = false;
        // timeScale KHÔNG ghi ở đây — GameSpeedController.Awake() lo
    }

    void Start()
    {
        UpdateUI();
        if (winPanel  != null) winPanel.SetActive(false);
        if (losePanel != null) losePanel.SetActive(false);
        StartCoroutine(PassiveCoinRoutine());
    }

    // ================== PASSIVE INCOME ==================

    IEnumerator PassiveCoinRoutine()
    {
        while (!IsGameOver)
        {
            yield return new WaitForSeconds(1.25f);
            if (!IsGameOver) AddMoney(1);
        }
    }

    // ================== TIỀN ==================

    public bool CanAfford(int amount) => Money >= amount;

    public bool SpendMoney(int amount)
    {
        if (!CanAfford(amount)) return false;
        Money -= amount;
        UpdateUI();
        return true;
    }

    public void AddMoney(int amount)
    {
        Money += amount;
        UpdateUI();
    }

    // ================== MÁU BASE ==================

    public void TakeDamage(int amount)
    {
        if (IsGameOver) return;
        Lives -= amount;
        UpdateUI();
        if (Lives <= 0) { Lives = 0; Lose(); }
    }

    // ================== THẮNG / THUA ==================

    public void Win()
    {
        if (IsGameOver) return;
        IsGameOver = true;

        // Lưu tiến độ map (Bước 7: SaveManager sẽ xử lý currentMapData)
        // SaveManager.SetMapCompleted(((MapData)currentMapData).mapId);

        if (winPanel != null) winPanel.SetActive(true);

        // Gọi qua GameSpeedController — KHÔNG ghi Time.timeScale trực tiếp
        if (GameSpeedController.Instance != null)
            GameSpeedController.Instance.StopGame();
        else
            Time.timeScale = 0f; // fallback an toàn nếu chưa có controller
    }

    void Lose()
    {
        IsGameOver = true;
        if (losePanel != null) losePanel.SetActive(true);

        if (GameSpeedController.Instance != null)
            GameSpeedController.Instance.StopGame();
        else
            Time.timeScale = 0f;
    }

    // ================== UI ==================

    public void SetWaveText(string text)
    {
        if (waveText != null) waveText.text = text;
    }

    void UpdateUI()
    {
        if (moneyText != null) moneyText.text = $"Vàng: {Money}";
        if (livesText != null) livesText.text = $"Máu: {Lives}";
    }
}