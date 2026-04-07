// =============================================================================
// Assignment_GachaSimulator.cs
// -----------------------------------------------------------------------------
// 목적: 천장(Pity) 메커니즘이 있는 뽑기(Gacha) 시스템 구현 (과제 스켈레톤)
// ★ 과제 설명:
//    기본 SSR(5성) 확률에 천장 메커니즘을 적용하는 확률 시스템.
// 사용법: 빈 GameObject에 부착, TMP_Text UI 연결, Inspector에서 파라미터 설정
// 씬: Ch05_Random
// 수업 단계: 심화② — 천장 메커니즘 (50분)
// =============================================================================

using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class Assignment_GachaSimulator : MonoBehaviour
{
    [Header("=== 뽑기 파라미터 ===")]
    [SerializeField] [Range(0.001f, 0.1f)] private float baseRate = 0.006f;
    [Tooltip("이 횟수 이후부터 확률 상승 시작")] 
    [SerializeField] [Range(1, 70)] private int softPityStart = 61;
    [Tooltip("이 횟수에서 100% 보장")] 
    [SerializeField] [Range(1, 90)] private int hardPity = 90;

    [Header("=== UI 참조 ===")]
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private TMP_Text pullHistoryText;
    [SerializeField] private TMP_Text expectedValueText;

    [Header("=== 디버그 정보 (읽기 전용) ===")]
    [SerializeField] private int totalPulls;
    [SerializeField] private int totalSSRs;
    [SerializeField] private int currentPityCount;
    [SerializeField] private float currentEffectiveRate;
    [SerializeField] private List<bool> pullHistory = new List<bool>();
    [SerializeField] private List<int> ssrPityList = new List<int>();
    private const int MAX_HISTORY = 30;

    private void Start()
    {
        if (statusText == null || pullHistoryText == null || expectedValueText == null)
        {
            Debug.LogError("[Assignment_GachaSimulator] TMP_Text UI가 할당되지 않았습니다.");
            return;
        }

        UpdateUI();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            PerformSinglePull();
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            PerformTenPulls();
        }
        if (Input.GetKeyDown(KeyCode.T))
        {
            PerformHardPityPulls();
        }
    }

    private void PerformSinglePull()
    {
        ExecutePull();
    }

    private void PerformTenPulls()
    {
        for (int i = 0; i < 10; i++)
        {
            ExecutePull();
        }
    }

    private void PerformHardPityPulls()
    {
       int pullsNeeded = hardPity - currentPityCount;
        for (int i = 0; i < pullsNeeded; i++)
        {
            ExecutePull();
        }
    }

    private void ExecutePull()
    {
        totalPulls++;
        currentPityCount++;

        if (currentPityCount == hardPity)
        {
            currentEffectiveRate = 1f; // 천장 도달 시 100% 확률
        }
        else if (currentPityCount >= softPityStart)
        {
            float pityProgress = (float)(currentPityCount - softPityStart) / (hardPity - softPityStart); // 천장까지의 진행도
            currentEffectiveRate = Mathf.Lerp(baseRate, 1f, pityProgress); // 선형 보간으로 확률 상승

        }
        else
        {
            currentEffectiveRate = baseRate; // 천장 시작 전에는 기본 확률
        }
        if ( Random.value < currentEffectiveRate)
        {
            totalSSRs++;
            pullHistory.Add(true);
            ssrPityList.Add(currentPityCount);
            currentPityCount = 0; // 천장 초기화
        }
        else
        {
            pullHistory.Add(false);
        }
        if (pullHistory.Count > MAX_HISTORY)
        {
            pullHistory.RemoveAt(0);
        }

        UpdateUI();
    }

    private void UpdateUI()
    {
        if (statusText == null) return;

        statusText.text = $"[뽑기 통계]\n" +
            $"총 뽑기: {totalPulls}\n" +
            $"획득 SSR: <color=yellow>{totalSSRs}</color>\n" +
            $"현재 천장: {currentPityCount}/{hardPity}\n" +
            $"유효 확률: <color=cyan>{currentEffectiveRate * 100f:F2}%</color>";

        if (pullHistoryText == null) return;

        string historyMsg = "[최근 뽑기 기록]\n";
        for (int i = 0; i < pullHistory.Count; i++)
        {
            historyMsg += pullHistory[i] ? "<color=yellow>★</color> " : "○ ";
            if ((i + 1) % 10 == 0) historyMsg += "\n";
        }

        pullHistoryText.text = historyMsg;

        if (expectedValueText == null) return;

        string expectedMsg = "[SSR 통계]\n";
        if (ssrPityList.Count > 0)
        {
            float averagePity = 0f;
            for (int i = 0; i < ssrPityList.Count; i++)
            {
                averagePity += ssrPityList[i];
            }
            averagePity /= ssrPityList.Count;
            expectedMsg += $"평균 천장: {averagePity:F1}\n";
            expectedMsg += $"최근 3회: ";
            for (int i = 0; i < Mathf.Min(3, ssrPityList.Count); i++)
            {
                expectedMsg += $"{ssrPityList[i]} ";
            }
        }
        else
        {
            expectedMsg += "아직 SSR 없음\n";
        }

        expectedMsg += $"\n[예상값]\n";
        float avgRate = (baseRate + currentEffectiveRate) / 2f;
        if (avgRate > 0)
        {
            float expectedPulls = (hardPity - currentPityCount) / avgRate;
            expectedMsg += $"예상 뽑기: {expectedPulls:F0}회";
        }

        expectedValueText.text = expectedMsg;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!enabled) return;

        Vector3 basePos = transform.position;
        float graphScale = 3f;
        float graphHeight = 2f;

        // 그래프 배경
        Gizmos.color = new Color(0.2f, 0.2f, 0.2f, 0.3f);
        Gizmos.DrawCube(basePos + Vector3.right * graphScale * 0.5f + Vector3.up * graphHeight * 0.5f,
                       new Vector3(graphScale, graphHeight, 0.1f));

        // 기본 확률 수평선 (피티 로직 구현 전까지 baseRate만 표시)
        Gizmos.color = new Color(0.2f, 1f, 0.2f, 0.8f);
        float baseY = baseRate * graphHeight;
        Gizmos.DrawLine(basePos + Vector3.up * baseY,
                       basePos + Vector3.right * graphScale + Vector3.up * baseY);

        // 현재 pity 위치 표시
        Gizmos.color = Color.red;
        float currentX = (float)currentPityCount / hardPity * graphScale;
        Gizmos.DrawLine(basePos + Vector3.right * currentX,
                       basePos + Vector3.right * currentX + Vector3.up * graphHeight);
    }
#endif
}
