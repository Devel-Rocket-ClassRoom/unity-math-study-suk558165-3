// =============================================================================
// ParentChildDemo.cs
// -----------------------------------------------------------------------------
// 부모-자식 관계에서 행렬을 사용한 로컬→월드 좌표 변환과 Unity 자동 계산 결과를 비교
// =============================================================================

using UnityEngine;
using TMPro;

public class ParentChildDemo : MonoBehaviour
{
    [Header("=== 회전 설정 ===")]
    [Tooltip("부모의 회전 속도 (도/초)")]
    [Range(0f, 180f)]
    [SerializeField] private float rotationSpeed = 45f;

    [Tooltip("자동 회전 활성화")]
    [SerializeField] private bool autoRotate = true;

    [Header("=== 자식 참조 ===")]
    [Tooltip("자식 오브젝트 Transform")]
    [SerializeField] private Transform childObject;

    [Header("=== 수동 계산 표시 ===")]
    [Tooltip("수동 계산 결과를 별도로 보이기")]
    [SerializeField] private bool showManualCalculation = true;

    [Header("=== 시각화 설정 ===")]
    [SerializeField] private Color colorParentAxis = Color.cyan;
    [SerializeField] private Color colorChildAxis = new Color(1f, 0.5f, 0f);
    [SerializeField] private Color colorManualResult = new Color(1f, 0.2f, 0.2f);

    [Header("=== UI 연결 ===")]
    [Tooltip("정보 표시용 TMP_Text")]
    [SerializeField] private TMP_Text uiText;

    [Header("=== 디버그 정보 (읽기 전용) ===")]
    [SerializeField] private Vector3 unityWorldPos;
    [SerializeField] private Vector3 manualWorldPos;
    [SerializeField] private float positionDifference;

    private void Update()
    {
        // TODO

        UpdateUI();
    }

    private void OnDrawGizmos()
    {
        if (!enabled) return;

        Vector3 parentPos = transform.position;
        Quaternion parentRot = transform.rotation;

        float parentAxisLength = 2f;
        VectorGizmoHelper.DrawArrow(parentPos, parentPos + parentRot * Vector3.right * parentAxisLength,
            Color.red, 0.2f);
        VectorGizmoHelper.DrawArrow(parentPos, parentPos + parentRot * Vector3.up * parentAxisLength,
            Color.green, 0.2f);
        VectorGizmoHelper.DrawArrow(parentPos, parentPos + parentRot * Vector3.forward * parentAxisLength,
            colorParentAxis, 0.2f);

        if (childObject == null) return;

        Vector3 childLocalPos = childObject.localPosition;

        Vector3 childWorldPos = childObject.position;

        float childAxisLength = 0.8f;
        VectorGizmoHelper.DrawArrow(childWorldPos, childWorldPos + parentRot * Vector3.right * childAxisLength,
            Color.red, 0.15f);
        VectorGizmoHelper.DrawArrow(childWorldPos, childWorldPos + parentRot * Vector3.up * childAxisLength,
            Color.green, 0.15f);
        VectorGizmoHelper.DrawArrow(childWorldPos, childWorldPos + parentRot * Vector3.forward * childAxisLength,
            colorChildAxis, 0.15f);

        Gizmos.color = new Color(1f, 1f, 0f, 1f);
        Gizmos.DrawWireSphere(childWorldPos, 0.2f);

#if UNITY_EDITOR
        VectorGizmoHelper.DrawLabel(childWorldPos + Vector3.up * 0.4f,
            "Unity 자동 계산", new Color(1f, 1f, 0f, 1f));
#endif

        if (showManualCalculation)
        {
            Matrix4x4 parentMatrix = Matrix4x4.TRS(parentPos, parentRot, Vector3.one);
            Vector3 manualPos = parentMatrix.MultiplyPoint3x4(childLocalPos);

            Gizmos.color = colorManualResult;
            Gizmos.DrawWireSphere(manualPos, 0.25f);

#if UNITY_EDITOR
            VectorGizmoHelper.DrawLabel(manualPos + Vector3.up * 0.4f,
                "수동 계산\n(행렬)", colorManualResult);
#endif
        }

        Gizmos.color = new Color(0.5f, 0.5f, 1f, 0.5f);
        Gizmos.DrawLine(parentPos, childWorldPos);

        if (autoRotate)
        {
            Gizmos.color = new Color(0.2f, 1f, 0.2f, 0.5f);
            float arcRadius = Vector3.Distance(parentPos, childWorldPos);
            if (arcRadius > 0.1f)
            {
                DrawRotationArc(parentPos, parentRot, arcRadius);
            }
        }
    }

    private void DrawRotationArc(Vector3 center, Quaternion rotation, float radius)
    {
        int segments = 12;
        Vector3 prevPoint = center + rotation * Vector3.forward * radius;

        for (int i = 1; i <= segments; i++)
        {
            float angle = (360f / segments) * i;
            Vector3 nextPoint = center + Quaternion.Euler(0f, angle, 0f) * Vector3.forward * radius;
            Gizmos.DrawLine(prevPoint, nextPoint);
            prevPoint = nextPoint;
        }
    }

    private void UpdateUI()
    {
        if (uiText == null) return;

        string verifyText = positionDifference < 0.001f ?
            "<color=green>완벽 일치</color>" :
            $"<color=orange>오차: {positionDifference:F4}</color>";

        uiText.text =
            $"[ParentChildDemo] 부모-자식 관계와 행렬\n" +
            $"\n" +
            $"<b>부모 정보:</b>\n" +
            $"위치: {transform.position:F2}\n" +
            $"회전: {transform.eulerAngles:F1}°\n" +
            $"\n" +
            $"<b>자식 정보:</b>\n" +
            $"로컬 위치: {childObject.localPosition:F2}\n" +
            $"\n" +
            $"<b>계산 비교:</b>\n" +
            $"Unity 자동: {unityWorldPos:F2}\n" +
            $"수동 계산:  {manualWorldPos:F2}\n" +
            $"검증: {verifyText}\n" +
            $"\n" +
            $"<b>원리:</b>\n" +
            $"자식 월드 = Parent.matrix × Child.local\n" +
            $"= P + R × (C_local)";
    }
}
