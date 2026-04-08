using UnityEngine;
using TMPro;

// =============================================================================
// BezierCurveDemo.cs
// -----------------------------------------------------------------------------
// 선형, 이차, 삼차 베지어 곡선 위의 점을 계산하고 오브젝트를 이동시키는 데모
// =============================================================================

public class BezierCurveDemo : MonoBehaviour
{
    public enum BezierType { Linear, Quadratic, Cubic }

    [Header("=== 곡선 설정 ===")]
    [Tooltip("베지어 곡선 종류 (선형/이차/삼차)")]
    [SerializeField] private BezierType curveType = BezierType.Cubic;

    [Tooltip("제어점 0 (곡선 시작)")]
    [SerializeField] private Vector3 p0 = new Vector3(-3f, 0f, 0f);

    [Tooltip("제어점 1")]
    [SerializeField] private Vector3 p1 = new Vector3(-1f, 3f, 0f);

    [Tooltip("제어점 2")]
    [SerializeField] private Vector3 p2 = new Vector3(1f, 3f, 0f);

    [Tooltip("제어점 3 (곡선 끝)")]
    [SerializeField] private Vector3 p3 = new Vector3(3f, 0f, 0f);

    [Header("=== 이동 설정 ===")]
    [Range(0.1f, 2f)]
    [Tooltip("곡선을 따라 이동하는 속도")]
    [SerializeField] private float speed = 0.5f;

    [Tooltip("t값 수동 조작 활성화")]
    [SerializeField] private bool useManualT = false;

    [Tooltip("수동 t값 (0~1)")]
    [Range(0f, 1f)]
    [SerializeField] private float manualT = 0f;

    [Tooltip("곡선을 반복할 때, 루프(Loop) 또는 핑퐁(Ping-Pong) 방식")]
    [SerializeField] private bool usePingPong = false;

    [Header("=== 시각화 설정 ===")]
    [SerializeField] private Color colorCurve = Color.cyan;
    [SerializeField] private Color colorControlPoints = Color.yellow;
    [SerializeField] private Color colorMovingPoint = Color.green;
    [SerializeField] private Color colorControlLines = Color.gray;

    [Header("=== UI 연결 ===")]
    [Tooltip("정보 표시용 TMP_Text")]
    [SerializeField] private TMP_Text uiInfoText;

    [Header("=== 디버그 정보 (읽기 전용) ===")]
    [SerializeField] private float currentT;
    [SerializeField] private Vector3 currentPosition;
    [SerializeField] private float elapsedTime;

    private void Start()
    {
        elapsedTime = 0f;
        currentT = 0f;
    }

    private void Update()
    {
        if (useManualT)
        {
            currentT = manualT;
        }
        else
        {
            elapsedTime += Time.deltaTime * speed;
            if (usePingPong)
            {
                currentT = Mathf.PingPong(elapsedTime, 1f);
            }
            else
            {
                currentT = Mathf.Repeat(elapsedTime, 1f);
            }
            currentPosition = EvaluateBezier(currentT);
            transform.position = currentPosition;
            UpdateUI();
        }
    }

    private Vector3 EvaluateBezier(float t)
    {
        // TODO
        return curveType switch
        {
            BezierType.Linear => LinearBezier(p0, p3, t),
            BezierType.Quadratic => QuadraticBezier(p0, p1, p2, t),
            BezierType.Cubic => CubicBezier(p0, p1, p2, p3, t),
            _ => Vector3.zero
        };
    }

    Vector3 LinearBezier(Vector3 p0, Vector3 p1, float t)
    {
        return (1f - t) * p0 + t * p1;  // = Vector3.Lerp(p0, p1, t)
    }

    Vector3 QuadraticBezier(Vector3 p0, Vector3 p1, Vector3 p2, float t)
    {
        // Lerp의 Lerp (de Casteljau 알고리즘)
        Vector3 a = Vector3.Lerp(p0, p1, t);
        Vector3 b = Vector3.Lerp(p1, p2, t);
        return Vector3.Lerp(a, b, t);
    }

