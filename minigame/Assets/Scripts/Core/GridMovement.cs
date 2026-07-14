using UnityEngine;
using System.Collections;

/// <summary>
/// 网格移动控制 - 玩家WASD/方向键网格化移动
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class GridMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public int stepCount { get; private set; } = 0;
    public System.Action<int> OnStepChanged;
    public static bool IsLevelComplete { get; set; } = false;

    public void DecrementStep()
    {
        stepCount--;
        OnStepChanged?.Invoke(stepCount);
    }

    private Vector2Int gridPos;
    private bool isMoving = false;
    private PlayerAnimator playerAnimator;

    public bool IsMoving => isMoving;

    void Start()
    {
        IsLevelComplete = false;
        playerAnimator = GetComponent<PlayerAnimator>();
        gridPos = GridManager.Instance.WorldToGrid(transform.position);
        transform.position = GridManager.Instance.GridToWorld(gridPos);
    }

    void Update()
    {
        if (isMoving) return;
        if (IsLevelComplete) return;

        // 撤销（Z键）
        if (Input.GetKeyDown(KeyCode.Z))
        {
            if (UndoManager.Instance != null)
            {
                UndoManager.Instance.Undo();
                DecrementStep();
            }
            return;
        }

        Vector2Int direction = Vector2Int.zero;

        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            direction = Vector2Int.up;
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            direction = Vector2Int.down;
        else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            direction = Vector2Int.left;
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            direction = Vector2Int.right;

        if (direction != Vector2Int.zero)
        {
            if (playerAnimator != null)
                playerAnimator.OnMoveStart(direction);
            TryMove(direction);
        }
    }

    void TryMove(Vector2Int direction)
    {
        Vector2Int newPos = gridPos + direction;

        // 检查墙壁和障碍物
        if (GridManager.Instance.IsBlocked(newPos))
            return;

        // 检查箱子
        if (GridManager.Instance.IsBox(newPos))
        {
            PushableBox box = GridManager.Instance.GetBox(newPos);
            if (box != null && box.TryPush(direction))
            {
                // 记录状态后移动
                if (UndoManager.Instance != null)
                    UndoManager.Instance.Record();
                AudioManager.Instance?.PlaySFX(AudioManager.Instance.footstep);
                MoveTo(newPos);
                stepCount++;
                OnStepChanged?.Invoke(stepCount);
            }
            return;
        }

        // 普通移动
        if (UndoManager.Instance != null)
            UndoManager.Instance.Record();
        AudioManager.Instance?.PlaySFX(AudioManager.Instance.footstep);
        MoveTo(newPos);
        stepCount++;
        OnStepChanged?.Invoke(stepCount);
    }

    void MoveTo(Vector2Int newPos)
    {
        isMoving = true;
        gridPos = newPos;
        StartCoroutine(SmoothMove(GridManager.Instance.GridToWorld(newPos)));
    }

    IEnumerator SmoothMove(Vector3 target)
    {
        while (Vector3.Distance(transform.position, target) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
            yield return null;
        }
        transform.position = target;
        isMoving = false;
        if (playerAnimator != null)
            playerAnimator.OnMoveEnd();
    }

    /// <summary>
    /// 瞬间移动到指定网格位置（用于撤销）
    /// </summary>
    public void MoveInstant(Vector2Int newPos)
    {
        gridPos = newPos;
        transform.position = GridManager.Instance.GridToWorld(newPos);
    }
}
