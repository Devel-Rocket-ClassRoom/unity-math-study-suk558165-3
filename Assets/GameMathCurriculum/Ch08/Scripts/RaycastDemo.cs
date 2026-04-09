using UnityEngine;
using TMPro;

// =============================================================================
// RaycastDemo.cs
// -----------------------------------------------------------------------------
// 레이캐스트로 오브젝트를 선택하고 바닥에 배치하는 데모
// =============================================================================

public class RaycastDemo : MonoBehaviour
{
    [Header("=== 레이캐스트 설정 ===")]
    [Tooltip("레이캐스트 최대 거리")]
    [Range(10f, 500f)]
    [SerializeField] private float maxRayDistance = 100f;

    [Tooltip("선택 가능한 오브젝트의 태그")]
    [SerializeField] private string selectableTag = "Selectable";

    [Tooltip("바닥 오브젝트의 태그")]
    [SerializeField] private string groundTag = "Ground";

    [Header("=== 오브젝트 배치 ===")]
    [Tooltip("우클릭 시 배치할 프리팹 (없으면 기본 큐브 생성)")]
    [SerializeField] private GameObject placePrefab;

    [Tooltip("배치 오브젝트의 바닥 오프셋 (y축)")]
    [Range(0f, 2f)]
    [SerializeField] private float placeOffsetY = 0.5f;

    [Header("=== 시각화 색상 ===")]
    [SerializeField] private Color colorSelected = Color.green;
    [SerializeField] private Color colorDefault = Color.white;
    [SerializeField] private Color colorRay = Color.cyan;
    [SerializeField] private Color colorHitPoint = Color.red;

    [Header("=== UI 연결 ===")]
    [Tooltip("정보 표시용 TMP_Text")]
    [SerializeField] private TMP_Text uiInfoText;

    // 디버그 정보 (매 프레임 갱신)
    private Ray lastRay;
    private RaycastHit lastHit;
    private bool isHitting;

    private Camera cam;
    private GameObject selectedObject;
    private Renderer selectedRenderer;
    private Color selectedOriginalColor;

    private void Start()
    {
        cam = Camera.main;
        if (cam == null)
        {
            Debug.LogError("[RaycastDemo] MainCamera를 찾을 수 없습니다. " +
                "카메라에 'MainCamera' 태그를 추가하세요.");
            enabled = false;
        }
    }

    private void Update()
    {
       Ray ray = cam.ScreenPointToRay(Input.mousePosition);
       lastRay = ray;

        if (Physics.Raycast(ray, out RaycastHit hit, maxRayDistance))
        {
            lastHit = hit;
            isHitting = true;
            if (Input.GetMouseButtonDown(0)) // 좌클릭
            {
                TrySelectObject(hit);
            }
            if (Input.GetMouseButtonDown(1)) // 우클릭
            {
                if (hit.collider.CompareTag(groundTag))
                {
                    TryPlaceObject(hit);
                }
            }
        }
        else
        {
            isHitting = false;
        }

        UpdateUI();
    }

    private void TrySelectObject(RaycastHit hit)
    {
        DeselectCurrent();

        if (hit.collider.CompareTag(selectableTag))
        {
            selectedObject = hit.collider.gameObject;
            selectedRenderer = selectedObject.GetComponent<Renderer>();

            if (selectedRenderer != null)
            {
                selectedOriginalColor = selectedRenderer.material.color;
                selectedRenderer.material.color = colorSelected;
            }

            Debug.Log($"[RaycastDemo] 선택됨: {selectedObject.name} at {hit.point}");
        }
    }

    private void DeselectCurrent()
    {
        if (selectedObject != null && selectedRenderer != null)
        {
            selectedRenderer.material.color = selectedOriginalColor;
        }
        selectedObject = null;
        selectedRenderer = null;
    }

    private void TryPlaceObject(RaycastHit hit)
    {
        if ( !selectedObject)
        {
            return;
        }
        if (!hit.collider.CompareTag(groundTag))
        {
            return;
        }
        Vector3 placePos = hit.point + Vector3.up * placeOffsetY; // 바닥에서 약간 띄워서 배치
        GameObject newGo = Instantiate(selectedObject, placePos, selectedObject.transform.rotation);
        newGo.tag = "Untagged"; // 새로 생성된 오브젝트는 선택 불가능하도록 태그 제거
        newGo.name = $"Placed_{selectedObject.name}";
        Debug.Log($"[RaycastDemo] '{selectedObject.name}' 배치됨: {placePos}");


        Debug.Log($"[RaycastDemo] 배치됨: {hit.point}");
    }

    private void OnDrawGizmos()
    {
        if (!enabled) return;

        if (!Application.isPlaying) return;

        if (isHitting)
        {
            Gizmos.color = colorRay;
            Gizmos.DrawLine(lastRay.origin, lastHit.point);

            Gizmos.color = colorHitPoint;
            Gizmos.DrawWireSphere(lastHit.point, 0.15f);

            VectorGizmoHelper.DrawArrow(
                lastHit.point,
                lastHit.point + lastHit.normal * 1.5f,
                Color.magenta, 0.2f);

#if UNITY_EDITOR
            VectorGizmoHelper.DrawLabel(
                lastHit.point + Vector3.up * 0.8f,
                $"Hit: {lastHit.collider?.name ?? "없음"}\n" +
                $"Pos: ({lastHit.point.x:F1}, {lastHit.point.y:F1}, {lastHit.point.z:F1})\n" +
                $"Dist: {lastHit.distance:F2}",
                colorHitPoint);
#endif
        }
        else
        {
            Gizmos.color = new Color(0f, 0.8f, 1f, 0.3f);
            Gizmos.DrawRay(lastRay.origin, lastRay.direction * maxRayDistance);
        }
    }

    private void UpdateUI()
    {
        if (uiInfoText == null) return;

        string hitStatus = isHitting
            ? $"<color=green>적중</color>: {lastHit.collider?.name ?? "없음"}"
            : "<color=red>미적중</color>";

        string selectedName = selectedObject != null
            ? $"<color=green>{selectedObject.name}</color>"
            : "없음";

        uiInfoText.text =
            $"[RaycastDemo] 레이캐스트 선택/배치\n" +
            $"Ray Origin: ({lastRay.origin.x:F1}, {lastRay.origin.y:F1}, {lastRay.origin.z:F1})\n" +
            $"Ray Dir: ({lastRay.direction.x:F2}, {lastRay.direction.y:F2}, {lastRay.direction.z:F2})\n" +
            $"판정: {hitStatus}\n" +
            $"충돌점: ({lastHit.point.x:F1}, {lastHit.point.y:F1}, {lastHit.point.z:F1})\n" +
            $"법선: ({lastHit.normal.x:F2}, {lastHit.normal.y:F2}, {lastHit.normal.z:F2})\n" +
            $"거리(t): {lastHit.distance:F2}\n" +
            $"선택: {selectedName}\n" +
            $"<color=yellow>좌클릭=선택 / 우클릭=배치</color>";
    }
}
