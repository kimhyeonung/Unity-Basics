using UnityEngine;

public class MissileController : MonoBehaviour
{
    public float damage = 25f;
    public float moveSpeed = 14f;
    public bool IsFlying { get; private set; }
    public bool IsReady => !IsFlying;
    Rigidbody2D body;
    float expires;

    void Awake()
    {
        body = GetComponent<Rigidbody2D>();
    }

    public void Fire(Vector2 position, Vector2 direction)
    {
        if (direction.sqrMagnitude < 0.001f) direction = Vector2.up;
        gameObject.SetActive(true);
        body.position = position;
        transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f);
        body.linearVelocity = direction.normalized * moveSpeed;
        expires = Time.time + 1.6f;
        IsFlying = true;
    }

    void Update()
    {
        if (Time.time >= expires) ReturnToPool();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsFlying) return;
        EnemyController enemy = other.GetComponent<EnemyController>();
        if (enemy == null || enemy.health.IsDead) return;
        enemy.TakeDamage(damage, body.linearVelocity.normalized);
        ReturnToPool();
    }

    public void ReturnToPool()
    {
        IsFlying = false;
        if (body != null) body.linearVelocity = Vector2.zero;
        gameObject.SetActive(false);
    }
}
