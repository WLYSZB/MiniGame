# Prologue Vertical Slice Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use compose:subagent (recommended) or compose:execute to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build a playable `MainMenu -> 起名 -> 序章对话 -> 推核心教程 -> 胜利 -> 返回主菜单` slice inside `Game/`.

**Architecture:** Keep the first slice scene-driven and minimal. Use a small pure grid-rules helper for testable movement/push logic, then wire that helper into simple MonoBehaviours for the board, dialogue, and menu flow. Avoid physics, pathfinding, AI, and Chapter 1/2 mechanics in this slice.

**Tech Stack:** 团结引擎 1.9.3 / Unity 2022.3, C#, uGUI, TextMeshPro, Unity Test Framework

## Global Constraints

- All implementation stays inside `Game/`.
- Only ship `MainMenu.scene` and `Prologue.scene` in this slice.
- Use placeholder sprites/UI blocks only; no formal art pipeline work in this slice.
- Do not implement Chapter 1 fireballs or Chapter 2 black-hole mechanics yet.
- Keep puzzle movement grid-based and deterministic; do not use Rigidbody2D or raycast-based pushing in this slice.
- Do not add third-party packages; use the packages already present in `Game/Packages/manifest.json`.
- Prefer serialized scene references over tags/layers unless a concrete need appears.

---

## Scope Anchors

- **[S1]** The slice lives inside the existing `Game/` Tuanjie project and must remain compatible with 团结引擎 1.9.3.
- **[S2]** The player starts from a main menu, enters a custom name, and transitions into the prologue.
- **[S3]** The prologue shows opening dialogue and replaces `{playerName}` tokens with the chosen name.
- **[S4]** The prologue puzzle is a grid tutorial where the player pushes a backup core onto a target workbench.
- **[S5]** The puzzle has a clear completion state and a visible way back to the main menu.
- **[S6]** Placeholder-only visuals are acceptable as long as the slice is playable from start to finish.
- **[S7]** Scope stays limited to the prologue vertical slice; later chapters remain untouched.

## File Structure

- `Game/Assets/Scripts/Core/GameManager.cs`
  Stores the chosen player name and scene-loading entry points.
- `Game/Assets/Scripts/Core/PrologueSceneController.cs`
  Starts the opening dialogue and enables puzzle input only after dialogue finishes.
- `Game/Assets/Scripts/Puzzle/PushMoveResult.cs`
  Immutable result struct returned by the puzzle rules helper.
- `Game/Assets/Scripts/Puzzle/PushPuzzleRules.cs`
  Pure grid logic for walking, blocked moves, and pushing a single core.
- `Game/Assets/Scripts/Puzzle/PushableCore.cs`
  Scene component for a movable backup core.
- `Game/Assets/Scripts/Puzzle/CoreTarget.cs`
  Scene marker for the target workbench cell.
- `Game/Assets/Scripts/Puzzle/CellMarker.cs`
  Scene marker for blocked wall cells.
- `Game/Assets/Scripts/Puzzle/PrologueBoard.cs`
  Scene-facing board controller that consumes `PushPuzzleRules` and updates transforms.
- `Game/Assets/Scripts/Player/PlayerController.cs`
  Reads input and forwards one grid step at a time to `PrologueBoard`.
- `Game/Assets/Scripts/UI/LevelUI.cs`
  Shows the win panel and returns to the main menu.
- `Game/Assets/Scripts/Dialogue/DialogueData.cs`
  ScriptableObject holding ordered prologue lines.
- `Game/Assets/Scripts/Dialogue/DialogueFormatter.cs`
  Pure token replacement helper for `{playerName}`.
- `Game/Assets/Scripts/Dialogue/DialogueManager.cs`
  Runs dialogue lines and notifies when finished.
- `Game/Assets/Scripts/UI/DialogueUI.cs`
  Shows dialogue text, speaker name, and next button.
- `Game/Assets/Scripts/UI/MainMenuUI.cs`
  Opens the name input panel.
- `Game/Assets/Scripts/UI/NameInputUI.cs`
  Validates name input and starts the prologue.
- `Game/Assets/Tests/EditMode/Puzzle/PushPuzzleRulesTests.cs`
  EditMode tests for grid puzzle rules.
