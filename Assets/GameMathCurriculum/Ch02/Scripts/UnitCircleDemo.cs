// =============================================================================
// UnitCircleDemo.cs
// -----------------------------------------------------------------------------
// 단위원 위의 점 좌표를 계산하고 시각화
// =============================================================================

using UnityEngine;
using TMPro;

public class UnitCircleDemo : MonoBehaviour
{
    [Header("=== 회전 설정 ===")]
    [SerializeField] private float rotationSpeed = 45f;
    [SerializeField] private float radius = 2f;
    [SerializeField] private bool autoRotate = true;

    [Header("=== 시각화 설정 ===")]
    [SerializeField] private bool showProjections = true;
    [SerializeField] private bool showAngleArc = true;
    [Tooltip("0~360도 수동 제어 (autoRotate 비활성화 시)")]
    [SerializeField] private float manualAngleDegrees = 0f;

    [Header("=== UI 텍스트 ===")]
    [SerializeField] private TextMeshProUGUI uiText;

    [Header("=== 디버그 정보 (읽기 전용) ===")]
    [SerializeField] private float currentAngleDegrees;
    [SerializeField] private float currentAngleRadians;
    [SerializeField] private float sinValue;
    [SerializeField] private float cosValue;
    [SerializeField] private Vector3 pointPosition;

    private void Update()
    {
        if (autoRotate)
        {
            currentAngleDegrees = (Time.time * rotationSpeed) % 360f;
        }
        else
        {
            currentAngleDegrees = Mathf.Clamp(manualAngleDegrees, 0f, 360f);
        }

        // TODO
        currentAngleRadians = currentAngleDegrees * Mathf.Deg2Rad;
        cosValue = Mathf.Cos(currentAngleRadians);
        sinValue = Mathf.Sin(currentAngleRadians);

        pointPosition = new Vector3(cosValue * radius, 0f, sinValue * radius);

        UpdateUI();
    }

    private void UpdateUI()
    {
        if (uiText == null) return;

        uiText.text = $"<b>[단위원 시뮬레이션]</b>\n" +
                     $"각도(°): {currentAngleDegrees:F1}°\n" +
                     $"각도(rad): {currentAngleRadians:F3}\n" +
                     $"<color=blue>cos(θ): {cosValue:F3}</color>\n" +
                     $"<color=green>sin(θ): {sinValue:F3}</color>\n" +
                     $"위치: ({cosValue:F3}, {sinValue:F3})";
    }

    private void OnDrawGizmos()
    {
        if (!enabled) return;

        if (!Application.isPlaying) return;

        VectorGizmoHelper.DrawCircleXZ(transform.position, radius, Color.white, 64);

        VectorGizmoHelper.DrawArrow(
            transform.position,
            transform.position + new Vector3(cosValue, 0, sinValue) * radius,
            Color.yellow
        );

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position + pointPosition, 0.08f);

        if (!showProjections) return;

        Gizmos.color = Color.blue;
        Vector3 cosProjection = transform.position + new Vector3(cosValue * radius, 0, 0);
        Gizmos.DrawLine(transform.position + pointPosition, cosProjection);
        Gizmos.DrawSphere(cosProjection, 0.06f);

        Gizmos.color = Color.green;
        Vector3 sinProjection = transform.position + new Vector3(0, 0, sinValue * radius);
        Gizmos.DrawLine(transform.position + pointPosition, sinProjection);
        Gizmos.DrawSphere(sinProjection, 0.06f);

        if (showAngleArc)
        {
            float halfArc = currentAngleDegrees * 0.5f;
            Vector3 arcForward = Quaternion.Euler(0, -halfArc, 0) * Vector3.right;
            VectorGizmoHelper.DrawFOV(
                transform.position,
                arcForward,
                halfArc,
                radius * 0.3f,
                Color.cyan,
                32
            );
        }

        VectorGizmoHelper.DrawLabel(
            transform.position + new Vector3(cosValue * radius * 0.5f, 0.3f, sinValue * radius * 0.5f),
            $"sin={sinValue:F2}\ncos={cosValue:F2}",
            Color.white
        );
    }
}
