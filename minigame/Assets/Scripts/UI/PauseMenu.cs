using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 暂停菜单 - ESC暂停，继续/重来/返回主菜单
/// </summary>
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
        resumeButton.onClick.AddListener(() => { AudioManager.Instance?.PlayClick(); Resume(); });
        restartButton.onClick.AddListener(() => { AudioManager.Instance?.PlayClick(); Restart(); });
        mainMenuButton.onClick.AddListener(() => { AudioManager.Instance?.PlayClick(); BackToMenu(); });
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
        GameManager.Instance.LoadMainMenu();
    }
}
