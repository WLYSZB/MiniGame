# Sprite 系统改造设计

## [S1] 问题

当前游戏使用 `OnGUI` 绘制调试级方块文字，没有任何图形资源，用户体验极差。

## [S2] 目标

将 `PrologueBoard` 从 OnGUI 文本渲染改为 Unity 2D Sprite 系统，支持：
- 角色（玩家、核心）使用 SpriteRenderer 显示
- 墙壁、目标点使用 SpriteRenderer 显示
- 美术组可以在 Unity 编辑器中直接拖拽素材到场景
- 保留现有推箱子逻辑不变

## [S3] 架构

### 3.1 场景结构

```
Prologue Scene
├── Board (PrologueBoard.cs)
│   ├── Player (SpriteRenderer + PlayerController)
│   ├── Core_1 (SpriteRenderer + PushableCore)
│   ├── Core_2 (SpriteRenderer + PushableCore)
│   ├── Target_1 (SpriteRenderer + CoreTarget)
│   ├── Target_2 (SpriteRenderer + CoreTarget)
│   ├── Wall_1 (SpriteRenderer + CellMarker)
│   ├── Wall_2 (SpriteRenderer + CellMarker)
│   └── ...
├── Camera
├── Canvas
│   ├── DialogueUI
│   └── LevelUI
└── GameManager
```

### 3.2 组件职责

| 组件 | 职责 |
|------|------|
| `PrologueBoard` | 管理关卡逻辑、网格坐标、胜负判定 |
| `PlayerController` | 处理玩家输入，调用 Board 移动 |
| `PushableCore` | 核心（箱子），持有 SpriteRenderer 引用 |
| `CoreTarget` | 目标位置，持有 SpriteRenderer 引用 |
| `CellMarker` | 墙壁/障碍物，持有 SpriteRenderer 引用 |

### 3.3 坐标系统

- 逻辑坐标：`Vector2Int`（网格坐标，如 (0,0), (1,2)）
- 世界坐标：`Vector3`（像素坐标，通过 `cellSize` 转换）
- 转换公式：`worldPos = logicalPos * cellSize`

## [S4] 改动点

### 4.1 PrologueBoard.cs

**删除**：
- `OnGUI()` 方法（整个 GUI 绘制逻辑）
- `GetCellLabel()` 方法
- `CellPixels` 常量

**修改**：
- `SnapPlayer()` → 使用 SpriteRenderer 的 GameObject.transform.position
- `CheckSolved()` → 保持不变
- 新增 `SnapCore()` 方法，更新核心的 GameObject 位置

**新增**：
- `[SerializeField] private float cellSize = 1f;`（已有，保持）
- `[SerializeField] private Transform playerSprite;`（玩家 Sprite 的 Transform 引用）
- `[SerializeField] private Transform[] coreSprites;`（核心 Sprite 的 Transform 数组）
- `[SerializeField] private Transform[] targetSprites;`（目标 Sprite 的 Transform 数组）
- `[SerializeField] private Transform[] wallSprites;`（墙壁 Sprite 的 Transform 数组）

### 4.2 PushableCore.cs

**修改**：
- 添加 `[SerializeField] private Transform spriteTransform;`
- `SetCell()` 方法中更新 `spriteTransform.position`

### 4.3 CoreTarget.cs

**修改**：
- 添加 `[SerializeField] private Transform spriteTransform;`

### 4.4 CellMarker.cs

**修改**：
- 添加 `[SerializeField] private Transform spriteTransform;`

### 4.5 PlayerController.cs

**保持不变**：
- 输入处理逻辑不变
- 仍然通过 `board.TryMovePlayer()` 移动

## [S5] 美术工作流

### 5.1 准备素材

美术组需要提供：
- 玩家角色 sprite（PNG，带透明通道）
- 核心 sprite（备用核心，蓝色球体）
- 目标点 sprite（工作台，标记位置）
- 墙壁 sprite（障碍物）

### 5.2 在 Unity 中设置

1. 将 sprite 拖入 Project 窗口
2. 在 Inspector 中设置：
   - Sprite Mode: Single
   - Pixels Per Unit: 根据素材调整
   - Filter Mode: Point (像素风) 或 Bilinear (平滑)
3. 创建 Prefab：
   - 玩家 Prefab：SpriteRenderer + PlayerController
   - 核心 Prefab：SpriteRenderer + PushableCore
   - 目标 Prefab：SpriteRenderer + CoreTarget
   - 墙壁 Prefab：SpriteRenderer + CellMarker

### 5.3 搭建关卡

1. 在 Prologue 场景中拖入 Prefab
2. 在 Inspector 中设置每个对象的：
   - Sprite：选择对应的 sprite
   - Cell：设置网格坐标（如 (0,0)）
3. 将所有对象拖入 PrologueBoard 的对应数组：
   - playerSprite
   - coreSprites
   - targetSprites
   - wallSprites

## [S6] 验证标准

- [ ] 玩家角色显示为 sprite，不再是方块文字
- [ ] 核心显示为 sprite
- [ ] 墙壁显示为 sprite
- [ ] 目标点显示为 sprite
- [ ] 推箱子逻辑正常工作
- [ ] 胜利判定正常
- [ ] 可以在 Unity 编辑器中替换 sprite
- [ ] 不影响对话系统和 UI

## [S7] 风险与缓解

| 风险 | 缓解措施 |
|------|----------|
| 现有逻辑被破坏 | 只改显示层，不碰 PushPuzzleRules |
| 坐标计算错误 | 保持 cellSize 一致，测试多个网格大小 |
| 美术素材尺寸不统一 | 提供 sprite 设置规范文档 |
