using UnityEngine;

// 오브젝트 풀: 미리 Instantiate 후 비활성 보관. 꺼낼 때 MakeObj, 넣을 때 SetActive(false).
public class ObjectManager : MonoBehaviour
{
    // --- 인스펙터에 연결할 프리팹 ---
    public GameObject enemyLPrefab;
    public GameObject enemyMPrefab;
    public GameObject enemySPrefab;
    public GameObject enemyBPrefab;
    public GameObject itemCoinPrefab;
    public GameObject itemPowerPrefab;
    public GameObject itemBoomPrefab;
    public GameObject bulletPlayerAPrefab;
    public GameObject bulletPlayerBPrefab;
    public GameObject bulletEnemyAPrefab;
    public GameObject bulletEnemyBPrefab;
    public GameObject bulletFollowerPrefab;
    public GameObject bulletBossPrefabA;
    public GameObject bulletBossPrefabB;

    // --- 풀 배열(런타임에 Generate에서 채움) ---
    GameObject[] enemyL;
    GameObject[] enemyM;
    GameObject[] enemyS;
    GameObject[] enemyB;

    GameObject[] itemCoin;
    GameObject[] itemPower;
    GameObject[] itemBoom;

    GameObject[] bulletPlayerA;
    GameObject[] bulletPlayerB;
    GameObject[] bulletEnemyA;
    GameObject[] bulletEnemyB;
    GameObject[] bulletFollower;
    GameObject[] bulletBossA;
    GameObject[] bulletBossB;

    GameObject[] targetPool;

    void Awake()
    {
        enemyL = new GameObject[10];
        enemyM = new GameObject[10];
        enemyS = new GameObject[20];
        enemyB = new GameObject[5];

        itemCoin = new GameObject[20];
        itemPower = new GameObject[10];
        itemBoom = new GameObject[10];

        bulletPlayerA = new GameObject[100];
        bulletPlayerB = new GameObject[100];
        bulletEnemyA = new GameObject[100];
        bulletEnemyB = new GameObject[100];
        bulletFollower = new GameObject[100];
        bulletBossA = new GameObject[50];
        bulletBossB = new GameObject[200];

        Generate();
    }

    void Generate()
    {
        for (int i = 0; i < enemyL.Length; i++)
        {
            enemyL[i] = Instantiate(enemyLPrefab);
            enemyL[i].SetActive(false);
        }

        for (int i = 0; i < enemyM.Length; i++)
        {
            enemyM[i] = Instantiate(enemyMPrefab);
            enemyM[i].SetActive(false);
        }

        for (int i = 0; i < enemyS.Length; i++)
        {
            enemyS[i] = Instantiate(enemySPrefab);
            enemyS[i].SetActive(false);
        }

        for (int i = 0; i < enemyB.Length; i++)
        {
            enemyB[i] = Instantiate(enemyBPrefab);
            enemyB[i].SetActive(false);
        }

        for (int i = 0; i < itemCoin.Length; i++)
        {
            itemCoin[i] = Instantiate(itemCoinPrefab);
            itemCoin[i].SetActive(false);
        }

        for (int i = 0; i < itemPower.Length; i++)
        {
            itemPower[i] = Instantiate(itemPowerPrefab);
            itemPower[i].SetActive(false);
        }

        for (int i = 0; i < itemBoom.Length; i++)
        {
            itemBoom[i] = Instantiate(itemBoomPrefab);
            itemBoom[i].SetActive(false);
        }

        for (int i = 0; i < bulletPlayerA.Length; i++)
        {
            bulletPlayerA[i] = Instantiate(bulletPlayerAPrefab);
            bulletPlayerA[i].SetActive(false);
        }

        for (int i = 0; i < bulletPlayerB.Length; i++)
        {
            bulletPlayerB[i] = Instantiate(bulletPlayerBPrefab);
            bulletPlayerB[i].SetActive(false);
        }

        for (int i = 0; i < bulletEnemyA.Length; i++)
        {
            bulletEnemyA[i] = Instantiate(bulletEnemyAPrefab);
            bulletEnemyA[i].SetActive(false);
        }

        for (int i = 0; i < bulletEnemyB.Length; i++)
        {
            bulletEnemyB[i] = Instantiate(bulletEnemyBPrefab);
            bulletEnemyB[i].SetActive(false);
        }

        for (int i = 0; i < bulletBossA.Length; i++)
        {
            bulletBossA[i] = Instantiate(bulletBossPrefabA);
            bulletBossA[i].SetActive(false);
        }

        for (int i = 0; i < bulletBossB.Length; i++)
        {
            bulletBossB[i] = Instantiate(bulletBossPrefabB);
            bulletBossB[i].SetActive(false);
        }

        for (int i = 0; i < bulletFollower.Length; i++)
        {
            bulletFollower[i] = Instantiate(bulletFollowerPrefab);
            bulletFollower[i].SetActive(false);
        }
    }

    public GameObject MakeObj(string type)
    {
        switch (type)
        {
            case "enemyL":
                targetPool = enemyL;
                break;
            case "enemyM":
                targetPool = enemyM;
                break;
            case "enemyS":
                targetPool = enemyS;
                break;
            case "enemyB":
                targetPool = enemyB;
                break;
            case "itemCoin":
                targetPool = itemCoin;
                break;
            case "itemPower":
                targetPool = itemPower;
                break;
            case "itemBoom":
                targetPool = itemBoom;
                break;
            case "bulletPlayerA":
                targetPool = bulletPlayerA;
                break;
            case "bulletPlayerB":
                targetPool = bulletPlayerB;
                break;
            case "bulletEnemyA":
                targetPool = bulletEnemyA;
                break;
            case "bulletEnemyB":
                targetPool = bulletEnemyB;
                break;
            case "bulletBossA":
                targetPool = bulletBossA;
                break;
            case "bulletBossB":
                targetPool = bulletBossB;
                break;
            case "bulletFollower":
                targetPool = bulletFollower;
                break;
            default:
                Debug.LogWarning($"MakeObj: unknown type '{type}'");
                return null;
        }

        if (targetPool == null)
            return null;

        for (int i = 0; i < targetPool.Length; i++)
        {
            if (targetPool[i] == null)
                continue;
            if (!targetPool[i].activeSelf)
            {
                targetPool[i].SetActive(true);
                return targetPool[i];
            }
        }
        return null;
    }

    public GameObject[] GetPool(string type)
    {
        switch (type)
        {
            case "enemyL":
                return enemyL;
            case "enemyM":
                return enemyM;
            case "enemyS":
                return enemyS;
            case "enemyB":
                return enemyB;
            case "itemCoin":
                return itemCoin;
            case "itemPower":
                return itemPower;
            case "itemBoom":
                return itemBoom;
            case "bulletPlayerA":
                return bulletPlayerA;
            case "bulletPlayerB":
                return bulletPlayerB;
            case "bulletEnemyA":
                return bulletEnemyA;
            case "bulletEnemyB":
                return bulletEnemyB;
            case "bulletBossA":
                return bulletBossA;
            case "bulletBossB":
                return bulletBossB;
            case "bulletFollower":
                return bulletFollower;
            default:
                Debug.LogWarning($"GetPool: unknown type '{type}'");
                return null;
        }
    }
}
