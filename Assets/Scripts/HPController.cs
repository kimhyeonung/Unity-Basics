using UnityEngine;
using UnityEngine.UI;

public class HPController : MonoBehaviour
{
    public Slider hpSlider;
    public float HP { get; private set; }
    public float MaxHP { get; private set; }
    public bool IsDead => HP <= 0f;

    public void SetHP(float value)
    {
        MaxHP = Mathf.Max(1f, value);
        HP = MaxHP;
        Refresh();
    }

    public void TakeDamage(float damage)
    {
        HP = Mathf.Max(0f, HP - Mathf.Max(0f, damage));
        Refresh();
    }

    void Refresh()
    {
        if (hpSlider != null) hpSlider.value = HP / MaxHP;
    }
}
