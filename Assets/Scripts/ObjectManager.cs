using UnityEngine;

// 오브젝트 풀: 미리 Instantiate 후 비활성 보관. 꺼낼 때 MakeObj, 넣을 때 SetActive(false).
public class ObjectManager : MonoBehaviour
{
    // --- 인스펙터에 연결할 프리팹 ---
    public GameObject enemyLPrefab;
    public GameObject enemyMPrefab;
    public GameObject enemySPrefab;
    public GameObject itemCoinPrefab;
    public GameObject itemPowerPrefab;
    public GameObject itemBoomPrefab;
    public GameObject bulletPlayerAPrefab;
    public GameObject bulletPlayerBPrefab;
    public GameObject bulletEnemyAPrefab;
    public GameObject bulletEnemyBPrefab;

    // --- 풀 배열(런타임에 Generate에서 채움) ---
    GameObject[] enemyL;
    GameObject[] enemyM;
    GameObject[] enemyS;

    GameObject[] itemCoin;
    GameObject[] itemPower;
    GameObject[] itemBoom;

    GameObject[] bulletPlayerA;
    GameObject[] bulletPlayerB;
    GameObject[] bulletEnemyA;
    GameObject[] bulletEnemyB;

    // MakeObj 시 현재 순회할 풀 참조
    GameObject[] targetPool;

    void Awake()
    {
        // 풀 크기만큼 배열 할당
        enemyL = new GameObject[5];
        enemyM = new GameObject[5];
        enemyS = new GameObject[5];

        itemCoin = new GameObject[3];
        itemPower = new GameObject[3];
        itemBoom = new GameObject[3];

        bulletPlayerA = new GameObject[20];
        bulletPlayerB = new GameObject[20];
        bulletEnemyA = new GameObject[20];
        bulletEnemyB = new GameObject[20];

        Generate();
    }

    void Generate() // 최초 풀 채우기: Instantiate 후 SetActive(false)
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
    }

    // type은 enemyL, bulletPlayerA 등 — 발사·스폰 코드와 문자열 통일
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
            default:
                Debug.LogWarning($"MakeObj: unknown type '{type}'");
                return null;
        }

        if (targetPool == null)
            return null;

        for (int i = 0; i < targetPool.Length; i++)
        {
            // 과거 Destroy로 깨진 슬롯 방지
            if (targetPool[i] == null)
                continue;
            if (!targetPool[i].activeSelf)
            {
                targetPool[i].SetActive(true);
                return targetPool[i];
            }
        }
        return null; // 풀 고갈
    }

    public GameObject[] GetPool(string type) // Boom 등 전체 풀 순회용, MakeObj와 같은 type 키
    {
        switch (type)
        {
            case "enemyL":
                return enemyL;
            case "enemyM":
                return enemyM;
            case "enemyS":
                return enemyS;
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
            default:
                Debug.LogWarning($"GetPool: unknown type '{type}'");
                return null;
        }
    }
}
