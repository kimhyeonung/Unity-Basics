using UnityEngine;

namespace FWeapon
{
    public class ProjectileControllerThreadloom : ProjectileController
    {
        public enum FlightPhase
        {
            Ready,
            Weaving,
            Orbiting,
            Returning
        }
        public FlightPhase Phase
        {
            get; private set;
        }
        public int CompletedCycles
        {
            get; private set;
        }
        ThreadloomDefinition settings;
        TrailRenderer thread;
        Vector3 origin;
        Vector3 forward;
        Vector3 side;
        Vector3 orbitCenter;
        Vector3 returnStart;
        float age;
        float outboundDuration;
        float travelDistance;
        public override void Build(WeaponDefinition definition, ProjectilePool pool)
        {
            base.Build(definition, pool);
            settings = definition as ThreadloomDefinition;
            thread = GetComponent<TrailRenderer>();
            travelDistance = Mathf.Max(1, definition.Range);
            outboundDuration = travelDistance / Mathf.Max(0.1f, moveSpeed);
            if (thread != null)
            {
                thread.time = settings != null ? settings.trailDuration : 0.42f;
                thread.startColor = definition.color;
                thread.endColor = new Color(definition.color.r, definition.color.g, definition.color.b, 0);
                thread.emitting = false;
            }
        }
        public override void Fire(Vector3 position, Vector3 direction, int generation = 0)
        {
            if (flying || direction.sqrMagnitude < 0.001f)
            {
                return;
            }
            base.Fire(position, direction.normalized, generation);
            origin = position;
            forward = direction.normalized;
            side = new Vector3(-forward.y, forward.x, 0);
            age = 0;
            Phase = FlightPhase.Weaving;
            float radius = settings != null ? settings.orbitRadius : 1.25f;
            orbitCenter = origin + forward * (travelDistance - radius);
            if (thread != null)
            {
                thread.Clear();
                thread.emitting = true;
            }
        }
        protected override void Move()
        {
            if (!flying)
            {
                return;
            }
            age += Time.deltaTime;
            Vector3 previous = transform.position;
            float amplitude = settings != null ? settings.weaveAmplitude : 0.65f;
            float orbitDuration = settings != null ? settings.orbitDuration : 1.2f;
            float returnDuration = settings != null ? settings.returnDuration : 0.75f;
            if (age < outboundDuration)
            {
                float t = age / outboundDuration;
                transform.position = origin + forward * (travelDistance * t) + side * (Mathf.Sin(t * Mathf.PI * 4) * amplitude * Mathf.Sin(t * Mathf.PI));
            }
            else if (age < outboundDuration + orbitDuration)
            {
                Phase = FlightPhase.Orbiting;
                float t = (age - outboundDuration) / orbitDuration;
                float turns = settings != null ? settings.orbitTurns : 1.5f;
                float radius = settings != null ? settings.orbitRadius : 1.25f;
                float angle = t * Mathf.PI * 2 * turns;
                transform.position = orbitCenter + (forward * Mathf.Cos(angle) + side * Mathf.Sin(angle)) * radius;
                returnStart = transform.position;
            }
            else
            {
                if (Phase != FlightPhase.Returning)
                {
                    returnStart = transform.position;
                    Phase = FlightPhase.Returning;
                }
                float t = Mathf.Clamp01((age - outboundDuration - orbitDuration) / returnDuration);
                transform.position = Vector3.Lerp(returnStart, origin, Mathf.SmoothStep(0, 1, t)) + side * (Mathf.Sin(t * Mathf.PI) * amplitude);
                if (t >= 1)
                {
                    CompletedCycles++;
                    ReturnToPool();
                    return;
                }
            }
            Vector3 velocity = transform.position - previous;
            if (velocity.sqrMagnitude > 0.000001f)
            {
                moveDirection = velocity.normalized;
                transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg);
            }
        }
        public override void ReturnToPool()
        {
            if (thread != null)
            {
                thread.emitting = false;
                thread.Clear();
            }
            age = 0;
            Phase = FlightPhase.Ready;
            base.ReturnToPool();
        }
    }
}
