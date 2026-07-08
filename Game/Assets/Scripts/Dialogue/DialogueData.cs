using UnityEngine;

[CreateAssetMenu(menuName = "MiniGame/Dialogue Data", fileName = "DialogueData")]
public class DialogueData : ScriptableObject
{
    [System.Serializable]
    public struct Line
    {
        public string speaker;
        [TextArea(2, 5)] public string text;
    }

    [SerializeField] private Line[] lines = new Line[0];

    public Line[] Lines => lines;
}
