using UnityEngine;
using System.Collections;

public class WaveSpawner : MonoBehaviour
{
    [Header("Setup")]
    public Transform spawnPoint;

    [Header("Wave Settings")]
    public int totalWaves = 5;             // ✅ số wave để thắng
    public float timeBetweenWaves = 5f;
    public float spawnRate = 0.5f;

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
            ObjectPool.Instance.SpawnFromPool("Enemy", spawnPoint.position, Quaternion.identity);
            yield return new WaitForSeconds(spawnRate);
        }

        if (waveIndex >= totalWaves)
            allWavesSpawned = true;
    }
}