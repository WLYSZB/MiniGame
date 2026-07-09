using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 场景加载管理器 - 管理游戏流程
/// </summary>
public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set; }

    // 场景名称常量
    public const string OPENING = "OpeningScene";
    public const string PROLOGUE_DIALOGUE = "PrologueDialogue";
    public const string PROLOGUE = "Prologue";
    public const string MAIN_MENU = "MainMenu";
    public const string CHAPTER1 = "Chapter1";

    private string[] sceneOrder = {
        OPENING,
        PROLOGUE_DIALOGUE,
        PROLOGUE,
        MAIN_MENU,
        CHAPTER1
    };

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void LoadNextScene()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        int currentIndex = System.Array.IndexOf(sceneOrder, currentScene);

        if (currentIndex >= 0 && currentIndex < sceneOrder.Length - 1)
        {
            SceneManager.LoadScene(sceneOrder[currentIndex + 1]);
        }
    }

    public void LoadMainMenu()
    {
        SceneManager.LoadScene(MAIN_MENU);
    }

    public void StartNewGame()
    {
        SceneManager.LoadScene(OPENING);
    }

    public void ReplayLevel(int levelIndex)
    {
        // 根据关卡索引加载对应场景
        switch (levelIndex)
        {
            case 0:
                SceneManager.LoadScene(PROLOGUE);
                break;
            case 1:
                SceneManager.LoadScene(CHAPTER1);
                break;
            default:
                Debug.LogWarning($"Level {levelIndex} not implemented yet");
                break;
        }
    }
}
