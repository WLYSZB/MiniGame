using UnityEngine;
using System.Collections;

/// <summary>
/// 可推动箱子 - 推箱子核心逻辑
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class PushableBox : MonoBehaviour
{
    private Vector2Int gridPos;
    public bool IsOnTarget { get; private set; } = false;

    public System.Action<bool> OnTargetStateChanged;

    [Header("Visual Feedback")]
    public Color normalColor = Color.white;
    public Color placedColor = new Color(0.3f, 1f, 0.5f, 1f); // 科技绿
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;

        BoxCollider2D col = GetComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(0.9f, 0.9f);
    }

    void Start()
    {
        gridPos = GridManager.Instance.WorldToGrid(transform.position);
        transform.position = GridManager.Instance.GridToWorld(gridPos);
        GridManager.Instance.RegisterBox(gridPos, this);
        OnTargetStateChanged += UpdateVisual;
        UpdateVisual(IsOnTarget);
    }

    public bool TryPush(Vector2Int direction)
    {
        Vector2Int newPos = gridPos + direction;

        if (GridManager.Instance.IsBlocked(newPos) || GridManager.Instance.IsBox(newPos))
            return false;

        GridManager.Instance.UnregisterBox(gridPos);
        gridPos = newPos;
        GridManager.Instance.RegisterBox(gridPos, this);
        AudioManager.Instance?.PlaySFX(AudioManager.Instance.push);
        FindObjectOfType<TutorialHint>()?.Dismiss();
        StartCoroutine(SmoothMove(GridManager.Instance.GridToWorld(newPos)));
        return true;
    }

    IEnumerator SmoothMove(Vector3 target)
    {
        while (Vector3.Distance(transform.position, target) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, target, 8f * Time.deltaTime);
            yield return null;
        }
        transform.position = target;
        CheckIfOnTarget();
    }

    void CheckIfOnTarget()
    {
        Collider2D[] hits = Physics2D.OverlapBoxAll(transform.position, new Vector2(0.4f, 0.4f), 0);
        bool onTarget = false;
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Target"))
            {
                onTarget = true;
                break;
            }
        }

        if (onTarget && !IsOnTarget)
        {
            IsOnTarget = true;
            AudioManager.Instance?.PlaySFX(AudioManager.Instance.placed);
            OnTargetStateChanged?.Invoke(true);
            CheckLevelComplete();
        }
        else if (!onTarget && IsOnTarget)
        {
            IsOnTarget = false;
            OnTargetStateChanged?.Invoke(false);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Target"))
        {
            IsOnTarget = true;
            AudioManager.Instance?.PlaySFX(AudioManager.Instance.placed);
            OnTargetStateChanged?.Invoke(true);
            CheckLevelComplete();
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Target"))
        {
            IsOnTarget = false;
            OnTargetStateChanged?.Invoke(false);
        }
    }

    void CheckLevelComplete()
    {
        var boxes = FindObjectsOfType<PushableBox>();
        bool allOnTarget = true;
        foreach (var box in boxes)
        {
            if (!box.IsOnTarget)
            {
                allOnTarget = false;
                break;
            }
        }

        if (allOnTarget)
        {
            Debug.Log("Level Complete!");

            LevelCompleteTrigger trigger = FindObjectOfType<LevelCompleteTrigger>();
            if (trigger != null)
            {
                trigger.OnLevelComplete();
            }
            else
            {
                FindObjectOfType<LevelCompleteUI>()?.Show();
            }
        }
    }

    /// <summary>
    /// 瞬间移动到指定位置（用于撤销）
    /// </summary>
    public void MoveInstant(Vector2Int newPos)
    {
        gridPos = newPos;
        transform.position = GridManager.Instance.GridToWorld(newPos);
        GridManager.Instance.RegisterBox(gridPos, this);
        CheckIfOnTarget();
    }

    void UpdateVisual(bool onTarget)
    {
        if (spriteRenderer == null) return;
        spriteRenderer.color = onTarget ? placedColor : normalColor;
    }
}
