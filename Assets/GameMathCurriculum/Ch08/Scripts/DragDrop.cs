using UnityEngine;

public class DragDrop : MonoBehaviour
{
    private Camera cam;
    private LayerMask ground;
    private LayerMask dragObject;
    private LayerMask dropZone;
    private bool isDraing = false;
    private DragObject dragginObject;

    private void Start()
    {
        cam = Camera.main;
        ground = LayerMask.GetMask("Ground");
        dragObject = LayerMask.GetMask("DragObject");
        dropZone = LayerMask.GetMask("DropZone");
    }

    private void Update()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        if (Input.GetMouseButtonDown(0))
        {
            if (Physics.Raycast(ray, out RaycastHit hitInfo, Mathf.Infinity, dragObject))
            {
                isDraing = true;
                dragginObject = hitInfo.collider.GetComponent<DragObject>();
                dragginObject.DragStart();
            }
        }

        if (Input.GetMouseButtonUp(0) && dragginObject != null)
        {
            if (Physics.Raycast(ray, out RaycastHit hitInfo, Mathf.Infinity, dropZone))
            {
                dragginObject.transform.position = hitInfo.collider.transform.position + Vector3.up * 0.5f; // 추가
                dragginObject.DragEnd();
            }
            else
                dragginObject.Return();

            isDraing = false;
            dragginObject = null;
        }

        if (isDraing && dragginObject != null)
        {
            if (Physics.Raycast(ray, out RaycastHit hitInfo, Mathf.Infinity, ground))
                dragginObject.transform.position = hitInfo.point;
        }
    }
}