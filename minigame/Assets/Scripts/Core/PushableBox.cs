using UnityEngine;
using System.Collections;

/// <summary>
/// 可推动箱子 - 推箱子核心逻辑
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class PushableBox : MonoBehaviour
{
    private Vector2Int gridPos;
    public bool IsOnTarget { get; private set; } = false;

    void Start()
    {
        gridPos = GridManager.Instance.WorldToGrid(transform.position);
        transform.position = GridManager.Instance.GridToWorld(gridPos);
        GridManager.Instance.RegisterBox(gridPos, this);
    }

    public bool TryPush(Vector2Int direction)
    {
        Vector2Int newPos = gridPos + direction;

        // 箱子不能推入墙壁、障碍物或其他箱子
        if (GridManager.Instance.IsBlocked(newPos) || GridManager.Instance.IsBox(newPos))
            return false;

        // 移动箱子
        GridManager.Instance.UnregisterBox(gridPos);
        gridPos = newPos;
        GridManager.Instance.RegisterBox(gridPos, this);
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
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Target"))
        {
            IsOnTarget = true;
            CheckLevelComplete();
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Target"))
            IsOnTarget = false;
    }

    void CheckLevelComplete()
    {
        // 检查所有箱子是否都在目标点上
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
            FindObjectOfType<LevelCompleteUI>()?.Show();
        }
    }
}
