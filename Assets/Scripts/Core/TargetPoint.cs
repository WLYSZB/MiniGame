using UnityEngine;

/// <summary>
/// 目标点 - 工作台位置，箱子需要推到此处
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class TargetPoint : MonoBehaviour
{
    void Awake()
    {
        // 确保Collider2D设置正确
        BoxCollider2D col = GetComponent<BoxCollider2D>();
        if (col == null)
            col = gameObject.AddComponent<BoxCollider2D>();

        col.isTrigger = true;
        col.size = new Vector2(0.9f, 0.9f);

        gameObject.tag = "Target";
    }
}
