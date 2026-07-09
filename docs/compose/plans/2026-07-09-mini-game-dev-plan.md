# MiniGame Implementation Plan (序章 + 第一章)

> **For agentic workers:** Use compose:execute or compose:subagent to implement this plan task-by-task.

**Goal:** 实现游戏序章（推箱子教程）和第一章（推箱子+火球躲避），包含完整对话系统和UI框架。

**Architecture:** 基于Unity的2D网格化游戏。所有游戏逻辑通过C#脚本实现，场景数据通过ScriptableObject配置，对话数据通过JSON文件驱动。玩家操作网格化移动，核心（箱子）可推动，目标点检测完成关卡。

**Tech Stack:** Unity 2022 LTS (团结引擎 1.9.3), C#, 2D Physics, ScriptableObject

---

## 文件结构总览

```
Assets/
├── Scripts/
│   ├── Core/
│   │   ├── GridMovement.cs          # 网格移动控制
│   │   ├── PushableBox.cs           # 可推动箱子逻辑
│   │   ├── TargetPoint.cs           # 目标点检测
│   │   └── GameManager.cs           # 游戏状态管理
│   ├── Dialogue/
│   │   ├── DialogueSystem.cs        # 对话系统核心
│   │   ├── DialogueData.cs          # 对话数据结构
│   │   ├── DialogueCharacter.cs     # 角色亮度控制
│   │   └── TypewriterEffect.cs      # 打字机效果
│   ├── Enemy/
│   │   └── Fireball.cs              # 火球AI
│   ├── UI/
│   │   ├── MainMenu.cs              # 主菜单（NPC展示）
│   │   ├── MapScreen.cs             # 地图界面
│   │   ├── PauseMenu.cs             # 暂停菜单
│   │   ├── LevelCompleteUI.cs       # 关卡完成UI
│   │   ├── GameOverUI.cs            # 失败UI
│   │   └── NameInputUI.cs           # 起名弹窗
│   └── Scene/
│       └── SceneLoader.cs           # 场景加载管理
├── Data/
│   ├── Dialogue/                    # 对话JSON文件
│   │   ├── prologue.json
│   │   └── chapter1.json
│   ├── Levels/                      # 关卡数据ScriptableObject
│   │   ├── LevelData.cs
│   │   ├── PrologueLevel.asset
│   │   └── Chapter1Level.asset
│   └── UI/                          # UI数据
│       └── MapNodeData.cs           # 地图节点配置
├── Prefabs/
│   ├── Player.prefab
│   ├── PushableBox.prefab
│   ├── TargetPoint.prefab
│   ├── Fireball.prefab
│   └── Walls/
├── Scenes/
│   ├── OpeningScene.unity         # 开场CG场景
│   ├── PrologueDialogue.unity     # 序章对话场景
│   ├── Prologue.unity             # 序章关卡
│   ├── MainMenu.unity             # 主菜单
│   └── Chapter1.unity             # 第一章关卡
├── Videos/
│   └── opening.mp4                # 开场CG视频
├── Sprites/
│   ├── Characters/                  # 从美术资源/人/复制
│   ├── Scenes/                      # 从美术资源/场景/复制
│   ├── UI/
│   └── Effects/
└── Resources/
    └── Dialogue/                    # 运行时加载对话数据
```

---

## Task 1: Unity项目搭建与基础配置

**Files:**
- Create: Unity项目（在用户本地用团结引擎创建）
- Create: `Assets/Scripts/Core/GameManager.cs`
- Create: `Assets/Data/Levels/LevelData.cs`

**Interfaces:**
- Produces: GameManager单例，提供场景切换、游戏状态管理

- [ ] **Step 1: 创建Unity项目**

用户在团结引擎中创建新的2D项目，项目名"MiniGame"。

- [ ] **Step 2: 创建文件夹结构**

在Unity Project窗口创建上述目录结构。

- [ ] **Step 3: 编写GameManager.cs**

```csharp
// Assets/Scripts/Core/GameManager.cs
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public string PlayerName { get; set; } = "垃圾袋";
    public int CurrentLevel { get; set; } = 0;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void LoadLevel(int levelIndex)
    {
        CurrentLevel = levelIndex;
        string[] levels = { "Prologue", "Chapter1", "Chapter2" };
        if (levelIndex < levels.Length)
            SceneManager.LoadScene(levels[levelIndex]);
    }

    public void CompleteLevel()
    {
        CurrentLevel++;
        if (CurrentLevel < 3)
            LoadLevel(CurrentLevel);
        else
            Debug.Log("Game Complete!");
    }
}
```

- [ ] **Step 4: 编写LevelData.cs**

```csharp
// Assets/Data/Levels/LevelData.cs
using UnityEngine;

[CreateAssetMenu(fileName = "LevelData", menuName = "MiniGame/Level Data")]
public class LevelData : ScriptableObject
{
    public string levelName;
    public int width = 8;
    public int height = 8;
    public Vector2Int playerStart;
    public Vector2Int[] boxPositions;
    public Vector2Int[] targetPositions;
    public Vector2Int[] wallPositions;
    public string dialogueBefore;
    public string dialogueAfter;
}
```

- [ ] **Step 5: 测试 GameManager**

在Unity中创建空物体挂载GameManager，确认单例正常工作。

- [ ] **Step 6: Commit**

```bash
git add Assets/Scripts/Core/ Assets/Data/Levels/
git commit -m "feat: 搭建项目基础框架，GameManager和LevelData"
```

---

## Task 2: 网格移动系统

**Files:**
- Create: `Assets/Scripts/Core/GridMovement.cs`
- Create: `Assets/Scripts/Core/GridManager.cs`

**Interfaces:**
- Consumes: GameManager
- Produces: 玩家网格化移动，方向输入处理

- [ ] **Step 1: 编写GridManager.cs**

