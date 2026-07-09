using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Map screen - horizontal level nodes
/// </summary>
public class MapScreen : MonoBehaviour
{
    [Header("UI")]
    public Button backButton;
    public Transform nodeContainer;

    [Header("Prefabs")]
    public GameObject mapNodePrefab;

    [Header("Map Data")]
    public MapNodeData[] mapNodes;

    void Awake()
    {
        backButton.onClick.AddListener(OnBack);
    }

    public void RefreshMap()
    {
        foreach (Transform child in nodeContainer)
            Destroy(child.gameObject);

        float xOffset = 0f;
        float spacing = 300f;

        foreach (var node in mapNodes)
        {
            GameObject nodeObj = Instantiate(mapNodePrefab, nodeContainer);
            RectTransform rect = nodeObj.GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(xOffset, 0);

            MapNodeUI nodeUI = nodeObj.GetComponent<MapNodeUI>();
            if (nodeUI != null)
                nodeUI.Setup(node);

            int levelIndex = node.levelIndex;
            Button btn = nodeObj.GetComponent<Button>();
            if (btn != null)
                btn.onClick.AddListener(() => OnNodeClick(levelIndex));

            xOffset += spacing;
        }
    }

    void OnNodeClick(int levelIndex)
    {
        if (GameManager.Instance != null && GameManager.Instance.IsLevelUnlocked(levelIndex))
        {
            GameManager.Instance.LoadLevel(levelIndex);
        }
    }

    void OnBack()
    {
        MainMenu mainMenu = GetComponentInParent<MainMenu>();
        if (mainMenu != null && mainMenu.mainPanel != null)
            mainMenu.mainPanel.SetActive(true);
        gameObject.SetActive(false);
    }
}
