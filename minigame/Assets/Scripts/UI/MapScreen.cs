using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 地图界面 - 水平排列关卡节点，可点击重玩已通过的关卡
/// </summary>
public class MapScreen : MonoBehaviour
{
    [Header("UI")]
    public Button backButton;
    public Transform nodeContainer;     // 关卡节点容器

    [Header("Prefabs")]
    public GameObject mapNodePrefab;    // 关卡节点Prefab

    [Header("Map Data")]
    public MapNodeData[] mapNodes;      // 关卡节点配置

    void Awake()
    {
        backButton.onClick.AddListener(OnBack);
    }

    public void RefreshMap()
    {
        // 清除旧节点
        foreach (Transform child in nodeContainer)
            Destroy(child.gameObject);

        // 水平排列关卡节点
        float xOffset = 0f;
        float spacing = 300f;

        foreach (var node in mapNodes)
        {
            GameObject nodeObj = Instantiate(mapNodePrefab, nodeContainer);
            RectTransform rect = nodeObj.GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(xOffset, 0);

            // 设置节点内容
            MapNodeUI nodeUI = nodeObj.GetComponent<MapNodeUI>();
            nodeUI.Setup(node);

            // 点击事件
            int levelIndex = node.levelIndex;
            nodeObj.GetComponent<Button>().onClick.AddListener(() => OnNodeClick(levelIndex));

            xOffset += spacing;
        }
    }

    void OnNodeClick(int levelIndex)
    {
        if (GameManager.Instance.IsLevelUnlocked(levelIndex))
        {
            GameManager.Instance.LoadLevel(levelIndex);
        }
    }

    void OnBack()
    {
        GetComponentInParent<MainMenu>().mainPanel.SetActive(true);
        gameObject.SetActive(false);
    }
}
