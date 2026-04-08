// =============================================================================
// AngleAxisDemo.cs
// -----------------------------------------------------------------------------
// 임의의 축과 각도로 쿼터니언 회전을 생성하고 시각화합니다.
// =============================================================================

using UnityEngine;
using TMPro;
using Unity.Mathematics;

public class AngleAxisDemo : MonoBehaviour
{
    [Header("=== 회전 축 설정 ===")]
    [SerializeField] private Vector3 rotationAxis = Vector3.up;
    [SerializeField] [Range(0f, 360f)] private float rotationAngle = 0f;

    [SerializeField] private bool autoRotate = false;
    [SerializeField] [Range(10f, 360f)] private float rotateSpeed = 90f;

    [Header("=== 시각화 ===")]
    [SerializeField] private bool showAxis = true;
    [SerializeField] [Range(1f, 5f)] private float axisLength = 3f;

    [Header("=== UI 연결 ===")]
    [SerializeField] private TMP_Text uiInfoText;

    [Header("=== 디버그 정보 (읽기 전용) ===")]
    [SerializeField] private float currentAngle = 0f;
    [SerializeField] private Vector4 currentQuaternion;

    private float accumulatedAngle = 0f;

    private void Update()
    {
        if (autoRotate)
        {
            accumulatedAngle += rotateSpeed * Time.deltaTime;
            currentAngle = accumulatedAngle % 360f;

        }
        else
        {
            accumulatedAngle = rotationAngle;
            currentAngle = rotationAngle;
        }
        Quaternion angelAxisQuat = Quaternion.AngleAxis(currentAngle, rotationAxis.normalized);
        transform.rotation = angelAxisQuat;
        currentQuaternion = new Vector4(angelAxisQuat.x, angelAxisQuat.y, angelAxisQuat.z, angelAxisQuat.w);
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (uiInfoText == null) return;

        Vector3 normalizedAxis = rotationAxis.normalized;
        string axisStr = $"({normalizedAxis.x:F2}, {normalizedAxis.y:F2}, {normalizedAxis.z:F2})";

        uiInfoText.text = $"[AngleAxisDemo]\n" +
            $"회전축: {axisStr}\n" +
            $"회전각: {currentAngle:F1}°\n" +
            $"\n<color=cyan>쿼터니언: " +
            $"({currentQuaternion.x:F3}, {currentQuaternion.y:F3}, " +
            $"{currentQuaternion.z:F3}, {currentQuaternion.w:F3})</color>";
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!enabled) return;

        if (!Application.isPlaying) return;

        DrawAngleAxisGizmos(transform.position, rotationAxis, currentAngle);
    }

    private void OnDrawGizmosSelected()
    {
        if (Application.isPlaying)
        {
            DrawAngleAxisGizmos(transform.position, rotationAxis, currentAngle);
        }
    }

    private void DrawAngleAxisGizmos(Vector3 pos, Vector3 axis, float angle)
    {
        if (!showAxis) return;

        Vector3 normalizedAxis = axis.normalized;

        Gizmos.color = Color.yellow;
        VectorGizmoHelper.DrawArrow(pos, pos + normalizedAxis * axisLength, Color.yellow, 0.2f, 20f);

        Vector3 perp1 = Mathf.Abs(normalizedAxis.x) < 0.9f ? Vector3.right : Vector3.up;
        Vector3 perp2 = Vector3.Cross(normalizedAxis, perp1).normalized;
        perp1 = Vector3.Cross(perp2, normalizedAxis).normalized;

        int segments = 32;
        float radius = 1.5f;
        Gizmos.color = Color.white;

        for (int i = 0; i < segments; i++)
        {
            float t1 = (float)i / segments * Mathf.PI * 2f;
            float t2 = (float)(i + 1) / segments * Mathf.PI * 2f;

            Vector3 p1 = pos + (Mathf.Cos(t1) * perp1 + Mathf.Sin(t1) * perp2) * radius;
            Vector3 p2 = pos + (Mathf.Cos(t2) * perp1 + Mathf.Sin(t2) * perp2) * radius;

            Gizmos.DrawLine(p1, p2);
        }

        float angleRad = angle * Mathf.Deg2Rad;
        Vector3 currentPos = pos + (Mathf.Cos(angleRad) * perp1 + Mathf.Sin(angleRad) * perp2) * radius;
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(currentPos, 0.1f);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(pos, currentPos);
    }
#endif
}
