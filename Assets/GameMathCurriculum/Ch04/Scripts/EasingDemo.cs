// =============================================================================
// EasingDemo.cs
// -----------------------------------------------------------------------------
// 다양한 이징 함수를 적용한 점프와 이동 애니메이션 데모
// =============================================================================

using UnityEngine;
using TMPro;

public class EasingDemo : MonoBehaviour
{
    public enum EasingType
    {
        Linear,
        EaseInQuad,
        EaseOutQuad,
        EaseInOutQuad,
        EaseInCubic,
        EaseOutCubic,
        EaseOutBounce,
        EaseOutElastic,
        EaseOutBack
    }

    [Header("=== 이징 설정 ===")]
    [SerializeField] private EasingType easingType = EasingType.EaseOutQuad;
    [SerializeField] private bool useAnimationCurve = false;
    [Tooltip("Inspector에서 커브를 직접 편집하여 커스텀 이징 함수 정의")]
    [SerializeField] private AnimationCurve customCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("=== 점프 설정 ===")]
    [SerializeField] private float jumpHeight = 3f;
    [SerializeField] [Range(0.2f, 3f)] private float jumpDuration = 0.8f;
    [SerializeField] private bool autoRepeat = true;

    [Header("=== 이동 데모 ===")]
    [SerializeField] [Range(1f, 20f)] private float moveDistance = 10f;
    [SerializeField] [Range(0.5f, 5f)] private float moveDuration = 2f;

    [Header("=== UI 연결 ===")]
    [SerializeField] private TextMeshProUGUI uiInfoText;

    [Header("=== 디버그 정보 (읽기 전용) ===")]
    [SerializeField] private float currentT = 0f;
    [SerializeField] private float currentEasedValue = 0f;
    [SerializeField] private bool isAnimating = false;

    private Vector3 startPos;
    private float jumpTimer = 0f;
    private float moveTimer = 0f;
    private bool isJumping = false;
    private bool isMoving = false;

    private void Start()
    {
        startPos = transform.position;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            StartJump();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            StartMove();
        }

        if (isJumping)
        {
            UpdateJump();
        }

        if (isMoving)
        {
            UpdateMove();
        }

        if (autoRepeat && !isJumping && !isMoving)
        {
            jumpTimer += Time.deltaTime;
            if (jumpTimer > 2f)
            {
                StartJump();
                jumpTimer = 0f;
            }
        }

        UpdateUI();
    }

    private void StartJump()
    {
        isJumping = true;
        jumpTimer = 0f;
    }

    private void UpdateJump()
    {
        jumpTimer += Time.deltaTime;
        currentT = Mathf.Clamp01(jumpTimer / jumpDuration);

        if (useAnimationCurve)
        {
            currentEasedValue = customCurve.Evaluate(currentT);
        }
        else
        {
            currentEasedValue = Ease(easingType, currentT);
        }
        // 0 ~ 1 ~ 0 형태로 점프 궤적 만들기
        float jumpCurve = currentEasedValue * (1f - currentEasedValue) * 4f;

        Vector3 newPos = startPos;
        newPos.y = startPos.y + jumpHeight * jumpCurve;
        transform.position = newPos;

        if (currentT >= 1f)
        {
            isJumping = false;
            transform.position = startPos;
        }
    }

    private void StartMove()
    {
        isMoving = true;
        moveTimer = 0f;
    }

    private void UpdateMove()
    {
        moveTimer += Time.deltaTime;
        currentT = Mathf.Clamp01(moveTimer / moveDuration);

        if (useAnimationCurve)
        {
            currentEasedValue = customCurve.Evaluate(currentT);
        }
        else
        {
            currentEasedValue = Ease(easingType, currentT);
        }

        Vector3 newPos = startPos;
        newPos.x = startPos.x + moveDistance * currentEasedValue;
        transform.position = newPos;

        if (currentT >= 1f)
        {
            isMoving = false;
            transform.position = startPos;
        }
    }

    private static float Ease(EasingType type, float t)
    {
        return type switch
        {
            EasingType.Linear => EaseLinear(t),
            EasingType.EaseInQuad => EaseInQuad(t),
            EasingType.EaseOutQuad => EaseOutQuad(t),
            EasingType.EaseInOutQuad => EaseInOutQuad(t),
            EasingType.EaseInCubic => EaseInCubic(t),
            EasingType.EaseOutCubic => EaseOutCubic(t),
            EasingType.EaseOutBounce => EaseOutBounce(t),
            EasingType.EaseOutElastic => EaseOutElastic(t),
            EasingType.EaseOutBack => EaseOutBack(t),
            _ => t
        };
    }

    private static float EaseLinear(float t)
    {
        return t;
    }

    private static float EaseInQuad(float t)
    {
        return t * t;
    }

    private static float EaseOutQuad(float t)
    {
        return t * (2f - t);
    }

    private static float EaseInOutQuad(float t)
    {
        if (t < 0.5f)
        {
            return 2f * t * t;
        }
        else
        {
            return 1f - Mathf.Pow(-2f * t + 2f, 2f) / 2f;
        }
    }

    private static float EaseInCubic(float t)
    {
        return t * t * t;
    }

    private static float EaseOutCubic(float t)
    {
        return 1f - Mathf.Pow(1f - t, 3f);
    }

    private static float EaseOutBounce(float t)
    {
        const float n1 = 7.5625f;
        const float d1 = 2.75f;

        if (t < 1f / d1)
        {
            return n1 * t * t;
        }
        else if (t < 2f / d1)
        {
            return n1 * (t -= 1.5f / d1) * t + 0.75f;
        }
        else if (t < 2.5f / d1)
        {
            return n1 * (t -= 2.25f / d1) * t + 0.9375f;
        }
        else
        {
            return n1 * (t -= 2.625f / d1) * t + 0.984375f;
        }
    }

    private static float EaseOutElastic(float t)
    {
        const float c4 = (2f * Mathf.PI) / 3f;

        if (t == 0f) return 0f;
        if (t == 1f) return 1f;

        return Mathf.Pow(2f, -10f * t) * Mathf.Sin((t * 10f - 0.75f) * c4) + 1f;
    }

    private static float EaseOutBack(float t)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;

        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }

    private void UpdateUI()
    {
        if (uiInfoText == null) return;

        string easingName = useAnimationCurve ? "AnimationCurve" : easingType.ToString();
        string statusColor = (isJumping || isMoving) ? "yellow" : "white";

        uiInfoText.text = $"[EasingDemo] 이징 함수 데모\n" +
            $"이징: <color={statusColor}>{easingName}</color>\n" +
            $"t: {currentT:F3} → 이징 값: {currentEasedValue:F3}\n" +
            $"Space: 점프 | R: 이동 재생 | AutoRepeat: {(autoRepeat ? "<color=green>ON</color>" : "<color=red>OFF</color>")}";
    }

    private void OnDrawGizmos()
    {
        if (!enabled) return;

#if UNITY_EDITOR

        if (!Application.isPlaying) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.2f);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(startPos, 0.15f);

        Vector3 graphOrigin = new Vector3(-5f, 5f, 0f);
        float graphScale = 3f;

        Gizmos.color = Color.cyan;
        Vector3 prevPoint = graphOrigin;

        for (int i = 0; i <= 20; i++)
        {
            float sampleT = i / 20f;
            float easedValue;

            if (useAnimationCurve)
            {
                easedValue = customCurve.Evaluate(sampleT);
            }
            else
            {
                easedValue = Ease(easingType, sampleT);
            }

            float displayValue = isJumping ? easedValue * (1f - easedValue) * 4f : easedValue;

            Vector3 newPoint = graphOrigin + new Vector3(sampleT * graphScale, displayValue * graphScale, 0f);
            Gizmos.DrawLine(prevPoint, newPoint);
            prevPoint = newPoint;
        }

        Gizmos.color = Color.yellow;
        float currentEased = useAnimationCurve ? customCurve.Evaluate(currentT) : Ease(easingType, currentT);
        float currentDisplay = isJumping ? currentEased * (1f - currentEased) * 4f : currentEased;
        Vector3 currentGraphPos = graphOrigin + new Vector3(currentT * graphScale, currentDisplay * graphScale, 0f);
        Gizmos.DrawWireSphere(currentGraphPos, 0.1f);

        Gizmos.color = Color.white;
        Gizmos.DrawLine(graphOrigin, graphOrigin + Vector3.right * graphScale);
        Gizmos.color = Color.gray;
        Gizmos.DrawLine(graphOrigin, graphOrigin + Vector3.up * graphScale);

#endif
    }
}
