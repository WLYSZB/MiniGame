using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public const string DefaultPlayerName = "Trash Bag";

    public static GameManager Instance { get; private set; }

    private string playerName = DefaultPlayerName;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetPlayerName(string value)
    {
        playerName = string.IsNullOrWhiteSpace(value) ? DefaultPlayerName : value.Trim();
    }

    public string GetPlayerName()
    {
        return string.IsNullOrWhiteSpace(playerName) ? DefaultPlayerName : playerName;
    }

    public void LoadPrologue()
    {
        SceneManager.LoadScene("Prologue");
    }

    public void LoadMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
