using System.Collections.Generic;
using UnityEngine;

namespace FWeapon
{
    [CreateAssetMenu(fileName = "WeaponLibrary", menuName = "CK_Unity/WeaponLibrary")]
    public class WeaponLibrary :ScriptableObject
    {
        public List<WeaponDefinition> Weaponlib;
        public int Count => Weaponlib.Count;

      /*  public WeaponDefinition GetWeapon(string name)
        {
            return 
        }
*/
    }
}