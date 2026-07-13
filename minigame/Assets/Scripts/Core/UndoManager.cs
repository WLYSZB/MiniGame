using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 撤销系统 - 记录每步操作，支持按Z键回退
/// </summary>
public class UndoManager : MonoBehaviour
{
    public static UndoManager Instance { get; private set; }

    private struct MoveSnapshot
    {
        public Vector2Int playerPos;
        public List<(Vector2Int pos, PushableBox box)> boxStates;
    }

    private Stack<MoveSnapshot> history = new Stack<MoveSnapshot>();

    void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// 记录当前状态（在玩家移动前调用）
    /// </summary>
    public void Record()
    {
        var snapshot = new MoveSnapshot
        {
            playerPos = GridManager.Instance.WorldToGrid(
                FindObjectOfType<GridMovement>().transform.position),
            boxStates = new List<(Vector2Int, PushableBox)>()
        };

        foreach (var kvp in GridManager.Instance.GetAllBoxes())
        {
            snapshot.boxStates.Add((kvp.Key, kvp.Value));
        }

        history.Push(snapshot);
    }

    /// <summary>
    /// 撤销上一步（按Z键时调用）
    /// </summary>
    public void Undo()
    {
        if (history.Count == 0) return;

        var snapshot = history.Pop();
        var player = FindObjectOfType<GridMovement>();

        // 瞬间恢复玩家位置
        player.MoveInstant(snapshot.playerPos);

        // 清除所有箱子注册，然后逐个恢复
        GridManager.Instance.UnregisterAllBoxes();

        foreach (var (pos, box) in snapshot.boxStates)
        {
            box.MoveInstant(pos);
        }
    }

    public void Clear()
    {
        history.Clear();
    }
}
