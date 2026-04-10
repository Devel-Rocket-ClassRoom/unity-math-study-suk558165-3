using UnityEngine;
using UnityEngine.UI;

public class OffScreenIndicator : MonoBehaviour
{
    [SerializeField] private Transform[] targets; // 추적할 타겟들 (플레이어, 적 등)
    [SerializeField] private RectTransform[] indicators; // 타겟에 대응하는 UI 인디케이터 (화면 가장자리에 표시)
    [SerializeField] private float padding = 50f; // 화면 가장자리에서 인디케이터가 떨어지는 거리
    private Camera cam;
     

    void Start()
    {
       cam = Camera.main;
        for (int i = 0; i < targets.Length; i++) // 타겟의 색상을 인디케이터에 적용 (타겟마다 다른 색상으로 표시)
        {
            Color color = targets[i].GetComponent<Renderer>().material.color; // 타겟의 색상 가져오기
            indicators[i].GetComponent<Image>().color = color; // 인디케이터 색상 설정
            indicators[i].sizeDelta = new Vector2(30f, 30f); // 인디케이터 크기 설정
        }
    }

    void Update()
    {

        for (int i = 0; i < targets.Length; i++)
        {
            Vector3 screenPos = cam.WorldToScreenPoint(targets[i].position); // 타겟의 월드 좌표를 화면 좌표로 변환

            if (screenPos.z < 0)
            {
                screenPos *= -1; // 카메라 뒤에 있는 경우, 반대 방향으로 표시
            }

            bool isOffScreen = screenPos.x < 0 || // 화면 밖에 있는지 체크
                screenPos.x > Screen.width ||  
                screenPos.y < 0 || screenPos.y > Screen.height;

            if (isOffScreen)
            {
                indicators[i].gameObject.SetActive(true); // 인디케이터 활성화

                Vector3 center = new Vector3(Screen.width / 2, Screen.height / 2f, 0); // 화면 중심점
                Vector3 dir = (screenPos - center).normalized; // 타겟 방향 벡터 (화면 중심에서 타겟까지의 방향)
                float slope = dir.y / dir.x; // 타겟 방향의 기울기 계산
                float halfW = Screen.width / 2f - padding; // 화면 절반 너비에서 패딩을 뺀 값
                float halfH = Screen.height / 2f - padding; // 화면 절반 높이에서 패딩을 뺀 값

                if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y)) // x 방향이 더 멀리 있는 경우, 화면의 좌우 가장자리에 인디케이터를 배치
                {
                    float edgeX = halfW * Mathf.Sign(dir.x); // x 방향으로 패딩을 적용한 가장자리 위치 계산
                    screenPos = center + new Vector3(edgeX, edgeX * slope, 0); // 화면 중심에서 가장자리로 향하는 방향에 따라 인디케이터 위치 계산
                }
                else
                {
                    float edgeY = halfH * Mathf.Sign(dir.y); // y 방향으로 패딩을 적용한 가장자리 위치 계산
                    screenPos = center + new Vector3(edgeY / slope, edgeY, 0); // 화면 중심에서 가장자리로 향하는 방향에 따라 인디케이터 위치 계산
                }

                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg; // 타겟 방향에 따라 인디케이터 회전 (화살표가 타겟을 가리키도록)
                indicators[i].rotation = Quaternion.Euler(0, 0, angle - 90); // 인디케이터가 타겟을 가리키도록 회전 (기본 화살표가 위쪽을 향하므로 -90도 보정)
                indicators[i].position = screenPos; // 계산된 화면 가장자리 위치에 인디케이터 배치
            }
            else
            {
                indicators[i].gameObject.SetActive(false);
            }
            }
        }
 }

