// =============================================================================
// TurretAim.cs
// -----------------------------------------------------------------------------
// atan2로 터렛이 타겟을 추적 조준
// =============================================================================

using UnityEngine;
using TMPro;

public class TurretAim : MonoBehaviour
{
    [Header("=== 타겟 설정 ===")]
    [SerializeField] private Transform target;
    [SerializeField] private float rotationSpeed = 180f;
    [SerializeField] private bool lockYAxis = true;

    [Header("=== 감지 범위 ===")]
    [SerializeField] private float detectionRange = 20f;
    [SerializeField] private bool drawDetectionCircle = true;

    [Header("=== UI 텍스트 ===")]
    [SerializeField] private TextMeshProUGUI uiText;

    [Header("=== 디버그 정보 (읽기 전용) ===")]
    [SerializeField] private Vector3 directionToTarget;
    [SerializeField] private float distanceToTarget;
    [SerializeField] private float targetAngleDegrees;
    [SerializeField] private float targetAngleRadians;
    [SerializeField] private bool targetInRange;

    private void Update()
    {
        if (target == null)
        {
            UpdateUI();
            return;
        }

        // TODO
        directionToTarget = target.position - transform.position;
        distanceToTarget = directionToTarget.magnitude;
        targetInRange = distanceToTarget <= detectionRange;

        if (!targetInRange)
        {
            UpdateUI();
            return;
        }

        directionToTarget.Normalize();
        targetAngleRadians = Mathf.Atan2(directionToTarget.z, directionToTarget.x);
        targetAngleDegrees = targetAngleRadians * Mathf.Rad2Deg;

        RotateTowardTarget();
        UpdateUI();
    }

    private void RotateTowardTarget()
    {
        // TODO
        var targetQuaternion = Quaternion.Euler(0f, 90f - targetAngleDegrees, 0f);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetQuaternion, rotationSpeed * Time.deltaTime);
    }

    private void UpdateUI()
    {
        if (uiText == null) return;

        if (target == null)
        {
            uiText.text = $"<b>[터렛 조준]</b>\n<color=red>타겟: 없음</color>";
            return;
        }

        string rangeStatus = targetInRange
            ? $"<color=green>범위 내</color>"
            : $"<color=red>범위 외</color>";

        uiText.text = $"<b>[터렛 조준]</b>\n" +
                     $"거리: {distanceToTarget:F2}u ({rangeStatus})\n" +
                     $"각도(°): {targetAngleDegrees:F1}°\n" +
                     $"각도(rad): {targetAngleRadians:F3}\n" +
                     $"방향: ({directionToTarget.x:F2}, {directionToTarget.y:F2}, {directionToTarget.z:F2})\n" +
                     $"감지 범위: {detectionRange}u";
    }

    private void OnDrawGizmos()
    {
        if (!enabled) return;

        if (!Application.isPlaying) return;
        if (target == null) return;

        if (drawDetectionCircle)
        {
            Gizmos.color = new Color(1f, 1f, 0f, 0.2f);
            VectorGizmoHelper.DrawCircleXZ(transform.position, detectionRange, Color.yellow, 32);
        }

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(target.position, 0.15f);

        if (!targetInRange) return;

        VectorGizmoHelper.DrawArrow(
            transform.position,
            target.position,
            Color.cyan
        );

        float currentYaw = transform.eulerAngles.y;
        float angleDiff = Mathf.DeltaAngle(currentYaw, 90f - targetAngleDegrees);

        VectorGizmoHelper.DrawFOV(
            transform.position,
            transform.forward,
            Mathf.Abs(angleDiff) * 0.5f,
            2f,
            new Color(0f, 1f, 1f, 0.5f),
            16
        );

        VectorGizmoHelper.DrawLabel(
            Vector3.Lerp(transform.position, target.position, 0.5f) + Vector3.up * 0.3f,
            $"d={distanceToTarget:F1}u\nθ={targetAngleDegrees:F0}°",
            Color.white
        );
    }
}
