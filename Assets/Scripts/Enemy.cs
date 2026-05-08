using UnityEngine;

public class Enemy : MonoBehaviour
{
    public string enemyName;
    public int enemyScore;
    public GameObject player;
    public GameObject bulletObjA;
    public GameObject bulletObjB;
    public float speed;
    public float health;
    public int dmg;
    public float maxShotDelay;
    public float curShotDelay;
    
    public Sprite[] sprites;
    SpriteRenderer spriteRenderer;
   

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        
    }

    void Update()
    {
            Fire();
            Reload();   
    }
        
    void Fire()
    {
        if(curShotDelay < maxShotDelay) return; //장전시간이 최대 장전시간보다 작으면 발사하지 않음
        if(enemyName =="S")
        {
            GameObject bullet = Instantiate(bulletObjA,transform.position,transform.rotation);
            Rigidbody2D rigid = bullet.GetComponent<Rigidbody2D>();
            Vector3 dirVec = player.transform.position - transform.position;
            rigid.AddForce(dirVec.normalized*10,ForceMode2D.Impulse);
        }
        else if(enemyName =="L")
        {
            GameObject bulletL = Instantiate(bulletObjB,transform.position + Vector3.right *0.3f,transform.rotation);
            GameObject bulletR = Instantiate(bulletObjB,transform.position + Vector3.left *0.3f,transform.rotation);
            Rigidbody2D rigidL = bulletL.GetComponent<Rigidbody2D>();
            Rigidbody2D rigidR = bulletR.GetComponent<Rigidbody2D>();
            Vector3 dirVecL = player.transform.position - (transform.position + Vector3.right *0.3f);
            Vector3 dirVecR = player.transform.position - (transform.position + Vector3.left *0.3f);
            rigidL.AddForce(dirVecL.normalized*10,ForceMode2D.Impulse);
            rigidR.AddForce(dirVecR.normalized*10,ForceMode2D.Impulse);
        }
        curShotDelay = 0;//총알쏘고 딜레이 변수 0 초기화
    }


    void Reload()
    {
        curShotDelay += Time.deltaTime;
    }

     void OnHit(int dmg)
    {
        health -= dmg;
        if (spriteRenderer != null && sprites != null && sprites.Length > 1 && sprites[1] != null)
        {
            spriteRenderer.sprite = sprites[1];
            Invoke("ReturnSprite", 0.1f);
        }
        if(health <= 0)
        {
            Player playerLogic = player.GetComponent<Player>();
            playerLogic.score += enemyScore;
            Destroy(gameObject);
        }
     }

    void ReturnSprite()
    {
        if (spriteRenderer != null && sprites != null && sprites.Length > 0 && sprites[0] != null)
        {
            spriteRenderer.sprite = sprites[0];
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.tag == "BorderBullet")
        {
            Destroy(gameObject);
        }
        else if(collision.gameObject.tag == "PlayerBullet")
        {
            Bullet bullet = collision.gameObject.GetComponent<Bullet>();
            if (bullet != null)
            {
                OnHit(bullet.Damage);
            }
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.gameObject.tag == "BorderBullet")
        Destroy(gameObject);

        else if(collision.gameObject.tag == "PlayerBullet")
        {
            Bullet bullet = collision.gameObject.GetComponent<Bullet>();
            if (bullet != null)
            {
                OnHit(bullet.Damage);
                Destroy(collision.gameObject);
            }
        }
    }
}
