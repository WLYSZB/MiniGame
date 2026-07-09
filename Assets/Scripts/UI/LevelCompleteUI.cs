using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 关卡完成UI - 解锁下一关，下一关/返回菜单按钮
/// </summary>
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
        GameManager.Instance.LoadMainMenu();
    }
}
