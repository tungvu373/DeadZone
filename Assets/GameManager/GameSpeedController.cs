using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameSpeedController : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI speedButtonText;

    private readonly float[] speeds = { 1f, 2f, 3f };
    private int speedIndex = 0;

    // Gắn vào nút Speed (OnClick)
    public void CycleSpeed()
    {
        if (GameManager.Instance.IsGameOver) return;   // game kết thúc thì không đổi

        speedIndex = (speedIndex + 1) % speeds.Length;
        Time.timeScale = speeds[speedIndex];

        if (speedButtonText != null)
            speedButtonText.text = $"x{speeds[speedIndex]:0}";
    }

    // Gắn vào nút "Chơi lại" trên Win/Lose Panel
    public void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}