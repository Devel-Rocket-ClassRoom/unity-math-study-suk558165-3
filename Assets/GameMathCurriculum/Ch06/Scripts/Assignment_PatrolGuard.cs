// =============================================================================
// Assignment_PatrolGuard.cs
// -----------------------------------------------------------------------------
// 목적: 웨이포인트 순찰 가드 과제 (심화 난이도) — 다양한 쿼터니언 API 활용
// ★ 과제 설명:
//    경비병이 웨이포인트를 순서대로 순회한다. 각 단계에서 다른 쿼터니언 API를 사용:
//    - Quaternion.Euler() — 초기 방향 설정
//    - Quaternion.FromToRotation() — 두 방향 벡터 사이 회전 계산
//    - Quaternion.RotateTowards() — 일정 각속도로 회전
// =============================================================================

using UnityEngine;
using TMPro;

public class Assignment_PatrolGuard : MonoBehaviour
{
    [Header("=== 웨이포인트 ===")]
    [Tooltip("순찰 경로를 이루는 웨이포인트 배열")]
    [SerializeField] private Transform[] waypoints;

    [Tooltip("이동 속도 (m/s)")]
    [SerializeField] [Range(1f, 10f)] private float moveSpeed = 3f;

    [Tooltip("회전 속도 (도/초)")]
    [SerializeField] [Range(30f, 360f)] private float turnSpeed = 120f;

    [Header("=== 초기 설정 ===")]
    [Tooltip("시작 시 Y축 회전 각도 (도)")]
    [SerializeField] [Range(0f, 360f)] private float startYAngle = 0f;

    [Header("=== UI 연결 ===")]
    [SerializeField] private TMP_Text uiInfoText;

    [Header("=== 디버그 정보 (읽기 전용) ===")]
    [SerializeField] private int currentWaypointIndex;
    [SerializeField] private float distanceToNext;
    [SerializeField] private float angleToTarget;

    private Quaternion targetRotation;
    private float arrivalThreshold = 0.5f;

    private void Start()
    {
        transform.rotation = Quaternion.Euler(0, startYAngle, 0); // 초기 방향 설정
    }

    private void Update()
    {
        if (waypoints == null || waypoints.Length == 0) return;

        Transform target = waypoints[currentWaypointIndex];
        Vector3 dirToNext = target.position - transform.position;
        dirToNext.y = 0f; // 수평면에서의 방향 벡터로 변환
        dirToNext.Normalize(); // 목표 방향 벡터 계산 (수평면에서)
        distanceToNext = Vector3.Distance(transform.position, target.position); // 다음 웨이포인트까지의 거리 계산


        Quaternion beetweenPoint = Quaternion.FromToRotation(transform.forward, dirToNext); // 현재 forward와 목표 방향 사이의 회전 계산
        targetRotation = transform.rotation * beetweenPoint; // 목표 회전 = 현재 회전

        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, turnSpeed * Time.deltaTime); // 일정 각속도로 회전

        // 이동 (제공됨 — 수정 불필요)
        transform.position += transform.forward * moveSpeed * Time.deltaTime;

        // 도착 판정
        if (distanceToNext < arrivalThreshold)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
        }

        angleToTarget = Quaternion.Angle(transform.rotation, targetRotation);

        UpdateUI();
    }

    private void UpdateUI()
    {
        if (uiInfoText == null) return;

        string waypointInfo = waypoints != null && waypoints.Length > 0
            ? $"{currentWaypointIndex + 1}/{waypoints.Length}"
            : "없음";

        uiInfoText.text = $"[Assignment_PatrolGuard]\n" +
            $"웨이포인트: {waypointInfo}\n" +
            $"거리: {distanceToNext:F1}m\n" +
            $"각도 차이: {angleToTarget:F1}°\n" +
            $"\n<color=yellow>경비병이 웨이포인트를 순찰합니다</color>";
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!enabled) return;

        if (waypoints == null || waypoints.Length == 0) return;

        // 웨이포인트 연결선
        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] == null) continue;

            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(waypoints[i].position, 0.3f);

            int next = (i + 1) % waypoints.Length;
            if (waypoints[next] != null)
            {
                Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
                Gizmos.DrawLine(waypoints[i].position, waypoints[next].position);
            }
        }

        if (!Application.isPlaying) return;

        // 현재 forward 방향 (초록)
        VectorGizmoHelper.DrawArrow(transform.position,
            transform.position + transform.forward * 1.5f,
            Color.green, 0.15f, 20f);

        // 목표 방향 (빨강)
        if (waypoints[currentWaypointIndex] != null)
        {
            Vector3 dirToTarget = (waypoints[currentWaypointIndex].position - transform.position).normalized;
            VectorGizmoHelper.DrawArrow(transform.position,
                transform.position + dirToTarget * 1.5f,
                Color.red, 0.15f, 20f);
        }
    }
#endif
}
