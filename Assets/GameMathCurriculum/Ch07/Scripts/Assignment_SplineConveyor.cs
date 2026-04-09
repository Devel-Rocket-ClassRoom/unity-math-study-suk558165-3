// =============================================================================
// Assignment_SplineConveyor.cs
// -----------------------------------------------------------------------------
// Catmull-Rom 스플라인 위를 여러 박스가 일정 간격으로 순환 이동하는 컨베이어.
//       AnimationCurve 로 구간별 속도를 조절한다.
// =============================================================================

using UnityEngine;
using TMPro;

public class Assignment_SplineConveyor : MonoBehaviour
{
    [Header("=== 스플라인 경로 ===")]
    [SerializeField] private Transform[] waypoints;

    [Header("=== 컨베이어 박스 ===")]
    [SerializeField] private Transform[] boxes;

    [Header("=== 속도 프로파일 ===")]
    [Tooltip("x축: globalT(0~1), y축: 속도 배율. 기본 Linear는 일정 속도")]
    [SerializeField] private AnimationCurve speedCurve = AnimationCurve.Linear(0f, 1f, 1f, 1f);

    [Range(1f, 20f)] [SerializeField] private float cycleDuration = 6f;

    [Header("=== 시각화 ===")]
    [Range(10, 100)] [SerializeField] private int splineResolution = 50;

    [Header("=== UI 연결 ===")]
    [SerializeField] private TMP_Text uiInfoText;

    [Header("=== 디버그 정보 (읽기 전용) ===")]
    [SerializeField] private float globalT;
    [SerializeField] private float currentSpeedMultiplier;

    private void Update()
    {
        currentSpeedMultiplier = speedCurve.Evaluate(globalT); // globalT에 따른 속도 배율 계산
        globalT += Time.deltaTime / cycleDuration * currentSpeedMultiplier; // 속도 배율을 적용하여 globalT 증가
        if (globalT > 1f) // globalT가 1을 초과하면 반복
        {
           globalT = Mathf.Repeat(globalT, 1f); 
        }
        for (int i = 0; i < waypoints.Length; i++) // 각 박스의 위치 계산
        {
           float boxT = Mathf.Repeat(globalT + (float)i / waypoints.Length, 1f); // 박스마다 globalT에 일정 간격을 더해서 위치 계산
            boxes[i].position = EvaluateSpline(waypoints, boxT); // boxT에 해당하는 스플라인 위치 계산
        }
        UpdateUI();
    }

    private Vector3 EvaluateSpline(Transform[] pts, float t)
    {
        int segmentCount = pts.Length - 1; // 스플라인 구간 수 (웨이포인트 수 - 1)
        float scaledT = t * segmentCount; // globalT(0~1)을 전체 구간 수로 스케일링
        int segment = Mathf.Clamp((int)scaledT, 0, segmentCount - 1); // 현재 구간 인덱스 계산 (0 ~ segmentCount-1)
        float localT = scaledT - segment; // 현재 구간 내에서의 t (0 ~ 1)

        Vector3 p0 = pts[Mathf.Max(0, segment - 1)].position; // 이전 구간의 웨이포인트 (첫 번째 구간에서는 첫 번째 웨이포인트를 반복)
        Vector3 p1 = pts[segment].position; // 현재 구간의 시작 웨이포인트
        Vector3 p2 = pts[Mathf.Min(pts.Length - 1, segment + 1)].position; // 현재 구간의 끝 웨이포인트 (마지막 구간에서는 마지막 웨이포인트를 반복)
        Vector3 p3 = pts[Mathf.Min(pts.Length - 1, segment + 2)].position; // 다음 구간의 웨이포인트 (마지막 구간에서는 마지막 웨이포인트를 반복)

        return CatmullRom(p0, p1, p2, p3, localT); // Catmull-Rom 스플라인 계산
    }

    private Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        float t2 = t * t, t3 = t2 * t;
        return 0.5f * (
            2f * p1 +
            (-p0 + p2) * t +
            (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 +
            (-p0 + 3f * p1 - 3f * p2 + p3) * t3
        );
    }

    private void UpdateUI()
    {
        if (uiInfoText == null) return;

        int boxCount = boxes != null ? boxes.Length : 0;
        int wpCount = waypoints != null ? waypoints.Length : 0;

        uiInfoText.text = $"[Assignment_SplineConveyor] 스플라인 컨베이어\n"
                        + $"웨이포인트: {wpCount}개 / 박스: {boxCount}개\n"
                        + $"globalT: {globalT:F2}\n"
                        + $"속도 배율: {currentSpeedMultiplier:F2}×\n"
                        + $"주기: {cycleDuration:F1}초\n"
                        + $"\n<color=yellow>AnimationCurve로 구간별 속도 조절</color>";
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!enabled) return;
        if (waypoints == null || waypoints.Length < 2) return;

        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] == null) continue;
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(waypoints[i].position, 0.2f);
        }

        Gizmos.color = Color.cyan;
        Vector3 prev = EvaluateSpline(waypoints, 0f);
        for (int i = 1; i <= splineResolution; i++)
        {
            float t = i / (float)splineResolution;
            Vector3 curr = EvaluateSpline(waypoints, t);
            Gizmos.DrawLine(prev, curr);
            prev = curr;
        }
    }
#endif
}
