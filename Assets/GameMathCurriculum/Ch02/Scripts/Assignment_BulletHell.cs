// =============================================================================
// Assignment_BulletHell.cs
// -----------------------------------------------------------------------------
// 삼각함수를 이용한 다양한 탄막 패턴 생성 및 발사
// =============================================================================

using UnityEngine;
using TMPro;

public class Assignment_BulletHell : MonoBehaviour
{
    public enum PatternType
    {
        Circle,
        Spiral,
        Fan
    }
    [Header("=== 탄막 설정 ===")]
    [SerializeField] private GameObject bulletPrefab;
    [Tooltip("발사 탄막 개수 (8~36권장)")] [Range(8, 36)]
    [SerializeField] private int bulletCount = 16;
    [Tooltip("발사된 탄막 속도 (단위/초)")] [Range(1f, 20f)]
    [SerializeField] private float bulletSpeed = 10f;
    [Tooltip("다음 발사까지 대기시간 (초)")] [Range(0.1f, 2f)]
    [SerializeField] private float fireInterval = 0.5f;

    [Header("=== 패턴 선택 ===")]
    [SerializeField] private PatternType patternType = PatternType.Circle;

    [Header("=== 나선형 패턴 파라미터 ===")]
    [Tooltip("나선형 회전 속도 (라디안/초)")] [Range(0.5f, 5f)]
    [SerializeField] private float spiralTurnSpeed = 2f;
    [SerializeField] private float offset;

    [Header("=== 부채꼴 패턴 파라미터 ===")]
    [Tooltip("부채꼴 각도 범위 (도, 360까지)")] [Range(30f, 360f)]
    [SerializeField] private float fanAngle = 120f;

    [Header("=== 디버그 정보 (읽기 전용) ===")]
    [SerializeField] private TextMeshProUGUI debugUI;
    [SerializeField] private float fireTimer = 0f;

    private void Start()
    {
        fireTimer = 0f;
    }

    private void Update()
    {
        fireTimer -= Time.deltaTime;
        offset = (Time.time * spiralTurnSpeed) % 360f;

        if (fireTimer <= 0f)
        {
            FireBulletPattern();
            fireTimer = fireInterval;
        }

        UpdateDebugUI();
    }

    private void FireBulletPattern()
    {
        if (bulletPrefab == null)
        {
            Debug.LogWarning("[BulletHell] bulletPrefab이 할당되지 않았습니다!");
            return;
        }

        for (int i = 0; i < bulletCount; i++)
        {
            Vector3 direction = patternType switch
            {
                PatternType.Circle => CalculateCircleDirection(i, bulletCount),
                PatternType.Spiral => CalculateSpiralDirection(i, bulletCount),
                PatternType.Fan => CalculateFanDirection(i, bulletCount),
                _ => Vector3.forward
            };

            GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
            Rigidbody rb = bullet.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = direction * bulletSpeed;
            }
        }
    }

    private Vector3 CalculateCircleDirection(int index, int total)
    {
        // 360도를 탄막 수로 균등 분할
        float angleSpacing = 360f / total;
        float angleDegree = index * angleSpacing;
        float angleRadian = angleDegree * Mathf.Deg2Rad;

        return new Vector3(Mathf.Cos(angleRadian), 0f, Mathf.Sin(angleRadian));
    }

    private Vector3 CalculateSpiralDirection(int index, int total)
    {
        // Circle과 동일하되, offset을 더해서 매 발사마다 회전
        float angleSpacing = 360f / total;
        float angleDegree = index * angleSpacing + offset;  // 360f → offset
        float angleRadian = angleDegree * Mathf.Deg2Rad;

        return new Vector3(Mathf.Cos(angleRadian), 0f, Mathf.Sin(angleRadian));
    }

    private Vector3 CalculateFanDirection(int index, int total)
    {
        // fanAngle 범위 안에서 균등 분할
        // total = 1이면 나누기 0 방지
        float angleSpacing = (total > 1) ? fanAngle / (total - 1) : 0f;

        // 중앙 기준으로 좌우 대칭
        // 예) fanAngle=120, total=3 → -60, 0, +60도
        float angleDegree = -fanAngle / 2f + index * angleSpacing;
        float angleRadian = angleDegree * Mathf.Deg2Rad;

        return new Vector3(Mathf.Cos(angleRadian), 0f, Mathf.Sin(angleRadian));
    }

    private void UpdateDebugUI()
    {
        if (debugUI == null) return;

        debugUI.text = $"<b>[BulletHell]</b>\n" +
            $"패턴: <color=yellow>{patternType}</color>\n" +
            $"탄막개수: {bulletCount}\n" +
            $"발사속도: {bulletSpeed:F1} u/s\n" +
            $"다음 발사: {fireTimer:F2}s";

        if (patternType == PatternType.Spiral)
            debugUI.text += $"\n나선속도: {spiralTurnSpeed:F2} rad/s";
        else if (patternType == PatternType.Fan)
            debugUI.text += $"\n부채꼴각도: {fanAngle:F0}°";
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!enabled) return;

        Gizmos.color = new Color(1f, 1f, 0f, 0.6f);
        float previewRadius = 3f;

        for (int i = 0; i < bulletCount; i++)
        {
            Vector3 direction = patternType switch
            {
                PatternType.Circle => CalculateCircleDirection(i, bulletCount),
                PatternType.Spiral => CalculateSpiralDirection(i, bulletCount),
                PatternType.Fan => CalculateFanDirection(i, bulletCount),
                _ => Vector3.forward
            };

            Vector3 endPos = transform.position + direction * previewRadius;
            Gizmos.DrawLine(transform.position, endPos);
            Gizmos.DrawSphere(endPos, 0.15f);
        }
    }
#endif
}
