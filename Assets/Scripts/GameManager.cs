using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject[] enemyObjs;
    public Transform[] spawnPoints;

    public float maxSpawnDelay;
    public float curSpawnDelay;

    public GameObject player;

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
        int ranPoint = Random.Range(0,9);
        GameObject enemy = Instantiate(enemyObjs[ranEnemy], 
        spawnPoints[ranPoint].position, 
        spawnPoints[ranPoint].rotation);
        Rigidbody2D rigid = enemy.GetComponent<Rigidbody2D>();
        Enemy enemyLogic = enemy.GetComponent<Enemy>();
        enemyLogic.player = player;

        //오른쪽 스폰
        if(ranPoint == 5 || ranPoint == 6)
        {
            enemy.transform.Rotate(Vector3.back*90);
            rigid.velocity = new Vector2(enemyLogic.speed * (-1), -1);
        }
        //왼쪽 스폰
        else if(ranPoint == 7 || ranPoint == 8)
        {
            enemy.transform.Rotate(Vector3.forward*90);
            rigid.velocity = new Vector2(enemyLogic.speed, -1);
        }
        //아래쪽 스폰
        else{
            rigid.velocity = new Vector2(0, enemyLogic.speed*(-1));
        }
    }

    //플레이어 복귀 로직
    public void RespawnPlayer()
    {
        Invoke("RespawnPlayerExe", 2f);
    }

    void RespawnPlayerExe()
    {
        player.transform.position = Vector3.down * 3.5f;
        player.SetActive(true);
    }
}
