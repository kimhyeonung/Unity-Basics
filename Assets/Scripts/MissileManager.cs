using UnityEngine;

public class MissileManager : MonoBehaviour
{
    public MissileController prefab;
    public int poolSize = 40;
    MissileController[] missiles;
    public int PoolSize => missiles.Length;

    void Awake()
    {
        missiles = new MissileController[poolSize];
        for (int i = 0; i < missiles.Length; i++)
        {
            missiles[i] = Instantiate(prefab, transform);
            missiles[i].ReturnToPool();
        }
    }

    public bool Fire(Vector2 position, Vector2 direction)
    {
        foreach (MissileController missile in missiles)
        {
            if (!missile.IsReady) continue;
            missile.Fire(position, direction);
            return true;
        }
        return false;
    }

    public void ResetPool()
    {
        foreach (MissileController missile in missiles) missile.ReturnToPool();
    }
}
