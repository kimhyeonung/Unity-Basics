using UnityEngine;

namespace FWeapon
{
    public class ProjectileControllerSplit
        : ProjectileController
    {
        [Header("Split")]
        [SerializeField]
        private int splitCount = 2;

        [SerializeField]
        private float splitAngle = 50f;

        [SerializeField]
        private int maxGeneration = 1;


        protected override void OnCollisionEnter2D(
            Collision2D other)
        {
            if (!other.gameObject.CompareTag("Enemy"))
                return;


            if (generation < maxGeneration)
            {
                Split();
            }


            ReturnToPool();
        }


        private void Split()
        {
            if (ownerPool == null)
                return;


            if (splitCount <= 0)
                return;


            // 한 발이면 현재 방향 그대로 발사
            if (splitCount == 1)
            {
                ownerPool.FireProjectile(transform.position, moveDirection, generation + 1);
                return;
            }


            float startAngle = -splitAngle * 0.5f;

            float angleStep = splitAngle / (splitCount - 1);


            for (int i = 0; i < splitCount; i++)
            {
                float angle = startAngle + angleStep * i;


                Vector3 splitDirection = Quaternion.Euler(0f, 0f, angle) * moveDirection;


                ownerPool.FireProjectile(transform.position, splitDirection, generation + 1);
            }
        }
    }
}