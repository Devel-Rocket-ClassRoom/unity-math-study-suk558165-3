// =============================================================================
// SlerpDemo.cs
// -----------------------------------------------------------------------------
// Quaternion.Lerp와 Quaternion.Slerp의 회전 보간 차이를 시각적으로 비교
// =============================================================================

using TMPro;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class SlerpDemo : MonoBehaviour
{
    public enum RotationInterpolationMode
    {
        Lerp,
        Slerp,
        AbsoluteSlerp
    }

    [Header("=== 회전 설정 ===")]
    [SerializeField]
    private Transform target;

    [SerializeField]
    [Range(1f, 20f)]
    [Tooltip("회전 속도 (초당 보간 진행도). Slerp 모드에서는 각속도 관점")]
    private float rotationSpeed = 5f;

    [SerializeField]
    [Tooltip("보간 모드 선택: Lerp(현 경로) / Slerp(호 경로) / AbsoluteSlerp(고정 시간)")]
    private RotationInterpolationMode rotationMode = RotationInterpolationMode.Slerp;

    [Header("=== 비교 모드 ===")]
    [SerializeField]
    [Tooltip("Lerp와 Slerp 두 경로를 동시에 표시")]
    private bool showComparison = true;

    [SerializeField]
    [Tooltip("비교 모드에서 고스트 터렛의 투명도")]
    [Range(0.2f, 0.8f)]
    private float ghostAlpha = 0.5f;

    [Header("=== UI 연결 ===")]
    [SerializeField]
    [Tooltip("회전 보간 정보를 표시할 TMP Text")]
    private TextMeshProUGUI uiInfoText;

    private Quaternion startRotation;
    private Quaternion targetRotation;
    private float elapsedTime = 0f;
    private float interpolationDuration = 1f;

    private Quaternion previousRotation;
    private float angularVelocity = 0f;

    [Header("=== 디버그 정보 (읽기 전용) ===")]
    [SerializeField]
    private float currentAngleToTarget = 0f;

    private Quaternion lerpRotation;
    private Quaternion slerpRotation;

    private void Start()
    {
        if (target == null)
        {
            Debug.LogWarning("[SlerpDemo] Target이 지정되지 않았습니다. 인스펙터에서 설정하세요.");
            return;
        }

        previousRotation = transform.rotation;
        startRotation = transform.rotation;
        targetRotation = Quaternion.LookRotation(target.position - transform.position);
    }

    private void Update()
    {
        if (target == null) return;

        Vector3 dir = target.position - transform.position;
        if (dir == Vector3.zero) return;
        Quaternion newTargetRotation = Quaternion.LookRotation(dir); if (Quaternion.Angle(targetRotation, newTargetRotation) > 0.1f)
        {
            startRotation = transform.rotation;
            targetRotation = newTargetRotation;
            elapsedTime = 0f;
        }

        switch (rotationMode)
        {
            case RotationInterpolationMode.Lerp:
                UpdateLerp();
                break;
            case RotationInterpolationMode.Slerp:
                UpdateSlerp();
                break;
            case RotationInterpolationMode.AbsoluteSlerp:
                UpdateAbsoluteSlerp();
                break;
        }

        CalculateAngularVelocity();

        currentAngleToTarget = Quaternion.Angle(transform.rotation, targetRotation);

        if (showComparison)
        {
            UpdateComparisonRotations();
        }

        UpdateUI();

        previousRotation = transform.rotation;
    }

    private void OnDrawGizmos()
    {
        if (!enabled) return;

        if (target == null || !Application.isPlaying) return;

        DrawRotationGizmos(transform.position, transform.rotation, Color.cyan, 2f);

    
        Vector3 lookDirection = target.position - transform.position;
        if (lookDirection == Vector3.zero) return;
        Quaternion targetRot = Quaternion.LookRotation(lookDirection); Vector3 targetDir = targetRot * Vector3.forward;

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + targetDir * 3f);

        DrawArcGizmo(transform.position, transform.forward, targetDir, currentAngleToTarget, Color.green, 1.5f);

        if (showComparison)
        {
            DrawGhostTurrets();
        }
    }

    private void UpdateLerp()
    {
        float t = rotationSpeed * Time.deltaTime;
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, t);
    }

    private void UpdateSlerp()
    {
        float t = rotationSpeed * Time.deltaTime;
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, t);
    }
    private void UpdateAbsoluteSlerp()
    {
        elapsedTime += Time.deltaTime;

        interpolationDuration = 1f / rotationSpeed;
        float t = elapsedTime / interpolationDuration;
        transform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);
    }

    private void UpdateComparisonRotations()
    {
        float t = rotationSpeed * Time.deltaTime;

        lerpRotation = Quaternion.Lerp(lerpRotation, targetRotation, t);
        slerpRotation = Quaternion.Slerp(slerpRotation, targetRotation, t);
    }

    private void CalculateAngularVelocity()
    {
        float angleDiff = Quaternion.Angle(previousRotation, transform.rotation);
        angularVelocity = angleDiff / Time.deltaTime;
    }

    private void UpdateUI()
    {
        if (uiInfoText == null) return;

        string modeColor = rotationMode switch
        {
            RotationInterpolationMode.Lerp => "yellow",
            RotationInterpolationMode.Slerp => "cyan",
            RotationInterpolationMode.AbsoluteSlerp => "magenta",
            _ => "white"
        };

        uiInfoText.text =
            $"[SlerpDemo] 회전 보간 비교\n" +
            $"모드: <color={modeColor}>{rotationMode}</color>\n" +
            $"목표까지 각도: {currentAngleToTarget:F1}°\n" +
            $"각속도: {angularVelocity:F2}°/s\n" +
            $"속도 설정: {rotationSpeed:F1}";

        if (showComparison)
        {
            float lerpAngle = Quaternion.Angle(lerpRotation, targetRotation);
            float slerpAngle = Quaternion.Angle(slerpRotation, targetRotation);
            uiInfoText.text += $"\n\n<size=80%>[비교 모드]\n" +
                $"Lerp 잔여각: {lerpAngle:F1}°\n" +
                $"Slerp 잔여각: {slerpAngle:F1}°</size>";
        }
    }

    private void DrawRotationGizmos(Vector3 pos, Quaternion rot, Color color, float scale)
    {
        Vector3 forward = rot * Vector3.forward;
        Vector3 right = rot * Vector3.right;
        Vector3 up = rot * Vector3.up;

        Gizmos.color = color;
        Gizmos.DrawLine(pos, pos + forward * scale);
        Gizmos.color = Color.red;
        Gizmos.DrawLine(pos, pos + right * scale * 0.7f);
        Gizmos.color = Color.green;
        Gizmos.DrawLine(pos, pos + up * scale * 0.7f);
    }

    private void DrawArcGizmo(Vector3 center, Vector3 from, Vector3 to, float angle, Color color, float radius)
    {
        Vector3 axis = Vector3.Cross(from, to).normalized;
        if (axis.magnitude < 0.001f) return;

        int segments = Mathf.Max(3, (int)(angle / 10f));
        Gizmos.color = color;

        for (int i = 0; i < segments; i++)
        {
            float t1 = i / (float)segments;
            float t2 = (i + 1) / (float)segments;

            Quaternion rot1 = Quaternion.AngleAxis(angle * t1, axis);
            Quaternion rot2 = Quaternion.AngleAxis(angle * t2, axis);

            Vector3 p1 = center + rot1 * from * radius;
            Vector3 p2 = center + rot2 * from * radius;

            Gizmos.DrawLine(p1, p2);
        }
    }

    private void DrawGhostTurrets()
    {
        Vector3 offset = Vector3.right * 2f;

        Gizmos.color = new Color(1f, 1f, 0f, ghostAlpha);
        DrawRotationGizmos(transform.position + offset, lerpRotation, new Color(1f, 1f, 0f, ghostAlpha), 1.5f);

        offset = Vector3.left * 2f;
        Gizmos.color = new Color(0f, 1f, 1f, ghostAlpha);
        DrawRotationGizmos(transform.position + offset, slerpRotation, new Color(0f, 1f, 1f, ghostAlpha), 1.5f);

        #if UNITY_EDITOR
        UnityEditor.Handles.color = new Color(1f, 1f, 0f, ghostAlpha);
        UnityEditor.Handles.Label(transform.position + Vector3.right * 2f + Vector3.up * 2f, "Lerp\n(현 경로)");
        UnityEditor.Handles.color = new Color(0f, 1f, 1f, ghostAlpha);
        UnityEditor.Handles.Label(transform.position + Vector3.left * 2f + Vector3.up * 2f, "Slerp\n(호 경로)");
        #endif
    }
}
