using UnityEngine;

public class DragAndDrop : MonoBehaviour
{
    [Header("=== 레이캐스트 설정 ===")]
    [Range(10f, 500f)]
    [SerializeField] private float maxRayDistance = 500f;
    [SerializeField] private string selectableTag = "Selectable";
    [SerializeField] private string groundTag = "Ground";

    [Header("=== 오브젝트 배치 ===")]
    [SerializeField] private GameObject placePrefab;
    [Range(0f, 2f)]
    [SerializeField] private float placeOffsetY = 0.5f;

    [Header("=== 시각화 색상 ===")]
    [SerializeField] private Color colorSelected = Color.green;

    private Ray lastRay;
    private RaycastHit lastHit;
    private bool isHitting;

    private Camera cam;
    private GameObject selectedObject;
    private Renderer selectedRenderer;
    private Color selectedOriginalColor;
    private bool isDragging;
    private Vector3 originPosition;
    private bool isReturning;
    private Vector3 returnVelocity;
    private float currentY;
    private float yVelocity;
    private GameObject lastObject;

    private void Start()
    {
        cam = Camera.main;
        if (cam == null)
        {
            Debug.LogError("[DragAndDrop] MainCamera를 찾을 수 없습니다.");
            enabled = false;
        }
    }

    private void Update()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        lastRay = ray;

        // 복귀 중
        if (isReturning && selectedObject != null)
        {
            if (Physics.Raycast(selectedObject.transform.position + Vector3.up * 10f, Vector3.down, out RaycastHit groundHit, 100f))
            {
                float targetY = groundHit.point.y + placeOffsetY;
                currentY = Mathf.SmoothDamp(currentY, targetY, ref yVelocity, 0.3f);
            }

            Vector3 target = new Vector3(originPosition.x, currentY, originPosition.z);
            selectedObject.transform.position = Vector3.SmoothDamp(
                selectedObject.transform.position, target, ref returnVelocity, 0.5f);

            Vector2 curXZ = new Vector2(selectedObject.transform.position.x, selectedObject.transform.position.z);
            Vector2 originXZ = new Vector2(originPosition.x, originPosition.z);
            if (Vector2.Distance(curXZ, originXZ) < 0.01f)
            {
                selectedObject.transform.position = originPosition;
                lastObject = selectedObject;
                isReturning = false;
            }
        }

        // 드래그 중
        if (isDragging && selectedObject != null)
        {
            Vector3 mousePosition = Input.mousePosition;
            mousePosition.z = Vector3.Distance(cam.transform.position, originPosition);
            Vector3 worldMousePosition = cam.ScreenToWorldPoint(mousePosition);

            // 위쪽에서 아래로 Raycast (오브젝트 위치 기준 위로 올려서 쏨)
            if (Physics.Raycast(selectedObject.transform.position + Vector3.up * 10f, Vector3.down, out RaycastHit groundHit, 100f))
            {
                float targetY = groundHit.point.y + placeOffsetY;
                currentY = Mathf.SmoothDamp(currentY, targetY, ref yVelocity, 0.3f);
                worldMousePosition.y = currentY;
            }
            else
                worldMousePosition.y = originPosition.y;

            selectedObject.transform.position = worldMousePosition; // placeOffsetY 중복 제거
        }

        // 드롭 판정
        if (Input.GetMouseButtonUp(0) && isDragging && selectedObject != null)
        {
            Collider[] cols = Physics.OverlapSphere(selectedObject.transform.position, 5f);
            bool snapped = false;

            foreach (var col in cols)
            {
                DropZone zone = col.GetComponent<DropZone>();
                if (zone != null && zone.ZoneColor == selectedOriginalColor)
                {
                    selectedObject.transform.position = col.transform.position + Vector3.up * placeOffsetY;
                    snapped = true;
                    DeselectCurrent();
                    break;
                }
            }

            if (!snapped)
                isReturning = true;

            isDragging = false;
        }

        // 클릭 감지
        if (Physics.Raycast(ray, out RaycastHit hit, maxRayDistance))
        {
            lastHit = hit;
            isHitting = true;

            if (Input.GetMouseButtonDown(0))
                TrySelectObject(hit);
        }
        else
        {
            isHitting = false;
        }

        // 복귀 완료 후 테라인 추적
        if (!isDragging && !isReturning && lastObject != null)
        {
            if (Physics.Raycast(lastObject.transform.position + Vector3.up * 10f, Vector3.down, out RaycastHit groundHit, 1000f))
            {
                float targetY = groundHit.point.y + placeOffsetY;
                currentY = Mathf.SmoothDamp(currentY, targetY, ref yVelocity, 0.3f);
                lastObject.transform.position = new Vector3(
                    lastObject.transform.position.x, currentY, lastObject.transform.position.z);
            }
        }
    }

    private void TrySelectObject(RaycastHit hit)
    {
        DeselectCurrent();

        if (hit.collider.CompareTag(selectableTag))
        {
            selectedObject = hit.collider.gameObject;
            currentY = selectedObject.transform.position.y;
            selectedRenderer = selectedObject.GetComponent<Renderer>();

            if (selectedRenderer != null)
            {
                selectedOriginalColor = selectedRenderer.material.color;
                selectedRenderer.material.color = new Color(
                    selectedOriginalColor.r,
                    selectedOriginalColor.g,
                    selectedOriginalColor.b,
                    0.5f);
            }

            originPosition = selectedObject.transform.position;
            isDragging = true;
            lastObject = null;
            Debug.Log($"[DragAndDrop] 선택됨: {selectedObject.name}");
        }
    }

    private void DeselectCurrent()
    {
        if (selectedObject != null && selectedRenderer != null)
            selectedRenderer.material.color = selectedOriginalColor;

        selectedObject = null;
        selectedRenderer = null;
    }
}