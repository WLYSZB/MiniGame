# 第一章详细实现文档

## 项目信息
- Unity 版本：2022 LTS
- 架构：传统结构
- 时间：1周内

## 项目结构

```
Assets/
├── Scripts/
│   ├── Player/
│   │   ├── PlayerController.cs      # 玩家移动控制
│   │   └── PlayerAnimation.cs        # 玩家动画
│   ├── Box/
│   │   ├── Box.cs                    # 箱子逻辑
│   │   └── BoxTarget.cs              # 目标位置检测
│   ├── Dialogue/
│   │   ├── DialogueData.cs           # ScriptableObject 数据
│   │   ├── DialogueManager.cs        # 对话管理器
│   │   └── DialogueUI.cs             # 对话 UI
│   ├── UI/
│   │   ├── MainMenuUI.cs             # 主菜单
│   │   ├── LevelUI.cs                # 关卡 UI
│   │   └── NameInputUI.cs            # 起名弹窗
│   └── GameManager.cs                # 游戏管理器
├── Scenes/
│   ├── MainMenu.unity                # 主菜单场景
│   └── Level1.unity                  # 第一章关卡
├── Prefabs/
│   ├── Player.prefab                 # 玩家预制体
│   ├── Box.prefab                    # 箱子预制体
│   └── BoxTarget.prefab              # 目标位置预制体
├── Art/
│   ├── Player/                       # 玩家美术资源
│   ├── Box/                          # 箱子美术资源
│   └── Background/                   # 背景美术资源
└── UI/
    ├── Sprites/                      # UI 图片
    └── Fonts/                        # 字体
```

## 第1步：Unity项目框架

### 1.1 创建项目
- 打开 Unity Hub，新建 2D 项目
- 项目名称：MiniGame
- 保存位置：C:\Users\19774\Desktop\MiniGame

### 1.2 文件夹结构
- 在 Assets 下创建上述文件夹结构
- 设置 .gitignore 忽略 Unity 临时文件

### 1.3 基础设置
- 设置默认分辨率：1920x1080
- 设置背景颜色：黑色
- 导入基础资源包（如果需要）

## 第2步：玩家控制

### 2.1 PlayerController.cs
```csharp
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private LayerMask obstacleLayer;
    
    private Rigidbody2D rb;
    private Vector2 moveInput;
    private bool isMoving = false;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    
    void Update()
    {
        if (!isMoving)
        {
            moveInput.x = Input.GetAxisRaw("Horizontal");
            moveInput.y = Input.GetAxisRaw("Vertical");
            
            if (moveInput != Vector2.zero)
            {
                TryMove(moveInput);
            }
        }
    }
    
    void TryMove(Vector2 direction)
    {
        // 检测前方是否有障碍物
        RaycastHit2D hit = Physics2D.Raycast(
            transform.position, 
            direction, 
            1f, 
            obstacleLayer
        );
        
        if (hit.collider == null)
        {
            // 没有障碍物，可以移动
            StartCoroutine(Move(direction));
        }
        else if (hit.collider.CompareTag("Box"))
        {
            // 前方是箱子，尝试推动
            Box box = hit.collider.GetComponent<Box>();
            if (box != null && box.TryPush(direction))
            {
                StartCoroutine(Move(direction));
            }
        }
    }
    
    System.Collections.IEnumerator Move(Vector2 direction)
    {
        isMoving = true;
        
        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + (Vector3)direction;
        
        float elapsed = 0f;
        while (elapsed < 1f)
        {
            transform.position = Vector3.Lerp(startPos, endPos, elapsed);
            elapsed += Time.deltaTime * moveSpeed;
            yield return null;
        }
        
        transform.position = endPos;
        isMoving = false;
    }
}
```

### 2.2 PlayerAnimation.cs
```csharp
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    private Animator animator;
    
    void Start()
    {
        animator = GetComponent<Animator>();
    }
    
    public void SetMoving(bool moving)
    {
        animator.SetBool("IsMoving", moving);
    }
    
    public void SetDirection(Vector2 direction)
    {
        animator.SetFloat("MoveX", direction.x);
        animator.SetFloat("MoveY", direction.y);
    }
}
```