```csharp
// Assets/Scripts/Core/GridManager.cs
using UnityEngine;
using System.Collections.Generic;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    public float cellSize = 1f;
    private HashSet<Vector2Int> walls = new HashSet<Vector2Int>();
    private Dictionary<Vector2Int, PushableBox> boxes = new Dictionary<Vector2Int, PushableBox>();

    void Awake() => Instance = this;

    public void RegisterWall(Vector2Int pos) => walls.Add(pos);
    public void RegisterBox(Vector2Int pos, PushableBox box) => boxes[pos] = box;
    public void UnregisterBox(Vector2Int pos) => boxes.Remove(pos);

    public bool IsWall(Vector2Int pos) => walls.Contains(pos);
    public bool IsBox(Vector2Int pos) => boxes.ContainsKey(pos);
    public PushableBox GetBox(Vector2Int pos) => boxes.TryGetValue(pos, out var box) ? box : null;

    public bool CanMoveTo(Vector2Int pos)
    {
        if (IsWall(pos)) return false;
        if (IsBox(pos))
        {
            Vector2Int nextPos = pos + (pos - pos); // 需要传入方向
            return false; // 由PushableBox处理
        }
        return true;
    }

    public Vector3 GridToWorld(Vector2Int gridPos)
    {
        return new Vector3(gridPos.x * cellSize, gridPos.y * cellSize, 0);
    }

    public Vector2Int WorldToGrid(Vector3 worldPos)
    {
        return new Vector2Int(
            Mathf.RoundToInt(worldPos.x / cellSize),
            Mathf.RoundToInt(worldPos.y / cellSize)
        );
    }

    public void Clear()
    {
        walls.Clear();
        boxes.Clear();
    }
}
```

- [ ] **Step 2: 编写GridMovement.cs**

```csharp
// Assets/Scripts/Core/GridMovement.cs
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class GridMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    private Vector2Int gridPos;
    private bool isMoving = false;

    void Start()
    {
        gridPos = GridManager.Instance.WorldToGrid(transform.position);
        transform.position = GridManager.Instance.GridToWorld(gridPos);
    }

    void Update()
    {
        if (isMoving) return;

        Vector2Int direction = Vector2Int.zero;

        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            direction = Vector2Int.up;
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            direction = Vector2Int.down;
        else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            direction = Vector2Int.left;
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            direction = Vector2Int.right;

        if (direction != Vector2Int.zero)
            TryMove(direction);
    }

    void TryMove(Vector2Int direction)
    {
        Vector2Int newPos = gridPos + direction;

        // 检查墙壁
        if (GridManager.Instance.IsWall(newPos))
            return;

        // 检查箱子
        if (GridManager.Instance.IsBox(newPos))
        {
            PushableBox box = GridManager.Instance.GetBox(newPos);
            if (box != null && box.TryPush(direction))
            {
                MoveTo(newPos);
            }
            return;
        }

        MoveTo(newPos);
    }

    void MoveTo(Vector2Int newPos)
    {
        isMoving = true;
        gridPos = newPos;
        StartCoroutine(SmoothMove(GridManager.Instance.GridToWorld(newPos)));
    }

    System.Collections.IEnumerator SmoothMove(Vector3 target)
    {
        while (Vector3.Distance(transform.position, target) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
            yield return null;
        }
        transform.position = target;
        isMoving = false;
    }
}
```

- [ ] **Step 3: 在Unity中测试网格移动**

创建Player物体，挂载GridMovement和SpriteRenderer，使用主角图片。确认WASD/方向键可以网格化移动。

- [ ] **Step 4: Commit**

```bash
git add Assets/Scripts/Core/GridManager.cs Assets/Scripts/Core/GridMovement.cs
git commit -m "feat: 实现网格移动系统"
```

---

## Task 3: 推箱子系统

**Files:**
- Create: `Assets/Scripts/Core/PushableBox.cs`
- Create: `Assets/Scripts/Core/TargetPoint.cs`

**Interfaces:**
- Consumes: GridManager
- Produces: 箱子推动逻辑，目标点检测

- [ ] **Step 1: 编写PushableBox.cs**

```csharp
// Assets/Scripts/Core/PushableBox.cs
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class PushableBox : MonoBehaviour
{
    private Vector2Int gridPos;
    public bool IsOnTarget { get; private set; } = false;

    void Start()
    {
        gridPos = GridManager.Instance.WorldToGrid(transform.position);
        transform.position = GridManager.Instance.GridToWorld(gridPos);
        GridManager.Instance.RegisterBox(gridPos, this);
    }

    public bool TryPush(Vector2Int direction)
    {
        Vector2Int newPos = gridPos + direction;

        // 箱子不能推入墙壁或其他箱子
        if (GridManager.Instance.IsWall(newPos) || GridManager.Instance.IsBox(newPos))
            return false;

        // 移动箱子
        GridManager.Instance.UnregisterBox(gridPos);
        gridPos = newPos;
        GridManager.Instance.RegisterBox(gridPos, this);
        StartCoroutine(SmoothMove(GridManager.Instance.GridToWorld(newPos)));
        return true;
    }

    System.Collections.IEnumerator SmoothMove(Vector3 target)
    {
        while (Vector3.Distance(transform.position, target) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, target, 8f * Time.deltaTime);
            yield return null;
        }
        transform.position = target;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Target"))
        {
            IsOnTarget = true;
            CheckLevelComplete();
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Target"))
            IsOnTarget = false;
    }

    void CheckLevelComplete()
    {
        // 检查所有箱子是否都在目标点上
        var boxes = FindObjectsOfType<PushableBox>();
        bool allOnTarget = true;
        foreach (var box in boxes)
        {
            if (!box.IsOnTarget)
            {
                allOnTarget = false;
                break;
            }
        }

        if (allOnTarget)
        {
            Debug.Log("Level Complete!");
            // 触发关卡完成逻辑
            FindObjectOfType<LevelCompleteUI>()?.Show();
        }
    }
}
```

- [ ] **Step 2: 编写TargetPoint.cs**

```csharp
// Assets/Scripts/Core/TargetPoint.cs
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class TargetPoint : MonoBehaviour
{
    void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
        gameObject.tag = "Target";
    }
}
```

- [ ] **Step 3: 在Unity中创建Prefabs**

1. 创建PushableBox prefab：蓝色方块图片 + BoxCollider2D + Rigidbody2D(Kinematic) + PushableBox脚本
2. 创建TargetPoint prefab：半透明绿色方块 + BoxCollider2D(IsTrigger) + TargetPoint脚本
3. 设置Tag "Target"

- [ ] **Step 4: 测试推箱子**

在场景中放置Player、几个Box、Target点，测试推动和完成检测。

- [ ] **Step 5: Commit**

