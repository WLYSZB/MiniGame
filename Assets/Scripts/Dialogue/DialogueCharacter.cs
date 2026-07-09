using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 管理对话场景中单个角色的显示、亮度和表情切换
/// 美术资源：每个角色有多个表情立绘（默认、开心、生气、疑惑等）
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class DialogueCharacter : MonoBehaviour
{
    public string characterName;
    private SpriteRenderer spriteRenderer;
    private float activeAlpha = 1.0f;    // 说话时的亮度
    private float inactiveAlpha = 0.4f;  // 不说话时的亮度

    // 表情立绘字典：emotion名 → Sprite
    private Dictionary<string, Sprite> emotionSprites = new Dictionary<string, Sprite>();
    private Sprite defaultSprite;  // 默认表情

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        defaultSprite = spriteRenderer.sprite;
    }

    /// <summary>
    /// 注册表情立绘
    /// </summary>
    public void RegisterEmotion(string emotion, Sprite sprite)
    {
        if (!string.IsNullOrEmpty(emotion) && sprite != null)
            emotionSprites[emotion] = sprite;
    }

    /// <summary>
    /// 设置角色为活跃状态（全亮）
    /// </summary>
    public void SetActive()
    {
        SetAlpha(activeAlpha);
    }

    /// <summary>
    /// 设置角色为非活跃状态（变暗）
    /// </summary>
    public void SetInactive()
    {
        SetAlpha(inactiveAlpha);
    }

    /// <summary>
    /// 设置角色表情（根据emotion名切换Sprite）
    /// 如果没有对应表情，使用默认表情
    /// </summary>
    public void SetEmotion(string emotion)
    {
        if (string.IsNullOrEmpty(emotion))
        {
            spriteRenderer.sprite = defaultSprite;
            return;
        }

        if (emotionSprites.ContainsKey(emotion))
            spriteRenderer.sprite = emotionSprites[emotion];
        else
            spriteRenderer.sprite = defaultSprite;
    }

    /// <summary>
    /// 直接设置Sprite（用于初始化）
    /// </summary>
    public void SetSprite(Sprite sprite)
    {
        if (sprite != null)
        {
            spriteRenderer.sprite = sprite;
            defaultSprite = sprite;
        }
    }

    void SetAlpha(float alpha)
    {
        Color color = spriteRenderer.color;
        color.a = alpha;
        spriteRenderer.color = color;
    }
}
