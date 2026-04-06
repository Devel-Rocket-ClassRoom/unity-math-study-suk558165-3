// =============================================================================
// SinWaveMotion.cs
// -----------------------------------------------------------------------------
// Sin 파동으로 오브젝트 진동 운동
// =============================================================================

using UnityEngine;
using TMPro;

public class SinWaveMotion : MonoBehaviour
{
    [Header("=== 파동 파라미터 ===")]
    [SerializeField] private float amplitude = 1f;
    [Range(0.1f, 10f)]
    [SerializeField] private float frequency = 1f;
    [Range(0f, 360f)]
    [SerializeField] private float phase = 0f;

    [Header("=== 운동 축 ===")]
    [SerializeField] private MotionAxis motionAxis = MotionAxis.Y;

    [Header("=== UI 텍스트 ===")]
    [SerializeField] private TextMeshProUGUI uiText;

    [Header("=== 디버그 정보 (읽기 전용) ===")]
    [SerializeField] private float currentOffset;

    private Vector3 startPosition;

    public enum MotionAxis { X, Y, Z }

    private void Start()
    {
        startPosition = transform.localPosition;
    }

    private void Update()
    {
        // TODO
        float phaseRadians = phase * Mathf.Deg2Rad;
        float timeIncycle = Time.time * frequency;
        currentOffset = amplitude * Mathf.Sin(2f * Mathf.PI * timeIncycle + phaseRadians);

        Vector3 newPosition = startPosition;
        switch (motionAxis)
        {
            case MotionAxis.X:
                newPosition.x += currentOffset;
                break;

            case MotionAxis.Y:
                newPosition.y += currentOffset;
                break;

            case MotionAxis.Z:
                newPosition.z += currentOffset;
                break;
        }

        transform.localPosition = newPosition;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (uiText == null) return;

        string axisName = motionAxis.ToString();
        uiText.text = $"<b>[Sin 파동 운동]</b>\n" +
                     $"축: <color=yellow>{axisName}</color>\n" +
                     $"진폭(A): {amplitude:F2}\n" +
                     $"주파수(f): {frequency:F2} Hz\n" +
                     $"위상(φ): {phase:F0}°\n" +
                     $"\n공식: y = A·sin(2πft + φ)\n" +
                     $"현재 오프셋: {currentOffset:F3}\n" +
                     $"경과 시간: {Time.time:F2}s";
    }

    private void OnDrawGizmos()
    {
        if (!enabled) return;

        if (!Application.isPlaying) return;

        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(startPosition, 0.1f);

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.localPosition, 0.1f);

        Gizmos.color = new Color(0f, 1f, 1f, 0.3f);
        Vector3 rangeMin = startPosition;
        Vector3 rangeMax = startPosition;

        switch (motionAxis)
        {
            case MotionAxis.X:
                rangeMin.x -= amplitude;
                rangeMax.x += amplitude;
                break;
            case MotionAxis.Y:
                rangeMin.y -= amplitude;
                rangeMax.y += amplitude;
                break;
            case MotionAxis.Z:
                rangeMin.z -= amplitude;
                rangeMax.z += amplitude;
                break;
        }

        Gizmos.DrawLine(rangeMin, rangeMax);

        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(rangeMin, 0.08f);
        Gizmos.DrawSphere(rangeMax, 0.08f);

        VectorGizmoHelper.DrawLabel(
            transform.position + Vector3.up * (amplitude + 0.5f),
            $"sin={currentOffset:F2}",
            Color.white
        );
    }
}