```bash
git add Assets/Scripts/Core/PushableBox.cs Assets/Scripts/Core/TargetPoint.cs
git commit -m "feat: 实现推箱子系统和目标点检测"
```

---

## Task 4: 对话系统（多角色亮度高亮 + 表情切换 + 自动打字 + 快进）

**设计要点：**
- 场景中同时显示所有参与对话的角色立绘（全尺寸Sprite）
- 说话的角色全亮（alpha=1.0），其他角色变暗（alpha=0.4）
- **表情切换**：每个角色有多个表情立绘（默认、生气、微笑、疑惑等），根据JSON中emotion字段自动切换
- 对话框在屏幕底部，半透明橄榄绿色背景（参考图样式）
- 打字机自动逐字显示，点击/空格可跳过打字
- 跳过打字后，再次点击/空格推进到下一句
- 快进按钮：跳过整个对话序列

**Files:**
- Create: `Assets/Scripts/Dialogue/DialogueSystem.cs`
- Create: `Assets/Scripts/Dialogue/DialogueData.cs`
- Create: `Assets/Scripts/Dialogue/TypewriterEffect.cs`
- Create: `Assets/Scripts/Dialogue/DialogueCharacter.cs`
- Create: `Assets/Data/Dialogue/prologue.json`
- Create: `Assets/Data/Dialogue/chapter1.json`

**Interfaces:**
- Consumes: GameManager
- Produces: 完整对话UI系统，多角色亮度高亮，打字机效果，快进

- [ ] **Step 1: 编写DialogueData.cs（支持表情立绘映射）**

```csharp
// Assets/Scripts/Dialogue/DialogueData.cs
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DialogueLine
{
    public string speaker;        // 说话者名字
    public string text;           // 对话文本
    public string emotion;        // 表情名（如 "angry", "smile", "confused"）
    public bool isPlayerChoice;
    public string[] choices;
}

[Serializable]
public class EmotionSprite
{
    public string emotion;        // 表情名
    public Sprite sprite;         // 对应的立绘图片
}

[Serializable]
public class DialogueCharacterInfo
{
    public string characterName;  // 角色名
    public Vector3 position;      // 场景中的位置
    public Sprite defaultPortrait; // 默认立绘
    public EmotionSprite[] emotions; // 表情立绘列表
}

[Serializable]
public class DialogueSequence
{
    public string id;
    public DialogueLine[] lines;
    public DialogueCharacterInfo[] characters; // 参与对话的角色列表
}

[Serializable]
public class DialogueContainer
{
    public DialogueSequence[] sequences;
}
```

- [ ] **Step 2: 编写DialogueCharacter.cs（角色亮度控制 + 表情切换）**

```csharp
// Assets/Scripts/Dialogue/DialogueCharacter.cs
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 管理对话场景中单个角色的显示、亮度和表情切换
/// 美术资源：每个角色有多个表情立绘（默认、开心、生气、疑惑等）
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class DialogueCharacter : MonoBehaviour
{
    public string characterName;
    private SpriteRenderer spriteRenderer;
    private float activeAlpha = 1.0f;    // 说话时的亮度
    private float inactiveAlpha = 0.4f;  // 不说话时的亮度

    // 表情立绘字典：emotion名 → Sprite
    private Dictionary<string, Sprite> emotionSprites = new Dictionary<string, Sprite>();
    private Sprite defaultSprite;  // 默认表情

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        defaultSprite = spriteRenderer.sprite;
    }

    /// <summary>
    /// 注册表情立绘
    /// </summary>
    public void RegisterEmotion(string emotion, Sprite sprite)
    {
        if (!string.IsNullOrEmpty(emotion) && sprite != null)
            emotionSprites[emotion] = sprite;
    }

    /// <summary>
    /// 批量注册表情立绘
    /// </summary>
    public void RegisterEmotions(Dictionary<string, Sprite> emotions)
    {
        foreach (var kvp in emotions)
            RegisterEmotion(kvp.Key, kvp.Value);
    }

    /// <summary>
    /// 设置角色为活跃状态（全亮）
    /// </summary>
    public void SetActive()
    {
        SetAlpha(activeAlpha);
    }

    /// <summary>
    /// 设置角色为非活跃状态（变暗）
    /// </summary>
    public void SetInactive()
    {
        SetAlpha(inactiveAlpha);
    }

    /// <summary>
    /// 设置角色表情（根据emotion名切换Sprite）
    /// 如果没有对应表情，使用默认表情
    /// </summary>
    public void SetEmotion(string emotion)
    {
        if (string.IsNullOrEmpty(emotion))
        {
            spriteRenderer.sprite = defaultSprite;
            return;
        }

        if (emotionSprites.ContainsKey(emotion))
            spriteRenderer.sprite = emotionSprites[emotion];
        else
            spriteRenderer.sprite = defaultSprite;
    }

    /// <summary>
    /// 直接设置Sprite（用于初始化）
    /// </summary>
    public void SetSprite(Sprite sprite)
    {
        if (sprite != null)
        {
            spriteRenderer.sprite = sprite;
            defaultSprite = sprite;
        }
    }

    void SetAlpha(float alpha)
    {
        Color color = spriteRenderer.color;
        color.a = alpha;
        spriteRenderer.color = color;
    }
}
```

- [ ] **Step 3: 编写TypewriterEffect.cs**

```csharp
// Assets/Scripts/Dialogue/TypewriterEffect.cs
using UnityEngine;
using TMPro;
using System.Collections;

public class TypewriterEffect : MonoBehaviour
{
    public TextMeshProUGUI textComponent;
    public float typingSpeed = 0.03f;
    private Coroutine typingCoroutine;
    private bool isTyping = false;

    /// <summary>
    /// 开始打字效果
    /// </summary>
    public void TypeText(string text, System.Action onComplete = null)
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(Type(text, onComplete));
    }

    IEnumerator Type(string text, System.Action onComplete)
    {
        isTyping = true;
        textComponent.text = "";
        foreach (char c in text)
        {
            textComponent.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }
        isTyping = false;
        onComplete?.Invoke();
    }

    /// <summary>
    /// 跳过打字，直接显示完整文本
    /// </summary>
    public void Skip()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }
        isTyping = false;
    }

    public bool IsTyping => isTyping;
}
```

