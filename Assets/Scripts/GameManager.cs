using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.IO;

// 스폰 타이머, UI, 리스폰·게임오버. 적은 MakeObj(enemyObjs 키).
public class GameManager : MonoBehaviour
{
    public string[] enemyObjs;       // MakeObj에 넘길 키: enemyL, enemyM, enemyS
    public Transform[] spawnPoints; // 랜덤 스폰 위치

    public float maxSpawnDelay; // 다음 스폰까지 대기 시간(매번 랜덤으로 갱신)
    public float curSpawnDelay;

    public GameObject player;
    public TextMeshProUGUI scoreText;
    public Image[] lifeImage;
    public Image[] boomImage;
    public GameObject gameOverSet;
    public ObjectManager objectManager;
    // Resources 폴더 안 텍스트 이름(.txt 빼고). 예: Resources/stage 0.txt → "stage 0"
    public string spawnTextResourceName = "stage 0";

    public List<Spawn> spawnList;
    public int spawnIndex;
    public bool spawnEnd;

    Player playerLogic;
    

    void Awake()
    {
        spawnList = new List<Spawn>();
        playerLogic = player.GetComponent<Player>();
        enemyObjs = new string[] { "enemyL", "enemyM", "enemyS" };
        ReadSpawnFile();
    }

    void ReadSpawnFile()
    {
        //1. 변수 초기화
        spawnList.Clear();
        spawnIndex = 0;
        spawnEnd = false;

        //2. 텍스트 파일 읽기 — 한 줄 형식: 지연시간,타입,포인트 (쉼표 구분)
        TextAsset textFile = Resources.Load<TextAsset>(spawnTextResourceName);
        if (textFile == null)
        {
            Debug.LogError($"GameManager: Resources에서 '{spawnTextResourceName}' 텍스트를 찾을 수 없습니다. (폴더 Resources, 확장자 .txt)");
            maxSpawnDelay = 999f;
            return;
        }
        StringReader stringReader = new StringReader(textFile.text);

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
            // 두 번째 열: L/M/S 한 글자(대소문 무관). 앞뒤 공백·숨은 문자(예: BOM/ZWSP) 있어도 첫 글자만 사용
            spawnData.type = ParseSpawnTypeLetter(cols[1]);
            spawnData.point = int.Parse(cols[2].Trim());
            spawnList.Add(spawnData);
        }

        stringReader.Close();

        if (spawnList.Count == 0)
        {
            Debug.LogError("GameManager: 스폰 목록이 비었습니다. Stage 파일 형식을 확인하세요.");
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
            if (u == 'L' || u == 'M' || u == 'S')
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

    void SpawnEnemy() // MakeObj 후 스폰점·방향별 회전·velocity
    {
        if (objectManager == null || spawnList == null || spawnIndex >= spawnList.Count)
            return;

        string type = (spawnList[spawnIndex].type ?? "").Trim();

        int enemyIndex = 0;
        // 스폰 텍스트 type L/M/S → 인덱스 0/1/2 → enemyObjs[i]는 풀 키 문자열(enemyL, enemyM, enemyS)
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
        }
        int enemyPoint = spawnList[spawnIndex].point;

        if (spawnPoints == null || enemyPoint < 0 || enemyPoint >= spawnPoints.Length || spawnPoints[enemyPoint] == null)
        {
            Debug.LogWarning($"GameManager: 스폰 포인트 없음 point={enemyPoint}");
            return;
        }

        GameObject enemy = objectManager.MakeObj(enemyObjs[enemyIndex]);
        if (enemy == null)
            return; // 풀 고갈 — 인덱스 유지 후 다음 타이머에 재시도

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
        enemyLogic.SetSpawnEnemyKind(type); // Spawn.type → Enemy 행동(체력·발사)

        // 스폰 포인트 인덱스에 따른 이동 방향
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
        //리스폰 인덱스 증가
        spawnIndex++;
        if(spawnIndex == spawnList.Count)
        {
            spawnEnd = true;
            return;
        }
        maxSpawnDelay = spawnList[spawnIndex].delay;
    }

    public void updateLifeIcon(int life) // 생명 아이콘
    {
        for (int i = 0; i < lifeImage.Length; i++)
            lifeImage[i].color = new Color(1, 1, 1, 0);

        for (int i = 0; i < life && i < lifeImage.Length; i++)
            lifeImage[i].color = new Color(1, 1, 1, 1);
    }

    public void updateBoomIcon(int boom) // 폭탄 아이콘
    {
        for (int i = 0; i < boomImage.Length; i++)
            boomImage[i].color = new Color(1, 1, 1, 0);

        for (int i = 0; i < boom && i < boomImage.Length; i++)
            boomImage[i].color = new Color(1, 1, 1, 1);
    }

    public void RespawnPlayer() // 2초 뒤 부활
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