- `Game/Assets/Tests/EditMode/Dialogue/DialogueFormatterTests.cs`
  EditMode tests for name token replacement.
- `Game/Assets/Data/Dialogue/PrologueDialogue.asset`
  Serialized opening dialogue content.
- `Game/Assets/Scenes/MainMenu.scene`
  Main menu scene with start button and naming popup.
- `Game/Assets/Scenes/Prologue.scene`
  Dialogue + tutorial puzzle scene.
- `Game/ProjectSettings/EditorBuildSettings.asset`
  Scene order for play/build.
- `Game/ProjectSettings/ProjectSettings.asset`
  Optional cleanup: rename product from `Game` to `MiniGame` once the slice is running.

## Execution Order

Follow Tasks 1 -> 2 -> 3 -> 4 in order. Task 1 produces the only new logic primitive; every later task consumes it.

### Task 1: Testable Grid Puzzle Rules

**Covers:** [S4], [S7]

**Files:**
- Create: `Game/Assets/Scripts/Puzzle/PushMoveResult.cs`
- Create: `Game/Assets/Scripts/Puzzle/PushPuzzleRules.cs`
- Create: `Game/Assets/Tests/EditMode/Puzzle/PushPuzzleRulesTests.cs`

**Interfaces:**
- Produces: `PushMoveResult PushPuzzleRules.TryMove(Vector2Int playerCell, Vector2Int direction, HashSet<Vector2Int> wallCells, HashSet<Vector2Int> coreCells)`
- Produces: `bool PushMoveResult.Moved`
- Produces: `Vector2Int PushMoveResult.NextPlayerCell`
- Produces: `bool PushMoveResult.MovedCore`
- Produces: `Vector2Int PushMoveResult.CoreFromCell`
- Produces: `Vector2Int PushMoveResult.CoreToCell`
- Consumed later by: `PrologueBoard.TryMovePlayer(Vector2Int direction)`

- [ ] **Step 1: Write the failing EditMode tests**

```csharp
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class PushPuzzleRulesTests
{
    [Test]
    public void TryMove_WalksIntoEmptyCell()
    {
        var result = PushPuzzleRules.TryMove(
            new Vector2Int(0, 0),
            Vector2Int.right,
            new HashSet<Vector2Int>(),
            new HashSet<Vector2Int>());

        Assert.That(result.Moved, Is.True);
        Assert.That(result.NextPlayerCell, Is.EqualTo(new Vector2Int(1, 0)));
        Assert.That(result.MovedCore, Is.False);
    }

    [Test]
    public void TryMove_PushesCore_WhenNextCellIsFree()
    {
        var result = PushPuzzleRules.TryMove(
            new Vector2Int(0, 0),
            Vector2Int.right,
            new HashSet<Vector2Int>(),
            new HashSet<Vector2Int> { new Vector2Int(1, 0) });

        Assert.That(result.Moved, Is.True);
        Assert.That(result.NextPlayerCell, Is.EqualTo(new Vector2Int(1, 0)));
        Assert.That(result.MovedCore, Is.True);
        Assert.That(result.CoreFromCell, Is.EqualTo(new Vector2Int(1, 0)));
        Assert.That(result.CoreToCell, Is.EqualTo(new Vector2Int(2, 0)));
    }

    [Test]
    public void TryMove_Blocks_WhenCoreWouldHitWall()
    {
        var result = PushPuzzleRules.TryMove(
            new Vector2Int(0, 0),
            Vector2Int.right,
            new HashSet<Vector2Int> { new Vector2Int(2, 0) },
            new HashSet<Vector2Int> { new Vector2Int(1, 0) });

        Assert.That(result.Moved, Is.False);
        Assert.That(result.NextPlayerCell, Is.EqualTo(new Vector2Int(0, 0)));
        Assert.That(result.MovedCore, Is.False);
    }
}
```

- [ ] **Step 2: Run the tests and confirm they fail**

Run: `团结编辑器 -> Window > General > Test Runner > EditMode > Run All`

Expected: `PushPuzzleRulesTests` fails because `PushPuzzleRules` and `PushMoveResult` do not exist yet.

