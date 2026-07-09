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
