using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public GameObject[] enemyObjs;
    public Transform[] spawnPoints;

    public float maxSpawnDelay;
    public float curSpawnDelay;

    public GameObject player;
    public TextMeshProUGUI scoreText;
    public Image[] lifeImage;
    public GameObject gameOverSet;
    Player playerLogic;

    void Awake()
    {
        playerLogic = player.GetComponent<Player>();
    }

    void Update()
    {
        curSpawnDelay += Time.deltaTime;
        if(curSpawnDelay > maxSpawnDelay)
        {
            SpawnEnemy();
            maxSpawnDelay = Random.Range(0.5f, 3f);
            curSpawnDelay = 0;
        }

        //UI Score Update
        scoreText.text = string.Format("{0:n0}", playerLogic.score);
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

    public void updateLifeIcon(int life)
    {
        //UI Life Init Disable
        for(int i = 0; i < lifeImage.Length; i++)
        {
            lifeImage[i].color = new Color(1,1,1,0);
        }
        
        //UI Life Active
        for(int i = 0; i < life && i < lifeImage.Length; i++)
        {
            lifeImage[i].color = new Color(1,1,1,1);
        }
        
    }

    //플레이어 복귀 로직
    public void RespawnPlayer()
    {
        Invoke("RespawnPlayerExe", 2f);
    }
    
    public void RespawnPlayerExe()
    {
        player.transform.position = Vector3.down * 3.5f;
        player.SetActive(true);

        Player playerLogic = player.GetComponent<Player>();
        playerLogic.isHit = false;
    }

    public void GameOver()
    {
        gameOverSet.SetActive(true);
    }

    public void GameRetry()
    {
        SceneManager.LoadScene(0);
    }

}
