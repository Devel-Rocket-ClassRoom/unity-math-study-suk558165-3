// =============================================================================
// DiagonalMoveFix.cs
// -----------------------------------------------------------------------------
// 정규화를 이용해 대각선 이동의 속도 보정을 구현하는 데모
// =============================================================================

using UnityEngine;
using TMPro;

public class DiagonalMoveFix : MonoBehaviour
{
    [Header("=== 이동 설정 ===")]
    [Tooltip("이동 속도 (units/sec)")]
    [Range(1f, 20f)]
    [SerializeField] private float moveSpeed = 5f;

    [Header("=== 보정 토글 ===")]
    [Tooltip("true: 정규화 적용 (올바른 대각선 속도)\nfalse: 정규화 미적용 (대각선이 √2배 빠름)")]
    [SerializeField] private bool useNormalized = false;

    [Header("=== UI 연결 ===")]
    [Tooltip("정보 표시용 TMP_Text (Canvas 하위에 배치)")]
    [SerializeField] private TMP_Text uiInfoText;

    [Header("=== 디버그 정보 (읽기 전용) ===")]
    [Tooltip("현재 프레임의 입력 방향 벡터")]
    [SerializeField] private Vector3 currentInputDirection;

    [Tooltip("현재 프레임의 입력 벡터 크기")]
    [SerializeField] private float currentInputMagnitude;

    [Tooltip("현재 프레임의 실제 이동 속도")]
    [SerializeField] private float currentSpeed;

    private void Update()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        // TODO
        Vector3 moveDirection = new Vector3(h, 0f, v);
        if (useNormalized)
        {
            moveDirection.Normalize();
        }
        transform.position += moveDirection * moveSpeed * Time.deltaTime;

        currentInputDirection = moveDirection;
        currentInputMagnitude = moveDirection.magnitude;
        currentSpeed = currentInputMagnitude * moveSpeed;

        UpdateUI();
    }

    private void OnDrawGizmos()
    {
        if (!enabled) return;

        if (!Application.isPlaying) return;

        Vector3 origin = transform.position;

        if (currentInputDirection != Vector3.zero)
        {
            Color arrowColor = useNormalized ? Color.green : Color.red;
            VectorGizmoHelper.DrawArrow(origin, origin + currentInputDirection * 2f, arrowColor, 0.3f);
        }

        VectorGizmoHelper.DrawCircleXZ(origin, 1f, Color.white);

#if UNITY_EDITOR
        string stateText = useNormalized ? "보정 후 (normalized)" : "보정 전 (원본)";
        string speedText = $"입력 크기: {currentInputMagnitude:F3}\n실제 속도: {currentSpeed:F2}";
        VectorGizmoHelper.DrawLabel(origin + Vector3.up * 2f, $"{stateText}\n{speedText}",
            useNormalized ? Color.green : Color.red);
#endif
    }

    private void UpdateUI()
    {
        if (uiInfoText == null) return;

        string mode = useNormalized
            ? "<color=#00FF00>보정 후 (normalized)</color>"
            : "<color=#FF0000>보정 전 (원본)</color>";

        uiInfoText.text =
            $"[DiagonalMoveFix]\n" +
            $"모드: {mode}\n" +
            $"입력 방향: {currentInputDirection}\n" +
            $"입력 크기: {currentInputMagnitude:F3} (이상적: 1.000)\n" +
            $"실제 속도: {currentSpeed:F2}\n" +
            $"Inspector에서 'Use Normalized' 토글로 비교하세요!";
    }
}
