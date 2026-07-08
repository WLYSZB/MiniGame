using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelUI : MonoBehaviour
{
    [SerializeField] private GameObject winPanel;

    private bool showingWinPanel;

    private void Awake()
    {
        if (winPanel != null)
        {
            winPanel.SetActive(false);
        }

        showingWinPanel = false;
    }

    public void ShowWinPanel()
    {
        if (winPanel != null)
        {
            winPanel.SetActive(true);
        }

        showingWinPanel = true;
    }

    public void OnBackToMenuClicked()
    {
        SceneManager.LoadScene("MainMenu");
    }

    private void OnGUI()
    {
        if (!showingWinPanel)
        {
            return;
        }

        var panelRect = new Rect(24f, 280f, 280f, 140f);
        GUI.Box(panelRect, "Tutorial Complete");
        GUI.Label(new Rect(panelRect.x + 16f, panelRect.y + 36f, 220f, 24f), "Backup core restored.");

        if (GUI.Button(new Rect(panelRect.x + 16f, panelRect.y + 76f, 248f, 36f), "Back To Main Menu"))
        {
            OnBackToMenuClicked();
        }
    }
}
