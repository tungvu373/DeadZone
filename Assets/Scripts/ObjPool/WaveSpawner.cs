using UnityEngine;
using System.Collections;

public class WaveSpawner : MonoBehaviour
{
    [Header("Setup")]
    public Transform spawnPoint;

    [Header("Wave Settings")]
    public int totalWaves = 5;             // ✅ số wave để thắng
    public float timeBetweenWaves = 5f;
    public float spawnRate = 0.5f;      // giãn cách giữa từng con

    private int waveIndex = 0;
    private float countdown = 3f;
    private bool allWavesSpawned = false;

    void Update()
    {
        if (GameManager.Instance.IsGameOver) return;

        // ✅ Hết wave + hết quái trên map → THẮNG
        if (allWavesSpawned)
        {
            if (EnemyMovement.ActiveEnemies.Count == 0)
                GameManager.Instance.Win();
            return;
        }

        if (countdown <= 0f)
        {
            StartCoroutine(SpawnWave());
            countdown = timeBetweenWaves;
        }
        countdown -= Time.deltaTime;
    }

    IEnumerator SpawnWave()
    {
        waveIndex++;
        GameManager.Instance.SetWaveText($"Wave: {waveIndex}/{totalWaves}");

        int enemyCount = waveIndex * 2 + 3;

        for (int i = 0; i < enemyCount; i++)
        {
            // ✅ Từ wave 2 trở đi, cứ 4 con thì con thứ 4 là Tanker
            bool isTanker = waveIndex >= 2 && (i % 4 == 3);
            string tag = isTanker ? "Tanker" : "Enemy";

            ObjectPool.Instance.SpawnFromPool(tag, spawnPoint.position, Quaternion.identity);
            yield return new WaitForSeconds(spawnRate);
        }

        if (waveIndex >= totalWaves)
            allWavesSpawned = true;
    }
}