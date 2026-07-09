using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 地图节点UI - 显示关卡信息和锁定状态
/// </summary>
public class MapNodeUI : MonoBehaviour
{
    [Header("UI References")]
    public Image characterPortrait;
    public TextMeshProUGUI characterName;
    public GameObject lockIcon;

    /// <summary>
    /// 设置节点数据
    /// </summary>
    public void Setup(MapNodeData data)
    {
        if (data == null) return;

        // 设置角色立绘
        if (characterPortrait != null && data.characterPortrait != null)
        {
            characterPortrait.sprite = data.characterPortrait;
        }

        // 设置角色名字
        if (characterName != null)
        {
            characterName.text = data.characterName;
        }

        // 检查解锁状态
        bool unlocked = GameManager.Instance != null && 
                       GameManager.Instance.IsLevelUnlocked(data.levelIndex);

        // 设置锁定图标
        if (lockIcon != null)
        {
            lockIcon.SetActive(!unlocked);
        }

        // 未解锁的角色变灰
        if (characterPortrait != null)
        {
            characterPortrait.color = unlocked ? Color.white : Color.gray;
        }
    }
}
