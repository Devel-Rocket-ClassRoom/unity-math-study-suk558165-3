// =============================================================================
// Assignment_RotationTrail.cs
// -----------------------------------------------------------------------------
// Matrix4x4로 회전 변환 행렬을 생성하고 끝점의 궤적을 추적하는 시스템
// =============================================================================

using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class Assignment_RotationTrail : MonoBehaviour
{

    [Header("=== 회전 설정 ===")]
    [Tooltip("회전 반경 (원점에서 끝점까지)")]
    [Range(1f, 5f)]
    [SerializeField] private float armLength = 2f;

    [Tooltip("회전 각도 (Z축 기준)")]
    [Range(0f, 360f)]
    [SerializeField] private float rotationAngle = 0f;

    [Header("=== 궤적 설정 ===")]
    [Tooltip("기록할 궤적의 최대 길이 (프레임 수)")]
    [Range(5, 60)]
    [SerializeField] private int trailLength = 30;

    [Tooltip("궤적의 색상")]
    [SerializeField] private Color trailColor = new Color(0f, 1f, 1f, 1f);

    [Header("=== 자동 회전 ===")]
    [Tooltip("자동 회전 속도 (도/초)")]
    [Range(30f, 360f)]
    [SerializeField] private float rotationSpeed = 120f;

    [Tooltip("자동 회전을 활성화할지 여부")]
    [SerializeField] private bool autoRotate = true;

    [Header("=== UI 연결 ===")]
    [Tooltip("궤적 정보를 표시할 TMP_Text")]
    [SerializeField] private TMP_Text uiText;

    private List<Vector3> trailPositions = new List<Vector3>();
    private Vector3 lastTipPos;

    private void Update()
    {
        // ── 각도 계산을 Time.time 방식으로 변경 ──────────────────────────
        // PlanetOrbit과 동일한 방식 → 속도값이 같으면 완벽히 동기화
        if (autoRotate)
            rotationAngle = Time.time * rotationSpeed;

        // ── 회전 행렬 생성 ────────────────────────────────────────────────
        Matrix4x4 rotMatrix = Matrix4x4.TRS(
            transform.position,
            Quaternion.Euler(0f, rotationAngle, 0f),
            Vector3.one
        );

        // ── 끝점 계산 ─────────────────────────────────────────────────────
        lastTipPos = rotMatrix.MultiplyPoint3x4(new Vector3(armLength, 0f, 0f));

        // ── 궤적 기록 ─────────────────────────────────────────────────────
        trailPositions.Add(lastTipPos);
        if (trailPositions.Count > trailLength)
            trailPositions.RemoveAt(0);

        UpdateUI();
    }

    private void OnDrawGizmos()
    {
        if (!enabled) return;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, lastTipPos);

        if (trailPositions.Count > 1)
        {
            for (int i = 0; i < trailPositions.Count - 1; i++)
            {
                float alpha = (float)i / trailPositions.Count;
                Color fadeColor = new Color(trailColor.r, trailColor.g, trailColor.b, alpha);

                Gizmos.color = fadeColor;
                Gizmos.DrawLine(trailPositions[i], trailPositions[i + 1]);
            }
        }
    }

    private void UpdateUI()
    {
        if (uiText == null) return;

        uiText.text =
            $"[과제] Matrix4x4 회전 궤적\n" +
            $"회전 반경: {armLength:F2}\n" +
            $"회전 각도: {rotationAngle:F1}°\n" +
            $"궤적 길이: {trailPositions.Count} / {trailLength}\n" +
            $"회전 속도: {rotationSpeed:F0}°/sec";
    }
}