- [ ] **Step 4: 编写DialogueSystem.cs（核心对话系统）**

```csharp
// Assets/Scripts/Dialogue/DialogueSystem.cs
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class DialogueSystem : MonoBehaviour
{
    [Header("UI References")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;
    public Button skipButton;           // 快进按钮（跳过整个对话）
    public Image dialogueBoxBg;         // 对话框背景（橄榄绿半透明）

    [Header("Typewriter")]
    public TypewriterEffect typewriter;

    [Header("Characters")]
    public Transform characterContainer; // 角色容器

    private List<DialogueCharacter> characters = new List<DialogueCharacter>();
    private DialogueSequence currentSequence;
    private int currentLineIndex;
    private bool isDialogueActive = false;
    private System.Action onDialogueComplete;

    void Awake()
    {
        dialoguePanel.SetActive(false);
        skipButton.onClick.AddListener(OnSkipAll);
    }

    void Update()
    {
        if (!isDialogueActive) return;

        // 点击或空格推进对话
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
        {
            OnAdvance();
        }
    }

    /// <summary>
    /// 开始对话序列
    /// </summary>
    public void StartDialogue(string sequenceId, System.Action onComplete = null)
    {
        var container = Resources.Load<DialogueContainer>("Dialogue/dialogues");
        if (container == null)
        {
            Debug.LogError("Dialogue data not found!");
            return;
        }

        foreach (var seq in container.sequences)
        {
            if (seq.id == sequenceId)
            {
                currentSequence = seq;
                break;
            }
        }

        if (currentSequence == null)
        {
            Debug.LogError($"Dialogue sequence '{sequenceId}' not found!");
            return;
        }

        onDialogueComplete = onComplete;
        currentLineIndex = 0;
        isDialogueActive = true;
        dialoguePanel.SetActive(true);

        // 创建角色立绘
        SpawnCharacters(currentSequence.characters);

        // 显示第一句
        ShowLine(currentSequence.lines[currentLineIndex]);
    }

    /// <summary>
    /// 在场景中生成角色立绘
    /// </summary>
    void SpawnCharacters(DialogueCharacterInfo[] characterInfos)
    {
        // 清除旧角色
        foreach (var oldChar in characters)
        {
            if (oldChar != null)
                Destroy(oldChar.gameObject);
        }
        characters.Clear();

        if (characterInfos == null) return;

        foreach (var info in characterInfos)
        {
            GameObject charObj = new GameObject($"Character_{info.characterName}");
            charObj.transform.SetParent(characterContainer);
            charObj.transform.localPosition = info.position;

            SpriteRenderer sr = charObj.AddComponent<SpriteRenderer>();
            sr.sprite = info.defaultPortrait;
            sr.sortingOrder = 10; // 确保在背景之上

            DialogueCharacter dc = charObj.AddComponent<DialogueCharacter>();
            dc.characterName = info.characterName;

            // 注册表情立绘
            if (info.emotions != null)
            {
                foreach (var emotionSprite in info.emotions)
                {
                    dc.RegisterEmotion(emotionSprite.emotion, emotionSprite.sprite);
                }
            }

            characters.Add(dc);
        }
    }

    /// <summary>
    /// 显示当前对话行
    /// </summary>
    void ShowLine(DialogueLine line)
    {
        // 更新角色亮度和表情
        UpdateCharacterHighlight(line.speaker);

        // 设置说话者的表情
        DialogueCharacter speakerChar = GetCharacter(line.speaker);
        if (speakerChar != null)
        {
            speakerChar.SetActive();
            speakerChar.SetEmotion(line.emotion);
        }

        // 打字机效果显示文本
        typewriter.TypeText(line.text);
    }

    DialogueCharacter GetCharacter(string name)
    {
        foreach (var character in characters)
        {
            if (character.characterName == name)
                return character;
        }
        return null;
    }

    /// <summary>
    /// 更新角色亮度高亮
    /// 说话的角色全亮，其他角色变暗
    /// </summary>
    void UpdateCharacterHighlight(string activeSpeaker)
    {
        foreach (var character in characters)
        {
            if (character.characterName == activeSpeaker)
                character.SetActive();
            else
                character.SetInactive();
        }
    }

    /// <summary>
    /// 推进对话（点击/空格）
    /// 打字中 → 跳过打字
    /// 打字完成 → 下一句
    /// </summary>
    void OnAdvance()
    {
        if (typewriter.IsTyping)
        {
            // 跳过打字，直接显示完整文本
            typewriter.Skip();
            return;
        }

        // 下一句
        currentLineIndex++;
        if (currentLineIndex < currentSequence.lines.Length)
        {
            ShowLine(currentSequence.lines[currentLineIndex]);
        }
        else
        {
            EndDialogue();
        }
    }

    /// <summary>
    /// 快进按钮：跳过整个对话
    /// </summary>
    void OnSkipAll()
    {
        EndDialogue();
    }

    void EndDialogue()
    {
        isDialogueActive = false;
        dialoguePanel.SetActive(false);

        // 清除角色
        foreach (var character in characters)
        {
            if (character != null)
                Destroy(character.gameObject);
        }
        characters.Clear();

        onDialogueComplete?.Invoke();
    }
}
```

- [ ] **Step 5: 创建对话JSON数据（含表情切换）**

**表情系统说明：**
- 每个角色在DialogueCharacterInfo中注册多个表情立绘（default、angry、smile等）
- 对话JSON中每行的`emotion`字段指定当前表情名
- 对话时自动切换到对应表情立绘

