using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float fireInterval = 0.2f;
    public MissileManager missileManager;
    public GameManager game;
    public Transform visual;
    public HPController health;
    Rigidbody2D body;
    SpriteRenderer sprite;
    Vector2 move;
    Vector2 aim = Vector2.up;
    float nextShot;
    float invincibleUntil;

    void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        sprite = visual.GetComponent<SpriteRenderer>();
    }

    public void ResetPlayer()
    {
        body.position = Vector2.zero;
        body.linearVelocity = Vector2.zero;
        health.SetHP(100f);
        invincibleUntil = 0f;
        nextShot = 0f;
        sprite.color = Color.white;
    }

    void Update()
    {
        move = Vector2.zero;
        if (!game.IsPlaying) return;
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null)
        {
            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) move.x += 1f;
            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) move.x -= 1f;
            if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) move.y += 1f;
            if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) move.y -= 1f;
        }
        move = move.normalized;
        Mouse mouse = Mouse.current;
        if (mouse != null)
        {
            Vector2 point = Camera.main.ScreenToWorldPoint(mouse.position.ReadValue());
            if ((point - body.position).sqrMagnitude > 0.01f) aim = (point - body.position).normalized;
        }
        bool fire = false;
        if (mouse != null && mouse.leftButton.isPressed) fire = true;
        if (keyboard != null && keyboard.spaceKey.isPressed) fire = true;
        if (fire && Time.time >= nextShot)
        {
            if (missileManager.Fire(body.position + aim * 0.65f, aim)) nextShot = Time.time + fireInterval;
        }
        sprite.flipX = aim.x < 0f;
    }

    void FixedUpdate()
    {
        if (!game.IsPlaying) return;
        Vector2 position = body.position + move * moveSpeed * Time.fixedDeltaTime;
        position.x = Mathf.Clamp(position.x, -8.7f, 8.7f);
        position.y = Mathf.Clamp(position.y, -4.5f, 4.5f);
        body.MovePosition(position);
    }

    public void TakeDamage(float damage)
    {
        if (!game.IsPlaying || Time.time < invincibleUntil) return;
        health.TakeDamage(damage);
        invincibleUntil = Time.time + 0.8f;
        if (health.IsDead) game.Finish(false);
    }
}
