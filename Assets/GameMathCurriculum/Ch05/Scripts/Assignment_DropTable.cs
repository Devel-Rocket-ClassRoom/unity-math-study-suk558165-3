// =============================================================================
// Assignment_DropTable.cs
// -----------------------------------------------------------------------------
// 목적: 가중치 기반 드롭 테이블 시스템 구현 (과제 스켈레톤)
// ★ 과제 설명:
//    각 아이템에 확률 가중치(weight)가 있을 때, 누적 확률 방식으로 드롭 결과를 결정하는 알고리즘 구현.
// 사용법: 빈 GameObject에 부착, TMP_Text UI 연결, Inspector에서 items 배열 설정
// 씬: Ch05_Random
// 수업 단계: 실습① — 드롭 테이블 (45분)
// =============================================================================

using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class Assignment_DropTable : MonoBehaviour
{
    [System.Serializable]
    public struct DropItem
    {
        public string name;
        [Range(0.1f, 100f)] public float weight;
        public Color color;
    }

    [Header("=== 드롭 테이블 설정 ===")]
    [SerializeField] private DropItem[] items = new DropItem[]
    {
        new DropItem { name = "일반(초록)", weight = 70f, color = new Color(0.2f, 0.8f, 0.2f) },
        new DropItem { name = "희귀(파랑)", weight = 25f, color = new Color(0.2f, 0.6f, 1.0f) },
        new DropItem { name = "전설(보라)", weight = 5f, color = new Color(1.0f, 0.2f, 1.0f) }
    };

    [Header("=== UI 참조 ===")]
    [SerializeField] private TMP_Text resultText;
    [SerializeField] private TMP_Text statisticsText;

    [Header("=== 디버그 정보 (읽기 전용) ===")]
    [SerializeField] private float totalWeight;
    [SerializeField] private int lastSelectedIndex = -1;
    [SerializeField] private List<int> dropHistory = new List<int>();
    private const int MAX_HISTORY = 20;

    private void Start()
    {
        if (resultText == null || statisticsText == null)
        {
            Debug.LogError("[Assignment_DropTable] TMP_Text UI가 할당되지 않았습니다.");
            return;
        }

        UpdateUI();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            PerformDrop();
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            for (int i = 0; i < 10; i++)
            {
                PerformDrop();
            }
        }
    }

    private void PerformDrop()
    {
        totalWeight = 0; // 가중치 총합 계산
        foreach (var item in items) // 각 아이템의 가중치를 누적하여 총합 계산
        {
            totalWeight += item.weight;
        }
        float roll = Random.Range(0f, totalWeight); // 0부터 총 가중치 사이의 난수 생성
        float cumulative = 0f; // 누적 가중치 계산

        for (int i = 0; i < items.Length; i++) // 각 아이템의 가중치를 누적하여 난수와 비교
        {
            cumulative += items[i].weight; // 현재 아이템의 가중치를 누적
            if (cumulative >= roll) // 누적 가중치가 난수보다 크거나 같으면 해당 아이템이 선택됨
            {
                lastSelectedIndex = i;
                break;
            }
        }
        dropHistory.Add(lastSelectedIndex); // 드롭 결과를 히스토리에 추가
        if (dropHistory.Count > MAX_HISTORY) // 히스토리가 최대 개수를 초과하면 가장 오래된 기록 제거
        {
            dropHistory.RemoveAt(0);
        }

        UpdateUI();
    }

    private void UpdateUI()
    {
        if (resultText == null) return;

        string resultMsg = "[드롭 테이블]\n";
        if (lastSelectedIndex >= 0 && lastSelectedIndex < items.Length)
        {
            DropItem selected = items[lastSelectedIndex];
            resultMsg += $"결과: <color=#{ColorUtility.ToHtmlStringRGB(selected.color)}>{selected.name}</color>\n";
        }
        else
        {
            resultMsg += "결과: <color=gray>아직 드롭하지 않음</color>\n";
        }

        resultText.text = resultMsg;

        if (statisticsText == null) return;

        string statsMsg = "[확률 분포]\n";
        if (totalWeight > 0)
        {
            for (int i = 0; i < items.Length; i++)
            {
                float probability = items[i].weight / totalWeight * 100f;
                statsMsg += $"{items[i].name}: {probability:F1}%\n";
            }
        }

        statsMsg += $"\n[최근 {dropHistory.Count}/{MAX_HISTORY}회]\n";
        for (int i = 0; i < dropHistory.Count; i++)
        {
            if (dropHistory[i] >= 0 && dropHistory[i] < items.Length)
            {
                DropItem item = items[dropHistory[i]];
                statsMsg += $"<color=#{ColorUtility.ToHtmlStringRGB(item.color)}>{item.name}</color> ";
                if ((i + 1) % 5 == 0) statsMsg += "\n";
            }
        }

        statisticsText.text = statsMsg;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!enabled) return;

        if (items.Length == 0) return;

        Vector3 basePos = transform.position;
        float barWidth = 0.5f;
        float spacing = 0.6f;

        float sum = 0f;
        for (int i = 0; i < items.Length; i++)
        {
            sum += items[i].weight;
        }

        if (sum <= 0) return;

        for (int i = 0; i < items.Length; i++)
        {
            float barHeight = (items[i].weight / sum) * 2f;
            Vector3 barPos = basePos + Vector3.right * (i * spacing);

            Gizmos.color = items[i].color;
            Gizmos.DrawCube(barPos + Vector3.up * barHeight * 0.5f,
                           new Vector3(barWidth, barHeight, 0.1f));
        }
    }
#endif
}
