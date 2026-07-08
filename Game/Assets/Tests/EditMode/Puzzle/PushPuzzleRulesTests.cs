using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class PushPuzzleRulesTests
{
    [Test]
    public void WalkingIntoEmptyCellSucceeds()
    {
        var result = PushPuzzleRules.TryMove(
            new Vector2Int(1, 1),
            Vector2Int.right,
            new HashSet<Vector2Int>(),
            new HashSet<Vector2Int>());

        Assert.That(result.Moved, Is.True);
        Assert.That(result.NextPlayerCell, Is.EqualTo(new Vector2Int(2, 1)));
        Assert.That(result.MovedCore, Is.False);
    }

    [Test]
    public void PushingCoreIntoFreeCellSucceeds()
    {
        var result = PushPuzzleRules.TryMove(
            new Vector2Int(1, 1),
            Vector2Int.right,
            new HashSet<Vector2Int>(),
            new HashSet<Vector2Int> { new Vector2Int(2, 1) });

        Assert.That(result.Moved, Is.True);
        Assert.That(result.NextPlayerCell, Is.EqualTo(new Vector2Int(2, 1)));
        Assert.That(result.MovedCore, Is.True);
        Assert.That(result.CoreFromCell, Is.EqualTo(new Vector2Int(2, 1)));
        Assert.That(result.CoreToCell, Is.EqualTo(new Vector2Int(3, 1)));
    }

    [Test]
    public void PushingCoreIntoWallFails()
    {
        var result = PushPuzzleRules.TryMove(
            new Vector2Int(1, 1),
            Vector2Int.right,
            new HashSet<Vector2Int> { new Vector2Int(3, 1) },
            new HashSet<Vector2Int> { new Vector2Int(2, 1) });

        Assert.That(result.Moved, Is.False);
        Assert.That(result.NextPlayerCell, Is.EqualTo(new Vector2Int(1, 1)));
        Assert.That(result.MovedCore, Is.False);
    }

    [Test]
    public void PushingCoreIntoAnotherCoreFails()
    {
        var result = PushPuzzleRules.TryMove(
            new Vector2Int(1, 1),
            Vector2Int.right,
            new HashSet<Vector2Int>(),
            new HashSet<Vector2Int> { new Vector2Int(2, 1), new Vector2Int(3, 1) });

        Assert.That(result.Moved, Is.False);
        Assert.That(result.NextPlayerCell, Is.EqualTo(new Vector2Int(1, 1)));
        Assert.That(result.MovedCore, Is.False);
    }

    [TestCase(0, 0)]
    [TestCase(1, 1)]
    [TestCase(2, 0)]
    [TestCase(-1, -1)]
    public void InvalidDirectionFails(int x, int y)
    {
        var result = PushPuzzleRules.TryMove(
            new Vector2Int(1, 1),
            new Vector2Int(x, y),
            new HashSet<Vector2Int>(),
            new HashSet<Vector2Int>());

        Assert.That(result.Moved, Is.False);
        Assert.That(result.NextPlayerCell, Is.EqualTo(new Vector2Int(1, 1)));
        Assert.That(result.MovedCore, Is.False);
    }
}
