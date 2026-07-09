using UnityEngine;
using System.Collections;

/// <summary>
/// 网格移动控制 - 玩家WASD/方向键网格化移动
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class GridMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    private Vector2Int gridPos;
    private bool isMoving = false;

    void Start()
    {
        gridPos = GridManager.Instance.WorldToGrid(transform.position);
        transform.position = GridManager.Instance.GridToWorld(gridPos);
    }

    void Update()
    {
        if (isMoving) return;

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
            TryMove(direction);
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
                MoveTo(newPos);
            }
            return;
        }

        MoveTo(newPos);
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
    }
}
