// =============================================================================
// CrossProductDemo.cs
// -----------------------------------------------------------------------------
// 외적을 이용해 대상이 자신의 좌측인지 우측인지 판별하는 데모
// =============================================================================

using UnityEngine;
using TMPro;

public class CrossProductDemo : MonoBehaviour
{
    [Header("=== 대상 설정 ===")]
    [Tooltip("판별할 대상 (비워두면 'Enemy' 태그로 자동 탐색)")]
    [SerializeField] private Transform target;

    [Header("=== 시각화 설정 ===")]
    [Tooltip("외적 결과 벡터 시각화 크기")]
    [Range(0.5f, 5f)]
    [SerializeField] private float crossVectorScale = 2f;

    [Header("=== UI 연결 ===")]
    [Tooltip("방향 결과 표시용 TMP_Text (화면 상단 중앙)")]
    [SerializeField] private TMP_Text uiDirectionText;
    [Tooltip("상세 정보 표시용 TMP_Text (화면 좌측 상단)")]
    [SerializeField] private TMP_Text uiInfoText;

    [Header("=== 디버그 정보 (읽기 전용) ===")]
    [SerializeField] private Vector3 crossProduct;
    [SerializeField] private float crossY;
    [SerializeField] private string directionResult = "";

    private void Start()
    {
        if (target == null)
        {
            GameObject enemy = GameObject.FindGameObjectWithTag("Enemy");
            if (enemy != null)
                target = enemy.transform;
            else
                Debug.LogWarning("[CrossProductDemo] 'Enemy' 태그를 가진 오브젝트를 찾을 수 없습니다.");
        }
    }

    private void Update()
    {
        if (target == null) return;

        directionResult = CheckLeftOrRight(target);

        UpdateUI();

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log($"[CrossProductDemo] 적은 내 {directionResult}에 있습니다! " +
                $"(외적 Y값: {crossY:F3})");
        }
    }

    private string CheckLeftOrRight(Transform targetTransform)
    {
        // TODO
        Vector3 toTarget = targetTransform.position - transform.position;
        toTarget.y = 0f;
        
        crossProduct = Vector3.Cross(transform.forward, toTarget.normalized);
        crossY = crossProduct.y;

        float threshold = 0.01f;

        if (crossY > threshold)
        {
            return "오른쪽";
        }
        else if (crossY < -threshold)
        {
            return "왼쪽";
        }
        else
        {
            return "정면이나 후면";
        }
    }

    private void OnDrawGizmos()
    {
        if (!enabled) return;

        Vector3 origin = transform.position;

        VectorGizmoHelper.DrawArrow(origin, origin + transform.forward * 3f, Color.cyan, 0.3f);

        if (target != null)
        {
            Vector3 dirToTarget = (target.position - transform.position);
            dirToTarget.y = 0f;
            VectorGizmoHelper.DrawArrow(origin, origin + dirToTarget.normalized * 3f, Color.white, 0.3f);

            if (crossProduct != Vector3.zero)
            {
                Color crossColor = crossY > 0 ? Color.green : Color.red;
                VectorGizmoHelper.DrawArrow(origin, origin + crossProduct.normalized * crossVectorScale, crossColor, 0.3f);
            }

            Color resultColor;
            if (directionResult == "오른쪽") resultColor = Color.green;
            else if (directionResult == "왼쪽") resultColor = Color.red;
            else resultColor = Color.yellow;

            Gizmos.color = resultColor;
            Gizmos.DrawWireSphere(target.position, 0.5f);

#if UNITY_EDITOR
            VectorGizmoHelper.DrawLabel(origin + Vector3.up * 2.5f,
                $"정면 (forward)", Color.cyan);
            VectorGizmoHelper.DrawLabel(target.position + Vector3.up * 1.5f,
                $"{directionResult}!\nCross.y = {crossY:F3}", resultColor);
            VectorGizmoHelper.DrawLabel(origin + crossProduct.normalized * crossVectorScale + Vector3.right * 0.3f,
                "외적 결과 (법선)", crossY > 0 ? Color.green : Color.red);
#endif
        }
    }

    private void UpdateUI()
    {
        if (target == null) return;

        string colorTag;
        if (directionResult == "오른쪽") colorTag = "#00FF00";
        else if (directionResult == "왼쪽") colorTag = "#FF0000";
        else colorTag = "#FFFF00";

        if (uiDirectionText != null)
        {
            uiDirectionText.text = $"<color={colorTag}><size=150%>{directionResult}!</size></color>";
        }

        if (uiInfoText != null)
        {
            uiInfoText.text =
                $"[CrossProductDemo] 외적 좌우 판별\n" +
                $"외적 결과: {crossProduct}\n" +
                $"Cross.y: {crossY:F3}\n" +
                $"(Space키를 눌러 콘솔 출력)";
        }
    }
}
