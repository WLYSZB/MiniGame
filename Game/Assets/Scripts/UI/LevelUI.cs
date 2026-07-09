using UnityEngine;

public class LevelUI : MonoBehaviour
{
    [SerializeField] private GameObject winPanel;

    private void Awake()
    {
        if (winPanel != null)
        {
            winPanel.SetActive(false);
        }
    }

    public void ShowWinPanel()
    {
        if (winPanel != null)
        {
            winPanel.SetActive(true);
        }
    }

    public void OnBackToMenuClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoadMainMenu();
        }
    }
}