## 第3步：推箱子机制

### 3.1 Box.cs
```csharp
using UnityEngine;

public class Box : MonoBehaviour
{
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private LayerMask targetLayer;
    
    private Rigidbody2D rb;
    private bool isMoving = false;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    
    public bool TryPush(Vector2 direction)
    {
        if (isMoving) return false;
        
        // 检测箱子前方是否有障碍物
        RaycastHit2D hit = Physics2D.Raycast(
            transform.position, 
            direction, 
            1f, 
            obstacleLayer
        );
        
        if (hit.collider == null)
        {
            // 没有障碍物，可以推动
            StartCoroutine(Move(direction));
            return true;
        }
        
        return false;
    }
    
    System.Collections.IEnumerator Move(Vector2 direction)
    {
        isMoving = true;
        
        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + (Vector3)direction;
        
        float elapsed = 0f;
        while (elapsed < 1f)
        {
            transform.position = Vector3.Lerp(startPos, endPos, elapsed);
            elapsed += Time.deltaTime * 5f;
            yield return null;
        }
        
        transform.position = endPos;
        isMoving = false;
        
        // 检查是否到达目标位置
        CheckTarget();
    }
    
    void CheckTarget()
    {
        RaycastHit2D hit = Physics2D.Raycast(
            transform.position, 
            Vector2.zero, 
            0f, 
            targetLayer
        );
        
        if (hit.collider != null)
        {
            // 到达目标位置
            BoxTarget target = hit.collider.GetComponent<BoxTarget>();
            if (target != null)
            {
                target.SetBoxArrived(true);
            }
        }
    }
}
```

### 3.2 BoxTarget.cs
```csharp
using UnityEngine;

public class BoxTarget : MonoBehaviour
{
    private bool boxArrived = false;
    
    public void SetBoxArrived(bool arrived)
    {
        boxArrived = arrived;
        if (arrived)
        {
            // 检查是否所有箱子都到达目标
            CheckAllTargets();
        }
    }
    
    void CheckAllTargets()
    {
        BoxTarget[] targets = FindObjectsOfType<BoxTarget>();
        foreach (BoxTarget target in targets)
        {
            if (!target.boxArrived)
            {
                return;
            }
        }
        
        // 所有箱子都到达目标，关卡完成
        GameManager.Instance.LevelComplete();
    }
}
```

## 第4步：对话系统

### 4.1 DialogueData.cs
```csharp
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Dialogue", menuName = "Dialogue/Dialogue Data")]
public class DialogueData : ScriptableObject
{
    [System.Serializable]
    public class DialogueLine
    {
        public string speakerName;           // 说话人名字
        public Sprite speakerSprite;         // 说话人立绘
        public Sprite expressionSprite;      // 表情立绘（可选）
        public string dialogueText;          // 对话文本
        public bool hasChoices;              // 是否有选项
        public List<DialogueChoice> choices; // 选项列表
    }
    
    [System.Serializable]
    public class DialogueChoice
    {
        public string choiceText;            // 选项文本
        public int nextLineIndex;            // 选择后跳转到哪一行
    }
    
    public List<DialogueLine> dialogueLines; // 对话行列表
    public float typingSpeed = 0.05f;        // 打字机速度
}
```

