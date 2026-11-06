using UnityEngine;

namespace FWeapon
{
    [CreateAssetMenu(fileName = "ThreadloomDefinition", menuName = "CK_Unity/Threadloom")]
    public class ThreadloomDefinition : WeaponDefinition
    {
        [Min(0.1f)]
        public float weaveAmplitude = 0.65f;
        [Min(0.1f)]
        public float orbitRadius = 1.25f;
        [Min(0.1f)]
        public float orbitDuration = 1.2f;
        [Min(0.1f)]
        public float orbitTurns = 1.5f;
        [Min(0.1f)]
        public float returnDuration = 0.75f;
        [Min(0.05f)]
        public float trailDuration = 0.42f;
    }
}
