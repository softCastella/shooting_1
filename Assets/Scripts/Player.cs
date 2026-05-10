using UnityEngine;

// 플레이어: 이동, 발사, 폭탄, 충돌. 풀 오브젝트는 Destroy 대신 SetActive(false)로 반환.
public class Player : MonoBehaviour
{
    public bool isTouchTop;
    public bool isTouchBottom;
    public bool isTouchRight;
    public bool isTouchLeft;

    public int life;
    public int score;
    public float speed;
    public int power;
    public int maxPower;
    public int boom;
    public int maxBoom;
    public float maxShotDelay;
    public float curShotDelay;
    public bool isHit;
    public bool isBoomTime;

    public GameObject bulletObjA;
    public GameObject bulletObjB;
    public GameObject boomEffect;

    public GameManager gameManager;
    public ObjectManager objectManager;
    public GameObject[] followers;

    Animator anim;

    void Awake()
    {
        anim = GetComponent<Animator>();
        if (gameManager == null)
            gameManager = FindFirstObjectByType<GameManager>();
        if (objectManager == null)
            objectManager = FindFirstObjectByType<ObjectManager>();
    }

    void Start()
    {
        ApplyInitialFollowers();
    }

    void Update()
    {
        Move();
        Fire();
        Boom();
        Reload();
    }

    void Fire()
    {
        if (!Input.GetButton("Fire1"))
            return;

        if (objectManager == null)
            return;

        float shotInterval = maxShotDelay > 0f ? maxShotDelay : 0.15f;
        if (curShotDelay < shotInterval)
            return;

        switch (power)
        {
            case 1:
                SpawnBulletA(transform.position);
                break;

            case 2:
                SpawnBulletA(transform.position + Vector3.left * 0.1f);
                SpawnBulletA(transform.position + Vector3.right * 0.1f);
                break;

            default:
                SpawnBulletA(transform.position + Vector3.left * 0.35f);
                SpawnBulletB(transform.position);
                SpawnBulletA(transform.position + Vector3.right * 0.35f);
                break;
        }

        curShotDelay = 0;
    }

    void SpawnBulletA(Vector3 pos)
    {
        GameObject bullet = objectManager.MakeObj("bulletPlayerA");
        if (bullet == null)
            return;
        bullet.transform.position = pos;
        Rigidbody2D rigid = bullet.GetComponent<Rigidbody2D>();
        if (rigid != null)
            rigid.AddForce(Vector2.up * 10, ForceMode2D.Impulse);
    }

    void SpawnBulletB(Vector3 pos)
    {
        GameObject bullet = objectManager.MakeObj("bulletPlayerB");
        if (bullet == null)
            return;
        bullet.transform.position = pos;
        Rigidbody2D rigid = bullet.GetComponent<Rigidbody2D>();
        if (rigid != null)
            rigid.AddForce(Vector2.up * 10, ForceMode2D.Impulse);
    }

    void Reload()
    {
        curShotDelay += Time.deltaTime;
    }

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
        Invoke(nameof(OffBoomEffect), 4f);

        HitEnemyPool(objectManager.GetPool("enemyL"));
        HitEnemyPool(objectManager.GetPool("enemyM"));
        HitEnemyPool(objectManager.GetPool("enemyS"));
        HitEnemyPool(objectManager.GetPool("enemyB"));

        GameObject[] bulletsA = objectManager.GetPool("bulletEnemyA");
        GameObject[] bulletsB = objectManager.GetPool("bulletEnemyB");
        if (bulletsA != null)
        {
            for (int i = 0; i < bulletsA.Length; i++)
            {
                if (bulletsA[i].activeSelf)
                    bulletsA[i].SetActive(false);
            }
        }
        if (bulletsB != null)
        {
            for (int i = 0; i < bulletsB.Length; i++)
            {
                if (bulletsB[i].activeSelf)
                    bulletsB[i].SetActive(false);
            }
        }
    }

    static void HitEnemyPool(GameObject[] pool)
    {
        if (pool == null)
            return;
        for (int i = 0; i < pool.Length; i++)
        {
            if (pool[i].activeSelf)
            {
                Enemy enemyLogic = pool[i].GetComponent<Enemy>();
                if (enemyLogic != null)
                    enemyLogic.OnHit(1000);
            }
        }
    }

    void Move()
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
                gameManager.RespawnPlayer();

            gameObject.SetActive(false);
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
                    {
                        power++;
                        AddFollower();
                    }
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
            collision.gameObject.SetActive(false);
        }
    }

    void OffBoomEffect()
    {
        boomEffect.SetActive(false);
        isBoomTime = false;
    }

    void ApplyInitialFollowers()
    {
        if (followers == null)
            return;
        if (followers.Length > 0 && followers[0] != null)
            followers[0].SetActive(power >= 4);
        if (followers.Length > 1 && followers[1] != null)
            followers[1].SetActive(power >= 5);
        if (followers.Length > 2 && followers[2] != null)
            followers[2].SetActive(power >= 6);
    }

    void AddFollower()
    {
        ApplyInitialFollowers();
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
