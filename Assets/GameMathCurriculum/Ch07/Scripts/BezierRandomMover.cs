using System.Collections.Generic;
using UnityEngine;

public class BezierRandomMover : MonoBehaviour
{
    [SerializeField] private Vector3 startPoint = new Vector3(-4f, 1f, 0f);
    [SerializeField] private Vector3 endPoint = new Vector3(4f, 1f, 0f);
    [SerializeField] private int sphereCount = 5;
    [SerializeField] private float minDuration = 1f;
    [SerializeField] private float maxDuration = 5f;
    [SerializeField] private float randomRange = 3f;

    private List<(GameObject obj, Vector3 p1, Vector3 p2, Vector3 p3, Vector3 landing, float duration, float t)> movers
    = new List<(GameObject, Vector3, Vector3, Vector3, Vector3, float, float)>();

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SpawnSpheres();
        }
        for (int i = movers.Count -1; i >= 0; i--)
        {
            var mover = movers[i];
            mover.t += Time.deltaTime / mover.duration;
            mover.obj.transform.position = CubucBezire(startPoint, mover.p1, mover.p2, mover.p3, mover.landing, mover.t);
            if (mover.t > 1f)
            {
                Destroy(mover.obj);
                movers.RemoveAt(i);
                continue;
            }
            movers[i] = mover;
        }
    }
    private void SpawnSpheres()
    {
        for (int i = 0; i < sphereCount; i++)
        {
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.position = startPoint;
            sphere.transform.localScale = Vector3.one * Random.Range(0.3f, 0.6f);

            var renderer = sphere.GetComponent<Renderer>();
            renderer.material.color = Random.ColorHSV(0f, 1f, 0.7f, 1f, 0.7f, 1f);

            float distance = Random.Range(5f, 10f);                      // 날아가는 거리
            float curveHeight = Random.Range(0.5f, 2f);                  // 곡선 휘어짐 정도 (작게)
            float spread = Random.Range(-1.5f, 1.5f);                    // 약간의 좌우 퍼짐

            Vector3 dir = Vector3.right;                                  // 오른쪽으로 직진

            // 제어점을 옆으로 배치 → 직선에 가깝지만 살짝 휘는 곡선
            Vector3 p1 = startPoint + dir * distance * 0.25f + Vector3.up * curveHeight + new Vector3(0, 0, spread);
            Vector3 p2 = startPoint + dir * distance * 0.5f + Vector3.up * curveHeight * 0.5f;
            Vector3 p3 = startPoint + dir * distance * 0.75f + Vector3.up * curveHeight * 0.2f;
            Vector3 landing = startPoint + dir * distance;               // 착지점 = 같은 높이

            float duration = Random.Range(minDuration, maxDuration);
            movers.Add((sphere, p1, p2, p3, landing, duration, 0f));
        }
    }

    private Vector3 CubucBezire(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4, float t)
    {
        Vector3 a = Vector3.Lerp(p0, p1, t);
        Vector3 b = Vector3.Lerp(p1, p2, t);
        Vector3 c = Vector3.Lerp(p2, p3, t);
        Vector3 d = Vector3.Lerp(p3, p4, t);

        Vector3 e = Vector3.Lerp(a, b, t);
        Vector3 f = Vector3.Lerp(b, c, t);
        Vector3 g = Vector3.Lerp(c, d, t);

        Vector3 h = Vector3.Lerp(e, f, t);
        Vector3 ii = Vector3.Lerp(f, g, t);

        return Vector3.Lerp(h, ii, t);
    }
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        foreach (var mover in movers)
        {
            int segments = 30;
            Vector3 prev = startPoint;

            for (int i = 1; i <= segments; i++)
            {
                float t = i / (float)segments;
                Vector3 curr = CubucBezire(startPoint, mover.p1, mover.p2, mover.p3, mover.landing, t);
                Gizmos.color = mover.obj.GetComponent<Renderer>().material.color; // ✅ 공이랑 같은 색
                Gizmos.DrawLine(prev, curr);
                prev = curr;
            }
        }
    }
#endif

}
