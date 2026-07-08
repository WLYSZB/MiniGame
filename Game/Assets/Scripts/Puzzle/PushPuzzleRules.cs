using System.Collections.Generic;
using UnityEngine;

public static class PushPuzzleRules
{
    public static PushMoveResult TryMove(
        Vector2Int playerCell,
        Vector2Int direction,
        HashSet<Vector2Int> wallCells,
        HashSet<Vector2Int> coreCells)
    {
        if (Mathf.Abs(direction.x) + Mathf.Abs(direction.y) != 1)
        {
            return new PushMoveResult(false, playerCell, false, default, default);
        }

        var nextPlayerCell = playerCell + direction;
        if (wallCells.Contains(nextPlayerCell))
        {
            return new PushMoveResult(false, playerCell, false, default, default);
        }

        if (!coreCells.Contains(nextPlayerCell))
        {
            return new PushMoveResult(true, nextPlayerCell, false, default, default);
        }

        var nextCoreCell = nextPlayerCell + direction;
        if (wallCells.Contains(nextCoreCell) || coreCells.Contains(nextCoreCell))
        {
            return new PushMoveResult(false, playerCell, false, default, default);
        }

        return new PushMoveResult(true, nextPlayerCell, true, nextPlayerCell, nextCoreCell);
    }
}
