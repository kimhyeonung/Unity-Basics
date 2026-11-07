using System.Collections;
using UnityEngine;
namespace FWeapon
{
    public class FirePattern_Triple : IFirePattern
    {
        public IEnumerator Fire(ProjectilePool pool, Transform muzzle, Vector3 direction, int burstCount = 1)
        {
            if (direction.sqrMagnitude == 0) yield break;
            for (int i = -1; i <= 1; i++)
            pool.FireProjectile(muzzle.position, Quaternion.Euler(0, 0, i * 15f) * direction.normalized);
            yield break;
        }
    }
}
