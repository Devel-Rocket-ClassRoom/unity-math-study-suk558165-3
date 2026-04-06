// =============================================================================
// TRSMatrixDemo.cs
// -----------------------------------------------------------------------------
// TRS 행렬 단계별 파이프라인(S→R→T)을 시각화하고 변환 순서 차이를 비교
// =============================================================================

using UnityEngine;
using TMPro;

public class TRSMatrixDemo : MonoBehaviour
{
    [Header("=== TRS 파라미터 ===")]
    [Tooltip("이동 거리")]
    [SerializeField] private Vector3 translation = new Vector3(3f, 0f, 0f);

    [Tooltip("Y축 회전 각도 (도)")]
    [Range(0f, 360f)]
    [SerializeField] private float rotationAngle = 45f;

    [Tooltip("스케일 배수")]
    [SerializeField] private Vector3 scale = Vector3.one;

    [Header("=== 시각화 설정 ===")]
    [Tooltip("S→R→T 중간 단계 표시")]
    [SerializeField] private bool showSteps = true;

    [Tooltip("올바른 순서(T×R×S)와 잘못된 순서(R×T×S)를 비교")]
    [SerializeField] private bool showOrderComparison = true;

    [Header("=== UI 연결 ===")]
    [Tooltip("정보 표시용 TMP_Text")]
    [SerializeField] private TMP_Text uiText;

    [Header("=== 디버그 정보 (읽기 전용) ===")]
    [SerializeField] private Matrix4x4 currentTRS;
    [SerializeField] private Vector3 transformedPoint;

    // 비대칭 ㄱ자 기본 도형 — 회전/스케일 효과가 명확히 드러나는 형태
    private static readonly Vector3[] baseShape =
    {
        Vector3.zero,               // [0] 꼭짓점: 원점
        new Vector3(1.5f, 0f, 0f),  // [1] X 방향 (긴 팔)
        new Vector3(0f, 0f, 0.8f),  // [2] Z 방향 (짧은 팔)
    };

    private void Update()
    {
        // TODO

        UpdateUI();
    }

    private void OnDrawGizmos()
    {
        if (!enabled) return;

        Vector3 originPos = transform.position;
        Quaternion rotQuat = Quaternion.Euler(0f, rotationAngle, 0f);

        // 1. 원본 도형 (흰색)
        DrawShape(originPos, Matrix4x4.identity, Color.white, "원본");

        // 2~3. 중간 단계
        if (showSteps)
        {
            // TODO
        }

        // 3. 최종 T × R × S 결과 (초록)
        Matrix4x4 trsMatrix = Matrix4x4.TRS(translation, rotQuat, scale);
        DrawShape(originPos, trsMatrix, Color.green, "③ 최종 (T×R×S)");

        // 원점 → 최종 위치 가이드 점선
        Vector3 finalOrigin = originPos + trsMatrix.MultiplyPoint3x4(Vector3.zero);
        DrawDashedLine(originPos, finalOrigin, new Color(1f, 1f, 1f, 0.3f));

        // 4. 순서 비교
        if (showOrderComparison)
        {
            // TODO
        }

        // 원점 표시
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(originPos, 0.15f);

#if UNITY_EDITOR
        VectorGizmoHelper.DrawLabel(originPos + Vector3.down * 0.4f, "원점", Color.white);
#endif
    }

    /// <summary>
    /// 기본 ㄱ자 도형을 행렬로 변환한 후 Gizmos로 그립니다.
    /// </summary>
    private void DrawShape(Vector3 worldOffset, Matrix4x4 matrix, Color color, string label)
    {
        Vector3[] pts = new Vector3[baseShape.Length];
        for (int i = 0; i < baseShape.Length; i++)
            pts[i] = worldOffset + matrix.MultiplyPoint3x4(baseShape[i]);

        VectorGizmoHelper.DrawArrow(pts[0], pts[1], color, 0.15f);
        VectorGizmoHelper.DrawArrow(pts[0], pts[2], color, 0.12f);

        Gizmos.color = color;
        Gizmos.DrawWireSphere(pts[0], 0.08f);

#if UNITY_EDITOR
        VectorGizmoHelper.DrawLabel(pts[0] + Vector3.up * 0.35f, label, color);
#endif
    }

    /// <summary>
    /// 두 점 사이에 점선(가이드 라인)을 그립니다.
    /// </summary>
    private static void DrawDashedLine(Vector3 from, Vector3 to, Color color, float dashLength = 0.25f)
    {
        Color prev = Gizmos.color;
        Gizmos.color = color;

        Vector3 dir = to - from;
        float dist = dir.magnitude;
        if (dist < 0.001f) { Gizmos.color = prev; return; }
        dir /= dist;

        float drawn = 0f;
        bool draw = true;
        while (drawn < dist)
        {
            float segLen = Mathf.Min(dashLength, dist - drawn);
            if (draw)
                Gizmos.DrawLine(from + dir * drawn, from + dir * (drawn + segLen));
            drawn += segLen;
            draw = !draw;
        }

        Gizmos.color = prev;
    }

    private void UpdateUI()
    {
        if (uiText == null) return;

        string stepsNote = showSteps
            ? "흰색: 원본 → 노랑: ①Scale → 청록: ②Rotate → 초록: ③최종"
            : "초록: T×R×S 최종 결과만 표시";

        string orderNote = showOrderComparison
            ? "\n\n<b>순서 비교:</b>\n" +
              "<color=green>초록: T×R×S (올바른 순서)</color>\n" +
              "<color=#FF3366>빨강: R×T×S (잘못된 순서 — 원점 중심 궤도!)</color>"
            : "";

        uiText.text =
            $"[TRSMatrixDemo] TRS 행렬 — 단계별 파이프라인\n" +
            $"\n" +
            $"<b>TRS 파라미터:</b>\n" +
            $"이동(T): {translation:F2}\n" +
            $"회전(R): {rotationAngle:F1}°\n" +
            $"스케일(S): {scale:F2}\n" +
            $"\n" +
            $"<b>적용 순서:</b> S → R → T (오른쪽→왼쪽)\n" +
            $"변환된 점(X-tip): {transformedPoint:F2}\n" +
            $"\n" +
            $"<b>시각화:</b>\n" +
            $"{stepsNote}" +
            $"{orderNote}";
    }
}
