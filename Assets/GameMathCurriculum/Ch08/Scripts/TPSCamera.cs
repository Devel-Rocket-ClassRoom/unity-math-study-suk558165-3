using TMPro;
using UnityEngine;

public class TPSCamera : MonoBehaviour
{
    [Header("=== 추적 대상 ===")]
    [Tooltip("카메라가 따라갈 플레이어(타겟)")]
    [SerializeField] private Transform target;

    [Tooltip("타겟으로부터 카메라의 오프셋(상대 위치)")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 5f, -8f);

    [Header("=== SmoothDamp 설정 ===")]
    [Tooltip("위치 보간 부드러움 정도 (초, 작을수록 빠름)")]
    [Range(0.01f, 1f)]
    [SerializeField] private float positionSmoothTime = 0.3f;

    [Tooltip("회전 보간 속도 (높을수록 빠르게 회전)")]
    [Range(1f, 20f)]
    [SerializeField] private float rotationSmoothSpeed = 5f;

    [Header("=== UI 연결 ===")]
    [Tooltip("정보 표시용 TMP_Text")]
    [SerializeField] private TMP_Text uiInfoText;

    [Tooltip("SmoothDamp 내부 위치 속도 (읽기 전용)")]
    [SerializeField] private Vector3 currentSmoothVelocity = Vector3.zero;

    private void LateUpdate()
    {
        if (target == null)
            return;
        // 타겟 위치에 오프셋을 더한 목표 위치 계산
        Vector3 targetPosition = target.position + offset;
        // SmoothDamp로 현재 위치에서 목표 위치로 부드럽게 이동
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref currentSmoothVelocity, positionSmoothTime);
        // 타겟을 향하도록 회전 (회전 보간)
        Quaternion targetRotation = Quaternion.LookRotation(target.position - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSmoothSpeed * Time.deltaTime);
        // UI 정보 업데이트
        if (uiInfoText != null)
        {
            uiInfoText.text = $"Camera Position: {transform.position}\n" +
                              $"Target Position: {target.position}\n" +
                              $"Current Smooth Velocity: {currentSmoothVelocity}";
        }

    }

}
