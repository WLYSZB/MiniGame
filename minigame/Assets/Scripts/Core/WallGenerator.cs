using UnityEngine;

/// <summary>
/// 自动生成墙壁和地板
/// </summary>
public class WallGenerator : MonoBehaviour
{
    [Header("Grid Size")]
    public int width = 9;
    public int height = 9;

    [Header("Prefabs")]
    public GameObject wallPrefab;
    public GameObject floorPrefab;

    [Header("Generation")]
    public bool generateFloor = true;

    void Start()
    {
        GenerateWalls();
        if (generateFloor)
            GenerateFloor();
    }

    /// <summary>
    /// 生成边界墙壁
    /// </summary>
    void GenerateWalls()
    {
        if (wallPrefab == null)
        {
            Debug.LogError("Wall Prefab is not assigned!");
            return;
        }

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // 只生成边界墙壁
                if (x == 0 || x == width - 1 || y == 0 || y == height - 1)
                {
                    Vector3 pos = new Vector3(x - width / 2, y - height / 2, 0);
                    GameObject wall = Instantiate(wallPrefab, pos, Quaternion.identity, transform);
                    wall.name = $"Wall_{x}_{y}";

                    // 注册到GridManager
                    Vector2Int gridPos = new Vector2Int(x - width / 2, y - height / 2);
                    GridManager.Instance?.RegisterWall(gridPos);
                }
            }
        }
    }

    /// <summary>
    /// 生成地板（内部区域）
    /// </summary>
    void GenerateFloor()
    {
        if (floorPrefab == null)
        {
            Debug.LogWarning("Floor Prefab is not assigned, skipping floor generation.");
            return;
        }

        for (int x = 1; x < width - 1; x++)
        {
            for (int y = 1; y < height - 1; y++)
            {
                Vector3 pos = new Vector3(x - width / 2, y - height / 2, 0);
                GameObject floor = Instantiate(floorPrefab, pos, Quaternion.identity, transform);
                floor.name = $"Floor_{x}_{y}";
                floor.transform.localScale = Vector3.one * 0.95f; // 稍微缩小，留一点缝隙
            }
        }
    }

    /// <summary>
    /// 清除所有生成的对象
    /// </summary>
    public void ClearAll()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }
}
