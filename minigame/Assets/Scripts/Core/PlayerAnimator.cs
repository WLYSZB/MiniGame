using UnityEngine;

/// <summary>
/// 角色动画控制器 - 方向感知 + 帧切换
/// 由 GridMovement 在移动开始/结束时调用
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class PlayerAnimator : MonoBehaviour
{
    [Header("Sprite Frames (4 directions × 2 frames: idle + step)")]
    public Sprite[] downFrames;    // [0]=站立, [1]=迈步
    public Sprite[] upFrames;
    public Sprite[] leftFrames;
    public Sprite[] rightFrames;

    [Header("Settings")]
    public float animationSpeed = 8f;

    private SpriteRenderer spriteRenderer;
    private Vector2Int lastDirection = Vector2Int.down;
    private bool stepping = false;
    private float animTimer = 0f;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        UpdateSprite();
    }

    /// <summary>
    /// 移动开始时调用，切换到迈步帧
    /// </summary>
    public void OnMoveStart(Vector2Int direction)
    {
        lastDirection = direction;
        stepping = true;
        animTimer = 0f;
        UpdateSprite();
    }

    /// <summary>
    /// 移动结束时调用，切换回站立帧
    /// </summary>
    public void OnMoveEnd()
    {
        stepping = false;
        animTimer = 0f;
        UpdateSprite();
    }

    void Update()
    {
        // 简单的待机动画：站立时偶尔切换帧（呼吸效果）
        if (!stepping)
        {
            animTimer += Time.deltaTime;
        }
    }

    void UpdateSprite()
    {
        Sprite frame = GetCurrentFrame();
        if (frame != null)
            spriteRenderer.sprite = frame;
    }

    Sprite GetCurrentFrame()
    {
        Sprite[] frames = GetFramesForDirection(lastDirection);
        if (frames == null || frames.Length == 0) return null;
        return stepping && frames.Length > 1 ? frames[1] : frames[0];
    }

    Sprite[] GetFramesForDirection(Vector2Int dir)
    {
        if (dir == Vector2Int.up) return upFrames;
        if (dir == Vector2Int.down) return downFrames;
        if (dir == Vector2Int.left) return leftFrames;
        if (dir == Vector2Int.right) return rightFrames;
        return downFrames;
    }
}
