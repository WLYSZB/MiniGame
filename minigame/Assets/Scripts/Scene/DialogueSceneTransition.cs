using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 对话场景结束后的跳转控制
/// </summary>
public class DialogueSceneTransition : MonoBehaviour
{
    public DialogueSystem dialogueSystem;
    public string nextScene = "Prologue";
    void Start()
    {
        // 对话完成后自动加载下一场景
        dialogueSystem.StartDialogue("prologue_start", () =>
        {
            SceneLoader.Instance?.LoadScene(nextScene);
        });
    }
}