using UnityEngine;

/// <summary>
/// 地图节点配置 - ScriptableObject
/// </summary>
[CreateAssetMenu(fileName = "MapNodeData", menuName = "MiniGame/Map Node Data")]
public class MapNodeData : ScriptableObject
{
    public string characterName;
    public Sprite characterPortrait;
    public int levelIndex;
    public string sceneName;
}
