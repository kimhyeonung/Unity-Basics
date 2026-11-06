using System.Collections.Generic;
using DefaultNamespace;
using UnityEngine;

namespace FWeapon
{
    public class ProjectilePool : MonoBehaviour
    {
        public List<ProjectileController> missiles = new();

        public void Build(WeaponDefinition def, int maxCount)
        {
            for (int i = 0; i < maxCount; i++)
            {

                if (def.projectilePrefab == null)
                {
                    Debug.LogError("MissilePool  WeaponDefinition missilePrefab is null");
                    return;
                }

                var m = Instantiate(def.projectilePrefab, transform);
                m.Build(def,this); // 미사일 데이터로 세팅
                m.gameObject.SetActive(false);
                missiles.Add(m);
            }
        }

        public void FireProjectile(Vector3 startPos, Vector3 direction, int generation = 0)
        {
            foreach (var m in missiles)
            {
                if (m.flying) continue;
                m.gameObject.SetActive(true);
                m.Fire(startPos, direction,generation);
                break;
            }
        }

    }
}
