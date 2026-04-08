// =============================================================================
// Assignment_CircularFormation.cs
// -----------------------------------------------------------------------------
// 목적: 원형 진형 배치 과제 (기본 난이도) — 쿼터니언 합성 & 벡터 곱셈 연습
// ★ 과제 설명:
//    리더 오브젝트 주위에 N개 유닛을 원형으로 배치한다.
//    리더가 이동/회전하면 진형 전체가 함께 따라 이동/회전한다.
//    - 쿼터니언 합성: 리더 회전 * 배치 회전
//    - 쿼터니언 * 벡터: 배치 쿼터니언 * Vector3.forward * radius → 오프셋 위치
// =============================================================================

using UnityEngine;
using TMPro;

public class Assignment_CircularFormation : MonoBehaviour
{
    [Header("=== 진형 설정 ===")]
    [Tooltip("진형의 중심이 될 리더 오브젝트")]
    [SerializeField] private Transform leader;

    [Tooltip("원형으로 배치할 유닛들의 Transform 배열")]
    [SerializeField] private Transform[] units;

    [Tooltip("리더로부터 유닛까지의 거리 (원의 반지름)")]
    [SerializeField] [Range(1f, 10f)] private float formationRadius = 3f;

    [Header("=== UI 연결 ===")]
    [SerializeField] private TMP_Text uiInfoText;

    [Header("=== 디버그 정보 (읽기 전용) ===")]
    [SerializeField] private int currentUnitCount;
    [SerializeField] private Vector3 leaderRotationEuler;

    private void Update()
    {
        if (leader == null || units == null || units.Length == 0)
        {
            UpdateUI();
            return;
        }

        currentUnitCount = units.Length;
        leaderRotationEuler = leader.rotation.eulerAngles;

        for (int i = 0; i < units.Length; i++)
        {
            if (units[i] == null) continue;

            float angleforthisUnit = (360f / currentUnitCount) * i; // 유닛마다 고유한 배치 각도 계산

            Quaternion slotRotation = Quaternion.AngleAxis(angleforthisUnit, Vector3.up); // 배치 회전: Y축 기준으로 각 유닛마다 다른 각도로 회전

            Vector3 offset = leader.rotation * slotRotation * Vector3.forward * formationRadius; // 오프셋 계산: 리더 회전 * 배치 회전 * 앞 방향 * 반지름
            units[i].position = leader.position + offset; // 유닛 위치 = 리더 위치 + 오프셋

            units[i].rotation = Quaternion.LookRotation(offset.normalized); // 유닛 회전: 오프셋 방향을 바라보도록 설정 (리더에서 유닛으로 향하는 방향)

        }
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (uiInfoText == null) return;

        string leaderInfo = leader != null ? leader.name : "없음";

        uiInfoText.text = $"[Assignment_CircularFormation]\n" +
            $"리더: {leaderInfo}\n" +
            $"유닛 수: {currentUnitCount}\n" +
            $"반지름: {formationRadius:F1}m\n" +
            $"리더 회전: ({leaderRotationEuler.x:F0}, {leaderRotationEuler.y:F0}, {leaderRotationEuler.z:F0})\n" +
            $"\n<color=yellow>리더를 이동/회전하여 진형 변화를 확인하세요!</color>";
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!enabled) return;

        if (!Application.isPlaying || leader == null || units == null) return;

        // 원형 궤도 표시
        VectorGizmoHelper.DrawCircleXZ(leader.position, formationRadius, new Color(0.3f, 1f, 0.3f, 0.3f));

        for (int i = 0; i < units.Length; i++)
        {
            if (units[i] == null) continue;

            // 리더 → 유닛 연결선
            Gizmos.color = new Color(0.3f, 1f, 0.3f, 0.5f);
            Gizmos.DrawLine(leader.position, units[i].position);

            // 유닛 위치 구
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(units[i].position, 0.15f);

            // 유닛 forward 방향
            VectorGizmoHelper.DrawArrow(units[i].position,
                units[i].position + units[i].forward * 0.8f,
                Color.cyan, 0.1f, 15f);
        }

        // 리더 위치 표시
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(leader.position, 0.25f);
    }
#endif
}
