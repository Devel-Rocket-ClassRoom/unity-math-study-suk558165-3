using UnityEngine;
using TMPro;

public class GimbalLockDemo : MonoBehaviour
{
    [Header("=== 오일러 각 설정 ===")]
    [SerializeField] private float rotationX = 0f;
    [SerializeField] private float rotationY = 0f;
    [SerializeField] private float rotationZ = 0f;

    [SerializeField] private bool autoRotateX = false;
    [SerializeField] private bool autoRotateY = false;
    [SerializeField] private bool autoRotateZ = false;

    [SerializeField] [Range(10f, 180f)] private float rotateSpeed = 45f;

    [Header("=== 짐벌락 감지 설정 ===")]
    [Tooltip("X축이 ±90도에서 이 각도 이내이면 짐벌락으로 판정합니다. 값을 바꿔가며 경계를 탐험해 보세요.")]
    [SerializeField] [Range(1f, 15f)] private float gimbalLockThreshold = 5f;

    [Header("=== UI 연결 ===")]
    [SerializeField] private TMP_Text uiInfoText;

    [Header("=== 디버그 정보 (읽기 전용) ===")]
    [SerializeField] private Vector3 currentEuler;
    [SerializeField] private bool isGimbalLocked = false;

    private void Update()
    {
        if (autoRotateX)
            rotationX = (rotationX + rotateSpeed * Time.deltaTime) % 360f;
        if (autoRotateY)
            rotationY = (rotationY + rotateSpeed * Time.deltaTime) % 360f;
        if (autoRotateZ)
            rotationZ = (rotationZ + rotateSpeed * Time.deltaTime) % 360f;

        currentEuler = new Vector3(rotationX, rotationY, rotationZ);
        transform.eulerAngles = currentEuler;

        float normalizedX = Mathf.Abs(rotationX % 180f);
        isGimbalLocked = Mathf.Abs(normalizedX - 90f) < gimbalLockThreshold;

        UpdateUI();
    }

    private void UpdateUI()
    {
        if (uiInfoText == null) return;

        string gimbalWarning = isGimbalLocked
            ? "<color=red>⚠️ 짐벌락 발생!</color>"
            : "<color=green>정상</color>";

        uiInfoText.text = $"[GimbalLockDemo]\n" +
            $"오일러 각: ({rotationX:F1}°, {rotationY:F1}°, {rotationZ:F1}°)\n" +
            $"상태: {gimbalWarning}\n" +
            $"\n<color=yellow>TIP: X를 90°로 설정하고\n" +
            $"자동회전 켜서 짐벌락 경험해보세요!</color>";
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!enabled) return;

        if (!Application.isPlaying)
        {
            DrawRotationGizmos(transform.position, transform.rotation);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (Application.isPlaying)
        {
            DrawRotationGizmos(transform.position, transform.rotation);
        }
    }

    private void DrawRotationGizmos(Vector3 pos, Quaternion rot)
    {
        float axisLength = 1.5f;

        VectorGizmoHelper.DrawArrow(pos, pos + rot * (Vector3.right * axisLength), Color.red, 0.15f, 15f);
        VectorGizmoHelper.DrawArrow(pos, pos + rot * (Vector3.up * axisLength), Color.green, 0.15f, 15f);
        VectorGizmoHelper.DrawArrow(pos, pos + rot * (Vector3.forward * axisLength), Color.blue, 0.15f, 15f);

        if (isGimbalLocked)
        {
            Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
            VectorGizmoHelper.DrawCircleXZ(pos, 2f, new Color(1f, 0f, 0f, 0.3f));
        }
    }
#endif
}
