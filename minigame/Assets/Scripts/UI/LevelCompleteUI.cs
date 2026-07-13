using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Level complete UI
/// </summary>
public class LevelCompleteUI : MonoBehaviour
{
    public GameObject completePanel;
    public Button nextButton;
    public Button menuButton;

    void Awake()
    {
        if (completePanel != null)
            completePanel.SetActive(false);

        if (nextButton != null)
            nextButton.onClick.AddListener(() => { AudioManager.Instance?.PlayClick(); OnNext(); });

        if (menuButton != null)
            menuButton.onClick.AddListener(() => { AudioManager.Instance?.PlayClick(); OnMenu(); });
    }

    public void Show()
    {
        Debug.Log("LevelCompleteUI.Show called");

        if (completePanel != null)
            completePanel.SetActive(true);

        // Unlock next level
        if (GameManager.Instance != null)
        {
            int currentLevel = GameManager.Instance.CurrentLevel;
            GameManager.Instance.UnlockLevel(currentLevel + 1);
        }
    }

    void OnNext()
    {
        if (completePanel != null)
            completePanel.SetActive(false);

        if (GameManager.Instance != null)
            GameManager.Instance.CompleteLevel();
    }

    void OnMenu()
    {
        if (completePanel != null)
            completePanel.SetActive(false);

        if (GameManager.Instance != null)
            GameManager.Instance.LoadMainMenu();
        else
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
}
