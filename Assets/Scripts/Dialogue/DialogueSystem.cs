using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 对话系统核心 - 多角色亮度高亮、表情切换、打字机效果、快进
/// </summary>
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
    [Range(0.1f, 1f)]
    public float characterScale = 0.35f; // 角色缩放比例（默认0.35）
    public float leftPositionX = -9f;    // 左侧角色X位置
    public float rightPositionX = 9f;    // 右侧角色X位置
    public float characterY = -1f;       // 角色Y位置（略低于中心）

    private List<DialogueCharacter> characters = new List<DialogueCharacter>();
    private DialogueSequence currentSequence;
    private int currentLineIndex;
    private bool isDialogueActive = false;
    private System.Action onDialogueComplete;

    // 对话数据缓存
    private DialogueContainer dialogueData;

    // 静态标志，防止多个实例同时开始对话
    private static bool staticDialogueStarted = false;

    void Awake()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (skipButton != null)
            skipButton.onClick.AddListener(OnSkipAll);

        LoadDialogueData();
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
    /// 从JSON文件加载对话数据
    /// </summary>
    void LoadDialogueData()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("Dialogue/dialogues");
        if (jsonFile != null)
        {
            dialogueData = JsonUtility.FromJson<DialogueContainer>(jsonFile.text);
        }
        else
        {
            Debug.LogError("Dialogue JSON file not found!");
        }
    }

    /// <summary>
    /// 开始对话序列
    /// </summary>
    public void StartDialogue(string sequenceId, System.Action onComplete = null)
    {
        Debug.Log($"StartDialogue called with id: {sequenceId}, isDialogueActive={isDialogueActive}, staticFlag={staticDialogueStarted}");

        // 如果对话已经在进行中，跳过
        if (isDialogueActive || staticDialogueStarted)
        {
            Debug.Log("Dialogue already active or started, skipping StartDialogue call");
            return;
        }

        // 设置静态标志
        staticDialogueStarted = true;

        if (dialogueData == null)
        {
            LoadDialogueData();
        }

        if (dialogueData == null)
        {
            Debug.LogError("Dialogue data not loaded!");
            return;
        }

        foreach (var seq in dialogueData.sequences)
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

        Debug.Log($"Starting dialogue: {currentSequence.id}, lines: {currentSequence.lines.Length}");

        onDialogueComplete = onComplete;
        currentLineIndex = 0;
        isDialogueActive = true;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        // 创建角色立绘
        SpawnCharacters(currentSequence.characters);

        // 显示第一句
        if (currentSequence.lines.Length > 0)
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

        // 根据角色数量分配位置（左边、右边）
        for (int i = 0; i < characterInfos.Length; i++)
        {
            var info = characterInfos[i];

            GameObject charObj = new GameObject($"Character_{info.characterName}");
            charObj.transform.SetParent(characterContainer);

            // 分配位置：第一个角色在左，第二个在右
            float posX = (i == 0) ? leftPositionX : rightPositionX;
            charObj.transform.localPosition = new Vector3(posX, characterY, 0);

            SpriteRenderer sr = charObj.AddComponent<SpriteRenderer>();

            // 加载默认立绘
            string spritePath = $"Sprites/Characters/{info.defaultPortraitName}";
            Debug.Log($"Loading sprite from: {spritePath}");
            Sprite defaultSprite = Resources.Load<Sprite>(spritePath);

            if (defaultSprite == null)
            {
                Debug.LogWarning($"Sprite not found: {spritePath}");
                // 尝试不带前缀加载
                defaultSprite = Resources.Load<Sprite>($"Sprites/Characters/{info.characterName}_默认");
                if (defaultSprite != null)
                    Debug.Log($"Found sprite with fallback name");
            }

            sr.sprite = defaultSprite;
            // 图层：背景是-10，UI是更高，角色在中间（5）
            sr.sortingOrder = 5;

            DialogueCharacter dc = charObj.AddComponent<DialogueCharacter>();
            dc.characterName = info.characterName;

            // 应用缩放
            charObj.transform.localScale = Vector3.one * characterScale;

            // 注册表情立绘
            if (info.emotions != null)
            {
                foreach (var emotionData in info.emotions)
                {
                    Sprite emotionSprite = Resources.Load<Sprite>($"Sprites/Characters/{emotionData.spriteName}");
                    if (emotionSprite != null)
                        dc.RegisterEmotion(emotionData.emotion, emotionSprite);
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
        if (line == null)
        {
            Debug.LogError("ShowLine: line is null!");
            return;
        }

        Debug.Log($"ShowLine: index={currentLineIndex}, speaker={line.speaker}, text={line.text}");

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
        if (typewriter != null && dialogueText != null)
        {
            // 直接显示完整文本，不用打字机效果
            dialogueText.text = line.text;
        }
        else
        {
            Debug.LogError("Typewriter or DialogueText is null!");
            if (dialogueText != null)
                dialogueText.text = line.text;
        }
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
        Debug.Log($"OnAdvance called, IsTyping={typewriter?.IsTyping}, currentLineIndex={currentLineIndex}");

        if (typewriter != null && typewriter.IsTyping)
        {
            // 跳过打字，直接显示完整文本
            Debug.Log("Skipping typewriter");
            typewriter.Skip();
            return;
        }

        // 下一句
        currentLineIndex++;
        Debug.Log($"Advancing to line {currentLineIndex}");

        if (currentLineIndex < currentSequence.lines.Length)
        {
            ShowLine(currentSequence.lines[currentLineIndex]);
        }
        else
        {
            Debug.Log("Dialogue complete, ending...");
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
        staticDialogueStarted = false;

        if (dialoguePanel != null)
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
