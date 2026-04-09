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
        for (int i = movers.Count - 1; i >= 0; i--)
        {
            var mover = movers[i]; // 튜플에서 값 꺼내기
            mover.t += Time.deltaTime / mover.duration; // t 증가 (0 ~ 1)
            mover.obj.transform.position = CubucBezire(startPoint, mover.p1, mover.p2, mover.p3, mover.landing, mover.t); // 위치 업데이트
            if (mover.t > 1f) // t가 1을 넘으면 이동 완료 → 오브젝트 제거
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
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere); // 구체 생성
            sphere.transform.position = startPoint; // 시작점에 생성
            sphere.transform.localScale = Vector3.one * Random.Range(0.3f, 0.6f); // 랜덤한 크기

            var renderer = sphere.GetComponent<Renderer>(); // 랜덤한 밝은 색상
            renderer.material.color = Random.ColorHSV(0f, 1f, 0.7f, 1f, 0.7f, 1f); // ✅ 밝은 색상 범위로 변경


            float angle = Random.Range(-20f, 20f); // 위아래 퍼짐 각도
            float rad = angle * Mathf.Deg2Rad; // 각도를 라디안으로 변환

            float distance = Vector3.Distance(startPoint, endPoint); // 시작점과 끝점 사이의 거리
            float spreadY = Mathf.Sin(rad) * distance * 0.8f; // 위아래 벌어짐
            float spreadZ = Random.Range(-0.5f, 0.5f);         // 약간의 Z 퍼짐 (입체감)

            Vector3 mid = (startPoint + endPoint) / 2f; // 시작-끝 중간점

            // ✅ 제어점을 중간점 기준으로 위아래로 배치
            Vector3 p1 = Vector3.Lerp(startPoint, mid, 0.33f) + new Vector3(0, spreadY * 0.5f, spreadZ);// 시작점과 중간점 사이에 제어점 1
            Vector3 p2 = mid + new Vector3(0, spreadY, spreadZ); // 중간점에 제어점 2 (더 크게 퍼지도록)
            Vector3 p3 = Vector3.Lerp(mid, endPoint, 0.66f) + new Vector3(0, spreadY * 0.5f, spreadZ); // 중간점과 끝점 사이에 제어점 3
            Vector3 landing = endPoint; // ✅ 항상 endPoint에 도착

            float duration = Random.Range(minDuration, maxDuration);
            movers.Add((sphere, p1, p2, p3, landing, duration, 0f));
        }
    }

    private Vector3 CubucBezire(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4, float t) // 4차 베지어 계산
    {
        Vector3 a = Vector3.Lerp(p0, p1, t); // 선형 보간으로 제어점 사이의 점 계산
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
    private void OnDrawGizmos() // 씬 뷰에서 곡선 경로 시각화
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
