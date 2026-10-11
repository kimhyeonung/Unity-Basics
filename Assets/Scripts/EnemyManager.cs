using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public EnemyController prefab;
    public PlayerController player;
    public GameManager game;
    public int poolSize = 12;
    EnemyController[] enemies;
    public int AliveCount
    {
        get
        {
            int count = 0;
            for (int i = 0; i < enemies.Length; i++)
            {
                if (enemies[i].gameObject.activeSelf) count++;
            }
            return count;
        }
    }

    void Awake()
    {
        Physics2D.IgnoreLayerCollision(9, 9, true);
        Physics2D.IgnoreLayerCollision(10, 10, true);
        Physics2D.IgnoreLayerCollision(8, 10, true);
        enemies = new EnemyController[poolSize];
        for (int i = 0; i < enemies.Length; i++)
        {
            enemies[i] = Instantiate(prefab, transform);
            enemies[i].game = game;
            enemies[i].target = player.transform;
            enemies[i].ReturnToPool();
        }
    }

    public void SpawnWave(int wave)
    {
        if (wave == 1) SpawnCircle();
        else if (wave == 2) SpawnGrid();
        else SpawnRows();
    }

    void SpawnCircle()
    {
        float angle = 0f;
        float angleOffset = 2f * Mathf.PI / enemies.Length;
        float radius = 4f;
        for (int i = 0; i < enemies.Length; i++)
        {
            float x = Mathf.Sin(angle) * radius;
            float y = Mathf.Cos(angle) * radius;
            enemies[i].Spawn(new Vector2(x, y));
            angle += angleOffset;
        }
    }

    void SpawnGrid()
    {
        for (int i = 0; i < enemies.Length; i++)
        {
            float x = -5f + (i % 6) * 2f;
            float y = 3.8f - (i / 6) * 1.2f;
            enemies[i].Spawn(new Vector2(x, y));
        }
    }

    void SpawnRows()
    {
        for (int i = 0; i < enemies.Length; i++)
        {
            float x = -5f + (i % 6) * 2f;
            float y = 3.5f;
            if (i >= 6) y = -3.5f;
            enemies[i].Spawn(new Vector2(x, y));
        }
    }

    public void ResetPool()
    {
        for (int i = 0; i < enemies.Length; i++)
        {
            enemies[i].ReturnToPool();
        }
    }
}
