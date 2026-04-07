// =============================================================================
// RandomBasics.cs
// -----------------------------------------------------------------------------
// 목적: 시드(Seed) 기반 난수와 균등 분포(Uniform Distribution) 이해 (따라하기 스켈레톤)
//       동일한 시드는 항상 동일한 난수 수열을 생성함을 시연
// 사용법: 빈 GameObject에 부착, 씬에서 Seed 값 변경 시 그리드 자동 재생성
// 씬: Ch05_Random
// 수업 단계: 이론①/실습① — 균등 분포와 시드 기반 난수 (5~35분)
// =============================================================================

using UnityEngine;
using TMPro;

public class RandomBasics : MonoBehaviour
{
    [Header("=== 난수 생성 설정 ===")]
    [Tooltip("난수 생성기를 초기화할 시드값. 같은 시드 = 같은 결과")]
    [SerializeField] private int seed = 12345;

    [Tooltip("그리드 크기 (N x N)")]
    [Range(3, 20)]
    [SerializeField] private int gridSize = 10;

    [Tooltip("각 셀의 높이 배율")]
    [Range(0.1f, 5f)]
    [SerializeField] private float heightScale = 1f;

    [Tooltip("그리드 셀 간 간격")]
    [Range(0.5f, 2f)]
    [SerializeField] private float cellSpacing = 1f;

    [Header("=== UI 연결 ===")]
    [SerializeField] private TMP_Text uiInfoText;

    [Header("=== 디버그 정보 (읽기 전용) ===")]
    [SerializeField] private float debugMinValue;
    [SerializeField] private float debugMaxValue;
    [SerializeField] private float debugAverageValue;
    [SerializeField] private int debugPreviousSeed = -1;

    private float[,] gridValues;
    private Color[,] gridColors;

    private void Start()
    {
        GenerateGrid();
    }

    private void OnValidate()
    {
        if (debugPreviousSeed != seed || gridValues == null || gridValues.GetLength(0) != gridSize)
        {
            debugPreviousSeed = seed;
            GenerateGrid();
        }
    }

    private void GenerateGrid()
    {
        gridValues = new float[gridSize, gridSize];
        gridColors = new Color[gridSize, gridSize];

        Random.InitState(seed);

        float minVal = float.MaxValue;
        float maxVal = float.MinValue;
        float sumVal = 0f;

        for (int y = 0; y < gridSize; y++)
        {
            for (int x = 0; x < gridSize; x++)
            {

                float randomValue = Random.value;
                gridValues[x, y] = randomValue * heightScale;

                gridColors[x, y] = GetHeightColor(randomValue);
            }
        }

        debugMinValue = minVal;
        debugMaxValue = maxVal;
        debugAverageValue = sumVal / (gridSize * gridSize);

        UpdateUI();
    }

    private Color GetHeightColor(float normalizedValue)
    {
        if (normalizedValue < 0.25f)
            return Color.Lerp(Color.blue, Color.green, normalizedValue / 0.25f);
        else if (normalizedValue < 0.5f)
            return Color.Lerp(Color.green, Color.yellow, (normalizedValue - 0.25f) / 0.25f);
        else if (normalizedValue < 0.75f)
            return Color.Lerp(Color.yellow, new Color(1f, 0.5f, 0f), (normalizedValue - 0.5f) / 0.25f);
        else
            return Color.Lerp(new Color(1f, 0.5f, 0f), Color.red, (normalizedValue - 0.75f) / 0.25f);
    }

