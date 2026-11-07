using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FWeapon;
using UnityEditor;
using UnityEditor.Media;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public static class ThreadloomValidation
{
    static readonly HashSet<ProjectileControllerThreadloom.FlightPhase> phases = new HashSet<ProjectileControllerThreadloom.FlightPhase>();
    static readonly List<string> checks = new List<string>();
    static ThreadloomArena arena;
    static MediaEncoder encoder;
    static RenderTexture renderTarget;
    static Texture2D texture;
    static int phase;
    static int frame;
    static int lastGameFrame = -1;
    static int[] initialIds;
    static double readyAt;
    static double deadline;
    static bool failed;
    static bool playRequested;
    static bool previousOptionsEnabled;
    static EnterPlayModeOptions previousOptions;
    static string output;
    public static void Run()
    {
        output = Path.GetFullPath("Submission");
        Directory.CreateDirectory(output);
        Directory.CreateDirectory("Logs/ThreadloomFrames");
        previousOptionsEnabled = EditorSettings.enterPlayModeOptionsEnabled;
        previousOptions = EditorSettings.enterPlayModeOptions;
        EditorSettings.enterPlayModeOptionsEnabled = true;
        EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;
        deadline = EditorApplication.timeSinceStartup + 600;
        EditorApplication.update += Tick;
        EditorSceneManager.OpenScene("Assets/Scenes/Threadloom.unity");
        readyAt = EditorApplication.timeSinceStartup + 3;
    }
    static void OnLog(string message, string stack, LogType type)
    {
        if (type == LogType.Exception || type == LogType.Error || type == LogType.Assert)
        {
            failed = true;
            File.AppendAllText("Logs/ThreadloomErrors.txt", message + "\n" + stack + "\n");
        }
    }
    static void Require(bool condition, string message)
    {
        if (!condition)
        {
            throw new Exception("VALIDATION FAILED: " + message);
        }
        checks.Add("PASS: " + message);
        Debug.Log("PASS: " + message);
    }
    static void Tick()
    {
        try
        {
            if (EditorApplication.timeSinceStartup > deadline)
            {
                throw new Exception("Validation timeout");
            }
            if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.timeSinceStartup < readyAt)
            {
                return;
            }
            if (phase == 0)
            {
                if (!playRequested)
                {
                    Application.logMessageReceived += OnLog;
                    EditorApplication.isPlaying = true;
                    playRequested = true;
                    readyAt = EditorApplication.timeSinceStartup + 1;
                    return;
                }
                if (!EditorApplication.isPlaying)
                {
                    return;
                }
                arena = UnityEngine.Object.FindFirstObjectByType<ThreadloomArena>();
                if (arena == null)
                {
                    return;
                }
                arena.enabled = false;
                Time.captureDeltaTime = 1f / 30;
                Screen.SetResolution(1280, 720, false);
                Require(arena.weapon.weaponIDList.Count == 3, "Three definitions built by the original WeaponController");
                Require(FirePatternFactory.CreateFirePattern(FirePatternType.Weave) is FirePattern_Weave, "New IFirePattern registered in the factory");
                var pools = arena.weapon.GetComponentsInChildren<ProjectilePool>();
                Require(pools.Length == 3 && pools.All(p => p.missiles.Count == 20), "Three preallocated pools of 20 projectiles");
                initialIds = pools.SelectMany(p => p.missiles).Select(p => p.GetInstanceID()).OrderBy(id => id).ToArray();
                foreach (var root in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
                {
                    foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
                    {
                        Require(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(child.gameObject) == 0, "No missing scripts on " + child.name);
                    }
                }
                encoder = new MediaEncoder(Path.Combine(output, "Threadloom_Demo.mp4"), new VideoTrackAttributes
                {
                    width = 1280, height = 720, frameRate = new MediaRational(30), includeAlpha = false
                }, new AudioTrackAttributes[0]);
                renderTarget = new RenderTexture(1280, 720, 24);
                renderTarget.Create();
                texture = new Texture2D(1280, 720, TextureFormat.RGBA32, false);
                phase = 1;
                return;
            }
            if (phase == 1)
            {
                if (Time.frameCount == lastGameFrame)
                {
                    return;
                }
                lastGameFrame = Time.frameCount;
                arena.SendMessage("Update");
                foreach (var needle in arena.weapon.GetComponentsInChildren<ProjectileControllerThreadloom>(true))
                {
                    if (needle.flying)
                    {
                        phases.Add(needle.Phase);
                    }
                }
                Act();
                Capture();
                frame++;
                if (frame == 780)
                {
                    encoder.Dispose();
                    encoder = null;
                    Validate();
                    File.WriteAllLines("Logs/Threadloom-Validation.txt", checks);
                    Debug.Log("THREADLOOM VALIDATION AND VIDEO PASSED");
                    Time.captureDeltaTime = 0;
                    EditorApplication.isPlaying = false;
                    phase = 2;
                    readyAt = EditorApplication.timeSinceStartup + 0.5;
                }
            }
            else if (phase == 2 && !EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorSettings.enterPlayModeOptionsEnabled = previousOptionsEnabled;
                EditorSettings.enterPlayModeOptions = previousOptions;
                EditorApplication.Exit(0);
            }
        }
        catch (Exception exception)
        {
            encoder?.Dispose();
            encoder = null;
            Debug.LogException(exception);
            File.WriteAllLines("Logs/Threadloom-Validation.txt", checks.Concat(new[]
            {
                exception.ToString()
            }));
            EditorSettings.enterPlayModeOptionsEnabled = previousOptionsEnabled;
            EditorSettings.enterPlayModeOptions = previousOptions;
            EditorApplication.Exit(1);
        }
    }
    static void Act()
    {
        Vector3 direction = Vector3.right;
        if (frame >= 180 && frame < 270)
        {
            direction = new Vector3(6, 1.3f, 0);
        }
        if (frame >= 270 && frame < 405)
        {
            direction = new Vector3(6, -1.3f, 0);
        }
        if (frame >= 405 && frame < 570)
        {
            direction = new Vector3(6, 0.5f, 0);
        }
        if (frame >= 570)
        {
            direction = Vector3.right;
        }
        arena.SetAim(direction);
        if (frame == 30 || frame == 130 || frame == 230 || frame == 330)
        {
            Require(arena.TryFire(), "Threadloom fire at frame " + frame);
            Require(!arena.TryFire(), "Cooldown blocks a duplicate same-frame shot");
        }
        if (frame == 125)
        {
            Require(phases.Contains(ProjectileControllerThreadloom.FlightPhase.Weaving), "Weaving phase observed");
            Require(phases.Contains(ProjectileControllerThreadloom.FlightPhase.Orbiting), "Orbiting phase observed");
            Require(phases.Contains(ProjectileControllerThreadloom.FlightPhase.Returning), "Return phase observed");
            Require(arena.weapon.GetComponentsInChildren<ProjectileControllerThreadloom>(true).Sum(p => p.CompletedCycles) >= 5, "All five needles completed a cycle and returned to their pool");
        }
        if (frame == 405 || frame == 570)
        {
            arena.weapon.SwitchWeapon();
        }
        if (frame == 420 || frame == 465 || frame == 510 || frame == 585 || frame == 615 || frame == 645 || frame == 675)
        {
            Require(arena.TryFire(), "Composed weapon fires at frame " + frame);
        }
    }
    static void Validate()
    {
        Require(!failed, "No errors or exceptions during the demo");
        var pools = arena.weapon.GetComponentsInChildren<ProjectilePool>();
        var finalIds = pools.SelectMany(p => p.missiles).Select(p => p.GetInstanceID()).OrderBy(id => id).ToArray();
        Require(initialIds.SequenceEqual(finalIds), "All three weapons reuse the same 60 instances throughout the demo");
        var loomPool = pools.First(p => p.missiles[0] is ProjectileControllerThreadloom);
        foreach (var projectile in loomPool.missiles)
        {
            projectile.ReturnToPool();
        }
        for (int i = 0; i < 30; i++)
        {
            loomPool.FireProjectile(new Vector3(-6, 0, 0), Vector3.right);
        }
        Require(loomPool.missiles.Count(p => p.flying) == 20, "Pool exhaustion stays bounded at 20 instances");
        foreach (var projectile in loomPool.missiles)
        {
            projectile.ReturnToPool();
        }
        Require(loomPool.missiles.All(p => !p.flying && !p.gameObject.activeSelf), "Pool return resets every projectile");
        var needle = (ProjectileControllerThreadloom)loomPool.missiles[0];
        Require(needle.Phase == ProjectileControllerThreadloom.FlightPhase.Ready && needle.GetComponent<TrailRenderer>().positionCount == 0, "Return resets state and clears the trail");
        int id = needle.GetInstanceID();
        loomPool.FireProjectile(new Vector3(-6, 0, 0), Vector3.right);
        Require(needle.flying && needle.Phase == ProjectileControllerThreadloom.FlightPhase.Weaving && needle.GetInstanceID() == id, "Reused needle restarts in weaving phase without new allocation");
        needle.ReturnToPool();
        needle.Fire(Vector3.zero, Vector3.zero);
        Require(!needle.flying, "Zero aim direction cannot fire a needle");
    }
    static void Capture()
    {
        var camera = Camera.main;
        var canvas = arena.weaponIndicators[0].canvas;
        camera.aspect = 1280f / 720;
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = camera;
        canvas.planeDistance = 1;
        Canvas.ForceUpdateCanvases();
        RenderPipeline.SubmitRenderRequest(camera, new UniversalRenderPipeline.SingleCameraRequest
        {
            destination = renderTarget
        });
        var previous = RenderTexture.active;
        RenderTexture.active = renderTarget;
        texture.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0);
        texture.Apply();
        RenderTexture.active = previous;
        if (!encoder.AddFrame(texture))
        {
            throw new Exception("Video encoder rejected frame " + frame);
        }
        if (frame == 0 || frame == 45 || frame == 65 || frame == 95 || frame == 250 || frame == 480 || frame == 630 || frame == 779)
        {
            File.WriteAllBytes("Logs/ThreadloomFrames/Frame_" + frame.ToString("000") + ".png", texture.EncodeToPNG());
        }
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.worldCamera = null;
    }
}
