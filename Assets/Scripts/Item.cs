using UnityEngine;

// 드랍 아이템: type으로 Player에서 분기. 수집 시 풀 반환(SetActive false).
public class Item : MonoBehaviour
{
    public string type; // "Coin", "Power", "Boom" — Player.OnTriggerEnter2D와 일치해야 함
    Rigidbody2D rigid;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
    }

    void OnEnable() // 풀에서 켤 때 아래로 이동
    {
        rigid.velocity = Vector2.down * 1.5f;
    }
}
