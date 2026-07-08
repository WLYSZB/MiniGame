using UnityEngine;

public class PushableCore : MonoBehaviour
{
    [field: SerializeField] public Vector2Int Cell { get; private set; }

    public void SetCell(Vector2Int cell, float cellSize)
    {
        Cell = cell;
        transform.position = new Vector3(cell.x * cellSize, cell.y * cellSize, 0f);
    }
}
