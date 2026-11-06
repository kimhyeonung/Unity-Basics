using DefaultNamespace;
using UnityEngine;

namespace FWeapon
{
    public class ProjectileController : MonoBehaviour
    {
        // Start is called once before the first execution of Update after the MonoBehaviour is created


        protected float moveSpeed = 10f;
        protected float baseDamage = 10;
        protected bool IsPiercing = false;
        
        protected ProjectilePool ownerPool;

        protected int generation;
        public float Damage => baseDamage;
        protected bool fired = false;
        public bool flying => fired; // 발사되었냐? 날아가고 있냐?
        public bool readyToMove => !fired; // 아직 발사안된거 맞지? 지금 장전되어있지?


        protected Vector3 moveDirection = Vector3.zero;
        public Vector3 Direction => moveDirection;


        public virtual void Build(WeaponDefinition def , ProjectilePool projectilePool)
        {

            moveSpeed = def.Speed;
            baseDamage = def.Damage;
            IsPiercing = def.isPiercing;
            ownerPool = projectilePool;
            generation = 0;

            var spriteRenderer = GetComponentInChildren<SpriteRenderer>();

            if (spriteRenderer == null)
            {
                Debug.LogError("MissileController : " + "SpriteRenderer not found");

                return;
            }

            
            spriteRenderer.color = def.color;

            if (def.sprite != null) spriteRenderer.sprite = def.sprite;

        }

        // Update is called once per frame
        protected virtual void Update()
        {
            Move();
            if (ScreenWorldBounds.IsOutside(transform.position, 3f) == true)
            {
                ReturnToPool();
            }

        }

        protected virtual void Move()
        {
            transform.position += moveDirection * moveSpeed * Time.deltaTime;
        }

        public virtual void Fire(Vector3 firePosition, Vector3 fireDirection, int generation=0)
        {
            if (flying) return; //미사일 발사되었어?

            if (fireDirection == Vector3.zero) return;

            gameObject.SetActive(true);

            transform.position = firePosition;
            moveDirection = fireDirection;
            this.generation = generation;
            fired = true;
        }

        public virtual void ReturnToPool()
        {
            gameObject.SetActive(false);
            fired = false;
            generation = 0;
        }

        protected virtual void OnCollisionEnter2D(Collision2D other) // 충돌처리....
        { 
            if (other.gameObject.CompareTag("Enemy"))
            {
                if (!IsPiercing) ReturnToPool();
            } 

        }
    }
}
