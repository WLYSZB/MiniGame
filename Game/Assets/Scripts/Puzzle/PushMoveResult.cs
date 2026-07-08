using UnityEngine;

public struct PushMoveResult
{
    public bool Moved { get; }

    public Vector2Int NextPlayerCell { get; }

    public bool MovedCore { get; }

    public Vector2Int CoreFromCell { get; }

    public Vector2Int CoreToCell { get; }

    public PushMoveResult(bool moved, Vector2Int nextPlayerCell, bool movedCore, Vector2Int coreFromCell, Vector2Int coreToCell)
    {
        Moved = moved;
        NextPlayerCell = nextPlayerCell;
        MovedCore = movedCore;
        CoreFromCell = coreFromCell;
        CoreToCell = coreToCell;
    }
}
