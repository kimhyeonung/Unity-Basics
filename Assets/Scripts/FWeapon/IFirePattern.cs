using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

namespace FWeapon
{
    public interface IFirePattern
    {
        public IEnumerator  Fire(ProjectilePool weapon,Transform startPos, Vector3 direction,int BurstCount=1);
    }

    public class FirePattern_Normal : IFirePattern
    { 
        public IEnumerator Fire(ProjectilePool weapon,Transform startPos, Vector3 direction,int BurstCount=1)
        {
            weapon.FireProjectile(startPos.position, direction);

            yield break;
        } 
    }

    public class FirePattern_Spread : IFirePattern
    {
        public  IEnumerator Fire(ProjectilePool weapon, Transform startPos, Vector3 direction, int burstCount=1)
        { 
            // 1. 1발이면   0 
            // 2. 2발이면  -10, +10
            // 3. 3발이면 -10, 0, 10
            // 4. 4발이면 -20, -10, 10, 20
            // 4. 5발이면 -20, -10, 0, 10, 20

            for (int i = 0; i < burstCount; i++)
            {
                float angle = 0f;

                if (burstCount % 2 != 0) 
                {
                    // 1. 홀수 발수 (1, 3, 5...) -> 0도 중심 정렬
                    angle = (i - (burstCount - 1) / 2f) * 10f;
                }
                else 
                {
                    // 2. 짝수 발수 (2, 4...) -> 0도를 건너뛰고 좌우 대칭 (-10, 10 중심)
                    int half = burstCount / 2;
                    angle = (i < half) ? (i - half) * 10f : (i - half + 1) * 10f;
                }

                // direction 벡터를 Z축 기준으로 angle만큼 회전
                Vector3 spread = Quaternion.Euler(0, 0, angle) * direction;
                weapon.FireProjectile(startPos.position, spread); 
            }
    
            yield break;
        }
    }

    public class FirePattern_Burst : IFirePattern
    {
        public IEnumerator Fire(ProjectilePool weapon,Transform startPos, Vector3 direction,int BurstCount=1) 
        {
            for (int i = 0; i < BurstCount; i++)
            {
                weapon.FireProjectile(startPos.position, direction);
                
                 yield return new WaitForSeconds(0.07f); 
            }
        } 
    }
}