- [ ] **Step 3: Implement the minimal puzzle rules helper**

```csharp
// Game/Assets/Scripts/Puzzle/PushMoveResult.cs
using UnityEngine;

public readonly struct PushMoveResult
{
    public bool Moved { get; }
    public Vector2Int NextPlayerCell { get; }
    public bool MovedCore { get; }
    public Vector2Int CoreFromCell { get; }
    public Vector2Int CoreToCell { get; }

    private PushMoveResult(
        bool moved,
        Vector2Int nextPlayerCell,
        bool movedCore,
        Vector2Int coreFromCell,
        Vector2Int coreToCell)
    {
        Moved = moved;
        NextPlayerCell = nextPlayerCell;
        MovedCore = movedCore;
        CoreFromCell = coreFromCell;
        CoreToCell = coreToCell;
    }

    public static PushMoveResult Blocked(Vector2Int currentPlayerCell)
    {
        return new PushMoveResult(false, currentPlayerCell, false, currentPlayerCell, currentPlayerCell);
    }

    public static PushMoveResult Walk(Vector2Int nextPlayerCell)
    {
        return new PushMoveResult(true, nextPlayerCell, false, nextPlayerCell, nextPlayerCell);
    }

    public static PushMoveResult Push(Vector2Int nextPlayerCell, Vector2Int coreFromCell, Vector2Int coreToCell)
    {
        return new PushMoveResult(true, nextPlayerCell, true, coreFromCell, coreToCell);
    }
}

// Game/Assets/Scripts/Puzzle/PushPuzzleRules.cs
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
        var nextPlayerCell = playerCell + direction;

        if (wallCells.Contains(nextPlayerCell))
        {
            return PushMoveResult.Blocked(playerCell);
        }

        if (!coreCells.Contains(nextPlayerCell))
        {
            return PushMoveResult.Walk(nextPlayerCell);
        }

        var nextCoreCell = nextPlayerCell + direction;

        if (wallCells.Contains(nextCoreCell) || coreCells.Contains(nextCoreCell))
        {
            return PushMoveResult.Blocked(playerCell);
        }

        return PushMoveResult.Push(nextPlayerCell, nextPlayerCell, nextCoreCell);
    }
}
```

- [ ] **Step 4: Re-run EditMode tests and confirm they pass**

Run: `团结编辑器 -> Window > General > Test Runner > EditMode > Run All`

Expected: `PushPuzzleRulesTests` shows 3 passing tests.

- [ ] **Step 5: Commit Task 1**

```bash
git add Game/Assets/Scripts/Puzzle/PushMoveResult.cs Game/Assets/Scripts/Puzzle/PushPuzzleRules.cs Game/Assets/Tests/EditMode/Puzzle/PushPuzzleRulesTests.cs
git commit -m "feat: add testable prologue push puzzle rules"
```

### Task 2: Runtime Puzzle Board And Prologue Scene

**Covers:** [S4], [S5], [S6], [S7]

**Files:**
- Create: `Game/Assets/Scripts/Puzzle/CellMarker.cs`
- Create: `Game/Assets/Scripts/Puzzle/PushableCore.cs`
- Create: `Game/Assets/Scripts/Puzzle/CoreTarget.cs`
- Create: `Game/Assets/Scripts/Puzzle/PrologueBoard.cs`
- Create: `Game/Assets/Scripts/Player/PlayerController.cs`
- Create: `Game/Assets/Scripts/UI/LevelUI.cs`
- Create: `Game/Assets/Scenes/Prologue.scene`

**Interfaces:**
- Consumes: `PushPuzzleRules.TryMove(...)`
- Produces: `bool PrologueBoard.TryMovePlayer(Vector2Int direction)`
- Produces: `void PrologueBoard.SetInputEnabled(bool enabled)`
- Produces: `void PushableCore.SetCell(Vector2Int cell, float cellSize)`
- Produces: `void PlayerController.BindBoard(PrologueBoard board)`
- Consumed later by: `PrologueSceneController`, `LevelUI`

- [ ] **Step 1: Create the scene-facing puzzle components**

