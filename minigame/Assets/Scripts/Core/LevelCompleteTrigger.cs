using UnityEngine;

/// <summary>
/// 关卡完成触发器 - 完成后先播放对话，再显示完成界面
/// </summary>
public class LevelCompleteTrigger : MonoBehaviour
{
    [Header("Dialogue")]
    public DialogueSystem dialogueSystem;
    public string completionDialogueId = "prologue_complete";

    [Header("UI")]
    public LevelCompleteUI levelCompleteUI;

    /// <summary>
    /// 被PushableBox调用，触发关卡完成流程
    /// </summary>
    public void OnLevelComplete()
    {
        GridMovement.IsLevelComplete = true;
        AudioManager.Instance?.PlaySFX(AudioManager.Instance.win);

        if (dialogueSystem != null)
        {
            dialogueSystem.StartDialogue(completionDialogueId, () =>
            {
                // 对话完成后显示完成界面
                ShowLevelCompleteUI();
            });
        }
        else
        {
            // 没有对话系统，直接显示完成界面
            ShowLevelCompleteUI();
        }
    }

    void ShowLevelCompleteUI()
    {
        if (levelCompleteUI != null)
            levelCompleteUI.Show();
    }
}
