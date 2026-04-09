using UnityEngine;
using TMPro;

// =============================================================================
// CoordinatePipelineDemo.cs
// -----------------------------------------------------------------------------
// 스크린→뷰포트→NDC→월드 좌표 변환 파이프라인을 실시간으로 시각화
// =============================================================================

public class CoordinatePipelineDemo : MonoBehaviour
{
    [Header("=== 변환 설정 ===")]
    [Tooltip("ScreenToWorldPoint에 사용할 z값 (카메라로부터의 거리)")]
    [Range(1f, 50f)]
    [SerializeField] private float worldDepth = 10f;

    [Header("=== UI 연결 ===")]
    [Tooltip("좌표 파이프라인 표시용 TMP_Text (우측 상단 패널)")]
    [SerializeField] private TMP_Text uiCoordText;

    [Tooltip("간단 정보 표시용 TMP_Text (좌측 상단 공용 패널)")]
    [SerializeField] private TMP_Text uiInfoText;

    [Header("=== 시각화 ===")]
    [Tooltip("월드 좌표 위치에 마커 표시")]
    [SerializeField] private bool showWorldMarker = true;

    [SerializeField] private Color markerColor = new Color(0f, 0.82f, 1f, 0.8f);

    [Header("=== 디버그 정보 (읽기 전용) ===")]
    [SerializeField] private Vector3 screenCoord;
    [SerializeField] private Vector3 viewportCoord;
    [SerializeField] private Vector2 ndcCoord;
    [SerializeField] private Vector3 worldCoord;

    private Camera cam;

    private void Start()
    {
        cam = Camera.main;
        if (cam == null)
        {
            Debug.LogError("[CoordinatePipelineDemo] MainCamera를 찾을 수 없습니다.");
            enabled = false;
        }
    }

    private void Update()
    {
        screenCoord = Input.mousePosition;
        viewportCoord = cam.ScreenToWorldPoint(screenCoord);
        ndcCoord = new Vector2(viewportCoord.x * 2f - 1f, viewportCoord.y * 2f - 1f);

        Vector3 screenPointWithDepth = screenCoord;
        screenPointWithDepth.z = worldDepth;
        worldCoord = cam.ScreenToWorldPoint(screenPointWithDepth);



        UpdateUI();
    }

    private void OnDrawGizmos()
    {
        if (!enabled) return;

        if (!Application.isPlaying || !showWorldMarker) return;

        Gizmos.color = new Color(markerColor.r, markerColor.g, markerColor.b, 0.3f);
        Gizmos.DrawLine(cam.transform.position, worldCoord);

        Gizmos.color = markerColor;
        float crossSize = 0.5f;
        Gizmos.DrawLine(worldCoord - Vector3.right * crossSize, worldCoord + Vector3.right * crossSize);
        Gizmos.DrawLine(worldCoord - Vector3.up * crossSize, worldCoord + Vector3.up * crossSize);
        Gizmos.DrawLine(worldCoord - Vector3.forward * crossSize, worldCoord + Vector3.forward * crossSize);

        Gizmos.DrawWireSphere(worldCoord, 0.2f);

#if UNITY_EDITOR
        VectorGizmoHelper.DrawLabel(
            worldCoord + Vector3.up * 0.8f,
            $"Screen: ({screenCoord.x:F0}, {screenCoord.y:F0})\n" +
            $"Viewport: ({viewportCoord.x:F2}, {viewportCoord.y:F2})\n" +
            $"NDC: ({ndcCoord.x:F2}, {ndcCoord.y:F2})\n" +
            $"World: ({worldCoord.x:F1}, {worldCoord.y:F1}, {worldCoord.z:F1})",
            markerColor);
#endif
    }

    private void UpdateUI()
    {
        if (uiCoordText != null)
        {
            uiCoordText.text =
                $"<color=#00d2ff>=== 좌표 변환 파이프라인 ===</color>\n\n" +
                $"<color=#aaa>① 스크린 (px)</color>\n" +
                $"   <color=white>({screenCoord.x:F0}, {screenCoord.y:F0})</color>\n" +
                $"   범위: 0~{Screen.width} × 0~{Screen.height}\n\n" +
                $"<color=#aaa>② 뷰포트 (0~1)</color>\n" +
                $"   <color=#ffd369>({viewportCoord.x:F3}, {viewportCoord.y:F3})</color>\n" +
                $"   = 스크린 ÷ 해상도\n\n" +
                $"<color=#aaa>③ NDC (-1~1)</color>\n" +
                $"   <color=#ff8c32>({ndcCoord.x:F3}, {ndcCoord.y:F3})</color>\n" +
                $"   = 뷰포트 × 2 - 1\n\n" +
                $"<color=#aaa>④ 월드 (3D)</color>\n" +
                $"   <color=#4ecca3>({worldCoord.x:F2}, {worldCoord.y:F2}, {worldCoord.z:F2})</color>\n" +
                $"   depth(z) = {worldDepth:F1}";
        }

        if (uiInfoText != null)
        {
            uiInfoText.text =
                $"[CoordinatePipelineDemo] 좌표계 파이프라인\n" +
                $"해상도: {Screen.width} × {Screen.height}\n" +
                $"카메라: {cam.transform.position:F1}\n" +
                $"투영: {(cam.orthographic ? "직교 (Orthographic)" : "원근 (Perspective)")}\n" +
                $"depth(z): <color=yellow>{worldDepth:F1}</color>";
        }
    }
}
