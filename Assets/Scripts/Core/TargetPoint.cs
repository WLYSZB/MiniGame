using UnityEngine;

/// <summary>
/// 目标点 - 工作台位置，箱子需要推到此处
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class TargetPoint : MonoBehaviour
{
    void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
        gameObject.tag = "Target";
    }
}
