using UnityEngine;

/// <summary>
/// 火球AI - 第一章躲避机制
/// </summary>
public class Fireball : MonoBehaviour
{
    public float speed = 3f;
    public Vector2 direction;
    private Vector2 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime);

        // 边界检测（可选：反弹或返回起点）
        if (Mathf.Abs(transform.position.x) > 10 || Mathf.Abs(transform.position.y) > 10)
        {
            ReturnToStart();
        }
    }

    void ReturnToStart()
    {
        transform.position = startPos;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // 玩家被击中
            Debug.Log("Player Hit!");
            FindObjectOfType<GameOverUI>()?.Show();
            Destroy(gameObject);
        }
    }
}
