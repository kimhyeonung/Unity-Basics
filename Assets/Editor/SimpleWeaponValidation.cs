using System;
using System.Linq;
using FWeapon;
using UnityEditor;
using UnityEngine;
public static class SimpleWeaponValidation
{
    [MenuItem("Threadloom/Validate Simple Weapons")]
    public static void Run()
    {
        var library = AssetDatabase.LoadAssetAtPath<WeaponLibrary>("Assets/Threadloom/Weapons/ThreadloomLibrary.asset");
        var root = new GameObject("Weapon validation");
        try
        {
            if (library.Count != 3) throw new Exception("Expected three weapons.");
            int[] expected =
            {
                1, 3, 1
            };
            for (int i = 0; i < 3; i++)
            {
                var pool = new GameObject("Pool").AddComponent<ProjectilePool>();
                pool.transform.SetParent(root.transform);
                var definition = library.Weaponlib[i];
                pool.Build(definition, 20);
                var routine = FirePatternFactory.CreateFirePattern(definition.firePatternType).Fire(pool, root.transform, Vector3.right, definition.BurstCount);
                while (routine.MoveNext())
                {
                }
                if (pool.missiles.Count(p => p.flying) != expected[i]) throw new Exception("Wrong shot count: " + definition.weaponID);
                if (i == 2)
                {
                    var shot = pool.missiles[0];
                    var speed = typeof(ProjectileController).GetField("moveSpeed", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                    speed.SetValue(shot, 20f);
                    shot.ReturnToPool();
                    shot.Fire(Vector3.zero, Vector3.right);
                    if ((float)speed.GetValue(shot) != definition.Speed) throw new Exception("Pooled acceleration did not reset.");
                }
            }
            Debug.Log("SIMPLE WEAPON VALIDATION PASSED: 1/3/1 shots, three pools, acceleration reset.");
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(root);
        }
    }
}