    private void UpdateUI()
    {
        if (uiInfoText == null) return;

        string infoText = $"<b>[RandomBasics]</b>\n";
        infoText += $"시드: <color=yellow>{seed}</color>\n";
        infoText += $"그리드 크기: {gridSize}×{gridSize}\n";
        infoText += $"높이 배율: {heightScale:F2}\n\n";
        infoText += $"<b>통계 정보:</b>\n";
        infoText += $"최소값: <color=blue>{debugMinValue:F3}</color>\n";
        infoText += $"최대값: <color=red>{debugMaxValue:F3}</color>\n";
        infoText += $"평균값: <color=green>{debugAverageValue:F3}</color>\n\n";
        infoText += $"<b>원리:</b>\n";
        infoText += $"<size=80%>Random.InitState(seed)로\n";
        infoText += $"같은 시드 = 같은 난수 수열</size>";

        uiInfoText.text = infoText;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!enabled) return;

        if (gridValues == null) return;

        Vector3 gridCenter = transform.position;
        float halfGridWidth = (gridSize - 1) * cellSpacing * 0.5f;
        float halfGridHeight = (gridSize - 1) * cellSpacing * 0.5f;

        Gizmos.color = Color.gray;
        for (int y = 0; y < gridSize; y++)
        {
            Vector3 start = gridCenter + new Vector3(-halfGridWidth, 0, (y - gridSize * 0.5f) * cellSpacing);
            Vector3 end = gridCenter + new Vector3(halfGridWidth, 0, (y - gridSize * 0.5f) * cellSpacing);
            Gizmos.DrawLine(start, end);
        }
        for (int x = 0; x < gridSize; x++)
        {
            Vector3 start = gridCenter + new Vector3((x - gridSize * 0.5f) * cellSpacing, 0, -halfGridHeight);
            Vector3 end = gridCenter + new Vector3((x - gridSize * 0.5f) * cellSpacing, 0, halfGridHeight);
            Gizmos.DrawLine(start, end);
        }

        for (int y = 0; y < gridSize; y++)
        {
            for (int x = 0; x < gridSize; x++)
            {
                Vector3 cellPos = gridCenter + new Vector3(
                    (x - gridSize * 0.5f) * cellSpacing,
                    gridValues[x, y] * 0.5f,
                    (y - gridSize * 0.5f) * cellSpacing
                );

                float height = gridValues[x, y];
                Vector3 scale = new Vector3(cellSpacing * 0.4f, height, cellSpacing * 0.4f);

                Gizmos.color = gridColors[x, y];
                DrawCube(cellPos, scale);
            }
        }
    }

    private void DrawCube(Vector3 center, Vector3 size)
    {
        Vector3 halfSize = size * 0.5f;
        Vector3[] corners = new Vector3[8];

        corners[0] = center + new Vector3(-halfSize.x, -halfSize.y, -halfSize.z);
        corners[1] = center + new Vector3(halfSize.x, -halfSize.y, -halfSize.z);
        corners[2] = center + new Vector3(halfSize.x, halfSize.y, -halfSize.z);
        corners[3] = center + new Vector3(-halfSize.x, halfSize.y, -halfSize.z);
        corners[4] = center + new Vector3(-halfSize.x, -halfSize.y, halfSize.z);
        corners[5] = center + new Vector3(halfSize.x, -halfSize.y, halfSize.z);
        corners[6] = center + new Vector3(halfSize.x, halfSize.y, halfSize.z);
        corners[7] = center + new Vector3(-halfSize.x, halfSize.y, halfSize.z);

        Gizmos.DrawLine(corners[0], corners[1]);
        Gizmos.DrawLine(corners[1], corners[5]);
        Gizmos.DrawLine(corners[5], corners[4]);
        Gizmos.DrawLine(corners[4], corners[0]);

        Gizmos.DrawLine(corners[2], corners[3]);
        Gizmos.DrawLine(corners[3], corners[7]);
        Gizmos.DrawLine(corners[7], corners[6]);
        Gizmos.DrawLine(corners[6], corners[2]);

        Gizmos.DrawLine(corners[0], corners[3]);
        Gizmos.DrawLine(corners[1], corners[2]);
        Gizmos.DrawLine(corners[4], corners[7]);
        Gizmos.DrawLine(corners[5], corners[6]);
    }
#endif
}