创建 `Assets/Resources/Dialogue/prologue.json`:
```json
{
  "sequences": [
    {
      "id": "prologue_start",
      "characters": [
        {
          "characterName": "主控",
          "position": { "x": -2, "y": 0, "z": 0 },
          "emotions": [
            { "emotion": "default", "sprite": "主控_默认" },
            { "emotion": "confused", "sprite": "主控_疑惑" },
            { "emotion": "embarrassed", "sprite": "主控_尴尬" }
          ]
        },
        {
          "characterName": "比宿",
          "position": { "x": 2, "y": 0, "z": 0 },
          "emotions": [
            { "emotion": "default", "sprite": "比宿_默认" },
            { "emotion": "angry", "sprite": "比宿_生气" },
            { "emotion": "thinking", "sprite": "比宿_思考" },
            { "emotion": "smile", "sprite": "比宿_核善微笑" }
          ]
        }
      ],
      "lines": [
        { "speaker": "主控", "text": "等等...我这是在哪？", "emotion": "confused" },
        { "speaker": "比宿", "text": "哇！我的平衡器！！你个混蛋！", "emotion": "angry" },
        { "speaker": "主控", "text": "对不起对不起！我不是故意的...", "emotion": "embarrassed" },
        { "speaker": "比宿", "text": "算了...你得帮我一个忙。", "emotion": "thinking" },
        { "speaker": "比宿", "text": "把这个核心放到星球们的工作台上。", "emotion": "default" }
      ]
    },
    {
      "id": "prologue_complete",
      "characters": [
        {
          "characterName": "主控",
          "position": { "x": -2, "y": 0, "z": 0 },
          "emotions": [
            { "emotion": "default", "sprite": "主控_默认" }
          ]
        },
        {
          "characterName": "比宿",
          "position": { "x": 2, "y": 0, "z": 0 },
          "emotions": [
            { "emotion": "default", "sprite": "比宿_默认" },
            { "emotion": "smile", "sprite": "比宿_核善微笑" }
          ]
        }
      ],
      "lines": [
        { "speaker": "主控", "text": "这不很简单么！", "emotion": "default" },
        { "speaker": "比宿", "text": "相信你已经熟练掌握了。^_^", "emotion": "smile" }
      ]
    }
  ]
}
```

- [ ] **Step 6: 在Unity中创建对话UI**

1. 创建Canvas → DialoguePanel（全屏，底部1/5区域）
2. 创建对话框背景：橄榄绿色半透明Image（参考图样式）
3. 添加DialogueText(TextMeshPro)，居中显示
4. 创建SkipButton（快进按钮），放在对话框右下角
5. 创建CharacterContainer空物体（用于存放角色立绘）
6. 挂载DialogueSystem、TypewriterEffect脚本

- [ ] **Step 7: 测试对话系统**

1. 调用 `DialogueSystem.StartDialogue("prologue_start")` 测试
2. 验证：角色立绘显示正确
3. 验证：说话角色全亮，其他变暗
4. 验证：打字机效果正常
5. 验证：点击/空格可以跳过打字和推进
6. 验证：快进按钮可以跳过整个对话

- [ ] **Step 8: Commit**

```bash
git add Assets/Scripts/Dialogue/ Assets/Resources/Dialogue/
git commit -m "feat: 实现对话系统（多角色亮度高亮、自动打字、快进）"
```

---

## Task 5: 主菜单、地图系统与关卡UI

**设计要点：**
- **主菜单背景**：展示已解锁的NPC角色站在一起
- **主菜单按钮**：开始游戏、地图、退出
- **地图界面**：水平排列关卡节点，每个节点有角色立绘+名字，可点击重玩
- **暂停菜单**：ESC暂停，继续/重来/返回主菜单
- **关卡完成/失败UI**

**Files:**
- Create: `Assets/Scripts/UI/MainMenu.cs`
- Create: `Assets/Scripts/UI/MapScreen.cs`
- Create: `Assets/Scripts/UI/PauseMenu.cs`
- Create: `Assets/Scripts/UI/LevelCompleteUI.cs`
- Create: `Assets/Scripts/UI/GameOverUI.cs`
- Create: `Assets/Scripts/UI/NameInputUI.cs`
- Create: `Assets/Data/UI/MapNodeData.cs`

**Interfaces:**
- Consumes: GameManager
- Produces: 完整UI框架，地图系统

- [ ] **Step 1: 编写GameManager扩展（关卡解锁数据）**

在 `GameManager.cs` 中添加关卡解锁状态管理：
```csharp
// 在GameManager.cs中添加以下内容
private HashSet<int> unlockedLevels = new HashSet<int> { 0 }; // 默认解锁序章

public bool IsLevelUnlocked(int levelIndex) => unlockedLevels.Contains(levelIndex);

public void UnlockLevel(int levelIndex)
{
    unlockedLevels.Add(levelIndex);
    SaveProgress();
}

public void SaveProgress()
{
    string data = string.Join(",", unlockedLevels);
    PlayerPrefs.SetString("UnlockedLevels", data);
    PlayerPrefs.Save();
}

public void LoadProgress()
{
    string data = PlayerPrefs.GetString("UnlockedLevels", "0");
    unlockedLevels.Clear();
    foreach (var s in data.Split(','))
    {
        if (int.TryParse(s, out int level))
            unlockedLevels.Add(level);
    }
}
```

- [ ] **Step 2: 编写MainMenu.cs**

```csharp
// Assets/Scripts/UI/MainMenu.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class MainMenu : MonoBehaviour
{
    [Header("Buttons")]
    public Button startButton;
    public Button mapButton;
    public Button quitButton;

    [Header("NPC Display")]
    public Transform npcContainer;     // NPC展示容器
    public GameObject npcPrefab;       // NPC立绘Prefab

    [Header("Panels")]
    public GameObject mainPanel;
    public GameObject mapPanel;
    public NameInputUI nameInputUI;

    [Header("NPC Data")]
    public NPCData[] availableNPCs;    // 可展示的NPC列表

    void Start()
    {
        mainPanel.SetActive(true);
        mapPanel.SetActive(false);

        startButton.onClick.AddListener(OnStart);
        mapButton.onClick.AddListener(OnMap);
        quitButton.onClick.AddListener(OnQuit);

        GameManager.Instance.LoadProgress();
        DisplayUnlockedNPCs();
    }

    void DisplayUnlockedNPCs()
    {
        // 清除旧NPC
        foreach (Transform child in npcContainer)
            Destroy(child.gameObject);

        // 显示已解锁的NPC
        foreach (var npc in availableNPCs)
        {
            if (GameManager.Instance.IsLevelUnlocked(npc.unlockLevel))
            {
                GameObject npcObj = Instantiate(npcPrefab, npcContainer);
                npcObj.GetComponentInChildren<SpriteRenderer>().sprite = npc.portrait;
                npcObj.GetComponentInChildren<TextMeshProUGUI>().text = npc.characterName;
            }
        }
    }

    void OnStart()
    {
        nameInputUI.Show(name =>
        {
            GameManager.Instance.PlayerName = name;
            GameManager.Instance.LoadLevel(0);
        });
    }

    void OnMap()
    {
        mainPanel.SetActive(false);
        mapPanel.SetActive(true);
        mapPanel.GetComponent<MapScreen>().RefreshMap();
    }

    void OnQuit()
    {
        Application.Quit();
    }
}

[System.Serializable]
public class NPCData
{
    public string characterName;
    public Sprite portrait;
    public int unlockLevel;  // 解锁该NPC的关卡ID
}
```

