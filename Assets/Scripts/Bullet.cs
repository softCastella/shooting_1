using UnityEngine;
using UnityEngine.Serialization;

public class Bullet : MonoBehaviour
{
    [FormerlySerializedAs("dmg")]
    public int dmg;
    public bool isRotate;

    public int Damage => dmg;

    [Header("Boss arc pattern (Enemy.FireArc)")]
    [SerializeField] float arcDownSpeed = 2.1f;
    [SerializeField] float arcLateralAmplitude = 1.15f;
    [Tooltip("아래로 1유닛 내려올 때 위상이 몇 라디안 진행할지 — 물결 간격(공간 기준)")]
    [SerializeField] float arcWobbleRadPerUnitFallen = 0.85f;

    Rigidbody2D rb;
    bool arcTrajectoryActive;
    float arcPhaseRad;
    float arcSpawnY;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void OnEnable()
    {
        arcTrajectoryActive = false;
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    }

    void FixedUpdate()
    {
        if (!arcTrajectoryActive || rb == null)
            return;
        // 낙하 거리만큼만 가로 성분 증가: Cos(phase+fallen*k)-Cos(phase) → 발사 직후 vx=0 (한 덩어리로 안 갈림)
        float fallen = Mathf.Max(0f, arcSpawnY - transform.position.y);
        float wobble = Mathf.Cos(arcPhaseRad + fallen * arcWobbleRadPerUnitFallen) - Mathf.Cos(arcPhaseRad);
        float vx = arcLateralAmplitude * wobble;
        rb.velocity = new Vector2(vx, -arcDownSpeed);
    }

    void Update()
    {
        if (isRotate)
            transform.Rotate(Vector3.forward * 10);
    }

    /// <summary>보스 아크 패턴: 처음은 아래로 동일, phase는 낙하 후 물결 위치만 어긋나게 함.</summary>
    public void EnableArcTrajectory(float phaseRadians)
    {
        arcTrajectoryActive = true;
        arcPhaseRad = phaseRadians;
        arcSpawnY = transform.position.y;
        if (rb != null)
            rb.velocity = new Vector2(0f, -arcDownSpeed);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "BorderBullet")
            gameObject.SetActive(false);
        // 보스/적 탄(EnemyBullet)도 Bullet 스크립트를 쓰므로, 플레이어 탄일 때만 Enemy 피격 처리
        else if (CompareTag("PlayerBullet") && collision.CompareTag("Enemy"))
        {
            Enemy enemy = collision.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.OnHit(dmg);
                gameObject.SetActive(false);
            }
        }
    }
}
