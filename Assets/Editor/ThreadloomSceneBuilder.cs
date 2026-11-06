using System;
using System.IO;
using FWeapon;
using TMPro;
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
    [MenuItem("Threadloom/Build Assignment Scene")]
    public static void Generate()
    {
        Directory.CreateDirectory(Folder + "/Art");
        Directory.CreateDirectory(Folder + "/Prefabs");
        Directory.CreateDirectory(Folder + "/Weapons");
        spriteMaterial = MaterialAsset("Sprite", "Universal Render Pipeline/2D/Sprite-Unlit-Default");
        threadMaterial = MaterialAsset("Thread", "Sprites/Default");
        square = CreateSprite("Square", (x, y) => Color.white);
        ring = CreateSprite("Ring", (x, y) =>
        {
            float distance = new Vector2(x, y).magnitude;
            return distance > 0.32f && distance < 0.46f ? Color.white : Color.clear;
        });
        needle = CreateSprite("Needle", (x, y) =>
        {
            bool body = x > -0.42f && x < 0.43f && Mathf.Abs(y) < (0.44f - x) * 0.18f;
            bool eye = x < -0.20f && x > -0.33f && Mathf.Abs(y) < 0.025f;
            return body && !eye ? Color.white : Color.clear;
        });
        drone = CreateSprite("Drone", (x, y) =>
        {
            float edge = Mathf.Max(Mathf.Abs(x), Mathf.Abs(y));
            bool rim = edge > 0.34f && edge < 0.45f;
            bool core = Mathf.Abs(x) + Mathf.Abs(y) < 0.13f;
            return rim || core ? Color.white : Color.clear;
        });
        var needlePrefab = CreateNeedle();
        var basicPrefab = CreateBasic();
        var loom = Definition<ThreadloomDefinition>("Threadloom", "THREADLOOM", needlePrefab, FirePatternType.Weave, 5, 18, 8, 6.2f, 1.05f, new Color(1, 0.64f, 0.24f));
        loom.weaveAmplitude = 0.68f;
        loom.orbitRadius = 1.25f;
        loom.orbitDuration = 1.25f;
        loom.orbitTurns = 1.5f;
        loom.returnDuration = 0.72f;
        var burst = Definition<ThreadloomDefinition>("NeedleBurst", "NEEDLE BURST", needlePrefab, FirePatternType.Burst, 3, 22, 10, 7, 0.85f, new Color(0.3f, 0.94f, 1));
        burst.weaveAmplitude = 0.32f;
        burst.orbitRadius = 0.8f;
        burst.orbitDuration = 0.85f;
        var fan = Definition<WeaponDefinition>("PrismFan", "PRISM FAN", basicPrefab, FirePatternType.Spread, 5, 16, 9, 12, 0.45f, new Color(0.7f, 0.55f, 1));
        fan.isPiercing = false;
        var library = Asset<WeaponLibrary>(Folder + "/Weapons/ThreadloomLibrary.asset");
        library.Weaponlib = new System.Collections.Generic.List<WeaponDefinition>
        {
            loom,
            burst,
            fan
        };
        EditorUtility.SetDirty(library);
        var originalLibrary = AssetDatabase.LoadAssetAtPath<WeaponLibrary>("Assets/Prefabs/WeaponLibrary.asset");
        if (!originalLibrary.Weaponlib.Contains(loom))
        {
            originalLibrary.Weaponlib.Add(loom);
            EditorUtility.SetDirty(originalLibrary);
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
        var title = Label(canvas.transform, "WeaponName", "THREADLOOM", new Vector2(50, -40), new Vector2(510, 55), 34, TextAlignmentOptions.Left, new Color(1, 0.72f, 0.4f));
        var score = Label(canvas.transform, "Score", "HITS  000     CLEARED  00", new Vector2(780, -40), new Vector2(450, 50), 21, TextAlignmentOptions.Right, new Color(0.75f, 0.85f, 0.94f));
        var poolLabel = Label(canvas.transform, "PoolCount", "POOL  00 / 60", new Vector2(980, -650), new Vector2(250, 40), 19, TextAlignmentOptions.Right, new Color(0.35f, 0.6f, 0.72f));
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
        serialized.FindProperty("TextUI_weaponID").objectReferenceValue = title;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        var arena = new GameObject("ThreadloomArena").AddComponent<ThreadloomArena>();
        arena.weapon = weapon;
        arena.library = library;
        arena.player = player.transform;
        arena.pointer = pointer;
        arena.muzzle = muzzle;
        arena.scoreText = score;
        arena.poolText = poolLabel;
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
            renderer.sharedMaterial = spriteMaterial;
            renderer.color = new Color(0.94f, 0.3f, 0.52f);
            renderer.sortingOrder = 2;
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
        definition.isPiercing = true;
        EditorUtility.SetDirty(definition);
        return definition;
    }

    static ProjectileController CreateNeedle()
    {
        var root = new GameObject("ThreadloomNeedle");
        root.tag = "Missile";
        var component = root.AddComponent<ProjectileControllerThreadloom>();
        var body = root.AddComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Kinematic;
        body.useFullKinematicContacts = true;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        var collider = root.AddComponent<CircleCollider2D>();
        collider.radius = 0.13f;
        var visual = SpriteObject("Needle", needle, Vector3.zero, new Vector3(0.75f, 0.75f, 1), Color.white);
        visual.transform.SetParent(root.transform, false);
        visual.GetComponent<SpriteRenderer>().sortingOrder = 6;
        var trail = root.AddComponent<TrailRenderer>();
        trail.sharedMaterial = threadMaterial;
        trail.startWidth = 0.065f;
        trail.endWidth = 0.008f;
        trail.minVertexDistance = 0.025f;
        trail.time = 0.42f;
        trail.sortingOrder = 5;
        trail.numCapVertices = 3;
        var prefab = PrefabUtility.SaveAsPrefabAsset(root, Folder + "/Prefabs/ThreadloomNeedle.prefab").GetComponent<ProjectileControllerThreadloom>();
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
        visual.sharedMaterial = spriteMaterial;
        visual.transform.localScale = Vector3.one * 0.58f;
        visual.sortingOrder = 5;
        root.GetComponent<CircleCollider2D>().radius = 0.11f;
        var prefab = PrefabUtility.SaveAsPrefabAsset(root, Folder + "/Prefabs/PrismShard.prefab").GetComponent<ProjectileController>();
        UnityEngine.Object.DestroyImmediate(root);
        return prefab;
    }

    static Sprite CreateSprite(string name, Func<float, float, Color> pixel)
    {
        var texture = new Texture2D(128, 128, TextureFormat.RGBA32, false);
        var colors = new Color[128 * 128];
        for (int y = 0; y < 128; y++)
        {
            for (int x = 0; x < 128; x++)
            {
                colors[y * 128 + x] = pixel((x + 0.5f) / 128 - 0.5f, (y + 0.5f) / 128 - 0.5f);
            }
        }

        texture.SetPixels(colors);
        texture.Apply();
        string path = Folder + "/Art/" + name + ".png";
        File.WriteAllBytes(path, texture.EncodeToPNG());
        UnityEngine.Object.DestroyImmediate(texture);
        AssetDatabase.ImportAsset(path);
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Sprite;
        importer.spritePixelsPerUnit = 128;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
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
        renderer.sharedMaterial = spriteMaterial;
        renderer.color = color;
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

    static TextMeshProUGUI Label(Transform parent, string name, string text, Vector2 position, Vector2 size, int fontSize, TextAlignmentOptions alignment, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        var label = go.GetComponent<TextMeshProUGUI>();
        label.font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
        label.text = text;
        label.fontSize = fontSize;
        label.color = color;
        label.alignment = alignment;
        label.raycastTarget = false;
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0, 1);
        rect.pivot = new Vector2(0, 1);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        return label;
    }
}
