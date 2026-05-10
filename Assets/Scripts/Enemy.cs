using UnityEngine;

// 적: 속도는 GameManager 스폰 시 설정. 발사·피격·아이템 드랍·총알 충돌.
public class Enemy : MonoBehaviour
{
    public string enemyName;

    public int enemyScore;

    public float speed;
    public float health;
    public int dmg;
    public float maxShotDelay;
    public float curShotDelay;

    public GameObject player;
    public GameObject bulletObjA;
    public GameObject bulletObjB;
    public ObjectManager objectManager;

    public Sprite[] sprites;
    SpriteRenderer spriteRenderer;
    Animator anim;

    public float bossDeathHideDelay = 0.12f;
    public string bossHitTriggerParameter = "OnHit";
    public float spawnHitIgnoreSeconds = 0.15f;

    bool deathHandled;
    float suppressHitUntilTime;

    static bool warnedBossHitTriggerMissing;
    public int patternIndex;
    public int curPatternCount;
    public int[] maxPatternCount;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        CacheAnimatorIfBoss();
    }

    void OnEnable()
    {
        CancelInvoke(nameof(ReturnToPool));
        CancelInvoke(nameof(ReturnSprite));
        deathHandled = false;
        suppressHitUntilTime = Time.time + spawnHitIgnoreSeconds;
        ApplyHealthFromEnemyName();
        if (enemyName == "B")
            ResetBossAnimatorTriggers();
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
            case "B":
                health = 3000;
                Invoke("stop", 2f);
                break;
        }
    }

    void stop()
    {
       if(!gameObject.activeSelf)
       
        return;
       
       Rigidbody2D rigid = GetComponent<Rigidbody2D>();
       rigid.velocity = Vector2.zero;

       Invoke("Think", 2);
    }

    void Think()
    {
        patternIndex = patternIndex==3 ? 0 : patternIndex+1;
        switch (patternIndex)
        {
            case 0:
            FireFoward();
                break;
            case 1:
            FireShot();
                break;
            case 2:
            FireArc();
                break;
            case 3:
            FireAround();
                break;
        }
    }

    /// 보스 본체 콜라이더 안에서 탄이 스폰되면 튕기거나 박혀서 제자리에서 빙글 도는 것처럼 보일 수 있음.
    static void ClearBossForwardBulletSpin(GameObject bullet)
    {
        if (bullet == null)
            return;
        Bullet b = bullet.GetComponent<Bullet>();
        if (b != null)
            b.isRotate = false;
    }

    void FireFoward()
    {
            // 몸통보다 살짝 아래에서 나오게 해 콜라이더와 겹침 완화
            Vector3 spawnY = transform.position + Vector3.down * 0.28f;
            GameObject bulletR = objectManager.MakeObj("bulletBossA");
            bulletR.transform.position = spawnY + Vector3.right * 0.3f;
            ClearBossForwardBulletSpin(bulletR);
            GameObject bulletRR = objectManager.MakeObj("bulletBossA");
            bulletRR.transform.position = spawnY + Vector3.right * 0.45f;
            ClearBossForwardBulletSpin(bulletRR);
            GameObject bulletL = objectManager.MakeObj("bulletBossA");
            bulletL.transform.position = spawnY + Vector3.left * 0.3f;
            ClearBossForwardBulletSpin(bulletL);
            GameObject bulletLL = objectManager.MakeObj("bulletBossA");
            bulletLL.transform.position = spawnY + Vector3.left * 0.45f;
            ClearBossForwardBulletSpin(bulletLL);

            Rigidbody2D rigidR = bulletR.GetComponent<Rigidbody2D>();
            Rigidbody2D rigidRR = bulletRR.GetComponent<Rigidbody2D>();
            Rigidbody2D rigidL = bulletL.GetComponent<Rigidbody2D>();
            Rigidbody2D rigidLL = bulletLL.GetComponent<Rigidbody2D>();
            if (rigidL == null || rigidR == null)
                return;

            
            rigidL.AddForce(Vector2.down * 8, ForceMode2D.Impulse);
            rigidR.AddForce(Vector2.down * 8, ForceMode2D.Impulse);
            rigidLL.AddForce(Vector2.down * 8, ForceMode2D.Impulse);
            rigidRR.AddForce(Vector2.down * 8, ForceMode2D.Impulse);
        
        
        curPatternCount++;
        if(curPatternCount < maxPatternCount[patternIndex])
        Invoke("FireFoward", 2);
        else
        Invoke("Think", 3);
    }

    void FireShot()
    {
        for(int i = 0; i < 5; i++)
        {

        GameObject bullet = objectManager.MakeObj("bulletEnemyB");
            if (bullet == null)
                return;

            bullet.transform.position = transform.position;

            Rigidbody2D rigid = bullet.GetComponent<Rigidbody2D>();
            if (rigid == null)
                return;

            Vector2 dirVec = player.transform.position - transform.position;
            Vector2 ranVec = new Vector2(Random.Range(-0.5f, 0.5f), Random.Range(0f, 2f));
            dirVec += ranVec;
            rigid.AddForce(dirVec.normalized * 3, ForceMode2D.Impulse);
        }

        curPatternCount++;
        if(curPatternCount < maxPatternCount[patternIndex])
        Invoke("FireShot", 3.5f);
        else
        Invoke("Think", 3);
    }

    void FireArc()
    {
        {

            GameObject bullet = objectManager.MakeObj("bulletEnemyA");
            if (bullet == null)
                return;

            bullet.transform.position = transform.position;
            bullet.transform.rotation = Quaternion.identity;

            Rigidbody2D rigid = bullet.GetComponent<Rigidbody2D>();
            if (rigid == null)
                return;

            Vector2 dirVec = new Vector2(Mathf.Sin(Mathf.PI * 2 * curPatternCount/maxPatternCount[patternIndex]), -1);
            rigid.AddForce(dirVec.normalized * 5, ForceMode2D.Impulse);
        }

        curPatternCount++;
        if(curPatternCount < maxPatternCount[patternIndex])
        Invoke("FireArc", 0.15f);
        else
        Invoke("Think", 3);
    }

    void FireAround()
    {
        Debug.Log("원 형태로 전체 공격");
        curPatternCount++;
        if(curPatternCount < maxPatternCount[patternIndex])
        Invoke("FireAround", 0.7f);
        else
        Invoke("Think", 3);
    }

    void CacheAnimatorIfBoss()
    {
        if (enemyName == "B")
            anim = GetComponent<Animator>();
    }

    void ResetBossAnimatorTriggers()
    {
        if (anim == null)
            anim = GetComponent<Animator>();
        if (anim == null)
            return;
        foreach (AnimatorControllerParameter p in anim.parameters)
        {
            if (p.type == AnimatorControllerParameterType.Trigger)
                anim.ResetTrigger(p.name);
        }
    }

    public void SetSpawnEnemyKind(string kind)
    {
        enemyName = (kind ?? "").Trim();
        CacheAnimatorIfBoss();
        ApplyHealthFromEnemyName();
        curShotDelay = 0f;
        suppressHitUntilTime = Time.time + spawnHitIgnoreSeconds;
        if (enemyName == "B")
            ResetBossAnimatorTriggers();
    }

    void Update()
    {
        if (enemyName == "B")
            return;
        Fire();
        Reload();
    }

    void Fire()
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

    void TrySetBossHitTrigger()
    {
        if (anim == null || string.IsNullOrEmpty(bossHitTriggerParameter))
            return;

        foreach (AnimatorControllerParameter p in anim.parameters)
        {
            if (p.type == AnimatorControllerParameterType.Trigger && p.name == bossHitTriggerParameter)
            {
                anim.SetTrigger(bossHitTriggerParameter);
                return;
            }
        }

        if (!warnedBossHitTriggerMissing)
        {
            warnedBossHitTriggerMissing = true;
            Debug.LogWarning($"[Enemy] Animator Trigger '{bossHitTriggerParameter}' 없음.");
        }
    }

    public void OnHit(int dmg)
    {
        if (health <= 0 || deathHandled)
            return;
        if (Time.time < suppressHitUntilTime)
            return;

        Debug.Log("OnHit: " + dmg);
        health -= dmg;

        if (enemyName == "B")
        {
            if (anim == null)
                anim = GetComponent<Animator>();
            TrySetBossHitTrigger();
        }
        else if (spriteRenderer != null && sprites != null && sprites.Length > 1 && sprites[1] != null)
        {
            spriteRenderer.sprite = sprites[1];
            Invoke(nameof(ReturnSprite), 0.1f);
        }

        if (health <= 0)
        {
            deathHandled = true;

            Player playerLogic = player.GetComponent<Player>();
            playerLogic.score += enemyScore;

            int ran = enemyName == "B" ? 0 : Random.Range(0, 10);
            if (ran < 3)
            {
                Debug.Log("NotItem");
            }
            else if (ran < 6 && objectManager != null)
            {
                GameObject itemCoin = objectManager.MakeObj("itemCoin");
                if (itemCoin != null)
                    itemCoin.transform.position = transform.position;
            }
            else if (ran < 8 && objectManager != null)
            {
                GameObject itemPower = objectManager.MakeObj("itemPower");
                if (itemPower != null)
                    itemPower.transform.position = transform.position;
            }
            else if (ran < 10 && objectManager != null)
            {
                GameObject itemBoom = objectManager.MakeObj("itemBoom");
                if (itemBoom != null)
                    itemBoom.transform.position = transform.position;
            }

            if (enemyName == "B" && bossDeathHideDelay > 0f)
                Invoke(nameof(ReturnToPool), bossDeathHideDelay);
            else
                ReturnToPool();
        }
    }

    void ReturnToPool()
    {
        gameObject.SetActive(false);
        transform.rotation = Quaternion.identity;
    }

    void ReturnSprite()
    {
        if (spriteRenderer != null && sprites != null && sprites.Length > 0 && sprites[0] != null)
            spriteRenderer.sprite = sprites[0];
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "BorderBullet" && enemyName != "B")
            ReturnToPool();
        else if (collision.gameObject.tag == "PlayerBullet")
        {
            Bullet bullet = collision.gameObject.GetComponent<Bullet>();
            if (bullet != null)
                OnHit(bullet.Damage);
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "BorderBullet" && enemyName != "B")
            ReturnToPool();
        else if (collision.gameObject.tag == "PlayerBullet")
        {
            Bullet bullet = collision.gameObject.GetComponent<Bullet>();
            if (bullet != null)
            {
                OnHit(bullet.Damage);
                collision.gameObject.SetActive(false);
            }
        }
    }
}
