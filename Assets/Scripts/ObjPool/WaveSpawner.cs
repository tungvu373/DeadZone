using UnityEngine;
using System.Collections;

public class WaveSpawner : MonoBehaviour
{
    [Header("Setup")]
    public Transform spawnPoint;

    [Header("Wave Settings")]
    public float timeBetweenWaves = 7f;   // nghỉ giữa các wave
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

        int enemyCount = waveIndex * 2 + 3;

        Debug.Log("Wave " + waveIndex);

        for (int i = 0; i < enemyCount; i++)
        {
            // Spawn Enemy
            ObjectPool.Instance.SpawnFromPool("Enemy", spawnPoint.position, Quaternion.identity);
            yield return new WaitForSeconds(spawnRate);

            // Cứ sau 5 Enemy thì spawn 2 Tanker
            if ((i + 1) % 5 == 0)
            {
                yield return new WaitForSeconds(1f); // nghỉ 1 giây

                for (int j = 0; j < 2; j++)
                {
                    ObjectPool.Instance.SpawnFromPool("Tanker", spawnPoint.position, Quaternion.identity);
                    yield return new WaitForSeconds(spawnRate);
                }
            }
        }
    }
}