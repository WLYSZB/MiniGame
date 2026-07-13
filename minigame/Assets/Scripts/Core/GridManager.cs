using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 网格管理器 - 管理游戏中的网格系统
/// </summary>
public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    public float cellSize = 1f;
    private HashSet<Vector2Int> walls = new HashSet<Vector2Int>();
    private HashSet<Vector2Int> obstacles = new HashSet<Vector2Int>();
    private Dictionary<Vector2Int, PushableBox> boxes = new Dictionary<Vector2Int, PushableBox>();

    void Awake()
    {
        Instance = this;
    }

    public void RegisterWall(Vector2Int pos) => walls.Add(pos);
    public void RegisterObstacle(Vector2Int pos) => obstacles.Add(pos);
    public void RegisterBox(Vector2Int pos, PushableBox box) => boxes[pos] = box;
    public void UnregisterBox(Vector2Int pos) => boxes.Remove(pos);

    public bool IsWall(Vector2Int pos) => walls.Contains(pos);
    public bool IsObstacle(Vector2Int pos) => obstacles.Contains(pos);
    public bool IsBox(Vector2Int pos) => boxes.ContainsKey(pos);
    public PushableBox GetBox(Vector2Int pos) => boxes.TryGetValue(pos, out var box) ? box : null;

    public bool IsBlocked(Vector2Int pos)
    {
        return IsWall(pos) || IsObstacle(pos);
    }

    public Vector3 GridToWorld(Vector2Int gridPos)
    {
        return new Vector3(gridPos.x * cellSize, gridPos.y * cellSize, 0);
    }

    public Vector2Int WorldToGrid(Vector3 worldPos)
    {
        return new Vector2Int(
            Mathf.RoundToInt(worldPos.x / cellSize),
            Mathf.RoundToInt(worldPos.y / cellSize)
        );
    }

    public Dictionary<Vector2Int, PushableBox> GetAllBoxes() => boxes;

    public void UnregisterAllBoxes()
    {
        boxes.Clear();
    }

    public void Clear()
    {
        walls.Clear();
        obstacles.Clear();
        boxes.Clear();
    }
}
