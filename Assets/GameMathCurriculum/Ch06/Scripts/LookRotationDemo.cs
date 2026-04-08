// =============================================================================
// LookRotationDemo.cs
// -----------------------------------------------------------------------------
// LookRotation과 Slerp를 사용하여 타겟 방향으로 부드럽게 회전합니다.
// =============================================================================

using UnityEngine;
using TMPro;

public class LookRotationDemo : MonoBehaviour
{
    [Header("=== 대상 설정 ===")]
    [SerializeField] private Transform target;

    [Header("=== 회전 설정 ===")]
    [SerializeField] [Range(0.5f, 20f)] private float turnSpeed = 5f;
    [SerializeField] private bool useSlerp = true;

    [Header("=== 시각화 색상 ===")]
    [SerializeField] private Color colorCurrentDir = Color.green;
    [SerializeField] private Color colorTargetDir = Color.red;

    [Header("=== UI 연결 ===")]
    [SerializeField] private TMP_Text uiInfoText;

    [Header("=== 디버그 정보 (읽기 전용) ===")]
    [SerializeField] private float angleDifference = 0f;
    [SerializeField] private float slerpProgress = 0f;

    private Quaternion targetRotation = Quaternion.identity;

    private void Update()
    {
        if (target == null)
        {
            GameObject targetObj = GameObject.FindWithTag("Target");
            if (targetObj != null)
                target = targetObj.transform;
        }

        Vector3 toTarget = (target.position - transform.position).normalized;
        targetRotation = Quaternion.LookRotation(toTarget, Vector3.up);

        transform.rotation = Quaternion.LookRotation(toTarget, Vector3.up);

        angleDifference = Quaternion.Angle(transform.rotation, targetRotation);
        if (useSlerp)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
            slerpProgress = 1f - Mathf.Clamp01(angleDifference/ 100f);
        }
        else
        {
            transform.rotation = targetRotation;
            slerpProgress = 1f;
        }
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (uiInfoText == null) return;

        string slerpMode = useSlerp ? "<color=cyan>Slerp (부드러움)</color>" : "<color=yellow>Instant (즉시)</color>";

        if (target == null)
        {
            uiInfoText.text = $"[LookRotationDemo]\n" +
                $"<color=red>Target이 없습니다!</color>\n" +
                $"'Target' 태그가 있는 객체를 찾아주세요.";
        }
        else
        {
            uiInfoText.text = $"[LookRotationDemo]\n" +
                $"각도 차이: {angleDifference:F1}°\n" +
                $"회전 모드: {slerpMode}\n" +
                $"Slerp 진행률: {slerpProgress:F2}\n" +
                $"회전속도: {turnSpeed:F2}";
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!enabled) return;

        if (!Application.isPlaying || target == null) return;

        DrawLookRotationGizmos(transform.position, transform.forward, target.position);
    }

    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying || target == null) return;

        DrawLookRotationGizmos(transform.position, transform.forward, target.position);
    }

    private void DrawLookRotationGizmos(Vector3 fromPos, Vector3 currentDir, Vector3 targetPos)
    {
        float rayLength = 3f;

        VectorGizmoHelper.DrawArrow(fromPos, fromPos + currentDir * rayLength, colorCurrentDir, 0.2f, 20f);

        Vector3 targetDir = (targetPos - fromPos).normalized;
        VectorGizmoHelper.DrawArrow(fromPos, fromPos + targetDir * rayLength, colorTargetDir, 0.2f, 20f);

        Gizmos.color = colorTargetDir;
        Gizmos.DrawSphere(targetPos, 0.15f);

        float angle = Vector3.Angle(currentDir, targetDir);
        if (angle > 1f)
        {
            Vector3 axis = Vector3.Cross(currentDir, targetDir).normalized;
            int steps = Mathf.Max(4, (int)(angle / 10f));

            Gizmos.color = new Color(1f, 1f, 0f, 0.6f);
            Vector3 prevPos = fromPos + currentDir * rayLength;
            for (int i = 1; i <= steps; i++)
            {
                float t = (float)i / steps;
                Quaternion rot = Quaternion.AngleAxis(angle * t, axis);
                Vector3 pos = fromPos + rot * (currentDir * rayLength);
                Gizmos.DrawLine(prevPos, pos);
                prevPos = pos;
            }
        }
    }
#endif
}
