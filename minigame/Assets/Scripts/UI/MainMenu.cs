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
        mainPanel.SetActive(true);
        mapPanel.SetActive(false);

        startButton.onClick.AddListener(OnStart);
        mapButton.onClick.AddListener(OnMap);
        quitButton.onClick.AddListener(OnQuit);

        GameManager.Instance.LoadProgress();
        DisplayUnlockedNPCs();
    }

    void DisplayUnlockedNPCs()
    {
        // 清除旧NPC
        foreach (Transform child in npcContainer)
            Destroy(child.gameObject);

        // 显示已解锁的NPC
        foreach (var npc in availableNPCs)
        {
            if (GameManager.Instance.IsLevelUnlocked(npc.unlockLevel))
            {
                GameObject npcObj = Instantiate(npcPrefab, npcContainer);
                npcObj.GetComponentInChildren<SpriteRenderer>().sprite = npc.portrait;
                npcObj.GetComponentInChildren<TextMeshProUGUI>().text = npc.characterName;
            }
        }
    }

    void OnStart()
    {
        nameInputUI.Show(name =>
        {
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
