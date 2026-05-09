using UnityEngine;

// 적: 속도는 GameManager 스폰 시 설정. 발사·피격·아이템 드랍·총알 충돌.
public class Enemy : MonoBehaviour
{
    public string enemyName; // "L" 대형, "M" 중형, "S" 소형 — 발사 패턴 분기
    public int enemyScore;

    public float speed;
    public float health;
    public int dmg;
    public float maxShotDelay; // 발사 간격(초)
    public float curShotDelay;

    public GameObject player;
    public GameObject bulletObjA; // 레거시 참조 가능
    public GameObject bulletObjB;
    public ObjectManager objectManager;

    public Sprite[] sprites; // [0] 기본, [1] 피격 시 잠시 교체
    SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void OnEnable() // 풀에서 켜질 때(아직 enemyName이 안 바뀐 프레임일 수 있음)
    {
        ApplyHealthFromEnemyName();
    }

    void ApplyHealthFromEnemyName()
    {
        switch (enemyName)
        {
            case "L":
                health = 40;
                break;
            case "M":
                health = 10;
                break;
            case "S":
                health = 3;
                break;
        }
    }

    // 텍스트 스폰 등에서 풀 활성화 후 호출 — OnEnable보다 늦게 타입이 정해질 때
    public void SetSpawnEnemyKind(string kind)
    {
        enemyName = (kind ?? "").Trim();
        ApplyHealthFromEnemyName();
        curShotDelay = 0f;
    }

    void Update()
    {
        Fire();
        Reload();
    }

    void Fire() // S: bulletEnemyA 1발, L: bulletEnemyB 2발 (풀)
    {
        if (curShotDelay < maxShotDelay)
            return;
        if (objectManager == null || player == null)
            return;

        if (enemyName == "S")
        {
            GameObject bullet = objectManager.MakeObj("bulletEnemyA");
            if (bullet == null)
                return;

            bullet.transform.position = transform.position;

            Rigidbody2D rigid = bullet.GetComponent<Rigidbody2D>();
            if (rigid == null)
                return;

            Vector3 dirVec = player.transform.position - transform.position;
            rigid.AddForce(dirVec.normalized * 10, ForceMode2D.Impulse);
        }
        else if (enemyName == "L")
        {
            GameObject bulletL = objectManager.MakeObj("bulletEnemyB");
            GameObject bulletR = objectManager.MakeObj("bulletEnemyB");
            if (bulletL == null || bulletR == null)
                return;

            bulletL.transform.position = transform.position + Vector3.right * 0.3f;
            bulletR.transform.position = transform.position + Vector3.left * 0.3f;

            Rigidbody2D rigidL = bulletL.GetComponent<Rigidbody2D>();
            Rigidbody2D rigidR = bulletR.GetComponent<Rigidbody2D>();
            if (rigidL == null || rigidR == null)
                return;

            Vector3 dirVecL = player.transform.position - (transform.position + Vector3.right * 0.3f);
            Vector3 dirVecR = player.transform.position - (transform.position + Vector3.left * 0.3f);
            rigidL.AddForce(dirVecL.normalized * 10, ForceMode2D.Impulse);
            rigidR.AddForce(dirVecR.normalized * 10, ForceMode2D.Impulse);
        }
        curShotDelay = 0;
    }

    void Reload()
    {
        curShotDelay += Time.deltaTime;
    }

    public void OnHit(int dmg) // 사망 시 점수·아이템·적 풀 반환(SetActive false)
    {
        if (health <= 0)
            return;

        Debug.Log("OnHit: " + dmg);
        health -= dmg;

        if (spriteRenderer != null && sprites != null && sprites.Length > 1 && sprites[1] != null)
        {
            spriteRenderer.sprite = sprites[1];
            Invoke("ReturnSprite", 0.1f);
        }

        if (health <= 0)
        {
            Player playerLogic = player.GetComponent<Player>();
            playerLogic.score += enemyScore;

            // 확률: 무드랍 30%, 코인 30%, 파워 20%, 붐 20%
            int ran = Random.Range(0, 10);
            if (ran < 3)
            {
                Debug.Log("NotItem");
            }
            else if (ran < 6 && objectManager != null)
            {
                GameObject itemCoin = objectManager.MakeObj("itemCoin");
                itemCoin.transform.position = transform.position;
            }
            else if (ran < 8 && objectManager != null)
            {
                GameObject itemPower = objectManager.MakeObj("itemPower");
                itemPower.transform.position = transform.position;
            }
            else if (ran < 10 && objectManager != null)
            {
                GameObject itemBoom = objectManager.MakeObj("itemBoom");
                itemBoom.transform.position = transform.position;
            }

            gameObject.SetActive(false);
            transform.rotation = Quaternion.identity;
        }
    }

    void ReturnSprite()
    {
        if (spriteRenderer != null && sprites != null && sprites.Length > 0 && sprites[0] != null)
            spriteRenderer.sprite = sprites[0];
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "BorderBullet")
        {
            gameObject.SetActive(false);
            transform.rotation = Quaternion.identity;
        }
        else if (collision.gameObject.tag == "PlayerBullet")
        {
            Bullet bullet = collision.gameObject.GetComponent<Bullet>();
            if (bullet != null)
                OnHit(bullet.Damage);
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "BorderBullet")
            gameObject.SetActive(false);
        else if (collision.gameObject.tag == "PlayerBullet")
        {
            Bullet bullet = collision.gameObject.GetComponent<Bullet>();
            if (bullet != null)
            {
                OnHit(bullet.Damage);
                collision.gameObject.SetActive(false); // 플레이어 총알 풀 반환
            }
        }
    }
}
