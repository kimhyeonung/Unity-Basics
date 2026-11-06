using UnityEngine;

namespace FWeapon
{
    public static class FirePatternFactory
    {
        public static IFirePattern CreateFirePattern(FirePatternType firePatternType)
        {
            return firePatternType switch
            {
                FirePatternType.Spread => new FirePattern_Spread(),
                FirePatternType.Burst => new FirePattern_Burst(),
                FirePatternType.Normal => new FirePattern_Normal(),
                FirePatternType.Weave => new FirePattern_Weave(),
                _ => Log(firePatternType),
            };
        }

        static IFirePattern Log(FirePatternType firePatternType)
        {
            Debug.LogError("FirePatternFactory::Log"+firePatternType);
            return null;
        }
        
    }
}
