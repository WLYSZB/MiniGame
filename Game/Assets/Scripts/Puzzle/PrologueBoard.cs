using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PrologueBoard : MonoBehaviour
{
    [SerializeField] private float cellSize = 1f;
    [SerializeField] private PlayerController player;
    [SerializeField] private Vector2Int playerCell;
    [SerializeField] private PushableCore[] cores = new PushableCore[0];
    [SerializeField] private CoreTarget[] targets = new CoreTarget[0];
    [SerializeField] private CellMarker[] walls = new CellMarker[0];
    [SerializeField] private LevelUI levelUI;
    [SerializeField] private Transform playerSprite;
    [SerializeField] private Transform[] coreSprites = new Transform[0];
    [SerializeField] private Transform[] wallSprites = new Transform[0];

    private bool inputEnabled = true;

    private void Awake()
    {
        if (player != null)
        {
            player.BindBoard(this);
            SnapPlayer();
        }

        for (int i = 0; i < cores.Length; i++)
        {
            if (cores[i] != null)
            {
                cores[i].SetCell(cores[i].Cell, cellSize);
                SnapCore(i, cores[i].Cell);
            }
        }

        SyncWallPositions();
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

                for (int i = 0; i < cores.Length; i++)
                {
                    if (cores[i] == movedCore)
                    {
                        SnapCore(i, result.CoreToCell);
                        break;
                    }
                }
            }
        }

        playerCell = result.NextPlayerCell;
        SnapPlayer();
        CheckSolved();
        return true;
    }

    private void SnapPlayer()
    {
        var worldPos = new Vector3(playerCell.x * cellSize, playerCell.y * cellSize, 0f);

        if (player != null)
        {
            player.transform.position = worldPos;
        }

        if (playerSprite != null)
        {
            playerSprite.position = worldPos;
        }
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

    private void SnapCore(int index, Vector2Int cell)
    {
        if (coreSprites == null || index >= coreSprites.Length || coreSprites[index] == null)
        {
            return;
        }

        coreSprites[index].position = new Vector3(cell.x * cellSize, cell.y * cellSize, 0f);
    }

    private void SyncWallPositions()
    {
        if (wallSprites == null || walls == null)
        {
            return;
        }

        for (int i = 0; i < Mathf.Min(walls.Length, wallSprites.Length); i++)
        {
            if (walls[i] != null && wallSprites[i] != null)
            {
                var cell = walls[i].Cell;
                wallSprites[i].position = new Vector3(cell.x * cellSize, cell.y * cellSize, 0f);
            }
        }
    }
}
