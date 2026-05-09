using UnityEngine;

// 플레이어: 이동, 발사, 폭탄, 충돌. 풀 오브젝트는 Destroy 대신 SetActive(false)로 반환.
public class Player : MonoBehaviour
{
    // --- 화면 경계 접촉 여부 (Border 트리거로 갱신) ---
    public bool isTouchTop;
    public bool isTouchBottom;
    public bool isTouchRight;
    public bool isTouchLeft;

    // --- 스탯 ---
    public int life;           // 남은 목숨
    public int score;          // 점수
    public float speed;        // 이동 속도
    public int power;          // 파워(총알 패턴 단계, 1~3)
    public int maxPower;
    public int boom;           // 사용 가능한 폭탄 개수
    public int maxBoom;
    public float maxShotDelay; // 발사 간격 상한(연사 제한에 사용 가능)
    public float curShotDelay; // 현재 장전 경과 시간
    public bool isHit;         // 피격 처리 중 플래그(연속 충돌 방지)
    public bool isBoomTime;    // 폭탄 연출 재생 중

    // --- 참조(인스펙터 또는 코드에서 연결) ---
    public GameObject bulletObjA; // 레거시 프리팹 참조(풀링 시 미사용 가능)
    public GameObject bulletObjB;
    public GameObject boomEffect; // 폭탄 시 활성화되는 이펙트 오브젝트

    public GameManager gameManager;
    public ObjectManager objectManager;
    Animator anim;

    void Awake()
    {
        anim = GetComponent<Animator>();
        gameManager = FindFirstObjectByType<GameManager>();
        // 인스펙터에 미할당 시 씬에서 자동 검색
        if (objectManager == null)
            objectManager = FindFirstObjectByType<ObjectManager>();
    }

    void Start()
    {
        // 시작 직후 첫 발사는 가능하도록 초기값 설정
        // curShotDelay = maxShotDelay;
    }

    void Update()
    {
        Move();
        Fire();
        Boom();
        Reload();
    }

    // Fire1: 플레이어 총알 풀에서 MakeObj 후 발사 (Enemy와 같이 maxShotDelay 간격으로만 발사)
    void Fire()
    {
        if (!Input.GetButton("Fire1"))
            return;

        if (objectManager == null)
            return;

        // 인스펙터에서 0이면 매 프레임 발사되어 풀고갈·겹침이 나므로 최소 간격 사용
        float shotInterval = maxShotDelay > 0f ? maxShotDelay : 0.15f;
        if (curShotDelay < shotInterval)
            return;

        switch (power)
        {
            case 1:
                GameObject bullet = objectManager.MakeObj("bulletPlayerA");
                if (bullet == null)
                    return;

                bullet.transform.position = transform.position;

                Rigidbody2D rigid = bullet.GetComponent<Rigidbody2D>();
                if (rigid == null)
                    return;

                rigid.AddForce(Vector2.up * 10, ForceMode2D.Impulse);
                break;

            case 2:
                GameObject bulletL = objectManager.MakeObj("bulletPlayerA");
                GameObject bulletR = objectManager.MakeObj("bulletPlayerA");
                if (bulletL == null || bulletR == null)
                    return;

                bulletL.transform.position = transform.position + Vector3.left * 0.1f;
                bulletR.transform.position = transform.position + Vector3.right * 0.1f;

                Rigidbody2D rigidL = bulletL.GetComponent<Rigidbody2D>();
                Rigidbody2D rigidR = bulletR.GetComponent<Rigidbody2D>();
                if (rigidL == null || rigidR == null)
                    return;

                rigidL.AddForce(Vector2.up * 10, ForceMode2D.Impulse);
                rigidR.AddForce(Vector2.up * 10, ForceMode2D.Impulse);
                break;

            case 3:
                GameObject bulletLL = objectManager.MakeObj("bulletPlayerA");
                GameObject bulletCC = objectManager.MakeObj("bulletPlayerB");
                GameObject bulletRR = objectManager.MakeObj("bulletPlayerA");
                if (bulletLL == null || bulletCC == null || bulletRR == null)
                    return;

                bulletLL.transform.position = transform.position + Vector3.left * 0.35f;
                bulletCC.transform.position = transform.position;
                bulletRR.transform.position = transform.position + Vector3.right * 0.35f;

                Rigidbody2D rigidLL = bulletLL.GetComponent<Rigidbody2D>();
                Rigidbody2D rigidCC = bulletCC.GetComponent<Rigidbody2D>();
                Rigidbody2D rigidRR = bulletRR.GetComponent<Rigidbody2D>();
                if (rigidLL == null || rigidCC == null || rigidRR == null)
                    return;

                rigidLL.AddForce(Vector2.up * 10, ForceMode2D.Impulse);
                rigidCC.AddForce(Vector2.up * 10, ForceMode2D.Impulse);
                rigidRR.AddForce(Vector2.up * 10, ForceMode2D.Impulse);
                break;
        }
        curShotDelay = 0; // 발사 직후 장전 타이머 리셋
    }

