using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class NaturalInfiniteScroll : MonoBehaviour
{
    [Header("UI 요소 연결")]
    public RectTransform firstImage;  // 1번째 이미지
    public RectTransform secondImage; // 2번째 이미지
    public RectTransform Canvas_1;

    [Header("설정")]
    public float speed = 100f;        // 흐르는 속도
    public float destroyPoint = -2500f; // 이미지가 왼쪽으로 이 좌표를 넘어가면 뒤로 보냄 (화면 밖 좌표)

    private float distance; // 1번과 2번 사이의 간격 (자동 계산)

    void Start()
    {
        Canvas_1.gameObject.SetActive(true);
        // 시작할 때 배치된 두 이미지 사이의 X축 거리를 계산해서 저장합니다.
        // 예: 1번이 0, 2번이 1500에 있다면 거리는 1500이 됩니다.
        distance = secondImage.anchoredPosition.x - firstImage.anchoredPosition.x;

        // (안전장치) 만약 실수로 2번을 1번보다 왼쪽에 뒀다면 절대값 처리
        distance = Mathf.Abs(distance);
    }

    void Update()
    {

        if(Input.GetKeyDown(KeyCode.Space))
        {
            //Canvas_1.gameObject.SetActive(false); 대화창 종료코드



        }
        // 1. 두 이미지를 왼쪽으로 이동
        firstImage.anchoredPosition += Vector2.left * speed * Time.deltaTime;
        secondImage.anchoredPosition += Vector2.left * speed * Time.deltaTime;

        // 2. 순환 로직 (꼬리 물기)

        // 1번 이미지가 화면 왼쪽 끝(destroyPoint)을 넘어갔다면?
        if (firstImage.anchoredPosition.x < destroyPoint)
        {
            // 2번 이미지의 현재 위치 + 저장해둔 간격(distance) 만큼 뒤로 보냄
            Vector2 newPos = secondImage.anchoredPosition;
            newPos.x += distance;
            firstImage.anchoredPosition = newPos;
        }

        // 2번 이미지가 화면 왼쪽 끝(destroyPoint)을 넘어갔다면?
        if (secondImage.anchoredPosition.x < destroyPoint)
        {
            // 1번 이미지의 현재 위치 + 저장해둔 간격(distance) 만큼 뒤로 보냄
            Vector2 newPos = firstImage.anchoredPosition;
            newPos.x += distance;
            secondImage.anchoredPosition = newPos;
        }
    }
}