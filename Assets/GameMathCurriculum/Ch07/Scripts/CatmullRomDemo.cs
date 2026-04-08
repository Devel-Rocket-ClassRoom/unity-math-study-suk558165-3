using UnityEngine;
using TMPro;

// =============================================================================
// CatmullRomDemo.cs
// -----------------------------------------------------------------------------
// Catmull-Rom 스플라인으로 경로를 따라 이동하고 접선 방향으로 회전하는 데모
// =============================================================================

public class CatmullRomDemo : MonoBehaviour
{
    [Header("=== 경로 설정 ===")]
    [Tooltip("경로의 중간 지점들 (최소 2개 필요)")]
    [SerializeField] private Transform[] waypoints = new Transform[0];

    [Header("=== 이동 설정 ===")]
    [Range(0.1f, 2f)]
    [Tooltip("스플라인을 따라 이동하는 속도")]
    [SerializeField] private float speed = 0.5f;

    [Tooltip("t값 수동 조작 활성화")]
    [SerializeField] private bool useManualT = false;

    [Tooltip("수동 t값 (0~1)")]
    [Range(0f, 1f)]
    [SerializeField] private float manualT = 0f;

    [Tooltip("경로 루핑 활성화")]
    [SerializeField] private bool loopPath = true;

    [Tooltip("현재 방향(접선)을 보도록 회전")]
    [SerializeField] private bool lookAlongCurve = true;

    [Header("=== 시각화 설정 ===")]
    [SerializeField] private Color colorSpline = Color.green;
    [SerializeField] private Color colorWaypoints = Color.yellow;
    [SerializeField] private Color colorMovingPoint = Color.cyan;

    [Header("=== UI 연결 ===")]
    [Tooltip("정보 표시용 TMP_Text")]
    [SerializeField] private TMP_Text uiInfoText;

    [Header("=== 디버그 정보 (읽기 전용) ===")]
    [SerializeField] private float currentT;
    [SerializeField] private Vector3 currentPosition;
    [SerializeField] private Vector3 currentTangent;
    [SerializeField] private int currentSegment;
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
            if (loopPath)
            {
                currentT = Mathf.Repeat(elapsedTime, 1f);
            }
            else
            {
                currentT = Mathf.Clamp01(elapsedTime);
            }

            currentPosition = EvaluateSpline(currentT);
            currentTangent = EvaluateSplineTangent(currentT);

            transform.position = currentPosition;

            if (lookAlongCurve && currentTangent != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(currentTangent.normalized);
            }
        }

        UpdateUI();
    }

    private Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        return 0.5f * (
            (2f * p1)
            + (-p0 + p2) * t
            + (2f * p0 - 5f * p1 + 4f * p2 - p3) * t * t
            + (-p0 + 3f * p1 - 3f * p2 + p3) * t * t * t
        );
    }

    private Vector3 EvaluateSpline(float t)
    {
        int n = waypoints.Length;
        if (n < 2) return transform.position;

        float globalT = t * (n - 1);
        int segmentIndex = Mathf.FloorToInt(globalT);
        float localT = globalT - segmentIndex;

        if (segmentIndex >= n - 1)
        {
            segmentIndex = n - 2;
            localT = 1f;
        }

        currentSegment = segmentIndex; 
        Vector3 p0 = GetWaypoint(segmentIndex - 1);
        Vector3 p1 = GetWaypoint(segmentIndex);
        Vector3 p2 = GetWaypoint(segmentIndex + 1);
        Vector3 p3 = GetWaypoint(segmentIndex + 2);

        return CatmullRom(p0, p1, p2, p3, localT);
    }

    private Vector3 EvaluateSplineTangent(float t)
    {
        float delta = 0.001f;
        Vector3 a = EvaluateSpline(Mathf.Clamp01(t + delta));
        Vector3 b = EvaluateSpline(Mathf.Clamp01(t - delta));
        return (a - b).normalized; 
    }

    private Vector3 GetWaypoint(int index)
    {
        int n = waypoints.Length;
        int wrappedIndex = Mathf.Clamp(index, 0, n - 1);
        return waypoints[wrappedIndex].position;
    }

    private void OnDrawGizmos()
    {
        if (!enabled) return;

        if (waypoints == null || waypoints.Length < 2) return;

        float wpSize = 0.2f;
        Gizmos.color = colorWaypoints;
        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] == null) continue;
            Gizmos.DrawSphere(waypoints[i].position, wpSize);
        }

        int segments = 100;
        Vector3 prevPoint = EvaluateSpline(0f);
        Gizmos.color = colorSpline;

        for (int i = 1; i <= segments; i++)
        {
            float t = i / (float)segments;
            Vector3 nextPoint = EvaluateSpline(t);
            Gizmos.DrawLine(prevPoint, nextPoint);
            prevPoint = nextPoint;
        }

        Gizmos.color = colorMovingPoint;
        Gizmos.DrawSphere(currentPosition, wpSize * 1.3f);

        if (currentTangent != Vector3.zero)
        {
            VectorGizmoHelper.DrawArrow(currentPosition,
                currentPosition + currentTangent * 0.5f,
                colorMovingPoint, 0.2f);
        }

#if UNITY_EDITOR
        Vector3 labelPos = currentPosition + Vector3.up * 0.6f;
        string info = $"t: {currentT:F3}\nSeg: {currentSegment}";
        VectorGizmoHelper.DrawLabel(labelPos, info, colorMovingPoint);
#endif
    }

    private void UpdateUI()
    {
        if (uiInfoText == null) return;

        int wpCount = waypoints != null ? waypoints.Length : 0;

        uiInfoText.text =
            $"[CatmullRomDemo] Catmull-Rom 스플라인\n" +
            $"경로점 개수: {wpCount}\n" +
            $"현재 t: {currentT:F3}\n" +
            $"현재 구간: {currentSegment}\n" +
            $"위치: ({currentPosition.x:F2}, {currentPosition.y:F2}, {currentPosition.z:F2})\n" +
            $"모드: {(loopPath ? "<color=green>루프</color>" : "<color=red>끝점 멈춤</color>")}";
    }
}
