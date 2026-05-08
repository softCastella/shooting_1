using System;
using UnityEngine;

public class Player : MonoBehaviour
{
    //이동 속도
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


    public GameManager manager;
    Animator anim;

    void Awake()
    {
        anim = GetComponent<Animator>();
        manager = FindFirstObjectByType<GameManager>();
    }

    void Start()
    {
        // 시작 직후 첫 발사는 가능하도록 초기값 설정
        // curShotDelay = maxShotDelay;
    }

    //플레이어 이동 키, 포지션 정보
    void Update()
    {
        Move();
        Fire();
        Boom();
        Reload();
    }

    void Fire() //발사 함수수
    {
        if(!Input.GetButton("Fire1")) return; //파이어버튼이 눌려야하고
        
        switch(power)
        {
            case 1:
                GameObject bullet = Instantiate(bulletObjA,transform.position,transform.rotation);
                Rigidbody2D rigid = bullet.GetComponent<Rigidbody2D>();
                rigid.AddForce(Vector2.up*10,ForceMode2D.Impulse);
                break;

            case 2:
                GameObject bulletL = Instantiate(bulletObjA,transform.position + Vector3.left *0.1f,transform.rotation);
                GameObject bulletR = Instantiate(bulletObjA,transform.position + Vector3.right *0.1f,transform.rotation);
                Rigidbody2D rigidL = bulletL.GetComponent<Rigidbody2D>();
                Rigidbody2D rigidR = bulletR.GetComponent<Rigidbody2D>();
                rigidL.AddForce(Vector2.up*10,ForceMode2D.Impulse);
                rigidR.AddForce(Vector2.up*10,ForceMode2D.Impulse);
                break;

            case 3:
                GameObject bulletLL = Instantiate(bulletObjA,transform.position + Vector3.left *0.35f,transform.rotation);
                GameObject bulletCC = Instantiate(bulletObjB,transform.position, transform.rotation);
                GameObject bulletRR = Instantiate(bulletObjA,transform.position + Vector3.right *0.35f,transform.rotation);
                Rigidbody2D rigidLL = bulletLL.GetComponent<Rigidbody2D>();
                Rigidbody2D rigidCC = bulletCC.GetComponent<Rigidbody2D>();
                Rigidbody2D rigidRR = bulletRR.GetComponent<Rigidbody2D>();
                rigidLL.AddForce(Vector2.up*10,ForceMode2D.Impulse);
                rigidCC.AddForce(Vector2.up*10,ForceMode2D.Impulse);
                rigidRR.AddForce(Vector2.up*10,ForceMode2D.Impulse);
                break;
        }
        curShotDelay = 0;//총알쏘고 딜레이 변수 0 초기화
    }


    void Reload() //장전 함수
    {
        curShotDelay += Time.deltaTime;
    }

    void Boom()
    {
        if(!Input.GetButton("Fire2")) return;
        if(isBoomTime) return;
        if(boom == 0) return;
        boom--;
        isBoomTime = true;
        manager.updateBoomIcon(boom);
        
        //폭발 이펙트 활성화
        boomEffect.SetActive(true);
        Invoke("OffBoomEffect", 4f);
        //적 파괴
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        for(int i = 0; i < enemies.Length; i++)
        {
        Enemy enemyLogic = enemies[i].GetComponent<Enemy>();
        enemyLogic.OnHit(1000);
        }
        //적 총알 파괴
        GameObject[] bullets = GameObject.FindGameObjectsWithTag("EnemyBullet");
        for(int i = 0; i < enemies.Length; i++)
        {
        Enemy enemyLogic = enemies[i].GetComponent<Enemy>();
        Destroy(bullets[i]);
        }
    }

    void Move()
    {
        float h = Input.GetAxisRaw("Horizontal");    
        if(isTouchRight && h ==1 || isTouchLeft && h ==-1)
        h = 0;
        float v = Input.GetAxisRaw("Vertical");
        if(isTouchTop && v ==1 || isTouchBottom && v ==-1)
        v = 0;
        Vector3 curPos = transform.position;
        Vector3 nextPos = new Vector3(h, v, 0) * speed * Time.deltaTime;

        transform.position = curPos + nextPos;

        if(Input.GetButtonDown("Horizontal")||Input.GetButtonUp("Horizontal"))
        {
            anim.SetInteger("Input",(int)h);
        }
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
        else if(collision.gameObject.tag == "Enemy" || collision.gameObject.tag == "EnemyBullet")
        {
            if(isHit) return;
            isHit = true;         
            life--;
            manager.updateLifeIcon(life);

            if(life == 0)
            {
                manager.GameOver();
            } 
            else 
            {  //플레이어 부활
                manager.RespawnPlayer();
            }
            
            gameObject.SetActive(false);
            Destroy(collision.gameObject);

        }
        else if(collision.gameObject.tag == "Item")
        {
            Item item = collision.gameObject.GetComponent<Item>();
            switch(item.type)
            {
                case "Coin":
                    score += 1000;
                    break;
                case "Power":
                if(power == maxPower)
                score += 500;
                else power++;
                    break;
                case "Boom":
                
                if(boom == maxBoom)
                score += 500;
                else {
                    boom++;
                    manager.updateBoomIcon(boom);
                }
                    break;
            }
            Destroy(collision.gameObject);
        }
    }


    void OffBoomEffect()
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

