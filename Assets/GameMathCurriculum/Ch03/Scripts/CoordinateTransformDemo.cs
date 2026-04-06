// =============================================================================
// CoordinateTransformDemo.cs
// -----------------------------------------------------------------------------
// TransformPoint와 InverseTransformPoint를 사용한 로컬↔월드 좌표 변환 시각화
// =============================================================================

using UnityEngine;
using TMPro;

public class CoordinateTransformDemo : MonoBehaviour
{
    [Header("=== 좌표 변환 대상 ===")]
    [Tooltip("자식 오브젝트 (로컬 위치를 월드 좌표로 변환할 대상)")]
    [SerializeField] private Transform childObject;

    [Tooltip("월드 타겟 (월드 좌표를 로컬 좌표로 역변환할 대상)")]
    [SerializeField] private Transform worldTarget;

    [Header("=== 시각화 설정 ===")]
    [SerializeField] private Color colorLocalX = Color.red;
    [SerializeField] private Color colorLocalY = Color.green;
    [SerializeField] private Color colorLocalZ = Color.blue;
    [SerializeField] private Color colorTransformedPoint = new Color(1f, 1f, 0f, 1f);

    [Header("=== UI 연결 ===")]
    [Tooltip("정보 표시용 TMP_Text")]
    [SerializeField] private TMP_Text uiText;

    [Header("=== 디버그 정보 (읽기 전용) ===")]
    [SerializeField] private Vector3 childLocalPos;
    [SerializeField] private Vector3 childWorldPos;
    [SerializeField] private Vector3 targetWorldPos;
    [SerializeField] private Vector3 targetLocalPos;

    private void Update()
    {
        if (childObject == null || worldTarget == null) return;

        // TODO

        UpdateUI();
    }

    private void OnDrawGizmos()
    {
        if (!enabled) return;

        if (childObject == null || worldTarget == null) return;

        Vector3 parentPos = transform.position;
        Quaternion parentRot = transform.rotation;

        float axisLength = 2f;
        VectorGizmoHelper.DrawArrow(parentPos, parentPos + parentRot * Vector3.right * axisLength,
            colorLocalX, 0.2f);
        VectorGizmoHelper.DrawArrow(parentPos, parentPos + parentRot * Vector3.up * axisLength,
            colorLocalY, 0.2f);
        VectorGizmoHelper.DrawArrow(parentPos, parentPos + parentRot * Vector3.forward * axisLength,
            colorLocalZ, 0.2f);

        if (childObject != null)
        {
            Vector3 childLocalPos = childObject.localPosition;
            Vector3 childWorldPos = transform.TransformPoint(childLocalPos);

            VectorGizmoHelper.DrawArrow(childWorldPos, childWorldPos + parentRot * Vector3.right * 0.5f,
                colorLocalX, 0.15f);
            VectorGizmoHelper.DrawArrow(childWorldPos, childWorldPos + parentRot * Vector3.up * 0.5f,
                colorLocalY, 0.15f);
            VectorGizmoHelper.DrawArrow(childWorldPos, childWorldPos + parentRot * Vector3.forward * 0.5f,
                colorLocalZ, 0.15f);

            Gizmos.color = colorTransformedPoint;
            Gizmos.DrawWireSphere(childWorldPos, 0.25f);

#if UNITY_EDITOR
            VectorGizmoHelper.DrawLabel(childWorldPos + Vector3.up * 0.5f,
                $"TransformPoint\nLocal→World", colorTransformedPoint);
#endif
        }

        if (worldTarget != null)
        {
            Vector3 targetWorldPos = worldTarget.position;
            Vector3 targetLocalPos = transform.InverseTransformPoint(targetWorldPos);

            Gizmos.color = new Color(0.5f, 0.5f, 1f, 1f);
            Gizmos.DrawWireSphere(targetWorldPos, 0.3f);

#if UNITY_EDITOR
            VectorGizmoHelper.DrawLabel(targetWorldPos + Vector3.up * 0.5f,
                $"InverseTransformPoint\nWorld→Local", new Color(0.5f, 0.5f, 1f, 1f));
#endif
        }

        if (childObject != null)
        {
            Vector3 childWorldPos = transform.TransformPoint(childObject.localPosition);
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.5f);
            Gizmos.DrawLine(parentPos, childWorldPos);
        }

        if (worldTarget != null)
        {
            Gizmos.color = new Color(0.5f, 0.5f, 1f, 0.5f);
            Gizmos.DrawLine(parentPos, worldTarget.position);
        }
    }

    private void UpdateUI()
    {
        if (uiText == null) return;

        uiText.text =
            $"[CoordinateTransformDemo] 로컬↔월드 좌표 변환\n" +
            $"\n" +
            $"<b>부모 정보:</b>\n" +
            $"위치: {transform.position:F2}\n" +
            $"회전: {transform.eulerAngles:F1}°\n" +
            $"\n" +
            $"<b>자식 (로컬→월드):</b>\n" +
            $"로컬 위치: {childLocalPos:F2}\n" +
            $"월드 위치: {childWorldPos:F2}\n" +
            $"\n" +
            $"<b>타겟 (월드→로컬):</b>\n" +
            $"월드 위치: {targetWorldPos:F2}\n" +
            $"로컬 위치: {targetLocalPos:F2}";
    }
}
