using UnityEngine;
using UnityEngine.Serialization;

// 총알: BorderBullet 닿으면 풀 반환. 대미지 dmg → Enemy는 Damage로 접근.
public class Bullet : MonoBehaviour
{
    [FormerlySerializedAs("dmg")]
    public int dmg;

    public int Damage => dmg;

    Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // 풀에서 다시 켤 때 이전 속도 제거 — 안 하면 가끔 멈춘 채로 보이거나 impulse가 꼬임
    void OnEnable()
    {
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "BorderBullet")
            gameObject.SetActive(false);
    }
}
