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
        for (int i = 0; i < targets.Length; i++)
        {
            Color color = targets[i].GetComponent<Renderer>().material.color;
            indicators[i].GetComponent<Image>().color = color;
            indicators[i].sizeDelta = new Vector2(30f, 30f);
        }
    }

    void Update()
    {

        for (int i = 0; i < targets.Length; i++)
        {
            Vector3 screenPos = cam.WorldToScreenPoint(targets[i].position);

            if (screenPos.z < 0)
            {
                screenPos *= -1; // 카메라 뒤에 있는 경우, 반대 방향으로 표시
            }

            bool isOffScreen = screenPos.x < 0 || // 화면 밖에 있는지 체크
                screenPos.x > Screen.width || 
                screenPos.y < 0 || screenPos.y > Screen.height;

            if (isOffScreen)
            {
                indicators[i].gameObject.SetActive(true); // 오프스크린이면 인디케이터 숨김   
            }
            else
            {
                indicators[i].gameObject.SetActive(false); // 화면 안에 있으면 인디케이터 숨김
                 
                Vector3 center = new Vector3(Screen.width / 2, Screen.height / 2f, 0); // 화면 중심
                Vector3 dir = (screenPos - center).normalized; // 중심에서 타겟 방향 계산
                float slope = dir.y / dir.x;
                float halfW = Screen.width / 2f - padding;
                float halfH = Screen.height / 2f - padding;

                if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
                {
                    // 좌우 가장자리에 닿는 경우
                    float edgeX = halfW * Mathf.Sign(dir.x);
                    screenPos = center + new Vector3(edgeX, edgeX * slope, 0);
                }
                else
                {
                    // 상하 가장자리에 닿는 경우
                    float edgeY = halfH * Mathf.Sign(dir.y);
                    screenPos = center + new Vector3(edgeY / slope, edgeY, 0);
                }
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                indicators[i].rotation = Quaternion.Euler(0, 0, angle - 90);
                indicators[i].position = screenPos;
            }
        }
    }
}
