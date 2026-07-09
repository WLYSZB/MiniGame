using UnityEngine;

/// <summary>
/// 关卡数据配置 - ScriptableObject
/// </summary>
[CreateAssetMenu(fileName = "LevelData", menuName = "MiniGame/Level Data")]
public class LevelData : ScriptableObject
{
    public string levelName;
    public int width = 9;
    public int height = 9;
    public Vector2Int playerStart;
    public Vector2Int[] boxPositions;
    public Vector2Int[] targetPositions;
    public Vector2Int[] wallPositions;
    public Vector2Int[] obstaclePositions; // 不可推动的障碍物
    public string dialogueBefore;
    public string dialogueAfter;
}
