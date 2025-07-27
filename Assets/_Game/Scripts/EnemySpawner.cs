using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public float spawnRate = 2f;
    public float spawnPadding = 0.5f;
    
    private Camera mainCam;
    private float timer;

    void Start()
    {
        mainCam = Camera.main;
    }

    void Update()
    {
        timer += Time.deltaTime;
        
        if(timer >= spawnRate)
        {
            SpawnEnemy();
            timer = 0f;
            spawnRate = Mathf.Max(0.5f, spawnRate * 0.95f); // Zorluk artışı
        }
    }

    void SpawnEnemy()
    {
        Vector2 spawnPos = GetSpawnPosition();
        Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
    }

    Vector2 GetSpawnPosition()
    {
        float camHeight = mainCam.orthographicSize;
        float camWidth = camHeight * mainCam.aspect;
        
        int edge = Random.Range(0, 4); // 0:üst, 1:sağ, 2:alt, 3:sol
        Vector2 spawnPos = Vector2.zero;
        
        switch(edge)
        {
            case 0: spawnPos = new Vector2(Random.Range(-camWidth, camWidth), camHeight + spawnPadding); break;
            case 1: spawnPos = new Vector2(camWidth + spawnPadding, Random.Range(-camHeight, camHeight)); break;
            case 2: spawnPos = new Vector2(Random.Range(-camWidth, camWidth), -camHeight - spawnPadding); break;
            case 3: spawnPos = new Vector2(-camWidth - spawnPadding, Random.Range(-camHeight, camHeight)); break;
        }
        
        return spawnPos;
    }
}