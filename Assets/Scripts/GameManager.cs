using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.IO;

// 스폰 타이머, UI, 리스폰·게임오버. 적은 MakeObj(enemyObjs 키).
public class GameManager : MonoBehaviour
{
    public string[] enemyObjs;
    public Transform[] spawnPoints;

    public float maxSpawnDelay;
    public float curSpawnDelay;

    public GameObject player;
    public TextMeshProUGUI scoreText;
    public Image[] lifeImage;
    public Image[] boomImage;
    public GameObject gameOverSet;
    public ObjectManager objectManager;
    public string spawnTextResourceName = "stage 0";

    public List<Spawn> spawnList;
    public int spawnIndex;
    public bool spawnEnd;

    Player playerLogic;

    void Awake()
    {
        spawnList = new List<Spawn>();
        playerLogic = player.GetComponent<Player>();
        enemyObjs = new string[] { "enemyL", "enemyM", "enemyS", "enemyB" };
        ReadSpawnFile();
    }

    void ReadSpawnFile()
    {
        spawnList.Clear();
        spawnIndex = 0;
        spawnEnd = false;

        TextAsset textFile = Resources.Load<TextAsset>(spawnTextResourceName);
        if (textFile == null)
        {
            Debug.LogError($"GameManager: Resources에서 '{spawnTextResourceName}' 텍스트를 찾을 수 없습니다.");
            maxSpawnDelay = 999f;
            return;
        }

        using (StringReader stringReader = new StringReader(textFile.text))
        {
            while (true)
            {
                string line = stringReader.ReadLine();
                if (line == null)
                    break;

                line = line.Trim();
                if (line.Length == 0 || line.StartsWith("#"))
                    continue;

                string[] cols = line.Split(',');
                if (cols.Length < 3)
                    continue;

                Spawn spawnData = new Spawn();
                spawnData.delay = float.Parse(cols[0].Trim());
                spawnData.type = ParseSpawnTypeLetter(cols[1]);
                spawnData.point = int.Parse(cols[2].Trim());
                spawnList.Add(spawnData);
            }
        }

        if (spawnList.Count == 0)
        {
            Debug.LogError("GameManager: 스폰 목록이 비었습니다.");
            maxSpawnDelay = 999f;
            spawnEnd = true;
            return;
        }
        maxSpawnDelay = spawnList[0].delay;
    }

    static string ParseSpawnTypeLetter(string raw)
    {
        if (string.IsNullOrEmpty(raw))
            return "S";
        foreach (char ch in raw.Trim())
        {
            char u = char.ToUpperInvariant(ch);
            if (u == 'L' || u == 'M' || u == 'S' || u == 'B')
                return u.ToString();
        }
        return "S";
    }

    void Update()
    {
        curSpawnDelay += Time.deltaTime;
        if (curSpawnDelay > maxSpawnDelay && !spawnEnd)
        {
            SpawnEnemy();
            curSpawnDelay = 0;
        }

        scoreText.text = string.Format("{0:n0}", playerLogic.score);
    }

    void SpawnEnemy()
    {
        if (objectManager == null || spawnList == null || spawnIndex >= spawnList.Count)
            return;

        string type = (spawnList[spawnIndex].type ?? "").Trim();

        int enemyIndex = 2;
        switch (type)
        {
            case "L":
                enemyIndex = 0;
                break;
            case "M":
                enemyIndex = 1;
                break;
            case "S":
                enemyIndex = 2;
                break;
            case "B":
                enemyIndex = 3;
                break;
            default:
                enemyIndex = 2;
                break;
        }

        if (enemyObjs == null || enemyIndex < 0 || enemyIndex >= enemyObjs.Length)
        {
            Debug.LogWarning($"GameManager: enemyObjs 오류 type={type} index={enemyIndex}");
            return;
        }

        int enemyPoint = spawnList[spawnIndex].point;

        if (spawnPoints == null || enemyPoint < 0 || enemyPoint >= spawnPoints.Length || spawnPoints[enemyPoint] == null)
        {
            Debug.LogWarning($"GameManager: 스폰 포인트 없음 point={enemyPoint}");
            return;
        }

        GameObject enemy = objectManager.MakeObj(enemyObjs[enemyIndex]);
        if (enemy == null)
            return;

        enemy.transform.rotation = Quaternion.identity;
        enemy.transform.position = spawnPoints[enemyPoint].position;

        Rigidbody2D rigid = enemy.GetComponent<Rigidbody2D>();
        Enemy enemyLogic = enemy.GetComponent<Enemy>();
        if (rigid == null || enemyLogic == null)
        {
            enemy.SetActive(false);
            return;
        }

        enemyLogic.player = player;
        enemyLogic.objectManager = objectManager;
        enemyLogic.SetSpawnEnemyKind(type);

        if (enemyPoint == 5 || enemyPoint == 6)
        {
            enemy.transform.Rotate(Vector3.back * 90);
            rigid.velocity = new Vector2(enemyLogic.speed * (-1), -1);
        }
        else if (enemyPoint == 7 || enemyPoint == 8)
        {
            enemy.transform.Rotate(Vector3.forward * 90);
            rigid.velocity = new Vector2(enemyLogic.speed, -1);
        }
        else
        {
            rigid.velocity = new Vector2(0, enemyLogic.speed * (-1));
        }

        spawnIndex++;
        if (spawnIndex == spawnList.Count)
        {
            spawnEnd = true;
            return;
        }
        maxSpawnDelay = spawnList[spawnIndex].delay;
    }

    public void updateLifeIcon(int life)
    {
        for (int i = 0; i < lifeImage.Length; i++)
            lifeImage[i].color = new Color(1, 1, 1, 0);

        for (int i = 0; i < life && i < lifeImage.Length; i++)
            lifeImage[i].color = new Color(1, 1, 1, 1);
    }

    public void updateBoomIcon(int boom)
    {
        for (int i = 0; i < boomImage.Length; i++)
            boomImage[i].color = new Color(1, 1, 1, 0);

        for (int i = 0; i < boom && i < boomImage.Length; i++)
            boomImage[i].color = new Color(1, 1, 1, 1);
    }

    public void RespawnPlayer()
    {
        Invoke(nameof(RespawnPlayerExe), 2f);
    }

    public void RespawnPlayerExe()
    {
        player.transform.position = Vector3.down * 3.5f;
        player.SetActive(true);

        Player pl = player.GetComponent<Player>();
        pl.isHit = false;
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
