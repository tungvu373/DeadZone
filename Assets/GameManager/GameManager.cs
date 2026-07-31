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

    public int Money { get; private set; }
    public int Lives { get; private set; }
    public bool IsGameOver { get; private set; }

    void Awake()
    {
        Instance = this;
        Money = startMoney;
        Lives = startLives;
        IsGameOver = false;
        Time.timeScale = 1f;
    }

    void Start()
    {
        UpdateUI();
        if (winPanel != null) winPanel.SetActive(false);
        if (losePanel != null) losePanel.SetActive(false);
        StartCoroutine(PassiveCoinRoutine());
    }

    // ================== PASSIVE INCOME ==================

    IEnumerator PassiveCoinRoutine()
    {
        while (!IsGameOver)
        {
            yield return new WaitForSeconds(1.25f);
            if (!IsGameOver)
                AddMoney(1);
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

        if (Lives <= 0)
        {
            Lives = 0;
            Lose();
        }
    }

    // ================== THẮNG / THUA ==================

    public void Win()
    {
        if (IsGameOver) return;
        IsGameOver = true;
        if (winPanel != null) winPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    void Lose()
    {
        IsGameOver = true;
        if (losePanel != null) losePanel.SetActive(true);
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