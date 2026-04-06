// =============================================================================
// DotProductDemo.cs
// -----------------------------------------------------------------------------
// 내적을 이용해 객체가 시야각 범위 안에 있는지 판정하는 데모
// =============================================================================

using UnityEngine;
using TMPro;

public class DotProductDemo : MonoBehaviour
{
    [Header("=== 시야 설정 ===")]
    [Tooltip("시야각(Field of View) — 전체 각도 (기본 120도)")]
    [Range(10f, 360f)]
    [SerializeField] private float fieldOfView = 120f;

    [Tooltip("시야 거리 (이 범위 안에 있어야 감지)")]
    [Range(1f, 50f)]
    [SerializeField] private float viewDistance = 10f;

    [Header("=== 대상 설정 ===")]
    [Tooltip("감지할 대상 (직접 지정하거나, 비워두면 'Enemy' 태그로 자동 탐색)")]
    [SerializeField] private Transform target;

    [Header("=== 시각화 색상 ===")]
    [SerializeField] private Color colorInSight = Color.green;
    [SerializeField] private Color colorOutOfSight = Color.red;
    [SerializeField] private Color colorFOV = new Color(1f, 1f, 0f, 0.5f);

    [Header("=== UI 연결 ===")]
    [Tooltip("정보 표시용 TMP_Text (Canvas 하위에 배치)")]
    [SerializeField] private TMP_Text uiInfoText;

    [Header("=== 디버그 정보 (읽기 전용) ===")]
    [SerializeField] private float dotProductValue;
    [SerializeField] private float angleBetween;
    [SerializeField] private bool isInSight;

    private void Start()
    {
        if (target == null)
        {
            GameObject enemy = GameObject.FindGameObjectWithTag("Enemy");
            if (enemy != null)
                target = enemy.transform;
            else
                Debug.LogWarning("[DotProductDemo] 'Enemy' 태그를 가진 오브젝트를 찾을 수 없습니다. " +
                    "Inspector에서 Target을 직접 지정하거나, 적 오브젝트에 'Enemy' 태그를 추가하세요.");
        }
    }

    private void Update()
    {
        if (target == null) return;

        isInSight = CheckInSight(target);

        Renderer targetRenderer = target.GetComponent<Renderer>();
        if (targetRenderer != null)
        {
            targetRenderer.material.color = isInSight ? colorInSight : colorOutOfSight;
        }

        UpdateUI();
    }

    private bool CheckInSight(Transform targetTransform)
    {
        // TODO
        Vector3 toTarget = targetTransform.position - transform.position;
        if (toTarget.magnitude > viewDistance)
        {
            return false;
        }

        Vector3 toTargetNormal = toTarget.normalized;
        dotProductValue = Vector3.Dot(transform.forward, toTargetNormal);

        angleBetween = Mathf.Acos(dotProductValue) * Mathf.Rad2Deg;

        float halfFovCos = Mathf.Cos(fieldOfView * 0.5f * Mathf.Deg2Rad);

        return dotProductValue > halfFovCos;
    }

    private void OnDrawGizmos()
    {
        if (!enabled) return;

        Vector3 origin = transform.position;
        Vector3 forward = Application.isPlaying ? transform.forward : transform.forward;

        float halfAngle = fieldOfView * 0.5f;
        VectorGizmoHelper.DrawFOV(origin, forward, halfAngle, viewDistance, colorFOV);

        VectorGizmoHelper.DrawArrow(origin, origin + forward * viewDistance, Color.cyan, 0.4f);

        if (target != null)
        {
            Color lineColor = isInSight ? colorInSight : colorOutOfSight;
            VectorGizmoHelper.DrawArrow(origin, target.position, lineColor, 0.3f);

            VectorGizmoHelper.DrawCircleXZ(origin, viewDistance, new Color(1f, 1f, 1f, 0.2f));

#if UNITY_EDITOR
            Vector3 midPoint = (origin + target.position) * 0.5f + Vector3.up * 0.5f;
            string info = $"Dot: {dotProductValue:F3}\n각도: {angleBetween:F1}°\n{(isInSight ? "시야 안" : "시야 밖")}";
            VectorGizmoHelper.DrawLabel(midPoint, info, lineColor);
#endif
        }
    }

    private void UpdateUI()
    {
        if (uiInfoText == null || target == null) return;

        string sightText = isInSight ? "<color=green>시야 안</color>" : "<color=red>시야 밖</color>";
        float distance = (target.position - transform.position).magnitude;

        uiInfoText.text =
            $"[DotProductDemo] 내적 시야각 판정\n" +
            $"내적(Dot) 값: {dotProductValue:F3}\n" +
            $"사이 각도: {angleBetween:F1}°  (FOV: {fieldOfView}°)\n" +
            $"판정 결과: {sightText}\n" +
            $"거리: {distance:F1} / {viewDistance}";
    }
}