- [ ] **Step 3: 编写MapScreen.cs**

```csharp
// Assets/Scripts/UI/MapScreen.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class MapScreen : MonoBehaviour
{
    [Header("UI")]
    public Button backButton;
    public Transform nodeContainer;     // 关卡节点容器

    [Header("Prefabs")]
    public GameObject mapNodePrefab;    // 关卡节点Prefab

    [Header("Map Data")]
    public MapNodeData[] mapNodes;      // 关卡节点配置

    void Awake()
    {
        backButton.onClick.AddListener(OnBack);
    }

    public void RefreshMap()
    {
        // 清除旧节点
        foreach (Transform child in nodeContainer)
            Destroy(child.gameObject);

        // 水平排列关卡节点
        float xOffset = 0f;
        float spacing = 300f;

        foreach (var node in mapNodes)
        {
            GameObject nodeObj = Instantiate(mapNodePrefab, nodeContainer);
            RectTransform rect = nodeObj.GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(xOffset, 0);

            // 设置节点内容
            MapNodeUI nodeUI = nodeObj.GetComponent<MapNodeUI>();
            nodeUI.Setup(node);

            // 点击事件
            int levelIndex = node.levelIndex;
            nodeObj.GetComponent<Button>().onClick.AddListener(() => OnNodeClick(levelIndex));

            xOffset += spacing;
        }
    }

    void OnNodeClick(int levelIndex)
    {
        if (GameManager.Instance.IsLevelUnlocked(levelIndex))
        {
            GameManager.Instance.LoadLevel(levelIndex);
        }
    }

    void OnBack()
    {
        GetComponentInParent<MainMenu>().mainPanel.SetActive(true);
        gameObject.SetActive(false);
    }
}

[System.Serializable]
public class MapNodeUI : MonoBehaviour
{
    public Image characterPortrait;
    public TextMeshProUGUI characterName;
    public GameObject lockIcon;

    public void Setup(MapNodeData data)
    {
        characterPortrait.sprite = data.characterPortrait;
        characterName.text = data.characterName;

        bool unlocked = GameManager.Instance.IsLevelUnlocked(data.levelIndex);
        lockIcon.SetActive(!unlocked);
        characterPortrait.color = unlocked ? Color.white : Color.gray;
    }
}
```

- [ ] **Step 4: 编写MapNodeData.cs**

```csharp
// Assets/Data/UI/MapNodeData.cs
using UnityEngine;

[CreateAssetMenu(fileName = "MapNodeData", menuName = "MiniGame/Map Node Data")]
public class MapNodeData : ScriptableObject
{
    public string characterName;
    public Sprite characterPortrait;
    public int levelIndex;
    public string sceneName;
}
```

- [ ] **Step 5: 编写NameInputUI.cs**

```csharp
// Assets/Scripts/UI/NameInputUI.cs
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class NameInputUI : MonoBehaviour
{
    public GameObject panel;
    public TMP_InputField nameInput;
    public Button confirmButton;
    private System.Action<string> onConfirm;

    void Awake() => panel.SetActive(false);

    public void Show(System.Action<string> callback)
    {
        onConfirm = callback;
        nameInput.text = "";
        panel.SetActive(true);
        confirmButton.onClick.RemoveAllListeners();
        confirmButton.onClick.AddListener(OnConfirm);
    }

    void OnConfirm()
    {
        string name = nameInput.text.Trim();
        if (string.IsNullOrEmpty(name))
            name = "垃圾袋";
        panel.SetActive(false);
        onConfirm?.Invoke(name);
    }
}
```

- [ ] **Step 6: 编写PauseMenu.cs**

```csharp
// Assets/Scripts/UI/PauseMenu.cs
using UnityEngine;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    public GameObject pausePanel;
    public Button resumeButton;
    public Button restartButton;
    public Button mainMenuButton;
    private bool isPaused = false;

    void Start()
    {
        pausePanel.SetActive(false);
        resumeButton.onClick.AddListener(Resume);
        restartButton.onClick.AddListener(Restart);
        mainMenuButton.onClick.AddListener(BackToMenu);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused) Resume();
            else Pause();
        }
    }

    void Pause()
    {
        isPaused = true;
        pausePanel.SetActive(true);
        Time.timeScale = 0f;
    }

    void Resume()
    {
        isPaused = false;
        pausePanel.SetActive(false);
        Time.timeScale = 1f;
    }

    void Restart()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    void BackToMenu()
    {
        Time.timeScale = 1f;
        GameManager.Instance.LoadScene("MainMenu");
    }
}
```

- [ ] **Step 7: 编写LevelCompleteUI.cs**

```csharp
// Assets/Scripts/UI/LevelCompleteUI.cs
using UnityEngine;
using UnityEngine.UI;

public class LevelCompleteUI : MonoBehaviour
{
    public GameObject completePanel;
    public Button nextButton;
    public Button menuButton;

    void Awake()
    {
        completePanel.SetActive(false);
        nextButton.onClick.AddListener(OnNext);
        menuButton.onClick.AddListener(OnMenu);
    }

    public void Show()
    {
        completePanel.SetActive(true);
        // 解锁下一关
        int currentLevel = GameManager.Instance.CurrentLevel;
        GameManager.Instance.UnlockLevel(currentLevel + 1);
    }

    void OnNext()
    {
        completePanel.SetActive(false);
        GameManager.Instance.CompleteLevel();
    }

    void OnMenu()
    {
        completePanel.SetActive(false);
        GameManager.Instance.LoadScene("MainMenu");
    }
}
```

- [ ] **Step 8: 编写GameOverUI.cs**