```csharp
// Game/Assets/Scripts/Puzzle/CellMarker.cs
using UnityEngine;

public class CellMarker : MonoBehaviour
{
    [field: SerializeField] public Vector2Int Cell { get; private set; }
}

// Game/Assets/Scripts/Puzzle/PushableCore.cs
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

// Game/Assets/Scripts/Puzzle/CoreTarget.cs
using UnityEngine;

public class CoreTarget : MonoBehaviour
{
    [field: SerializeField] public Vector2Int Cell { get; private set; }
}

// Game/Assets/Scripts/Player/PlayerController.cs
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private PrologueBoard board;

    public void BindBoard(PrologueBoard targetBoard)
    {
        board = targetBoard;
    }

    private void Update()
    {
        if (board == null)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            board.TryMovePlayer(Vector2Int.up);
        }
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            board.TryMovePlayer(Vector2Int.down);
        }
        else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            board.TryMovePlayer(Vector2Int.left);
        }
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            board.TryMovePlayer(Vector2Int.right);
        }
    }
}

// Game/Assets/Scripts/UI/LevelUI.cs
using UnityEngine;

public class LevelUI : MonoBehaviour
{
    [SerializeField] private GameObject winPanel;

    public void ShowWinPanel()
    {
        winPanel.SetActive(true);
    }

    public void OnBackToMenuClicked()
    {
        GameManager.Instance.LoadMainMenu();
    }
}
```

- [ ] **Step 2: Implement the board controller that consumes Task 1 rules**

```csharp
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PrologueBoard : MonoBehaviour
{
    [SerializeField] private float cellSize = 1f;
    [SerializeField] private PlayerController player;
    [SerializeField] private Vector2Int playerCell;
    [SerializeField] private PushableCore[] cores;
    [SerializeField] private CoreTarget[] targets;
    [SerializeField] private CellMarker[] walls;
    [SerializeField] private LevelUI levelUI;

    private bool inputEnabled;

    private void Awake()
    {
        player.BindBoard(this);
        SnapPlayer();

        foreach (var core in cores)
        {
            core.SetCell(core.Cell, cellSize);
        }
    }

    public void SetInputEnabled(bool enabled)
    {
        inputEnabled = enabled;
    }

    public bool TryMovePlayer(Vector2Int direction)
    {
        if (!inputEnabled)
        {
            return false;
        }

        var wallCells = new HashSet<Vector2Int>(walls.Select(wall => wall.Cell));
        var coreCells = new HashSet<Vector2Int>(cores.Select(core => core.Cell));
        var result = PushPuzzleRules.TryMove(playerCell, direction, wallCells, coreCells);

        if (!result.Moved)
        {
            return false;
        }

        if (result.MovedCore)
        {
            var movedCore = cores.First(core => core.Cell == result.CoreFromCell);
            movedCore.SetCell(result.CoreToCell, cellSize);
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
        var coreCells = new HashSet<Vector2Int>(cores.Select(core => core.Cell));
        var targetCells = new HashSet<Vector2Int>(targets.Select(target => target.Cell));

        if (targetCells.SetEquals(coreCells))
        {
            inputEnabled = false;
            levelUI.ShowWinPanel();
        }
    }
}
```

- [ ] **Step 3: Assemble `Prologue.scene` with the minimum playable hierarchy**

```text
Prologue.scene
|- Main Camera
|- EventSystem
|- LevelCanvas
|  |- WinPanel
|     |- WinText
|     |- BackToMenuButton
|- BoardRoot (PrologueBoard)
|  |- Player (PlayerController)
|  |- Core_A (PushableCore, Cell = 1,1)
|  |- Target_A (CoreTarget, Cell = 2,1)
|  |- Wall_0 (CellMarker)
|  |- Wall_1 (CellMarker)
|  |- Wall_2 (CellMarker)
```

Use a 4x4 tutorial layout with exactly one core and one target. Keep the puzzle solvable in 3-5 moves.

- [ ] **Step 4: Manually verify puzzle movement before dialogue wiring exists**

Run: `打开 Prologue.scene -> 在 Inspector 里把 PrologueBoard.inputEnabled 临时设为 true -> 点击 Play -> 使用 WASD/方向键推动核心`

Expected:
- Player moves exactly one cell per key press.
- Core moves only when pushed from an adjacent cell.
- Core does not pass through wall markers.
- Win panel appears when the core reaches the target cell.