    Vector3 CubicBezier(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        // de Casteljau 알고리즘 — 3단계 Lerp
        Vector3 a = Vector3.Lerp(p0, p1, t);
        Vector3 b = Vector3.Lerp(p1, p2, t);
        Vector3 c = Vector3.Lerp(p2, p3, t);

        Vector3 d = Vector3.Lerp(a, b, t);
        Vector3 e = Vector3.Lerp(b, c, t);

        return Vector3.Lerp(d, e, t);
    }

    private void OnDrawGizmos()
    {
        if (!enabled) return;

        float cpSize = 0.15f;
        Vector3 labelOffset = Vector3.up * 0.3f;

        Gizmos.color = colorControlPoints;
        Gizmos.DrawSphere(p0, cpSize);

        switch (curveType)
        {
            case BezierType.Linear:
                Gizmos.DrawSphere(p3, cpSize);
                break;

            case BezierType.Quadratic:
                Gizmos.DrawSphere(p1, cpSize);
                Gizmos.DrawSphere(p2, cpSize);
                Gizmos.color = colorControlLines;
                Gizmos.DrawLine(p0, p1);
                Gizmos.DrawLine(p1, p2);
                break;

            case BezierType.Cubic:
                Gizmos.DrawSphere(p1, cpSize);
                Gizmos.DrawSphere(p2, cpSize);
                Gizmos.DrawSphere(p3, cpSize);
                Gizmos.color = colorControlLines;
                Gizmos.DrawLine(p0, p1);
                Gizmos.DrawLine(p1, p2);
                Gizmos.DrawLine(p2, p3);
                break;
        }

#if UNITY_EDITOR
        VectorGizmoHelper.DrawLabel(p0 + labelOffset, "P0", colorControlPoints);
        switch (curveType)
        {
            case BezierType.Linear:
                VectorGizmoHelper.DrawLabel(p3 + labelOffset, "P3", colorControlPoints);
                break;
            case BezierType.Quadratic:
                VectorGizmoHelper.DrawLabel(p1 + labelOffset, "P1", colorControlPoints);
                VectorGizmoHelper.DrawLabel(p2 + labelOffset, "P2", colorControlPoints);
                break;
            case BezierType.Cubic:
                VectorGizmoHelper.DrawLabel(p1 + labelOffset, "P1", colorControlPoints);
                VectorGizmoHelper.DrawLabel(p2 + labelOffset, "P2", colorControlPoints);
                VectorGizmoHelper.DrawLabel(p3 + labelOffset, "P3", colorControlPoints);
                break;
        }
#endif

        int segments = 50;
        Vector3 prevPoint = EvaluateBezier(0f);
        Gizmos.color = colorCurve;

        for (int i = 1; i <= segments; i++)
        {
            float t = i / (float)segments;
            Vector3 nextPoint = EvaluateBezier(t);
            Gizmos.DrawLine(prevPoint, nextPoint);
            prevPoint = nextPoint;
        }

        Gizmos.color = colorMovingPoint;
        Gizmos.DrawSphere(currentPosition, cpSize * 1.2f);

#if UNITY_EDITOR
        Vector3 labelPos = currentPosition + Vector3.up * 0.5f;
        string info = $"t: {currentT:F3}\nCurve: {curveType}";
        VectorGizmoHelper.DrawLabel(labelPos, info, colorMovingPoint);
#endif
    }

    private void UpdateUI()
    {
        if (uiInfoText == null) return;

        uiInfoText.text =
            $"[BezierCurveDemo] 베지어 곡선\n" +
            $"곡선 타입: <color=cyan>{curveType}</color>\n" +
            $"t 값: {currentT:F3}\n" +
            $"현재 위치: ({currentPosition.x:F2}, {currentPosition.y:F2}, {currentPosition.z:F2})\n" +
            $"모드: {(usePingPong ? "핑퐁" : "루프")}";
    }
}
