using System;
using System.IO;
using FWeapon;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class ThreadloomSceneBuilder
{
    const string Folder = "Assets/Threadloom";
    static Material spriteMaterial;
    static Material threadMaterial;
    static Sprite square;
    static Sprite ring;
    static Sprite needle;
    static Sprite drone;
    static bool updateOnly;
    [MenuItem("Threadloom/Apply Simple Weapons")]
    public static void ApplySimpleWeapons()
    {
        updateOnly = true;
        try
        {
            Generate();
        }
        finally
        {
            updateOnly = false;
        }
    }
    [MenuItem("Threadloom/Build Assignment Scene")]
    public static void Generate()
    {
        Directory.CreateDirectory(Folder + "/Art");
        Directory.CreateDirectory(Folder + "/Prefabs");
        Directory.CreateDirectory(Folder + "/Weapons");
        spriteMaterial = MaterialAsset("Sprite", "Universal Render Pipeline/2D/Sprite-Unlit-Default");
        threadMaterial = MaterialAsset("Thread", "Sprites/Default");
        square = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        ring = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        needle = square;
        drone = square;
        var basicPrefab = CreateBasic();
        var accelerationPrefab = CreateAccelerating(basicPrefab);
        var loom = Definition<WeaponDefinition>("BasicGun", "BASIC GUN", basicPrefab, FirePatternType.Normal, 1, 30, 10, 20, 0.3f, Color.cyan);
        var burst = Definition<WeaponDefinition>("Shotgun", "SHOTGUN", basicPrefab, FirePatternType.Triple, 3, 20, 9, 20, 0.7f, new Color(1, 0.65f, 0.2f));
        var fan = Definition<WeaponDefinition>("AcceleratingGun", "ACCELERATING GUN", accelerationPrefab, FirePatternType.Normal, 1, 45, 3, 20, 0.6f, new Color(0.7f, 0.5f, 1));
        var library = Asset<WeaponLibrary>(Folder + "/Weapons/ThreadloomLibrary.asset");
        library.Weaponlib = new System.Collections.Generic.List<WeaponDefinition>
        {
            loom,
            burst,
            fan
        };
        EditorUtility.SetDirty(library);
        if (updateOnly)
        {
            AssetDatabase.SaveAssets(); Debug.Log("SIMPLE WEAPONS APPLIED"); return;
        }
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var camera = new GameObject("Main Camera").AddComponent<Camera>();
        camera.tag = "MainCamera";
        camera.transform.position = new Vector3(0, 0, -10);
        camera.orthographic = true;
        camera.orthographicSize = 6;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.025f, 0.045f, 0.08f);
        Grid();
        var canvasObject = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler));
        var canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720);
        scaler.matchWidthOrHeight = 0.5f;
        var indicators = new Image[3];
        for (int i = 0; i < indicators.Length; i++)
        indicators[i] = Indicator(canvas.transform, "Weapon_" + i, new Vector2(50 + i * 55, -40), new Vector2(40, 40), Color.white);
        var player = SpriteObject("Player", ring, new Vector3(-6, 0, 0), new Vector3(0.8f, 0.8f, 1), new Color(0.4f, 1, 0.95f));
        player.tag = "Player";
        var pointer = new GameObject("Aim").transform;
        pointer.SetParent(player.transform, false);
        var pointerVisual = SpriteObject("NeedlePointer", needle, Vector3.zero, new Vector3(0.7f, 0.7f, 1), new Color(0.7f, 1, 1));
        pointerVisual.transform.SetParent(pointer, false);
        pointerVisual.transform.localPosition = new Vector3(0.55f, 0, 0);
        var muzzle = new GameObject("Muzzle").transform;
        muzzle.position = player.transform.position + Vector3.right * 0.55f;
        muzzle.SetParent(player.transform, true);
        var weaponObject = new GameObject("WeaponSystem");
        var weapon = weaponObject.AddComponent<WeaponController>();
        weapon.projectilePoolPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/MissilePool.prefab").GetComponent<ProjectilePool>();
        var serialized = new SerializedObject(weapon);
        serialized.FindProperty("weaponLibList").objectReferenceValue = library;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        var arena = new GameObject("ThreadloomArena").AddComponent<ThreadloomArena>();
        arena.weapon = weapon;
        arena.library = library;
        arena.player = player.transform;
        arena.pointer = pointer;
        arena.muzzle = muzzle;
        arena.weaponIndicators = indicators;
        var positions = new[]
        {
            new Vector3(-2.8f, 0, 0),
            new Vector3(-0.4f, 1.3f, 0),
            new Vector3(-0.4f, -1.3f, 0),
            new Vector3(1.8f, 0, 0),
            new Vector3(3.8f, 2.5f, 0),
            new Vector3(3.8f, -2.5f, 0),
            new Vector3(6.8f, 0, 0)
        };
        var enemyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemy.prefab").GetComponent<EnemyController>();
        for (int i = 0; i < positions.Length; i++)
        {
            var enemy = (EnemyController)PrefabUtility.InstantiatePrefab(enemyPrefab);
            enemy.name = "Target_" + (i + 1).ToString("00");
            enemy.transform.position = positions[i];
            enemy.transform.localScale = Vector3.one * 0.9f;
            enemy.maxHP = 90;
            var renderer = enemy.GetComponent<SpriteRenderer>();
            renderer.sprite = drone;
            renderer.drawMode = SpriteDrawMode.Sliced;
            renderer.size = Vector2.one;
            renderer.sharedMaterial = spriteMaterial;
            renderer.color = new Color(0.94f, 0.3f, 0.52f);
            renderer.sortingOrder = 2;
            foreach (var image in enemy.GetComponentsInChildren<Image>(true))
            image.sprite = null;
            foreach (var child in enemy.GetComponentsInChildren<SpriteRenderer>())
            {
                if (child != renderer)
                {
                    child.enabled = false;
                }
            }
            enemy.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeAll;
            foreach (var box in enemy.GetComponents<BoxCollider2D>())
            {
                box.size = Vector2.one * 0.85f;
            }
            enemy.transform.localScale = Vector3.one * 0.9f;
            arena.targets.Add(enemy);
        }
        const string scenePath = "Assets/Scenes/Threadloom.unity";
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), scenePath);
        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(scenePath, true),
            new EditorBuildSettingsScene("Assets/Scenes/GameTest.unity", true)
        };
        AssetDatabase.SaveAssets();
        Debug.Log("THREADLOOM GENERATED: original controller pipeline, new projectile + IFirePattern, three composable weapon definitions.");
    }
    static T Asset<T>(string path)
    where T : ScriptableObject
    {
        var value = AssetDatabase.LoadAssetAtPath<T>(path);
        if (value == null)
        {
            value = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(value, path);
        }
        return value;
    }
    static T Definition<T>(string name, string id, ProjectileController prefab, FirePatternType pattern, int count, float damage, float speed, float range, float cooldown, Color color)
    where T : WeaponDefinition
    {
        var definition = Asset<T>(Folder + "/Weapons/" + name + ".asset");
        definition.weaponID = id;
        definition.weaponType = WeaponType.Normal;
        definition.firePatternType = pattern;
        definition.projectilePrefab = prefab;
        definition.BurstCount = count;
        definition.Damage = damage;
        definition.Speed = speed;
        definition.Range = range;
        definition.cooldown = cooldown;
        definition.color = color;
        definition.isPiercing = false;
        EditorUtility.SetDirty(definition);
        return definition;
    }
    static ProjectileController CreateAccelerating(ProjectileController basic)
    {
        var root = UnityEngine.Object.Instantiate(basic.gameObject);
        root.name = "AcceleratingBullet";
        UnityEngine.Object.DestroyImmediate(root.GetComponent<ProjectileController>());
        root.AddComponent<ProjectileControllerAccelerating>();
        var prefab = PrefabUtility.SaveAsPrefabAsset(root, Folder + "/Prefabs/AcceleratingBullet.prefab").GetComponent<ProjectileController>();
        UnityEngine.Object.DestroyImmediate(root);
        return prefab;
    }
    static ProjectileController CreateBasic()
    {
        var original = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/ProjectileMovingNormal.prefab");
        var root = UnityEngine.Object.Instantiate(original);
        root.name = "PrismShard";
        root.transform.localScale = Vector3.one;
        var visual = root.GetComponentInChildren<SpriteRenderer>();
        visual.sprite = needle;
        visual.drawMode = SpriteDrawMode.Sliced;
        visual.size = new Vector2(1, 0.25f);
        visual.sharedMaterial = spriteMaterial;
        visual.transform.localScale = Vector3.one * 0.58f;
        visual.sortingOrder = 5;
        root.GetComponent<CircleCollider2D>().radius = 0.11f;
        var prefab = PrefabUtility.SaveAsPrefabAsset(root, Folder + "/Prefabs/PrismShard.prefab").GetComponent<ProjectileController>();
        UnityEngine.Object.DestroyImmediate(root);
        return prefab;
    }
    static Material MaterialAsset(string name, string shader)
    {
        string path = Folder + "/Art/" + name + ".mat";
        var material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            material = new Material(Shader.Find(shader));
            AssetDatabase.CreateAsset(material, path);
        }
        return material;
    }
    static GameObject SpriteObject(string name, Sprite sprite, Vector3 position, Vector3 scale, Color color)
    {
        var go = new GameObject(name);
        go.transform.position = position;
        go.transform.localScale = scale;
        var renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.drawMode = SpriteDrawMode.Sliced;
        renderer.size = Vector2.one;
        renderer.sharedMaterial = spriteMaterial;
        renderer.color = color;
        go.transform.localScale = scale;
        return go;
    }
    static void Grid()
    {
        Color color = new Color(0.055f, 0.105f, 0.15f);
        for (int x = -10; x <= 10; x++)
        {
            SpriteObject("GridVertical", square, new Vector3(x, 0, 2), new Vector3(0.012f, 9, 1), color);
        }
        for (int y = -4; y <= 4; y++)
        {
            SpriteObject("GridHorizontal", square, new Vector3(0, y, 2), new Vector3(20, 0.012f, 1), color);
        }
        Color border = new Color(0.1f, 0.27f, 0.33f);
        SpriteObject("TopRail", square, new Vector3(0, 4.55f, 1), new Vector3(20, 0.025f, 1), border);
        SpriteObject("BottomRail", square, new Vector3(0, -4.55f, 1), new Vector3(20, 0.025f, 1), border);
    }
    static Image Indicator(Transform parent, string name, Vector2 position, Vector2 size, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(parent, false);
        var image = go.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        var rect = image.rectTransform;
        rect.anchorMin = rect.anchorMax = new Vector2(0, 1);
        rect.pivot = new Vector2(0, 1);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        return image;
    }
}
