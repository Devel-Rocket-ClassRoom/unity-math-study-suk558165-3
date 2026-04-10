using UnityEngine;

public class DragAndDrop : MonoBehaviour
{
    [Range(10f, 500f)]
    [SerializeField] private float maxRayDistance = 500f;
    [SerializeField] private string selectableTag = "Selectable";
    [Range(0f, 2f)]
    [SerializeField] private float placeOffsetY = 0.5f;

    private Camera cam;
    private GameObject selectedObject;
    private Renderer selectedRenderer;
    private Color selectedOriginalColor;
    private bool isDragging;
    private Vector3 originPosition;
    private bool isReturning;
    private Vector3 returnVelocity;
    private Vector3 dragVelocity;
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

        // 드래그 중
        if (isDragging && selectedObject != null)
        {
            Vector3 mousePosition = Input.mousePosition;
            mousePosition.z = Vector3.Distance(cam.transform.position, originPosition);
            Vector3 worldMousePosition = cam.ScreenToWorldPoint(mousePosition);

            if (Physics.Raycast(selectedObject.transform.position + Vector3.up * 10f, Vector3.down, out RaycastHit groundHit, 1000f))
            {
                float targetY = groundHit.point.y + placeOffsetY;
                currentY = Mathf.SmoothDamp(currentY, targetY, ref yVelocity, 0.8f);
                worldMousePosition.y = currentY;
            }
            else
                worldMousePosition.y = originPosition.y;

            Vector3 targetPos = new Vector3(worldMousePosition.x, currentY, worldMousePosition.z);
            selectedObject.transform.position = Vector3.SmoothDamp(
                selectedObject.transform.position, targetPos, ref dragVelocity, 0.1f);
        }

        // 복귀 중
        if (isReturning && selectedObject != null)
        {
            if (Physics.Raycast(selectedObject.transform.position + Vector3.up * 10f, Vector3.down, out RaycastHit groundHit, 1000f))
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
            if (Input.GetMouseButtonDown(0))
                TrySelectObject(hit);
        }
    }

    private void TrySelectObject(RaycastHit hit) // 클릭한 오브젝트가 선택 가능한지 판정
    {
        DeselectCurrent(); // 기존 선택 해제

        if (hit.collider.CompareTag(selectableTag)) // 선택 가능한 태그인지 체크
        {
            selectedObject = hit.collider.gameObject; // 선택된 오브젝트 저장
            currentY = selectedObject.transform.position.y; //  현재 Y값 초기화
            selectedRenderer = selectedObject.GetComponent<Renderer>(); // 렌더러 컴포넌트 가져오기

            if (selectedRenderer != null) // 렌더러가 있는 경우, 색상 변경하여 선택 표시
            {
                selectedOriginalColor = selectedRenderer.material.color; 
                selectedRenderer.material.color = new Color(
                    selectedOriginalColor.r,
                    selectedOriginalColor.g,
                    selectedOriginalColor.b,
                    0.5f);
            }

            originPosition = selectedObject.transform.position;
            if (Physics.Raycast(selectedObject.transform.position + Vector3.up * 10f, Vector3.down, out RaycastHit groundHit, 1000f))
                originPosition.y = groundHit.point.y + placeOffsetY;

            isDragging = true;
            lastObject = null;
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