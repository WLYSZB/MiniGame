using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

/// <summary>
/// 游戏管理器 - 单例模式，管理游戏状态和场景切换
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public string PlayerName { get; set; } = "垃圾袋";
    public int CurrentLevel { get; set; } = 0;

    // 关卡解锁状态
    private HashSet<int> unlockedLevels = new HashSet<int> { 0 }; // 默认解锁序章

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadProgress();
    }

    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void LoadLevel(int levelIndex)
    {
        CurrentLevel = levelIndex;
        string[] levels = { "Prologue", "Chapter1", "Chapter2" };
        if (levelIndex < levels.Length)
            SceneManager.LoadScene(levels[levelIndex]);
    }

    public void CompleteLevel()
    {
        UnlockLevel(CurrentLevel + 1);
        CurrentLevel++;
        LoadMainMenu();
    }

    public void LoadMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    // 关卡解锁管理
    public bool IsLevelUnlocked(int levelIndex) => unlockedLevels.Contains(levelIndex);

    public void UnlockLevel(int levelIndex)
    {
        unlockedLevels.Add(levelIndex);
        SaveProgress();
    }

    public void SaveProgress()
    {
        string data = string.Join(",", unlockedLevels);
        PlayerPrefs.SetString("UnlockedLevels", data);
        PlayerPrefs.SetString("PlayerName", PlayerName);
        PlayerPrefs.Save();
    }

    public void LoadProgress()
    {
        string data = PlayerPrefs.GetString("UnlockedLevels", "0");
        unlockedLevels.Clear();
        foreach (var s in data.Split(','))
        {
            if (int.TryParse(s, out int level))
                unlockedLevels.Add(level);
        }
        PlayerName = PlayerPrefs.GetString("PlayerName", "垃圾袋");
    }
}
