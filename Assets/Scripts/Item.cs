using UnityEngine;

public class Item : MonoBehaviour
{
    public string type;
    Rigidbody2D rigid;


    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        rigid.velocity = Vector2.down * 0.1f;
    }

}