- [ ] **Step 5: Commit Task 2**

```bash
git add Game/Assets/Scripts/Puzzle/CellMarker.cs Game/Assets/Scripts/Puzzle/PushableCore.cs Game/Assets/Scripts/Puzzle/CoreTarget.cs Game/Assets/Scripts/Puzzle/PrologueBoard.cs Game/Assets/Scripts/Player/PlayerController.cs Game/Assets/Scripts/UI/LevelUI.cs Game/Assets/Scenes/Prologue.scene
git commit -m "feat: add playable prologue puzzle board"
```

### Task 3: Dialogue, Naming, And Scene Flow

**Covers:** [S2], [S3], [S5], [S6], [S7]

**Files:**
- Create: `Game/Assets/Scripts/Core/GameManager.cs`
- Create: `Game/Assets/Scripts/Core/PrologueSceneController.cs`
- Create: `Game/Assets/Scripts/Dialogue/DialogueData.cs`
- Create: `Game/Assets/Scripts/Dialogue/DialogueFormatter.cs`
- Create: `Game/Assets/Scripts/Dialogue/DialogueManager.cs`
- Create: `Game/Assets/Scripts/UI/DialogueUI.cs`
- Create: `Game/Assets/Scripts/UI/MainMenuUI.cs`
- Create: `Game/Assets/Scripts/UI/NameInputUI.cs`
- Create: `Game/Assets/Tests/EditMode/Dialogue/DialogueFormatterTests.cs`
- Create: `Game/Assets/Data/Dialogue/PrologueDialogue.asset`
- Create: `Game/Assets/Scenes/MainMenu.scene`

**Interfaces:**
- Produces: `void GameManager.SetPlayerName(string playerName)`
- Produces: `string GameManager.GetPlayerName()`
- Produces: `void GameManager.LoadPrologue()`
- Produces: `void DialogueManager.Play(DialogueData dialogueData, System.Action onFinished)`
- Produces: `string DialogueFormatter.Format(string template, string playerName)`
- Produces: `void LevelUI.ShowWinPanel()`
- Consumes: `PrologueBoard.SetInputEnabled(bool enabled)`

- [ ] **Step 1: Write the failing token-replacement tests**

```csharp
using NUnit.Framework;

public class DialogueFormatterTests
{
    [Test]
    public void Format_Replaces_PlayerName_Token()
    {
        var result = DialogueFormatter.Format("你好，{playerName}", "袋袋");
        Assert.That(result, Is.EqualTo("你好，袋袋"));
    }

    [Test]
    public void Format_FallsBack_WhenNameIsBlank()
    {
        var result = DialogueFormatter.Format("你好，{playerName}", " ");
        Assert.That(result, Is.EqualTo("你好，垃圾袋"));
    }
}
```

- [ ] **Step 2: Run EditMode tests and confirm they fail**

Run: `团结编辑器 -> Window > General > Test Runner > EditMode > Run All`

Expected: `DialogueFormatterTests` fails because `DialogueFormatter` does not exist yet.

- [ ] **Step 3: Implement the minimal dialogue and flow scripts**

```csharp
// Game/Assets/Scripts/Dialogue/DialogueFormatter.cs
public static class DialogueFormatter
{
    public static string Format(string template, string playerName)
    {
        var safeName = string.IsNullOrWhiteSpace(playerName) ? "垃圾袋" : playerName.Trim();
        return template.Replace("{playerName}", safeName);
    }
}

// Game/Assets/Scripts/Dialogue/DialogueData.cs
using UnityEngine;

[CreateAssetMenu(menuName = "MiniGame/Dialogue Data", fileName = "DialogueData")]
public class DialogueData : ScriptableObject
{
    [System.Serializable]
    public struct Line
    {
        public string Speaker;
        [TextArea(2, 5)] public string Text;
    }

    public Line[] Lines;
}

// Game/Assets/Scripts/Core/GameManager.cs
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private string playerName = "垃圾袋";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetPlayerName(string value)
    {
        playerName = string.IsNullOrWhiteSpace(value) ? "垃圾袋" : value.Trim();
    }

    public string GetPlayerName()
    {
        return playerName;
    }

    public void LoadPrologue()
    {
        SceneManager.LoadScene("Prologue");
    }

    public void LoadMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}

// Game/Assets/Scripts/UI/MainMenuUI.cs
using UnityEngine;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private NameInputUI nameInputUI;

    public void OnStartClicked()
    {
        nameInputUI.Show();
    }
}

// Game/Assets/Scripts/UI/NameInputUI.cs
using TMPro;
using UnityEngine;

public class NameInputUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_InputField inputField;

    public void Show()
    {
        panel.SetActive(true);
        inputField.text = string.Empty;
        inputField.ActivateInputField();
    }

    public void OnConfirmClicked()
    {
        GameManager.Instance.SetPlayerName(inputField.text);
        panel.SetActive(false);
        GameManager.Instance.LoadPrologue();
    }
}

```

