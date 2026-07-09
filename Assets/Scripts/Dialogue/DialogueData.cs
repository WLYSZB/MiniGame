using System;
using UnityEngine;

/// <summary>
/// 对话数据结构
/// </summary>
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
