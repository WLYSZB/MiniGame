using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PrologueBoard : MonoBehaviour
{
    private const float CellPixels = 56f;
    private const float BoardMargin = 24f;

    [SerializeField] private float cellSize = 1f;
    [SerializeField] private PlayerController player;
    [SerializeField] private Vector2Int playerCell;
    [SerializeField] private PushableCore[] cores = new PushableCore[0];
    [SerializeField] private CoreTarget[] targets = new CoreTarget[0];
    [SerializeField] private CellMarker[] walls = new CellMarker[0];
    [SerializeField] private LevelUI levelUI;

    private bool inputEnabled = true;

    private void Awake()
    {
        if (player != null)
        {
            player.BindBoard(this);
            SnapPlayer();
        }

        foreach (var core in cores)
        {
            if (core != null)
            {
                core.SetCell(core.Cell, cellSize);
            }
        }
    }

    public void SetInputEnabled(bool enabled)
    {
        inputEnabled = enabled;
    }

    public bool TryMovePlayer(Vector2Int direction)
    {
        if (!inputEnabled || player == null)
        {
            return false;
        }

        var wallCells = new HashSet<Vector2Int>(walls.Where(wall => wall != null).Select(wall => wall.Cell));
        var coreCells = new HashSet<Vector2Int>(cores.Where(core => core != null).Select(core => core.Cell));
        var result = PushPuzzleRules.TryMove(playerCell, direction, wallCells, coreCells);

        if (!result.Moved)
        {
            return false;
        }

        if (result.MovedCore)
        {
            var movedCore = cores.FirstOrDefault(core => core != null && core.Cell == result.CoreFromCell);
            if (movedCore != null)
            {
                movedCore.SetCell(result.CoreToCell, cellSize);
            }
        }

        playerCell = result.NextPlayerCell;
        SnapPlayer();
        CheckSolved();
        return true;
    }

    private void SnapPlayer()
    {
        player.transform.position = new Vector3(playerCell.x * cellSize, playerCell.y * cellSize, 0f);
    }

    private void CheckSolved()
    {
        var coreCells = new HashSet<Vector2Int>(cores.Where(core => core != null).Select(core => core.Cell));
        var targetCells = new HashSet<Vector2Int>(targets.Where(target => target != null).Select(target => target.Cell));

        if (targetCells.Count == 0 || !targetCells.SetEquals(coreCells))
        {
            return;
        }

        inputEnabled = false;

        if (levelUI != null)
        {
            levelUI.ShowWinPanel();
        }
    }

    private void OnGUI()
    {
        var occupiedCells = new List<Vector2Int> { playerCell };
        occupiedCells.AddRange(cores.Where(core => core != null).Select(core => core.Cell));
        occupiedCells.AddRange(targets.Where(target => target != null).Select(target => target.Cell));
        occupiedCells.AddRange(walls.Where(wall => wall != null).Select(wall => wall.Cell));

        if (occupiedCells.Count == 0)
        {
            return;
        }

        var minX = occupiedCells.Min(cell => cell.x) - 1;
        var maxX = occupiedCells.Max(cell => cell.x) + 1;
        var minY = occupiedCells.Min(cell => cell.y) - 1;
        var maxY = occupiedCells.Max(cell => cell.y) + 1;

        GUI.Label(new Rect(BoardMargin, 8f, 420f, 24f), "Push the backup core onto the target. Move with WASD or arrow keys.");

        for (var y = maxY; y >= minY; y--)
        {
            for (var x = minX; x <= maxX; x++)
            {
                var cell = new Vector2Int(x, y);
                var rect = new Rect(
                    BoardMargin + (x - minX) * CellPixels,
                    BoardMargin + (maxY - y) * CellPixels,
                    CellPixels,
                    CellPixels);

                GUI.Box(rect, GetCellLabel(cell));
            }
        }
    }

    private string GetCellLabel(Vector2Int cell)
    {
        var hasWall = walls.Any(wall => wall != null && wall.Cell == cell);
        if (hasWall)
        {
            return "#";
        }

        var hasPlayer = playerCell == cell;
        var hasCore = cores.Any(core => core != null && core.Cell == cell);
        var hasTarget = targets.Any(target => target != null && target.Cell == cell);

        if (hasPlayer && hasTarget)
        {
            return "P/X";
        }

        if (hasCore && hasTarget)
        {
            return "C/X";
        }

        if (hasPlayer)
        {
            return "P";
        }

        if (hasCore)
        {
            return "C";
        }

        if (hasTarget)
        {
            return "X";
        }

        return string.Empty;
    }
}
