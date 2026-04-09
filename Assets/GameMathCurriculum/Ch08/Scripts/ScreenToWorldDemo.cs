using UnityEngine;
using TMPro;

// =============================================================================
// ScreenToWorldDemo.cs
// -----------------------------------------------------------------------------
// 마우스 스크린 좌표를 월드 좌표로 변환하여 오브젝트를 배치하는 데모
// =============================================================================

public class ScreenToWorldDemo : MonoBehaviour
{
    [Header("=== 변환 설정 ===")]
    [Tooltip("카메라로부터의 거리 (z값) — 이 값에 따라 월드 좌표가 달라짐")]
    [Range(1f, 50f)]
    [SerializeField] private float distanceFromCamera = 10f;

    [Header("=== 오브젝트 생성 ===")]
    [Tooltip("클릭 시 생성할 프리팹 (없으면 기본 Sphere 생성)")]
    [SerializeField] private GameObject spawnPrefab;

    [Tooltip("한 번에 존재할 수 있는 최대 오브젝트 수")]
    [Range(1, 50)]
    [SerializeField] private int maxSpawnCount = 20;

    [Header("=== 시각화 ===")]
    [Tooltip("마우스 추적 오브젝트 (씬에 미리 배치, 없으면 자동 생성)")]
    [SerializeField] private Transform cursorFollower;

    [SerializeField] private Color cursorColor = Color.yellow;

    [Header("=== UI 연결 ===")]
    [Tooltip("정보 표시용 TMP_Text")]
    [SerializeField] private TMP_Text uiInfoText;

    [Header("=== 디버그 정보 (읽기 전용) ===")]
    [SerializeField] private Vector3 currentScreenPos;
    [SerializeField] private Vector3 currentWorldPos;
    [SerializeField] private int spawnedCount;

    private Camera cam;
    private GameObject[] spawnedObjects;
    private int spawnIndex;

    private void Start()
    {
        cam = Camera.main;
        if (cam == null)
        {
            Debug.LogError("[ScreenToWorldDemo] MainCamera를 찾을 수 없습니다. " +
                "카메라에 'MainCamera' 태그를 추가하세요.");
            enabled = false;
            return;
        }

        if (cursorFollower == null)
        {
            GameObject follower = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            follower.name = "CursorFollower";
            follower.transform.localScale = Vector3.one * 0.5f;

            Destroy(follower.GetComponent<Collider>());

            Renderer rend = follower.GetComponent<Renderer>();
            if (rend != null) rend.material.color = cursorColor;

            cursorFollower = follower.transform;
        }

        spawnedObjects = new GameObject[maxSpawnCount];
        spawnIndex = 0;
        spawnedCount = 0;
    }

    private void Update()
    {
        currentScreenPos = Input.mousePosition;

        var currentScreenPosWithDepth = currentScreenPos;
        currentScreenPosWithDepth.z = distanceFromCamera;

        currentWorldPos = cam.ScreenToWorldPoint(currentScreenPosWithDepth);

        cursorFollower.position = currentWorldPos;
        if (Input.GetMouseButtonDown(0))
        {
            SpawnObject(currentWorldPos);
        }
        UpdateUI();
    }

    private void SpawnObject(Vector3 position)
    {
        if (spawnedObjects[spawnIndex] != null)
        {
            Destroy(spawnedObjects[spawnIndex]);
            spawnedCount--;
        }

        GameObject obj;
        if (spawnPrefab != null)
        {
            obj = Instantiate(spawnPrefab, position, Quaternion.identity);
        }
        else
        {
            obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            obj.transform.position = position;
            obj.transform.localScale = Vector3.one * 0.3f;
            Renderer rend = obj.GetComponent<Renderer>();
            if (rend != null) rend.material.color = Color.cyan;
        }
        obj.name = $"Spawned_{spawnIndex}";

        spawnedObjects[spawnIndex] = obj;
        spawnIndex = (spawnIndex + 1) % maxSpawnCount;
        spawnedCount = Mathf.Min(spawnedCount + 1, maxSpawnCount);
    }

    private void OnDrawGizmos()
    {
        if (!enabled) return;

        if (cam == null && Camera.main != null) cam = Camera.main;
        if (cam == null) return;

        if (Application.isPlaying)
        {
            Gizmos.color = cursorColor;
            Gizmos.DrawLine(cam.transform.position, currentWorldPos);
            Gizmos.DrawWireSphere(currentWorldPos, 0.3f);

#if UNITY_EDITOR
            VectorGizmoHelper.DrawLabel(
                currentWorldPos + Vector3.up * 0.5f,
                $"World: ({currentWorldPos.x:F1}, {currentWorldPos.y:F1}, {currentWorldPos.z:F1})\n" +
                $"Depth: {distanceFromCamera:F1}",
                cursorColor);
#endif
        }
    }

    private void UpdateUI()
    {
        if (uiInfoText == null) return;

        uiInfoText.text =
            $"[ScreenToWorldDemo] 마우스 → 월드 좌표 변환\n" +
            $"스크린: ({currentScreenPos.x:F0}, {currentScreenPos.y:F0})\n" +
            $"z (depth): <color=yellow>{distanceFromCamera:F1}</color>\n" +
            $"월드: <color=green>({currentWorldPos.x:F2}, {currentWorldPos.y:F2}, {currentWorldPos.z:F2})</color>\n" +
            $"생성된 오브젝트: {spawnedCount}/{maxSpawnCount}";
    }
}