- [ ] **Step 4: Implement the dialogue runtime and scene controller**

```csharp
// Game/Assets/Scripts/UI/DialogueUI.cs
using TMPro;
using UnityEngine;

public class DialogueUI : MonoBehaviour
{
    [SerializeField] private GameObject root;
    [SerializeField] private TextMeshProUGUI speakerNameText;
    [SerializeField] private TextMeshProUGUI dialogueText;

    public void Show(string speaker, string text)
    {
        root.SetActive(true);
        speakerNameText.text = speaker;
        dialogueText.text = text;
    }

    public void Hide()
    {
        root.SetActive(false);
    }
}

// Game/Assets/Scripts/Dialogue/DialogueManager.cs
using System;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    [SerializeField] private DialogueUI dialogueUI;

    private DialogueData currentData;
    private int currentIndex;
    private Action onFinished;

    public void Play(DialogueData dialogueData, Action finished)
    {
        currentData = dialogueData;
        currentIndex = 0;
        onFinished = finished;
        ShowCurrentLine();
    }

    public void OnNextClicked()
    {
        currentIndex++;

        if (currentIndex >= currentData.Lines.Length)
        {
            dialogueUI.Hide();
            onFinished?.Invoke();
            return;
        }

        ShowCurrentLine();
    }

    private void ShowCurrentLine()
    {
        var line = currentData.Lines[currentIndex];
        var text = DialogueFormatter.Format(line.Text, GameManager.Instance.GetPlayerName());
        dialogueUI.Show(line.Speaker, text);
    }
}

// Game/Assets/Scripts/Core/PrologueSceneController.cs
using UnityEngine;

public class PrologueSceneController : MonoBehaviour
{
    [SerializeField] private DialogueData openingDialogue;
    [SerializeField] private DialogueManager dialogueManager;
    [SerializeField] private PrologueBoard board;

    private void Start()
    {
        board.SetInputEnabled(false);
        dialogueManager.Play(openingDialogue, OnDialogueFinished);
    }

    private void OnDialogueFinished()
    {
        board.SetInputEnabled(true);
    }
}
```

Create `PrologueDialogue.asset` with the first 5-6 lines only:

```text
行星: 哇！我的平衡器！！你个混蛋！
主角: 啊哈哈……
行星: 这是备用核心，你要做的就是把核心放到星球们的工作台上。
主角: 我明白了……大概。
行星: 先拿这个练手，别再给我惹事了。
```

- [ ] **Step 5: Assemble `MainMenu.scene` and wire both scenes**

```text
MainMenu.scene
|- Main Camera
|- EventSystem
|- GameManager
|- Canvas
|  |- TitleText
|  |- StartButton (calls MainMenuUI.OnStartClicked)
|  |- NameInputPanel (inactive by default)
|     |- NameInputField (TMP Input Field)
|     |- ConfirmButton (calls NameInputUI.OnConfirmClicked)
```

In `Prologue.scene`, wire:
- `DialogueCanvas` and `DialoguePanel` hierarchy.
- `DialogueUI.Show()` and `DialogueManager.OnNextClicked()` to the next button.
- `LevelUI.OnBackToMenuClicked()` to the win panel button.
- `PrologueSceneController` references to `DialogueData`, `DialogueManager`, and `PrologueBoard`.

- [ ] **Step 6: Re-run EditMode tests and confirm the formatter tests pass**

