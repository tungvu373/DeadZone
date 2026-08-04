using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// OWNER DUY NHẤT của Time.timeScale trong toàn bộ project.
/// Mọi nơi khác (GameManager, PauseMenu...) phải gọi qua các method của class này.
/// </summary>
public class GameSpeedController : MonoBehaviour
{
    public static GameSpeedController Instance;
    [Header("UI")]
    public TextMeshProUGUI speedButtonText;

    private readonly float[] speeds = { 1f, 2f, 3f };
    private int speedIndex = 0;

    private bool isPaused = false;
    private float speedBeforePause = 1f;

    void Awake()
    {
        Instance = this;
        // Reset timeScale vô điều kiện mỗi lần scene load
        // Giải quyết bug: Win/Lose set timeScale=0, load scene mới vẫn đóng băng
        Time.timeScale = 1f;
        speedIndex = 0;
        isPaused = false;
        UpdateButtonText();
    }

    // ================== SPEED ==================

    /// <summary>Gắn vào nút Speed (OnClick)</summary>
    public void CycleSpeed()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return;
        if (isPaused) return;

        speedIndex = (speedIndex + 1) % speeds.Length;
        Time.timeScale = speeds[speedIndex];
        UpdateButtonText();
    }

    // ================== PAUSE / RESUME ==================

    /// <summary>Gọi khi pause game. Lưu lại tốc độ hiện tại để resume đúng.</summary>
    public void PauseGame()
    {
        if (isPaused) return;
        speedBeforePause = Time.timeScale;
        Time.timeScale = 0f;
        isPaused = true;
    }

    /// <summary>Gọi khi resume từ pause. Khôi phục đúng tốc độ (×2 vẫn là ×2).</summary>
    public void ResumeGame()
    {
        if (!isPaused) return;
        Time.timeScale = speedBeforePause;
        isPaused = false;
    }

    /// <summary>
    /// Gọi khi game kết thúc (Win hoặc Lose).
    /// Khác PauseGame: không lưu tốc độ, không cho Resume.
    /// </summary>
    public void StopGame()
    {
        isPaused = false; // không dùng isPaused để tránh Resume sau đó
        Time.timeScale = 0f;
    }

    // ================== RESTART / QUIT ==================

    /// <summary>Gắn vào nút "Chơi lại" trên Win/Lose Panel</summary>
    public void RestartLevel()
    {
        // timeScale sẽ được reset trong Awake() của scene mới
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    /// <summary>Gắn vào nút "Về Menu"</summary>
    public void GoToMainMenu()
    {
        SceneManager.LoadScene("MainMenu"); // tên scene menu
    }

    // ================== PRIVATE ==================

    void UpdateButtonText()
    {
        if (speedButtonText != null)
            speedButtonText.text = $"x{speeds[speedIndex]:0}";
    }
}