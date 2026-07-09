# Sprite 系统改造实施计划

> **For agentic workers:** REQUIRED SUB-SKILL: Use compose:subagent (recommended) or compose:execute to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 将 PrologueBoard 从 OnGUI 文本渲染改为 Unity 2D Sprite 系统

**Architecture:** 删除 OnGUI 绘制逻辑，改为通过 SerializeField 引用 Sprite GameObject 的 Transform，在逻辑坐标变化时更新 Transform.position

**Tech Stack:** Unity 2022 LTS, C#, SpriteRenderer

## Global Constraints

- Unity 版本：团结引擎 1.9.3 (Unity 2022 LTS)
- 保持现有推箱子逻辑不变（PushPuzzleRules.cs 不动）
- 所有显示相关的 SerializeField 必须在 Inspector 中手动绑定
- cellSize 必须在所有组件中保持一致

---

## File Structure

| 文件 | 操作 | 职责 |
|------|------|------|
| `PrologueBoard.cs` | 修改 | 删除 OnGUI，添加 Sprite Transform 引用，更新坐标时同步 Transform |
| `LevelUI.cs` | 修改 | 删除 OnGUI，改用 Unity UI（保留现有 winPanel 引用） |
| `PushableCore.cs` | 不变 | 已有 transform.position 更新逻辑 |
| `CoreTarget.cs` | 不变 | 静态目标点，不需要动态更新 |
| `CellMarker.cs` | 不变 | 静态墙壁，不需要动态更新 |

---

### Task 1: 改造 PrologueBoard.cs

**Covers:** [S4]

**Files:**
- Modify: `Game/Assets/Scripts/Puzzle/PrologueBoard.cs`

**Interfaces:**
- Consumes: 无
- Produces: `PrologueBoard` 组件，通过 Inspector 绑定 Sprite GameObject

- [ ] **Step 1: 删除 OnGUI 相关代码**

删除以下内容：
```csharp
// 删除这些常量和方法
private const float CellPixels = 56f;
private const float BoardMargin = 24f;

private void OnGUI() { ... }
private string GetCellLabel(Vector2Int cell) { ... }
```

- [ ] **Step 2: 添加 Sprite Transform 引用**

在 `PrologueBoard` 类中添加字段：
```csharp
[SerializeField] private Transform playerSprite;
[SerializeField] private Transform[] coreSprites = new Transform[0];
[SerializeField] private Transform[] targetSprites = new Transform[0];
[SerializeField] private Transform[] wallSprites = new Transform[0];
```

- [ ] **Step 3: 修改 SnapPlayer 方法**

将玩家 Sprite 的 Transform 也同步位置：
```csharp
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
```

- [ ] **Step 4: 添加 SnapCore 方法**

用于更新核心 Sprite 的位置：
```csharp
private void SnapCore(int index, Vector2Int cell)
{
    if (coreSprites == null || index >= coreSprites.Length || coreSprites[index] == null)
    {
        return;
    }
    
    coreSprites[index].position = new Vector3(cell.x * cellSize, cell.y * cellSize, 0f);
}
```

- [ ] **Step 5: 修改 TryMovePlayer 方法**

在移动核心后调用 SnapCore：
```csharp
if (result.MovedCore)
{
    var movedCore = cores.FirstOrDefault(core => core != null && core.Cell == result.CoreFromCell);
    if (movedCore != null)
    {
        movedCore.SetCell(result.CoreToCell, cellSize);
        
        // 找到对应的 sprite index 并同步位置
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
```

- [ ] **Step 6: 修改 Awake 方法**

初始化时同步所有 Sprite 位置：
```csharp
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
    
    // 同步墙壁位置
    SyncWallPositions();
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
```

- [ ] **Step 7: Commit**

```bash
git add Game/Assets/Scripts/Puzzle/PrologueBoard.cs
git commit -m "refactor: replace OnGUI with Sprite Transform references in PrologueBoard"
```

---

### Task 2: 改造 LevelUI.cs

**Covers:** [S4]

**Files:**
- Modify: `Game/Assets/Scripts/UI/LevelUI.cs`

**Interfaces:**
- Consumes: 无
- Produces: `LevelUI` 组件使用 Unity UI 显示胜利面板

- [ ] **Step 1: 删除 OnGUI 方法**

删除整个 `OnGUI` 方法：
```csharp
private void OnGUI()
{
    if (!showingWinPanel)
    {
        return;
    }

    var panelRect = new Rect(24f, 280f, 280f, 140f);
    GUI.Box(panelRect, "Tutorial Complete");
    GUI.Label(new Rect(panelRect.x + 16f, panelRect.y + 36f, 220f, 24f), "Backup core restored.");

    if (GUI.Button(new Rect(panelRect.x + 16f, panelRect.y + 76f, 248f, 36f), "Back To Main Menu"))
    {
        OnBackToMenuClicked();
    }
}
```

- [ ] **Step 2: 保留现有 winPanel 引用**

现有代码已经正确使用 `winPanel.SetActive()`，无需修改。

- [ ] **Step 3: Commit**

```bash
git add Game/Assets/Scripts/UI/LevelUI.cs
git commit -m "refactor: remove OnGUI from LevelUI, keep Unity UI winPanel"
```

---

### Task 3: 验证和测试

**Covers:** [S6]

**Files:**
- 无代码修改

**Interfaces:**
- Consumes: Task 1, Task 2 的修改
- Produces: 验证报告

- [ ] **Step 1: 在 Unity 编辑器中测试**

1. 打开 Prologue 场景
2. 在 Hierarchy 中创建以下 GameObjects：
   - 玩家：创建空对象，添加 SpriteRenderer + PlayerController
   - 核心：创建空对象，添加 SpriteRenderer + PushableCore
   - 目标：创建空对象，添加 SpriteRenderer + CoreTarget
   - 墙壁：创建空对象，添加 SpriteRenderer + CellMarker
3. 将这些对象拖入 PrologueBoard 的对应数组
4. 运行游戏，验证：
   - [ ] 玩家显示为 sprite（不是方块文字）
   - [ ] 核心显示为 sprite
   - [ ] 墙壁显示为 sprite
   - [ ] 目标显示为 sprite
   - [ ] 推箱子逻辑正常
   - [ ] 胜利判定正常

- [ ] **Step 2: 检查 Inspector 绑定**

确保以下字段在 Inspector 中正确绑定：
- [ ] PrologueBoard.playerSprite
- [ ] PrologueBoard.coreSprites[]
- [ ] PrologueBoard.targetSprites[]
- [ ] PrologueBoard.wallSprites[]

- [ ] **Step 3: 记录测试结果**

如果发现问题，记录到 `docs/TESTING.md`：
```
## Sprite 系统测试
- 日期：2026-07-09
- 测试结果：通过/失败
- 问题：（如有）
```
