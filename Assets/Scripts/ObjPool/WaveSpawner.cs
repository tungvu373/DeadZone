using UnityEngine;
using System.Collections;

public class WaveSpawner : MonoBehaviour
{
    [Header("Setup")]
    public Transform spawnPoint;

    [Header("Wave Settings")]
    public float timeBetweenWaves = 5f;   // nghỉ giữa các wave
    public float spawnRate = 0.5f;        // giãn cách giữa từng con

    private int waveIndex = 0;
    private float countdown = 3f;         // đếm ngược wave đầu tiên

    void Update()
    {
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
        int enemyCount = waveIndex * 2 + 3;  // wave sau đông hơn wave trước

        Debug.Log("Wave " + waveIndex + " - " + enemyCount + " enemies!");

        for (int i = 0; i < enemyCount; i++)
        {
            ObjectPool.Instance.SpawnFromPool("Enemy", spawnPoint.position, Quaternion.identity);
            yield return new WaitForSeconds(spawnRate);
        }
    }
}