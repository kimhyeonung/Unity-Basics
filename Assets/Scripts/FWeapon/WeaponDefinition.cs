using UnityEngine;
using UnityEngine.Serialization;

namespace FWeapon
{ 
    [CreateAssetMenu(fileName = "WeaponDefinition", menuName = "CK_Unity/WeaponDefinition")]
    public class WeaponDefinition : ScriptableObject
    {
        public string weaponID = "WID_";
        public  WeaponType weaponType;
        public  FirePatternType firePatternType;
        public Sprite sprite;
        public int BurstCount= 1;
        public float Damage;
        public float Speed;
        public float Range;
        public float cooldown;
        public bool isPiercing;
        public Color color;
        
        [FormerlySerializedAs("missilePrefab")] public ProjectileController projectilePrefab;
    }
}