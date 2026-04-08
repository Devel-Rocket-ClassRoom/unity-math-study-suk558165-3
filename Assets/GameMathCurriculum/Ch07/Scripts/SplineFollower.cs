using UnityEngine;
using Unity.Mathematics;
using UnityEngine.Splines;

public class SplineFollower : MonoBehaviour
{
    [SerializeField] private SplineContainer splineContainer;
    public Transform mover;
    public float duration = 5f;
    private float t;

    private void Start()
    {
        splineContainer = GetComponent<SplineContainer>();

        if (splineContainer == null)
        {
            Debug.LogError("SplineContainer 컴포넌트가 없습니다!", this);
            return;
        }

        // ✅ Knot 자동 생성 (원형 경로)
        SetupSpline();
    }

    private void SetupSpline()
    {
        var spline = splineContainer.Spline;
        spline.Clear();

        // 원형으로 5개 Knot 배치
        int knotCount = 5;
        float radius = 4f;

        for (int i = 0; i < knotCount; i++)
        {
            float angle = i / (float)knotCount * Mathf.PI * 2f;
            float x = Mathf.Cos(angle) * radius;
            float z = Mathf.Sin(angle) * radius;

            var knot = new BezierKnot(new float3(x, 0f, z));
            spline.Add(knot, TangentMode.AutoSmooth);
        }

        spline.Closed = true; // 루프 경로
    }

    private void Update()
    {
        if (splineContainer == null) return;

        t += Time.deltaTime / duration;
        t = Mathf.Repeat(t, 1f);

        if (!splineContainer.Evaluate(splineContainer.Spline, t,
            out float3 position,
            out float3 tangent,
            out float3 upVector))
        {
            return;
        }

        mover.position = (Vector3)position;
        mover.rotation = Quaternion.LookRotation((Vector3)tangent, (Vector3)upVector);
    }
}