```csharp
// Assets/Scripts/UI/GameOverUI.cs
using UnityEngine;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    public GameObject gameOverPanel;
    public Button retryButton;
    public Button mainMenuButton;

    void Awake()
    {
        gameOverPanel.SetActive(false);
        retryButton.onClick.AddListener(OnRetry);
        mainMenuButton.onClick.AddListener(OnMenu);
    }

    public void Show()
    {
        gameOverPanel.SetActive(true);
    }

    void OnRetry()
    {
        gameOverPanel.SetActive(false);
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    void OnMenu()
    {
        gameOverPanel.SetActive(false);
        GameManager.Instance.LoadScene("MainMenu");
    }
}
```

- [ ] **Step 9: 在Unity中创建UI**

1. **MainMenu场景**：
   - NPC展示区域（角色立绘+名字，水平排列）
   - 标题文字
   - 三个按钮：开始、地图、退出

2. **MapPanel**：
   - 水平滚动区域
   - 关卡节点Prefab（角色立绘+名字+锁定图标）
   - 返回按钮

3. **PauseMenu**：半透明遮罩 + Resume/Restart/Menu按钮

4. **LevelCompleteUI**：恭喜面板 + 下一关/返回菜单按钮

5. **GameOverUI**：失败面板 + 重试/返回菜单按钮

- [ ] **Step 10: Commit**

```bash
git add Assets/Scripts/UI/ Assets/Data/UI/
git commit -m "feat: 实现主菜单、地图系统、关卡UI"
```

---

## Task 6: 火球系统（第一章）

**Files:**
- Create: `Assets/Scripts/Enemy/Fireball.cs`
- Create: `Assets/Scripts/Enemy/FireballSpawner.cs`

**Interfaces:**
- Consumes: GridManager, GameManager
- Produces: 火球AI，碰撞检测，玩家受伤/失败

- [ ] **Step 1: 编写Fireball.cs**

```csharp
// Assets/Scripts/Enemy/Fireball.cs
using UnityEngine;

public class Fireball : MonoBehaviour
{
    public float speed = 3f;
    public Vector2 direction;
    private Vector2 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime);

        // 边界检测（可选：反弹或销毁）
        if (Mathf.Abs(transform.position.x) > 10 || Mathf.Abs(transform.position.y) > 10)
        {
            ReturnToStart();
        }
    }

    void ReturnToStart()
    {
        transform.position = startPos;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // 玩家被击中
            Debug.Log("Player Hit!");
            FindObjectOfType<GameOverUI>()?.Show();
            Destroy(gameObject);
        }
    }
}
```

- [ ] **Step 2: 编写FireballSpawner.cs**

```csharp
// Assets/Scripts/Enemy/FireballSpawner.cs
using UnityEngine;
using System.Collections;

public class FireballSpawner : MonoBehaviour
{
    public GameObject fireballPrefab;
    public int fireballCount = 5;
    public float spawnRadius = 5f;
    public float spawnInterval = 2f;

    void Start()
    {
        StartCoroutine(SpawnFireballs());
    }

    IEnumerator SpawnFireballs()
    {
        while (true)
        {
            for (int i = 0; i < fireballCount; i++)
            {
                Vector2 randomPos = (Random.insideUnitCircle.normalized) * spawnRadius;
                GameObject fireball = Instantiate(fireballPrefab, randomPos, Quaternion.identity);
                Fireball fb = fireball.GetComponent<Fireball>();
                if (fb != null)
                {
                    fb.direction = -randomPos.normalized; // 向中心飞
                }
            }
            yield return new WaitForSeconds(spawnInterval);
        }
    }
}
```

- [ ] **Step 3: 在Unity中创建Fireball Prefab**

1. 创建Fireball prefab：火球图片 + CircleCollider2D(IsTrigger) + Rigidbody2D(Kinematic) + Fireball脚本
2. 创建FireballSpawner空物体 + FireballSpawner脚本

- [ ] **Step 4: 测试火球系统**

在第一章场景放置Spawner，测试火球生成和碰撞检测。

- [ ] **Step 5: Commit**

```bash
git add Assets/Scripts/Enemy/
git commit -m "feat: 实现火球系统（第一章躲避机制）"
```

---

## Task 7: 关卡搭建与集成

**关卡设计规格：**
- **网格大小**：9x9
- **序章**：1-2个箱子，纯推箱子教程，简单墙壁布局
- **第一章**：3-4个箱子，推箱子+火球躲避，墙壁+障碍物布局
- **美术资源**：核心（箱子）和工作台（目标点）由美术组提供

**Files:**
- Modify: `Assets/Scenes/Prologue.unity`
- Modify: `Assets/Scenes/Chapter1.unity`
- Create: `Assets/Data/Dialogue/dialogues.asset` (ScriptableObject)

**Interfaces:**
- Consumes: 所有前序Task
- Produces: 可玩的序章和第一章

- [ ] **Step 1: 搭建序章场景（9x9网格）**

1. 创建GridManager物体 + GridManager脚本，设置cellSize=1
2. 创建Player物体（主角图片 + GridMovement脚本）
3. 创建墙壁围成9x9的边界
4. 在内部添加少量障碍物（不可推动）
5. 放置1-2个PushableBox
6. 放置对应数量的TargetPoint（工作台位置）
7. 创建DialogueSystem UI
8. 创建LevelCompleteUI
9. 创建PauseMenu

- [ ] **Step 2: 搭建第一章场景（9x9网格）**

1. 创建新的9x9网格布局
2. 添加更多墙壁和障碍物，增加复杂度
3. 放置3-4个PushableBox
4. 放置对应数量的TargetPoint
5. 添加FireballSpawner（火球生成器）
6. 配置火球运动轨迹和生成间隔
7. 创建DialogueSystem UI
8. 创建LevelCompleteUI
9. 创建PauseMenu

- [ ] **Step 3: 配置对话数据**

创建JSON文件，包含序章和第一章的所有对话文本。

- [ ] **Step 4: 测试完整流程**

1. 主菜单 → 起名 → 序章对话 → 推箱子 → 完成
2. 第一章对话 → 推箱子+躲火球 → 完成
3. 验证关卡解锁和地图显示

- [ ] **Step 5: Commit**

```bash
git add Assets/Scenes/ Assets/Data/
git commit -m "feat: 完成序章和第一章关卡搭建"
```

---

## Task 8: 场景管理与开场CG

