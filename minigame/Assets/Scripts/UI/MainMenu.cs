using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 主菜单 - 展示已解锁的NPC，开始/地图/退出按钮
/// </summary>
public class MainMenu : MonoBehaviour
{
    [Header("Buttons")]
    public Button startButton;
    public Button mapButton;
    public Button quitButton;

    [Header("NPC Display")]
    public Transform npcContainer;     // NPC展示容器
    public GameObject npcPrefab;       // NPC立绘Prefab

    [Header("Panels")]
    public GameObject mainPanel;
    public GameObject mapPanel;
    public NameInputUI nameInputUI;

    [Header("NPC Data")]
    public NPCData[] availableNPCs;    // 可展示的NPC列表

    void Start()
    {
        if (mainPanel != null)
            mainPanel.SetActive(true);
        if (mapPanel != null)
            mapPanel.SetActive(false);

        if (startButton != null)
            startButton.onClick.AddListener(OnStart);
        if (mapButton != null)
            mapButton.onClick.AddListener(OnMap);
        if (quitButton != null)
            quitButton.onClick.AddListener(OnQuit);

        // 确保GameManager存在
        if (GameManager.Instance == null)
        {
            GameObject gm = new GameObject("GameManager");
            gm.AddComponent<GameManager>();
        }

        GameManager.Instance.LoadProgress();
        DisplayUnlockedNPCs();
    }

    void DisplayUnlockedNPCs()
    {
        // 如果没有NPC预制体或容器，跳过
        if (npcPrefab == null || npcContainer == null)
        {
            Debug.LogWarning("NPC Prefab or Container not assigned, skipping NPC display");
            return;
        }

        // 清除旧NPC
        foreach (Transform child in npcContainer)
            Destroy(child.gameObject);

        // 显示已解锁的NPC
        foreach (var npc in availableNPCs)
        {
            if (GameManager.Instance != null && GameManager.Instance.IsLevelUnlocked(npc.unlockLevel))
            {
                GameObject npcObj = Instantiate(npcPrefab, npcContainer);
                SpriteRenderer sr = npcObj.GetComponentInChildren<SpriteRenderer>();
                if (sr != null && npc.portrait != null)
                    sr.sprite = npc.portrait;

                TextMeshProUGUI tmp = npcObj.GetComponentInChildren<TextMeshProUGUI>();
                if (tmp != null)
                    tmp.text = npc.characterName;
            }
        }
    }

    void OnStart()
    {
        Debug.Log("OnStart clicked");

        if (nameInputUI == null)
        {
            Debug.LogError("nameInputUI is not assigned!");
            return;
        }

        Debug.Log("Showing name input UI");
        nameInputUI.Show(name =>
        {
            Debug.Log($"Player name: {name}");

            // 确保GameManager存在
            if (GameManager.Instance == null)
            {
                GameObject gm = new GameObject("GameManager");
                gm.AddComponent<GameManager>();
            }

            GameManager.Instance.PlayerName = name;
            GameManager.Instance.LoadLevel(0);
        });
    }

    void OnMap()
    {
        mainPanel.SetActive(false);
        mapPanel.SetActive(true);
        mapPanel.GetComponent<MapScreen>().RefreshMap();
    }

    void OnQuit()
    {
        Application.Quit();
    }
}

[System.Serializable]
public class NPCData
{
    public string characterName;
    public Sprite portrait;
    public int unlockLevel;  // 解锁该NPC的关卡ID
}
