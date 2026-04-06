// =============================================================================
// Assignment_LissajousCurve.cs
// -----------------------------------------------------------------------------
// 리사주 곡선을 따라 오브젝트를 이동시키는 시스템
// =============================================================================

using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class Assignment_LissajousCurve : MonoBehaviour
{
    [Header("=== 리사주 곡선 파라미터 ===")]
    [Tooltip("X축 진폭")] [Range(0.5f, 5f)]
    [SerializeField] private float amplitudeX = 2f;

    [Tooltip("Z축 진폭")] [Range(0.5f, 5f)]
    [SerializeField] private float amplitudeZ = 2f;

    [Tooltip("X축 주파수")] [Range(0.1f, 3f)]
    [SerializeField] private float frequencyX = 1f;

    [Tooltip("Z축 주파수")] [Range(0.1f, 3f)]
    [SerializeField] private float frequencyZ = 2f;

    [Tooltip("X축 위상 (도, 0~360)")] [Range(0f, 360f)]
    [SerializeField] private float phaseX = 0f;

    [Tooltip("Z축 위상 (도, 0~360)")] [Range(0f, 360f)]
    [SerializeField] private float phaseZ = 0f;

    [Tooltip("자취 길이 (이전 위치 개수)")] [Range(10, 200)]
    [SerializeField] private int trailLength = 50;

    [Header("=== 디버그 정보 (읽기 전용) ===")]
    [SerializeField] private TextMeshProUGUI debugUI;
    [SerializeField] private Vector3 initialPosition;
    [SerializeField] private Vector3 currentPosition;

    private Queue<Vector3> positionTrail;

    private void Start()
    {
        initialPosition = transform.position;
        positionTrail = new Queue<Vector3>();
    }

    private void Update()
    {
        currentPosition = CalculateLissajousPosition(Time.time);

        transform.position = currentPosition;

        positionTrail.Enqueue(currentPosition);
        if (positionTrail.Count > trailLength)
            positionTrail.Dequeue();

        UpdateDebugUI();
    }

    private Vector3 CalculateLissajousPosition(float time)
    {
        // x(t) = Ax × sin(2π × fx × t + φx)
        // z(t) = Az × sin(2π × fz × t + φz)

        float x = amplitudeX * Mathf.Sin(2f * Mathf.PI * frequencyX * time + phaseX * Mathf.Deg2Rad);
        float z = amplitudeZ * Mathf.Sin(2f * Mathf.PI * frequencyZ * time + phaseZ * Mathf.Deg2Rad);

        return initialPosition + new Vector3(x, 0f, z);
    }

    private void UpdateDebugUI()
    {
        if (debugUI == null) return;

        float freqRatio = frequencyX > 0 ? frequencyZ / frequencyX : 1f;
        string patternName = PatternNameFromRatio(frequencyX, frequencyZ);

        debugUI.text = $"<b>[LissajousCurve]</b>\n" +
            $"시간: {Time.time:F2}s\n" +
            $"위치: ({currentPosition.x:F2}, {currentPosition.z:F2})\n" +
            $"주파수: <color=cyan>X={frequencyX:F1}, Z={frequencyZ:F1}</color>\n" +
            $"비율: {freqRatio:F2}:1 → <color=green>{patternName}</color>\n" +
            $"자취: {positionTrail.Count}/{trailLength}";
    }

    private string PatternNameFromRatio(float fx, float fz)
    {
        if (fx == 0f) return "Static";

        float ratio = Mathf.Abs(fz / fx);
        float tolerance = 0.1f;

        if (Mathf.Abs(ratio - 1f) < tolerance) return "원형";
        if (Mathf.Abs(ratio - 2f) < tolerance) return "8자형";
        if (Mathf.Abs(ratio - 3f) < tolerance) return "세잎";
        if (Mathf.Abs(ratio - 1.5f) < tolerance) return "장미곡선";

        return "복합패턴";
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!enabled) return;

        if (positionTrail != null && positionTrail.Count > 1)
        {
            Gizmos.color = new Color(0f, 1f, 1f, 0.4f);
            List<Vector3> trail = new List<Vector3>(positionTrail);

            for (int i = 0; i < trail.Count - 1; i++)
            {
                Gizmos.DrawLine(trail[i], trail[i + 1]);
            }
        }

        if (!Application.isPlaying)
        {
            Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
            Vector3 prevPos = initialPosition;

            for (int i = 0; i < 100; i++)
            {
                float t = (i / 100f) * 2 * Mathf.PI;
                Vector3 nextPos = CalculateLissajousPosition(t);
                Gizmos.DrawLine(prevPos, nextPos);
                prevPos = nextPos;
            }
        }
    }
#endif
}