Run: `团结编辑器 -> Window > General > Test Runner > EditMode > Run All`

Expected: `PushPuzzleRulesTests` and `DialogueFormatterTests` all pass.

- [ ] **Step 7: Commit Task 3**

```bash
git add Game/Assets/Scripts/Core/GameManager.cs Game/Assets/Scripts/Core/PrologueSceneController.cs Game/Assets/Scripts/Dialogue/DialogueData.cs Game/Assets/Scripts/Dialogue/DialogueFormatter.cs Game/Assets/Scripts/Dialogue/DialogueManager.cs Game/Assets/Scripts/UI/DialogueUI.cs Game/Assets/Scripts/UI/MainMenuUI.cs Game/Assets/Scripts/UI/NameInputUI.cs Game/Assets/Tests/EditMode/Dialogue/DialogueFormatterTests.cs Game/Assets/Data/Dialogue/PrologueDialogue.asset Game/Assets/Scenes/MainMenu.scene Game/Assets/Scenes/Prologue.scene
git commit -m "feat: add prologue menu and dialogue flow"
```

### Task 4: Scene List, Naming Cleanup, And End-To-End Verification

**Covers:** [S1], [S2], [S3], [S4], [S5], [S6], [S7]

**Files:**
- Modify: `Game/ProjectSettings/EditorBuildSettings.asset`
- Modify: `Game/ProjectSettings/ProjectSettings.asset`
- Optional modify after slice works: `README.md`
- Optional modify after slice works: `docs/TEAM.md`

**Interfaces:**
- Consumes: all runtime scripts and scenes from Tasks 1-3
- Produces: fully playable start-to-finish vertical slice

- [ ] **Step 1: Put the correct scenes into build settings**

```text
Scene 0: Assets/Scenes/MainMenu.scene
Scene 1: Assets/Scenes/Prologue.scene
```

Remove `Assets/Scenes/SampleScene.scene` from build settings once the two real scenes are saved.

- [ ] **Step 2: Rename the product from `Game` to `MiniGame`**

Change in `Game/ProjectSettings/ProjectSettings.asset`:

```yaml
companyName: DefaultCompany
productName: MiniGame
applicationIdentifier:
  Standalone: com.DefaultCompany.MiniGame
```

- [ ] **Step 3: Run the full end-to-end slice manually**

Run: `打开 MainMenu.scene -> 点击 Play`

Expected flow:
- Start button opens the naming panel.
- Entering `袋袋` and confirming loads `Prologue.scene`.
- Dialogue shows and replaces `{playerName}` with `袋袋`.
- Puzzle input stays disabled until the last dialogue line is closed.
- Player pushes the core onto the target.
- Win panel appears.
- Back-to-menu returns to `MainMenu.scene` without errors.

- [ ] **Step 4: Run the automated tests again before claiming completion**

Run: `团结编辑器 -> Window > General > Test Runner > EditMode > Run All`

Expected: all EditMode tests pass.

- [ ] **Step 5: Only after the slice works, sync the top-level docs**

Update these two mismatches after gameplay is proven:
- `README.md`: replace the old `src/` structure with the real `Game/` structure.
- `docs/TEAM.md`: replace `Unity (C#)` with `团结引擎 1.9.3` and update the gameplay scope from `射击/解密/推箱子` to `序章/第一章/第二章`.

- [ ] **Step 6: Commit Task 4**

```bash
git add Game/ProjectSettings/EditorBuildSettings.asset Game/ProjectSettings/ProjectSettings.asset README.md docs/TEAM.md
git commit -m "chore: finalize prologue vertical slice setup"
```

## Self-Review

- **Spec coverage:** `[S1]` through `[S7]` are all covered by Tasks 1-4.
- **Placeholder scan:** no `TODO`, `TBD`, or undefined hand-waving instructions remain.
- **Type consistency:** `PushPuzzleRules.TryMove`, `PrologueBoard.TryMovePlayer`, `DialogueFormatter.Format`, and `DialogueManager.Play` are defined once and reused consistently.

## Recommended First Execution Batch

If implementing inline, execute Task 1 and Task 2 first, then stop for a quick playable-board review before wiring the dialogue flow.