**场景流程：**
1. **OpeningScene**：开场CG视频播放
2. **PrologueDialogue**：序章对话
3. **Prologue**：序章推箱子关卡
4. **MainMenu**：主菜单（序章完成后进入）
5. **Chapter1**：第一章关卡
6. **地图选择**：只能重玩已通过的关卡

**Files:**
- Create: `Assets/Scripts/Scene/SceneLoader.cs`
- Create: `Assets/Scripts/Scene/OpeningVideo.cs`
- Create: `Assets/Scenes/OpeningScene.unity`
- Create: `Assets/Scenes/PrologueDialogue.unity`

**Interfaces:**
- Consumes: GameManager
- Produces: 完整场景流程管理

- [ ] **Step 1: 编写SceneLoader.cs**

```csharp
// Assets/Scripts/Scene/SceneLoader.cs
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set; }

    // 场景名称常量
    public const string OPENING = "OpeningScene";
    public const string PROLOGUE_DIALOGUE = "PrologueDialogue";
    public const string PROLOGUE = "Prologue";
    public const string MAIN_MENU = "MainMenu";
    public const string CHAPTER1 = "Chapter1";

    private string[] sceneOrder = {
        OPENING,
        PROLOGUE_DIALOGUE,
        PROLOGUE,
        MAIN_MENU,
        CHAPTER1
    };

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void LoadNextScene()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        int currentIndex = System.Array.IndexOf(sceneOrder, currentScene);

        if (currentIndex >= 0 && currentIndex < sceneOrder.Length - 1)
        {
            SceneManager.LoadScene(sceneOrder[currentIndex + 1]);
        }
    }

    public void LoadMainMenu()
    {
        SceneManager.LoadScene(MAIN_MENU);
    }

    public void StartNewGame()
    {
        SceneManager.LoadScene(OPENING);
    }

    public void ReplayLevel(int levelIndex)
    {
        // 根据关卡索引加载对应场景
        switch (levelIndex)
        {
            case 0:
                SceneManager.LoadScene(PROLOGUE);
                break;
            case 1:
                SceneManager.LoadScene(CHAPTER1);
                break;
            default:
                Debug.LogWarning($"Level {levelIndex} not implemented yet");
                break;
        }
    }
}
```

- [ ] **Step 2: 编写OpeningVideo.cs**

```csharp
// Assets/Scripts/Scene/OpeningVideo.cs
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;

public class OpeningVideo : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public Button skipButton;
    public CanvasGroup fadeOverlay;

    void Start()
    {
        videoPlayer.Play();
        skipButton.onClick.AddListener(SkipVideo);
        videoPlayer.loopPointReached += OnVideoEnd;
    }

    void OnVideoEnd(VideoPlayer vp)
    {
        // 视频播放完毕，进入下一场景
        SceneLoader.Instance.LoadScene(SceneLoader.PROLOGUE_DIALOGUE);
    }

    void SkipVideo()
    {
        videoPlayer.Stop();
        SceneLoader.Instance.LoadScene(SceneLoader.PROLOGUE_DIALOGUE);
    }

    void Update()
    {
        // 按任意键跳过（可选）
        if (Input.anyKeyDown && videoPlayer.isPlaying)
        {
            SkipVideo();
        }
    }
}
```

- [ ] **Step 3: 在Unity中创建OpeningScene**

1. 创建新场景 `OpeningScene`
2. 创建空物体挂载VideoPlayer组件
3. **占位处理**：先创建一个2秒空白视频文件作为占位（后续替换正式CG）
4. 设置VideoPlayer：Play On Awake=true, Loop=false
5. 创建SkipButton（跳过按钮）
6. 挂载OpeningVideo脚本

- [ ] **Step 4: 创建PrologueDialogue场景**

1. 创建新场景 `PrologueDialogue`
2. 复制对话系统UI
3. 配置序章对话数据
4. 对话完成后自动加载Prologue关卡

- [ ] **Step 5: 修改GameManager使用SceneLoader**

更新 `GameManager.cs`：
```csharp
// 在GameManager.cs中修改LoadScene方法
public void LoadScene(string sceneName)
{
    SceneLoader.Instance?.LoadScene(sceneName);
}

public void LoadMainMenu()
{
    SceneLoader.Instance?.LoadMainMenu();
}
```

- [ ] **Step 6: Commit**

```bash
git add Assets/Scripts/Scene/ Assets/Scenes/OpeningScene* Assets/Scenes/PrologueDialogue*
git commit -m "feat: 实现场景管理与开场CG视频播放"
```

---

## Task 9: 美术资源集成

**Files:**
- Copy: `美术资源/人/*.png` → `Assets/Sprites/Characters/`
- Copy: `美术资源/场景/*.png` → `Assets/Sprites/Scenes/`
- Modify: 所有Prefab的SpriteRenderer

**Interfaces:**
- Consumes: 所有Prefab
- Produces: 使用正式美术资源的游戏

- [ ] **Step 1: 导入美术资源**

将美术资源目录下的图片复制到Unity项目的Sprites目录。

- [ ] **Step 2: 更新Prefab**

1. Player prefab → 使用主控.png
2. PushableBox → 设计专用箱子图片（可请美术提供或用简单方块）
3. 场景背景 → 使用对应场景图

- [ ] **Step 3: 设置Sprite属性**

1. Texture Type: Sprite (2D and UI)
2. Pixels Per Unit: 根据图片尺寸调整
3. Filter Mode: Point (像素风格)

- [ ] **Step 4: Commit**

```bash
git add Assets/Sprites/
git commit -m "art: 集成正式美术资源"
```

---

## 验证清单

完成所有Task后，验证以下内容：

- [ ] 主菜单可以正常显示和点击
- [ ] 起名弹窗可以输入名字
- [ ] 序章对话可以正常播放
- [ ] 序章推箱子可以完成
- [ ] 第一章对话可以正常播放
- [ ] 第一章火球可以躲避
- [ ] 第一章推箱子+躲避可以完成
- [ ] 暂停菜单可以正常工作
- [ ] 关卡完成/失败UI正常
- [ ] 没有严重Bug
- [ ] 美术资源完整显示

---

## 后续扩展（第二章Boss战，后续再做）

- Boss引力系统
- 地形移动机制
- 更复杂的关卡设计
- 开场CG动画
