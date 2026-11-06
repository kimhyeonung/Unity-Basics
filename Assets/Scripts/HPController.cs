using UnityEngine;
using UnityEngine.UI;

namespace DefaultNamespace
{
    public class HPController : MonoBehaviour
    { 
        [SerializeField]
       private Slider hpSlider;
       public bool IsDead => currentHP <= 0;
       public bool IsAlive => currentHP > 0;
       private float currentHP;
       private float maxHP;

       public void Awake()
       {
           if (hpSlider == null)
           {
               hpSlider = GetComponentInChildren<Slider>();
           }

           SetMaxHP(100);
       }

       public void SetMaxHP(float maxHP)
       {
           this.maxHP = maxHP;
           currentHP = maxHP;
       }
       
       public void TakeDamage(float damage)
       {
           currentHP -= damage;
           
           if(currentHP <= 0)  currentHP = 0;
           
       }

        void FixedUpdate()
        {
            hpSlider.value = currentHP/maxHP;
            hpSlider.transform.rotation = Quaternion.identity;

        } 
    }
}