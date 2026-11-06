using System.Collections.Generic;
using UnityEngine;

namespace DefaultNamespace
{
    public class BloodEffectPool : MonoBehaviour
    {
        public enum BloodEffectType
        {
            small=0,
            large=1
        }
        
        private List<BloodEffectController> bloodEffects = new();
        [SerializeField] private BloodEffectController bloodEffectPrefab;
        [SerializeField] private int maxBloodEffectsCount= 100;

        private const int burstCountHigh = 8;
        private const int burstCountLow = 2;
        
        void Awake()
        {

            if (bloodEffectPrefab == null)
            {
                Debug.LogError("BloodEffectPool needs a BloodEffectPrefab");

                return;
            }
            
            for (int i = 0; i < maxBloodEffectsCount; i++)
            {
                var BloodEffectController = Instantiate(bloodEffectPrefab, transform);
                BloodEffectController.gameObject.SetActive(false);
                bloodEffects.Add(BloodEffectController);
            }
        }


     public void Show(Color color, Vector3 position, Vector3 direction, BloodEffectType type  )
     {

         int burstCount = 1;
         
         if (type == BloodEffectType.small)
         {
             burstCount = burstCountLow;
         }
         else burstCount = burstCountHigh;
         
         
         int count = 0;
            foreach (var effect in bloodEffects)
            {
                if(effect.gameObject.activeSelf) continue;
                effect.gameObject.SetActive(true);
                effect.Show(color, position, direction);
                count++;
                
                if(count >= burstCount) return;
            }
            
            //
        }
    }
}