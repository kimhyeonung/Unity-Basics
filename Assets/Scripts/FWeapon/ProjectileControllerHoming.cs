using UnityEngine;

namespace FWeapon
{
    public class ProjectileControllerHoming : ProjectileController
    {
        [Header("Target")] [SerializeField] private float searchRange = 20f;

        [SerializeField] private LayerMask enemyLayer;


        [Header("Homing")] [SerializeField] private float maxTurnSpeed = 420f;

        [SerializeField] private float initialAngleRange = 90f;


        [Header("Curve")] [SerializeField] private float curveDuration = 1.2f;

        [SerializeField] private AnimationCurve speedCurve = AnimationCurve.EaseInOut(0f, 0.3f, 1f, 1f);

        [SerializeField] private AnimationCurve turnCurve = AnimationCurve.EaseInOut(0f, 0.1f, 1f, 1f);


        private Transform target;
        private float homingTime = 0f;

        private TrailRenderer trailRenderer;


        void Awake()
        {


            if (trailRenderer == null)
            {
                trailRenderer = GetComponent<TrailRenderer>();
            }

        }


        public override void Fire(Vector3 firePosition, Vector3 fireDirection, int generation=0)
        {

            base.Fire(firePosition, fireDirection);

            if (!flying)
                return;

            homingTime = 0f;
            target = FindNearestTarget();

            float randomAngle = Random.Range(-initialAngleRange, initialAngleRange);
            moveDirection = Quaternion.Euler(0f, 0f, randomAngle) * moveDirection;
            moveDirection.Normalize();

            if (trailRenderer != null)
            {
                trailRenderer.Clear();
                trailRenderer.enabled = true;
            }
        }


        protected override void Move()
        {
            homingTime += Time.deltaTime;


            float normalizedTime = Mathf.Clamp01(homingTime / curveDuration);
            float speedMultiplier = speedCurve.Evaluate(normalizedTime);
            float currentSpeed = moveSpeed * speedMultiplier;

            if (target == null || !target.gameObject.activeInHierarchy)
            {
                target = FindNearestTarget();
            }

            if (target != null)
            {
                Vector3 targetDirection = target.position - transform.position;
                targetDirection.Normalize();


                float turnMultiplier = turnCurve.Evaluate(normalizedTime);
                float currentTurnSpeed = maxTurnSpeed * turnMultiplier;
                float maxRadiansDelta = currentTurnSpeed * Mathf.Deg2Rad * Time.deltaTime;


                moveDirection = Vector3.RotateTowards(moveDirection, targetDirection, maxRadiansDelta, 0f);
                moveDirection.Normalize();
            }


            transform.position += moveDirection * currentSpeed * Time.deltaTime;
            float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }



        private Transform FindNearestTarget()
        {
            Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, searchRange, enemyLayer);
            Transform nearest = null;

            float nearestDistance = float.MaxValue;


            foreach (Collider2D enemy in enemies)
            {
                float distance = Vector2.Distance(transform.position, enemy.transform.position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearest = enemy.transform;
                }
            }

            return nearest;
        }

        public override void ReturnToPool()
        {
            target = null;
            homingTime = 0f;

            if (trailRenderer != null)
            {
                trailRenderer.Clear();
                trailRenderer.enabled = false;
            }


            base.ReturnToPool();
        }
    }
}