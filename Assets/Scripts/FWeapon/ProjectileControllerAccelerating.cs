using UnityEngine;
namespace FWeapon
{
    public class ProjectileControllerAccelerating : ProjectileController
    {
        float initialSpeed;
        public override void Build(WeaponDefinition definition, ProjectilePool pool)
        {
            base.Build(definition, pool);
            initialSpeed = definition.Speed;
        }
        public override void Fire(Vector3 position, Vector3 direction, int generation = 0)
        {
            if (flying || direction.sqrMagnitude == 0) return;
            moveSpeed = initialSpeed;
            base.Fire(position, direction.normalized, generation);
        }
        protected override void Move()
        {
            if (!flying) return;
            moveSpeed = Mathf.Min(moveSpeed + 12f * Time.deltaTime, 20f);
            base.Move();
        }
        public override void ReturnToPool()
        {
            moveSpeed = initialSpeed;
            base.ReturnToPool();
        }
    }
}
