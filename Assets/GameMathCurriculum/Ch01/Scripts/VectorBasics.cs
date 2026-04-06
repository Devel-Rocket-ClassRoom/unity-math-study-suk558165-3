// =============================================================================
// VectorBasics.cs
// -----------------------------------------------------------------------------
// 벡터의 덧셈, 뺄셈, 스칼라 곱 연산을 구현하고 Gizmo로 시각화하는 데모
// =============================================================================

using UnityEngine;

public class VectorBasics : MonoBehaviour
{
    [Header("=== 벡터 입력 ===")]
    [Tooltip("첫 번째 벡터 (파란색 화살표)")]
    [SerializeField] private Vector3 vectorA = new Vector3(3f, 0f, 1f);

    [Tooltip("두 번째 벡터 (빨간색 화살표)")]
    [SerializeField] private Vector3 vectorB = new Vector3(1f, 0f, 3f);

    [Header("=== 스칼라 곱 ===")]
    [Tooltip("벡터 A에 곱할 스칼라 값")]
    [Range(-3f, 3f)]
    [SerializeField] private float scalar = 2f;

    [Header("=== 좌표계 기준 ===")]
    [Tooltip("true: 월드 원점(0,0,0) 기준 / false: 오브젝트 로컬 위치 기준")]
    [SerializeField] private bool useWorldOrigin = true;

    [Header("=== 시각화 옵션 ===")]
    [Tooltip("덧셈 결과 표시 여부")]
    [SerializeField] private bool showAddition = true;

    [Tooltip("뺄셈 결과 표시 여부")]
    [SerializeField] private bool showSubtraction = true;

    [Tooltip("스칼라 곱 결과 표시 여부")]
    [SerializeField] private bool showScalarMultiply = true;

    private Vector3 sum;
    private Vector3 diff;
    private Vector3 scaled;

    private void Start()
    {
        PrintVectorOperations();
    }

    private void OnValidate()
    {
        PrintVectorOperations();
    }

    private void PrintVectorOperations()
    {
        // TODO
        sum = vectorA + vectorB;
        diff = vectorA - vectorB;
        scaled = scalar * vectorA;

        Debug.Log($"[VectorBasics] A = {vectorA}, B = {vectorB}");
        Debug.Log($"  덧셈  A + B = {sum}  (크기: {sum.magnitude:F2})");
        Debug.Log($"  뺄셈  A - B = {diff}  (크기: {diff.magnitude:F2})");
        Debug.Log($"  스칼라곱  {scalar} * A = {scaled}  (크기: {scaled.magnitude:F2})");
        Debug.Log($"  A의 크기(magnitude) = {vectorA.magnitude:F2}");
        Debug.Log($"  A의 정규화(normalized) = {vectorA.normalized}");
    }

    private void OnDrawGizmos()
    {
        if (!enabled) return;

        Vector3 origin = useWorldOrigin ? Vector3.zero : transform.position;

        VectorGizmoHelper.DrawArrow(origin, origin + vectorA, Color.blue);
        VectorGizmoHelper.DrawArrow(origin, origin + vectorB, Color.red);

        if (showAddition)
        {
            VectorGizmoHelper.DrawArrow(origin, origin + sum, Color.yellow, 0.3f);

            Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
            Gizmos.DrawLine(origin + vectorA, origin + vectorA + vectorB);
            Gizmos.color = new Color(0f, 0f, 1f, 0.3f);
            Gizmos.DrawLine(origin + vectorB, origin + vectorB + vectorA);
        }

        if (showSubtraction)
        {
            VectorGizmoHelper.DrawArrow(origin + vectorB, origin + vectorA, Color.green, 0.3f);

            Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
            Gizmos.DrawLine(origin, origin + diff);
        }

        if (showScalarMultiply)
        {
            VectorGizmoHelper.DrawArrow(origin, origin + scaled, Color.magenta, 0.3f);
        }

#if UNITY_EDITOR
        VectorGizmoHelper.DrawLabel(origin + vectorA + Vector3.up * 0.3f, "A", Color.blue);
        VectorGizmoHelper.DrawLabel(origin + vectorB + Vector3.up * 0.3f, "B", Color.red);

        if (showAddition)
            VectorGizmoHelper.DrawLabel(origin + vectorA + vectorB + Vector3.up * 0.3f, "A+B", Color.yellow);
        if (showSubtraction)
            VectorGizmoHelper.DrawLabel(origin + vectorA + Vector3.up * 0.3f + Vector3.right * 0.3f, "A-B", Color.green);
        if (showScalarMultiply)
            VectorGizmoHelper.DrawLabel(origin + vectorA * scalar + Vector3.up * 0.3f, $"{scalar}*A", Color.magenta);
#endif
    }
}
