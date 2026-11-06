using System.Collections;
using UnityEngine;

namespace FWeapon
{
    public class FirePattern_Weave : IFirePattern
    {
        public IEnumerator Fire(ProjectilePool weapon, Transform startPos, Vector3 direction, int burstCount = 1)
        {
            if (weapon == null || startPos == null || direction.sqrMagnitude < 0.001f)
            {
                yield break;
            }

            int count = Mathf.Clamp(burstCount, 1, 5);
            Vector3 origin = startPos.position;
            Vector3 forward = direction.normalized;
            Vector3 side = new Vector3(-forward.y, forward.x, 0);
            for (int i = 0; i < count; i++)
            {
                float lane = i - (count - 1) * 0.5f;
                Vector3 offset = side * lane * 0.16f;
                Vector3 heading = Quaternion.Euler(0, 0, lane * 11f) * forward;
                weapon.FireProjectile(origin + offset, heading);
                if (i < count - 1)
                {
                    yield return new WaitForSeconds(0.09f);
                }
            }
        }
    }
}
