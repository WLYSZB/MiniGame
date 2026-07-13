using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// 游戏内HUD - 步数计数器 + 重置按钮 + 回退按钮
/// </summary>
public class GameplayHUD : MonoBehaviour
{
    public TextMeshProUGUI stepText;
    public UnityEngine.UI.Button resetButton;
    public UnityEngine.UI.Button undoButton;

    void Start()
    {
        if (resetButton != null)
            resetButton.onClick.AddListener(OnReset);

        if (undoButton != null)
            undoButton.onClick.AddListener(OnUndo);

        // 运行时自动连接 GridMovement 的步数回调
        var gridMovement = FindObjectOfType<GridMovement>();
        if (gridMovement != null)
            gridMovement.OnStepChanged += UpdateSteps;

        UpdateSteps(0);
    }

    public void UpdateSteps(int count)
    {
        if (stepText != null)
            stepText.text = $"步数: {count}";
    }

    void OnReset()
    {
        if (GridMovement.IsLevelComplete) return;
        AudioManager.Instance?.PlayClick();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void OnUndo()
    {
        if (GridMovement.IsLevelComplete) return;
        AudioManager.Instance?.PlayClick();
        var gridMovement = FindObjectOfType<GridMovement>();
        if (gridMovement != null && UndoManager.Instance != null)
        {
            UndoManager.Instance.Undo();
            gridMovement.DecrementStep();
        }
    }
}
