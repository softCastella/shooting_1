using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Follower : MonoBehaviour
{
    public float maxShotDelay; // 발사 간격 상한(연사 제한에 사용 가능)
    public float curShotDelay; // 현재 장전 경과 시간
    public ObjectManager objectManager;
    //팔로우가 따라다닐 위치치
    public Vector3 followPos;
    public int followDelay;
    public Transform parent;
    public Queue<Vector3> parentPos;  //FIFO: 먼저들어가면 먼저나옴

    void Awake()
    {
        parentPos = new Queue<Vector3>();
    }

    void OnEnable()
    {
        if (parentPos != null)
            parentPos.Clear();
        if (parent != null)
            followPos = parent.position;
    }

    void Update()
    {
        Watch();
        Follow();
        Fire();
        Reload();
    }

    void Watch()
    {
        if (parent == null)
            return;

        // 부모 위치가 바뀐 경우만 기록(정지 시 동일 좌표 반복 enqueue 방지)
        if (!parentPos.Contains(parent.position))
            parentPos.Enqueue(parent.position);

        // 지연 큐가 차기 전에는 플레이어 위치를 따름 — 미갱신 시 followPos=(0,0,0) 버그 방지
        if (parentPos.Count > followDelay)
            followPos = parentPos.Dequeue();
        else
            followPos = parent.position;
    }

    void Follow() // 이동 입력 + 경계에서 막음
    {
        transform.position = followPos;
    }

    // Fire1: 플레이어 총알 풀에서 MakeObj 후 발사 (Enemy와 같이 maxShotDelay 간격으로만 발사)
    void Fire()
    {
        if (!Input.GetButton("Fire1"))
            return;

        if (objectManager == null)
            return;

        // 인스펙터에서 0이면 매 프레임 발사되어 풀고갈·겹침이 나므로 최소 간격 사용
        float shotInterval = maxShotDelay > 0f ? maxShotDelay : 0.15f;
        if (curShotDelay < shotInterval)
            return;

        GameObject bullet = objectManager.MakeObj("bulletFollower");
        if (bullet == null)
            return;

        bullet.transform.position = transform.position;

        Rigidbody2D rigid = bullet.GetComponent<Rigidbody2D>();
        if (rigid == null)
            return;

        rigid.AddForce(Vector2.up * 10, ForceMode2D.Impulse);

        curShotDelay = 0; // 발사 직후 장전 타이머 리셋
    }

    void Reload() // 장전 시간 누적
    {
        curShotDelay += Time.deltaTime;
    }

    // Fire2 폭탄: 활성 적 피격, 적 총알 풀 순회해 비활성화
}