### 4.2 DialogueManager.cs
```csharp
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;
    
    [SerializeField] private DialogueUI dialogueUI;
    
    private DialogueData currentDialogue;
    private int currentLineIndex = 0;
    private bool isDialogueActive = false;
    private bool isTyping = false;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public void StartDialogue(DialogueData dialogue)
    {
        currentDialogue = dialogue;
        currentLineIndex = 0;
        isDialogueActive = true;
        
        dialogueUI.Show();
        ShowCurrentLine();
    }
    
    void ShowCurrentLine()
    {
        if (currentLineIndex >= currentDialogue.dialogueLines.Count)
        {
            EndDialogue();
            return;
        }
        
        DialogueData.DialogueLine line = currentDialogue.dialogueLines[currentLineIndex];
        
        dialogueUI.SetSpeakerName(line.speakerName);
        dialogueUI.SetSpeakerSprite(line.speakerSprite);
        
        if (line.expressionSprite != null)
        {
            dialogueUI.SetExpressionSprite(line.expressionSprite);
        }
        
        StartCoroutine(TypeText(line.dialogueText));
    }
    
    IEnumerator TypeText(string text)
    {
        isTyping = true;
        dialogueUI.SetDialogueText("");
        
        foreach (char c in text)
        {
            dialogueUI.AddChar(c);
            yield return new WaitForSeconds(currentDialogue.typingSpeed);
        }
        
        isTyping = false;
        
        // 检查是否有选项
        DialogueData.DialogueLine line = currentDialogue.dialogueLines[currentLineIndex];
        if (line.hasChoices)
        {
            dialogueUI.ShowChoices(line.choices);
        }
    }
    
    public void OnNextButtonClicked()
    {
        if (isTyping)
        {
            // 如果正在打字，立即显示全部文本
            StopAllCoroutines();
            isTyping = false;
            DialogueData.DialogueLine line = currentDialogue.dialogueLines[currentLineIndex];
            dialogueUI.SetDialogueText(line.dialogueText);
        }
        else
        {
            // 如果没有选项，继续下一行
            DialogueData.DialogueLine line = currentDialogue.dialogueLines[currentLineIndex];
            if (!line.hasChoices)
            {
                currentLineIndex++;
                ShowCurrentLine();
            }
        }
    }
    
    public void OnChoiceSelected(int choiceIndex)
    {
        DialogueData.DialogueLine line = currentDialogue.dialogueLines[currentLineIndex];
        DialogueData.DialogueChoice choice = line.choices[choiceIndex];
        
        currentLineIndex = choice.nextLineIndex;
        dialogueUI.HideChoices();
        ShowCurrentLine();
    }
    
    void EndDialogue()
    {
        isDialogueActive = false;
        dialogueUI.Hide();
        
        // 通知 GameManager 对话结束
        GameManager.Instance.OnDialogueEnd();
    }
}
```

### 4.3 DialogueUI.cs
```csharp
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class DialogueUI : MonoBehaviour
{
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private Image speakerImage;
    [SerializeField] private TextMeshProUGUI speakerNameText;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private Transform choiceContainer;
    [SerializeField] private GameObject choiceButtonPrefab;
    
    private List<GameObject> choiceButtons = new List<GameObject>();
    
    public void Show()
    {
        dialoguePanel.SetActive(true);
    }
    
    public void Hide()
    {
        dialoguePanel.SetActive(false);
    }
    
    public void SetSpeakerName(string name)
    {
        speakerNameText.text = name;
    }
    
    public void SetSpeakerSprite(Sprite sprite)
    {
        speakerImage.sprite = sprite;
    }
    
    public void SetExpressionSprite(Sprite sprite)
    {
        // 直接替换立绘
        speakerImage.sprite = sprite;
    }
    
    public void SetDialogueText(string text)
    {
        dialogueText.text = text;
    }
    
    public void AddChar(char c)
    {
        dialogueText.text += c;
    }
    
    public void ShowChoices(List<DialogueData.DialogueChoice> choices)
    {
        HideChoices();
        
        for (int i = 0; i < choices.Count; i++)
        {
            GameObject choiceObj = Instantiate(choiceButtonPrefab, choiceContainer);
            choiceObj.GetComponentInChildren<TextMeshProUGUI>().text = choices[i].choiceText;
            
            int index = i;
            choiceObj.GetComponent<Button>().onClick.AddListener(() => 
            {
                DialogueManager.Instance.OnChoiceSelected(index);
            });
            
            choiceButtons.Add(choiceObj);
        }
    }
    
    public void HideChoices()
    {
        foreach (GameObject obj in choiceButtons)
        {
            Destroy(obj);
        }
        choiceButtons.Clear();
    }
}
```

