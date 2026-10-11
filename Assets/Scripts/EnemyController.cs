using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public HPController health;
    public float moveSpeed = 1f;
    public GameManager game;
    public Transform target;
    Rigidbody2D body;
    float startMoving;
    float knockbackUntil;

    void Awake()
    {
        body = GetComponent<Rigidbody2D>();
    }

    public void Spawn(Vector2 position)
    {
        gameObject.SetActive(true);
        body.position = position;
        body.linearVelocity = Vector2.zero;
        health.SetHP(50f);
        startMoving = Time.time + 1.5f;
        knockbackUntil = 0f;
    }

    void FixedUpdate()
    {
        if (!game.IsPlaying || target == null || Time.time < startMoving || Time.time < knockbackUntil) return;
        Vector2 direction = ((Vector2)target.position - body.position).normalized;
        body.linearVelocity = direction * moveSpeed;
    }

    void OnTriggerStay2D(Collider2D other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null) player.TakeDamage(12f);
    }

    public void TakeDamage(float damage, Vector2 direction)
    {
        if (health.IsDead) return;
        health.TakeDamage(damage);
        body.linearVelocity = Vector2.zero;
        body.AddForce(direction * 3f, ForceMode2D.Impulse);
        knockbackUntil = Time.time + 0.15f;
        if (!health.IsDead) return;
        game.AddScore(10);
        ReturnToPool();
    }

    public void ReturnToPool()
    {
        if (body != null) body.linearVelocity = Vector2.zero;
        gameObject.SetActive(false);
    }
}
