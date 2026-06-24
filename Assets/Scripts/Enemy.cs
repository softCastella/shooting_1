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

    [Tooltip("보스(B) 최대 체력 — ApplyHealthFromEnemyName에서 설정")]
    public float bossMaxHealth;

    bool deathHandled;
    float suppressHitUntilTime;

    static bool warnedBossHitTriggerMissing;
    public int patternIndex;
    public int curPatternCount;
    public int[] maxPatternCount;

    [Header("보스 FireAround (원형 탄)")]
    [Tooltip("짝수 번째 원(첫·셋째…) 한 바퀴 발 수")]
    [SerializeField] int fireAroundCountEven = 50;
    [Tooltip("홀수 번째 원(둘째·넷째…) 한 바퀴 발 수")]
    [SerializeField] int fireAroundCountOdd = 40;
    [Tooltip("원형 패턴이 한 바퀴돌고 같은 패턴으로 반복될 때 간격(초). 늘리면 웨이브 사이가 넓어짐.")]
    [SerializeField] float fireAroundRepeatDelay = 0.7f;
    [Tooltip("홀수 번째 원마다 각도에 π/N(반 칸) 보정. 짝·홀 발 수가 다를 때 둘째 원이 첫 원과 같은 방향 기준으로만 돌아가 덜 어색하게 맞춤.")]
    [SerializeField] bool fireAroundHalfStepOddWaves = true;

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
                bossMaxHealth = health;
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
        // --- [테스트] FireAround만 — 복구: 아래 4줄 삭제 후 주석(/* … */) 해제 ---
        patternIndex = 3;
        curPatternCount = 0;
        FireAround();
        return;

        /*
        patternIndex = patternIndex == 3 ? 0 : patternIndex + 1;
        curPatternCount = 0;
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
        */
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
        if (health <= 0)
            return;
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
        if (curPatternCount < maxPatternCount[patternIndex])
            Invoke(nameof(FireFoward), 2f);
        else
            Invoke(nameof(Think), 3f);
    }

    void FireShot()
    {
        if (health <= 0)
            return;
        for (int i = 0; i < 5; i++)
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
        if (curPatternCount < maxPatternCount[patternIndex])
            Invoke(nameof(FireShot), 3.5f);
        else
            Invoke(nameof(Think), 3f);
    }

    void FireArc()
    {
        if (health <= 0)
            return;

        GameObject bullet = objectManager.MakeObj("bulletEnemyA");
        if (bullet == null)
            return;

        bullet.transform.position = transform.position;
        bullet.transform.rotation = Quaternion.identity;

        Rigidbody2D rigid = bullet.GetComponent<Rigidbody2D>();
        if (rigid == null)
            return;

        int arcSteps = Mathf.Max(1, maxPatternCount[patternIndex]);
        float phase = Mathf.PI * 2f * curPatternCount / arcSteps;

        Bullet bulletLogic = bullet.GetComponent<Bullet>();
        if (bulletLogic != null)
            bulletLogic.EnableArcTrajectory(phase);
        else
            rigid.AddForce(new Vector2(Mathf.Sin(phase), -1f).normalized * 5f, ForceMode2D.Impulse);

        curPatternCount++;
        if (curPatternCount < maxPatternCount[patternIndex])
            Invoke(nameof(FireArc), 0.15f);
        else
            Invoke(nameof(Think), 3f);
    }

    void FireAround()
    {
        if (health <= 0)
            return;

        int roundNumA = Mathf.Max(3, fireAroundCountEven);
        int roundNumB = Mathf.Max(3, fireAroundCountOdd);
        int roundNum = curPatternCount % 2 == 0 ? roundNumA : roundNumB;

        bool oddWave = curPatternCount % 2 == 1;
        float halfStepRad = fireAroundHalfStepOddWaves && oddWave ? Mathf.PI / roundNum : 0f;
        float halfStepDeg = fireAroundHalfStepOddWaves && oddWave ? 180f / roundNum : 0f;

        for (int i = 0; i < roundNum; i++)
        {
            GameObject bullet = objectManager.MakeObj("bulletBossB");
            if (bullet == null)
                return;

            float angle = Mathf.PI * 2f * i / roundNum + halfStepRad;
            Vector2 dirVec = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

            bullet.transform.position = transform.position + (Vector3)(dirVec * 0.35f);
            bullet.transform.rotation = Quaternion.identity;

            Rigidbody2D rigid = bullet.GetComponent<Rigidbody2D>();
            if (rigid == null)
                return;

            rigid.AddForce(dirVec * 5f, ForceMode2D.Impulse);

            Vector3 rotVec = Vector3.forward * (360f * i / roundNum + halfStepDeg + 90f);
            bullet.transform.Rotate(rotVec);
        }

        curPatternCount++;
        if (curPatternCount < maxPatternCount[patternIndex])
            Invoke(nameof(FireAround), fireAroundRepeatDelay);
        else
            Invoke(nameof(Think), 3f);
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

        health -= dmg;

        if (enemyName == "B")
        {
            Debug.Log($"[Boss] HP {health}/{bossMaxHealth}");
            if (anim == null)
                anim = GetComponent<Animator>();
            TrySetBossHitTrigger();
        }
        else
        {
            Debug.Log("OnHit: " + dmg);
            if (spriteRenderer != null && sprites != null && sprites.Length > 1 && sprites[1] != null)
            {
                spriteRenderer.sprite = sprites[1];
                Invoke(nameof(ReturnSprite), 0.1f);
            }
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
        // PlayerBullet 피격은 Bullet.OnTriggerEnter2D에서만 처리 (EnemyBullet이 Enemy와 겹칠 때 오인 방지)
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "BorderBullet" && enemyName != "B")
            ReturnToPool();
    }
}