## 第5步：UI系统

### 5.1 MainMenuUI.cs
```csharp
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    public void OnStartButtonClicked()
    {
        // 显示起名弹窗
        NameInputUI.Instance.Show();
    }
    
    public void OnQuitButtonClicked()
    {
        Application.Quit();
    }
}
```

### 5.2 NameInputUI.cs
```csharp
using UnityEngine;
using TMPro;

public class NameInputUI : MonoBehaviour
{
    public static NameInputUI Instance;
    
    [SerializeField] private GameObject nameInputPanel;
    [SerializeField] private TMP_InputField nameInputField;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public void Show()
    {
        nameInputPanel.SetActive(true);
        nameInputField.text = "";
    }
    
    public void OnConfirmButtonClicked()
    {
        string playerName = nameInputField.text;
        if (!string.IsNullOrEmpty(playerName))
        {
            // 保存玩家名字
            PlayerPrefs.SetString("PlayerName", playerName);
            
            // 隐藏起名弹窗
            nameInputPanel.SetActive(false);
            
            // 开始第一章
            GameManager.Instance.StartChapter1();
        }
    }
}
```

### 5.3 LevelUI.cs
```csharp
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelUI : MonoBehaviour
{
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject winPanel;
    
    public void OnPauseButtonClicked()
    {
        pausePanel.SetActive(true);
        Time.timeScale = 0f;
    }
    
    public void OnResumeButtonClicked()
    {
        pausePanel.SetActive(false);
        Time.timeScale = 1f;
    }
    
    public void OnRestartButtonClicked()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    public void OnMainMenuButtonClicked()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
    
    public void ShowWinPanel()
    {
        winPanel.SetActive(true);
    }
}
```

## 第6步：GameManager.cs

```csharp
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    
    [SerializeField] private DialogueData chapter1Dialogue;
    [SerializeField] private LevelUI levelUI;
    
    private string playerName;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        playerName = PlayerPrefs.GetString("PlayerName", "垃圾袋");
    }
    
    public void StartChapter1()
    {
        // 加载第一章场景
        SceneManager.LoadScene("Level1");
        
        // 等待场景加载完成后开始对话
        StartCoroutine(StartChapter1Dialogue());
    }
    
    System.Collections.IEnumerator StartChapter1Dialogue()
    {
        yield return new WaitForSeconds(0.5f);
        
        // 替换对话文本中的玩家名字
        DialogueData dialogue = Instantiate(chapter1Dialogue);
        foreach (var line in dialogue.dialogueLines)
        {
            line.dialogueText = line.dialogueText.Replace("{playerName}", playerName);
        }
        
        // 开始对话
        DialogueManager.Instance.StartDialogue(dialogue);
    }
    
    public void OnDialogueEnd()
    {
        // 对话结束，显示关卡 UI
        levelUI.Show();
    }
    
    public void LevelComplete()
    {
        // 关卡完成，显示胜利面板
        levelUI.ShowWinPanel();
    }
    
    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
```

## 第7步：整合

### 7.1 创建场景
1. MainMenu 场景
   - Canvas + 按钮（开始、退出）
   - 起名弹窗

2. Level1 场景
   - 3x3 或 4x4 网格地图
   - 玩家预制体
   - 2-3 个箱子
   - 2-3 个目标位置
   - 对话 UI
   - 关卡 UI

### 7.2 制作对话数据
1. 在 Assets/Dialogue 文件夹右键
2. Create > Dialogue > Dialogue Data
3. 填写对话内容

### 7.3 设置 UI
1. 创建 Canvas
2. 添加对话面板、关卡面板、胜利面板
3. 绑定按钮事件

### 7.4 测试流程
1. 主菜单 → 起名 → 对话 → 关卡 → 胜利 → 返回主菜单

## 验证标准

- 玩家可以控制角色移动
- 箱子可以被推动
- 箱子到达目标位置后显示胜利
- 对话系统正常工作
- 完整流程可以跑通
