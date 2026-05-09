using UnityEngine;

// 배경 스크롤 + 타일 순환(아래 타일이 나가면 위로 붙임).
public class Background : MonoBehaviour
{
    public float speed;

    public int startIndex; // 순환 시작 인덱스
    public int endIndex;   // 화면 최하단 타일 인덱스

    public Transform[] sprites;
    float viewHeight; // 카메라 세로 길이(월드 단위)

    private void Awake()
    {
        viewHeight = Camera.main.orthographicSize * 2f;
    }

    void Update()
    {
        Move();
        Scrolling();
    }

    private void Move() // 배경 루트 아래로 이동
    {
        Vector3 curPos = transform.position;
        Vector3 nextPos = Vector3.down * speed * Time.deltaTime;
        transform.position = curPos + nextPos;
    }

    void Scrolling() // endIndex 타일을 위로 재배치 후 인덱스 회전
    {
        if (sprites[endIndex].position.y < viewHeight * (-1))
        {
            Vector3 backSpritePos = sprites[startIndex].localPosition;
            sprites[endIndex].transform.localPosition = backSpritePos + Vector3.up * viewHeight;

            int startIndexSave = startIndex;
            startIndex = endIndex;
            endIndex = (startIndexSave - 1 == -1) ? sprites.Length - 1 : startIndexSave - 1;
        }
    }
}
