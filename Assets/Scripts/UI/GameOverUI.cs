using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 失败UI - 重试/返回主菜单
/// </summary>
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
        GameManager.Instance.LoadMainMenu();
    }
}
