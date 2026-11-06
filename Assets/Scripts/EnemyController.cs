 
using System;
using System.Collections;
using DefaultNamespace; 
using UnityEngine;
using FWeapon;
  
public class EnemyController : MonoBehaviour
{
    // public PlayerController playerController;
    private float moveSpeed = 0;
    private Vector3 moveDirection;
    private Rigidbody2D rigidbody2D;
    private Transform target = null;
    private SpriteRenderer spriteRenderer = null;
    public HPController hpController;
    public float maxHP = 100;
    
    private Color originalColor;
    private Vector3 originalScale;
    
   //public 펑션껍데기; //몸뚱이가 없어요..껍데기만 있어요 또는 이름만 있어요.
    public   event Action <Color, Vector3,Vector3>OnDied;
    public event Action<Color, Vector3,Vector3> OnHit;

    void Awake()
    { 
        
        if (rigidbody2D == null) rigidbody2D = GetComponent<Rigidbody2D>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (hpController == null) hpController = GetComponentInChildren<HPController>();
        
        originalScale = spriteRenderer.transform.localScale;
        originalColor = spriteRenderer.color; 
    }  
    
    public void SetFillFullHP()
    {
        hpController.SetMaxHP(maxHP);
    }  
    public void SetTarget(Transform target)
    {
        this.target= target;
    }
 
    public void SetColor(Color color)
    {
        spriteRenderer.color = color; 
        originalColor = color; 
    }

    public void SetMoveSpeed(float speed)
    {
        this.moveSpeed = speed;
    }

    void MoveToTarget()
    {
        moveDirection = target.position - transform.position;
        moveDirection.Normalize();
      
        rigidbody2D.MovePosition(transform.position + moveDirection * moveSpeed * Time.fixedDeltaTime); 
    }

    void Die(  Vector3 hitDirection)
    {
        Debug.Log("EnemyController Die"); 
        gameObject.SetActive(false);
        
        
        //펑션껍데기.실행()
        OnDied?.Invoke(originalColor, transform.position, hitDirection);
    }  
    
    void FixedUpdate()
    { 
        if (target == null) return;

        MoveToTarget();
    }

    void  Hit(float damage , Vector3 hitDirection)
    {
        StartCoroutine(HitFlash()); // 내 캐릭이 맞는 애니
        
        //부스러기가 떨어지는 이팩트처리
        
        OnHit?.Invoke(originalColor, transform.position ,hitDirection );
        
         hpController.TakeDamage( damage); 
         
         if (hpController.IsDead)
         {
             Die(hitDirection); 
         } 
    }

    IEnumerator  HitFlash()
    {
        spriteRenderer.color = Color.red;
        transform.localScale = originalScale * 1.07f;
        
        yield return new WaitForSeconds(0.05f);
        
        spriteRenderer.color = originalColor;
        transform.localScale = originalScale;

        yield  break;
    }

    private void OnCollisionEnter2D(Collision2D other) // 충돌처리....
    {
        //1.방법 슬쩍 먼저 묻는다. 너 미사일 컨트롤러 붙어있어?
        // var missile = other.gameObject.GetComponent<MissileController>(); 
        // if (missile == null) return;

        //2.방법 : TAG를 확인하자
        if (other.gameObject.CompareTag("Enemy") || other.gameObject.CompareTag("Player")) return;

        moveDirection.Normalize();

        var missle = other.gameObject.GetComponent<ProjectileController>();
        if (missle != null)
        {
             Hit(missle.Damage, missle.Direction);
        }
    }
}
