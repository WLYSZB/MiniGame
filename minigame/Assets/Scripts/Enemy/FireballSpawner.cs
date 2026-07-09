using UnityEngine;
using System.Collections;

/// <summary>
/// 火球生成器 - 第一章火球AI
/// </summary>
public class FireballSpawner : MonoBehaviour
{
    public GameObject fireballPrefab;
    public int fireballCount = 5;
    public float spawnRadius = 5f;
    public float spawnInterval = 2f;

    void Start()
    {
        StartCoroutine(SpawnFireballs());
    }

    IEnumerator SpawnFireballs()
    {
        while (true)
        {
            for (int i = 0; i < fireballCount; i++)
            {
                Vector2 randomPos = (Random.insideUnitCircle.normalized) * spawnRadius;
                GameObject fireball = Instantiate(fireballPrefab, randomPos, Quaternion.identity);
                Fireball fb = fireball.GetComponent<Fireball>();
                if (fb != null)
                {
                    fb.direction = -randomPos.normalized; // 向中心飞
                }
            }
            yield return new WaitForSeconds(spawnInterval);
        }
    }
}
