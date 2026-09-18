using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// One-time editor utility to (1) group the shared gameplay/UI objects under a
/// single "GameplayRig" root and save it as a prefab, and (2) create a Sandbox
/// scene that reuses that prefab, plus register both scenes in Build Settings.
///
/// Why: both SampleScene and Sandbox drop in the SAME prefab, so a change to the
/// wiring is made once (in the prefab) and both scenes inherit it. The rig holds
/// every object that cross-references another (managers <-> spawn/path/castle/UI),
/// so all serialized references stay internal to the prefab and survive intact.
/// The environment (camera, light, ground, volume) stays per-scene.
///
/// Editor-only (lives in an Editor folder). Safe to delete after use.
/// </summary>
public static class GameplayRigSetup
{
    // Roots that go INTO the prefab. Everything else in the scene (Main Camera,
    // Directional Light, Ground, Global Volume) stays out.
    static readonly string[] RigRoots =
    {
        "GameManager", "HandManager", "UIManager", "GameOverManager",
        "MonsterSpawner", "Castle_Placeholder", "Spawn_Marker", "Path",
        "Canvas", "EventSystem"
    };

    const string PrefabPath = "Assets/Prefabs/GameplayRig.prefab";
    const string MainScenePath = "Assets/Scenes/SampleScene.unity";
    const string SandboxScenePath = "Assets/Scenes/Sandbox.unity";

    [MenuItem("Castle Attack/Setup/1. Create GameplayRig Prefab")]
    public static void CreateRigPrefab()
    {
        Scene scene = EditorSceneManager.GetActiveScene();
        if (scene.path != MainScenePath)
        {
            EditorUtility.DisplayDialog("Wrong scene",
                "Open SampleScene first (this is currently: " + scene.path + ").", "OK");
            return;
        }

        if (GameObject.Find("GameplayRig") != null)
        {
            EditorUtility.DisplayDialog("Already done",
                "A 'GameplayRig' already exists in this scene. Aborting so nothing is duplicated.", "OK");
            return;
        }

        // Collect the target roots by name from the scene's actual root objects.
        var roots = scene.GetRootGameObjects().ToDictionary(g => g.name, g => g);
        var missing = RigRoots.Where(n => !roots.ContainsKey(n)).ToList();
        if (missing.Count > 0)
        {
            EditorUtility.DisplayDialog("Missing objects",
                "These expected root objects were not found:\n" + string.Join(", ", missing), "OK");
            return;
        }

        // Create the rig root at origin and reparent, keeping world positions.
        var rig = new GameObject("GameplayRig");
        Undo.RegisterCreatedObjectUndo(rig, "Create GameplayRig");
        rig.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);

        foreach (var name in RigRoots)
            roots[name].transform.SetParent(rig.transform, worldPositionStays: true);

        // Save as a prefab and connect the scene instance to it.
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
            AssetDatabase.CreateFolder("Assets", "Prefabs");

        var prefab = PrefabUtility.SaveAsPrefabAssetAndConnect(rig, PrefabPath, InteractionMode.UserAction);
        if (prefab == null)
        {
            Debug.LogError("GameplayRigSetup: prefab save failed.");
            return;
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log($"GameplayRigSetup: saved {PrefabPath} and grouped {RigRoots.Length} objects. " +
                  "SampleScene saved. Now run step 2.");
    }

    [MenuItem("Castle Attack/Setup/2. Create Sandbox Scene + Build Settings")]
    public static void CreateSandboxAndBuildSettings()
    {
        Scene scene = EditorSceneManager.GetActiveScene();
        if (scene.path != MainScenePath)
        {
            EditorUtility.DisplayDialog("Wrong scene",
                "Open SampleScene first, then run this so Sandbox is copied from it.", "OK");
            return;
        }
        if (GameObject.Find("GameplayRig") == null)
        {
            EditorUtility.DisplayDialog("Run step 1 first",
                "No GameplayRig in the scene yet — run step 1 before creating the Sandbox copy.", "OK");
            return;
        }

        // Save a COPY of the current scene as Sandbox (does not switch the open scene).
        EditorSceneManager.SaveScene(scene, SandboxScenePath, saveAsCopy: true);
        AssetDatabase.Refresh();

        // Register both scenes in Build Settings (SampleScene index 0, Sandbox index 1).
        var wanted = new List<EditorBuildSettingsScene>
        {
            new EditorBuildSettingsScene(MainScenePath, true),
            new EditorBuildSettingsScene(SandboxScenePath, true),
        };
        // Keep any other already-registered scenes, but ensure ours are present/enabled.
        var others = EditorBuildSettings.scenes
            .Where(s => s.path != MainScenePath && s.path != SandboxScenePath);
        EditorBuildSettings.scenes = wanted.Concat(others).ToArray();

        Debug.Log($"GameplayRigSetup: created {SandboxScenePath} (copy of SampleScene) and set Build Settings. " +
                  "Both scenes now share the GameplayRig prefab.");
    }
}
