using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject[] enemyObjs;
    public Transform[] spawnPoints;

    public float maxSpawnDelay;
    public float curSpawnDelay;

    void Update()
    {
        curSpawnDelay += Time.deltaTime;
        if(curSpawnDelay > maxSpawnDelay)
        {
            SpawnEnemy();
            maxSpawnDelay = Random.Range(0.5f, 3f);
            curSpawnDelay = 0;
        }
    }

    void SpawnEnemy()
    {
        //소환될 적
        int ranEnemy = Random.Range(0,3);
        //소환될 위치
        int ranPoint = Random.Range(0,5);

        Instantiate(enemyObjs[ranEnemy], 
        spawnPoints[ranPoint].position, 
        spawnPoints[ranPoint].rotation);
    }
}