    void Reload() // 장전 시간 누적
    {
        curShotDelay += Time.deltaTime;
    }

    // Fire2 폭탄: 활성 적 피격, 적 총알 풀 순회해 비활성화
    void Boom()
    {
        if (!Input.GetButton("Fire2"))
            return;
        if (isBoomTime)
            return;
        if (boom == 0)
            return;

        boom--;
        isBoomTime = true;
        gameManager.updateBoomIcon(boom);

        boomEffect.SetActive(true);
        Invoke("OffBoomEffect", 4f);

        // 풀에서 꺼내 활성화 중인 적만 피격 처리
        GameObject[] enemiesL = objectManager.GetPool("enemyL");
        GameObject[] enemiesM = objectManager.GetPool("enemyM");
        GameObject[] enemiesS = objectManager.GetPool("enemyS");

        for (int i = 0; i < enemiesL.Length; i++)
        {
            if (enemiesL[i].activeSelf)
            {
                Enemy enemyLogic = enemiesL[i].GetComponent<Enemy>();
                enemyLogic.OnHit(1000);
            }
        }
        for (int i = 0; i < enemiesM.Length; i++)
        {
            if (enemiesM[i].activeSelf)
            {
                Enemy enemyLogic = enemiesM[i].GetComponent<Enemy>();
                enemyLogic.OnHit(1000);
            }
        }
        for (int i = 0; i < enemiesS.Length; i++)
        {
            if (enemiesS[i].activeSelf)
            {
                Enemy enemyLogic = enemiesS[i].GetComponent<Enemy>();
                enemyLogic.OnHit(1000);
            }
        }

        // 적 총알 오브젝트를 풀에 반환(비활성화)
        GameObject[] bulletsA = objectManager.GetPool("bulletEnemyA");
        GameObject[] bulletsB = objectManager.GetPool("bulletEnemyB");
        for (int i = 0; i < bulletsA.Length; i++)
        {
            if (bulletsA[i].activeSelf)
                bulletsA[i].SetActive(false);
        }
        for (int i = 0; i < bulletsB.Length; i++)
        {
            if (bulletsB[i].activeSelf)
                bulletsB[i].SetActive(false);
        }
    }

    void Move() // 이동 입력 + 경계에서 막음
    {
        float h = Input.GetAxisRaw("Horizontal");
        if (isTouchRight && h == 1 || isTouchLeft && h == -1)
            h = 0;

        float v = Input.GetAxisRaw("Vertical");
        if (isTouchTop && v == 1 || isTouchBottom && v == -1)
            v = 0;

        Vector3 curPos = transform.position;
        Vector3 nextPos = new Vector3(h, v, 0) * speed * Time.deltaTime;
        transform.position = curPos + nextPos;

        if (Input.GetButtonDown("Horizontal") || Input.GetButtonUp("Horizontal"))
            anim.SetInteger("Input", (int)h);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Border")
        {
            switch (collision.gameObject.name)
            {
                case "Top":
                    isTouchTop = true;
                    break;
                case "Bottom":
                    isTouchBottom = true;
                    break;
                case "Right":
                    isTouchRight = true;
                    break;
                case "Left":
                    isTouchLeft = true;
                    break;
            }
        }
        else if (collision.gameObject.tag == "Enemy" || collision.gameObject.tag == "EnemyBullet")
        {
            if (isHit)
                return;

            isHit = true;
            life--;
            gameManager.updateLifeIcon(life);

            if (life == 0)
                gameManager.GameOver();
            else
                gameManager.RespawnPlayer(); // 잠시 후 부활

            gameObject.SetActive(false);
            // 풀링: 적·적 총알은 Destroy하지 않고 비활성화만
            collision.gameObject.SetActive(false);
        }
        else if (collision.gameObject.tag == "Item")
        {
            Item item = collision.gameObject.GetComponent<Item>();
            switch (item.type)
            {
                case "Coin":
                    score += 1000;
                    break;
                case "Power":
                    if (power == maxPower)
                        score += 500;
                    else
                        power++;
                    break;
                case "Boom":
                    if (boom == maxBoom)
                        score += 500;
                    else
                    {
                        boom++;
                        gameManager.updateBoomIcon(boom);
                    }
                    break;
            }
            collision.gameObject.SetActive(false); // 아이템 풀 반환
        }
    }

    void OffBoomEffect() // 폭탄 이펙트 끄고 재입력 허용
    {
        boomEffect.SetActive(false);
        isBoomTime = false;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Border")
        {
            switch (collision.gameObject.name)
            {
                case "Top":
                    isTouchTop = false;
                    break;
                case "Bottom":
                    isTouchBottom = false;
                    break;
                case "Right":
                    isTouchRight = false;
                    break;
                case "Left":
                    isTouchLeft = false;
                    break;
            }
        }
    }
}
