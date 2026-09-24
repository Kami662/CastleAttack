using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// One-shot setup that stops SampleScene and Sandbox drifting apart by moving
/// everything both scenes need into prefabs, built from SampleScene (canonical):
///   - GameplayRig gets the Ground (its size follows the path layout the rig
///     already owns), plus SampleScene's unapplied rig overrides (tower
///     placement, the PathVisualizer on Path).
///   - A new SceneEnvironment prefab holds the camera, light, global volume
///     and RunManager.
/// Sandbox then drops its stale copies and instances SceneEnvironment.
/// Disposable — delete after it has run once.
/// </summary>
public static class SceneSyncSetup
{
    const string SampleScenePath = "Assets/Scenes/SampleScene.unity";
    const string SandboxPath = "Assets/Scenes/Sandbox.unity";
    const string RigPath = "Assets/Prefabs/GameplayRig.prefab";
    const string EnvironmentPath = "Assets/Prefabs/SceneEnvironment.prefab";

    static readonly string[] EnvironmentNames = { "Main Camera", "Directional Light", "Global Volume" };

    [MenuItem("Castle Attack/Setup/4. Sync scenes (environment into prefabs)")]
    static void Run()
    {
        if (EditorApplication.isPlaying)
        {
            Debug.LogError("SceneSyncSetup: exit Play mode first.");
            return;
        }
        if (AssetDatabase.LoadAssetAtPath<GameObject>(EnvironmentPath) != null)
        {
            Debug.LogError($"SceneSyncSetup: {EnvironmentPath} already exists — this has already run.");
            return;
        }
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        // --- SampleScene (canonical) ---
        Scene sample = EditorSceneManager.OpenScene(SampleScenePath, OpenSceneMode.Single);
        GameObject rig = FindRigInstance(sample);
        GameObject ground = FindRoot(sample, "Ground");
        if (rig == null || ground == null)
        {
            Debug.LogError("SceneSyncSetup: couldn't find the GameplayRig instance or a root 'Ground' in SampleScene. Nothing changed.");
            return;
        }

        // Parent Ground under the rig instance, then push every unapplied
        // override on the instance (Ground included) into the prefab asset.
        ground.transform.SetParent(rig.transform, true);
        PrefabUtility.ApplyPrefabInstance(rig, InteractionMode.AutomatedAction);

        var env = new GameObject("SceneEnvironment");
        foreach (string name in EnvironmentNames)
        {
            GameObject go = FindRoot(sample, name);
            if (go != null) go.transform.SetParent(env.transform, true);
            else Debug.LogWarning($"SceneSyncSetup: no root '{name}' in SampleScene.");
        }
        GameObject runManager = FindRootWith<RunManager>(sample);
        if (runManager != null)
        {
            runManager.name = "RunManager";
            runManager.transform.SetParent(env.transform, true);
        }
        PrefabUtility.SaveAsPrefabAssetAndConnect(env, EnvironmentPath, InteractionMode.AutomatedAction);
        EditorSceneManager.MarkSceneDirty(sample);
        EditorSceneManager.SaveScene(sample);

        // --- Sandbox: drop stale per-scene copies, use the shared prefab ---
        Scene sandbox = EditorSceneManager.OpenScene(SandboxPath, OpenSceneMode.Single);
        foreach (string name in EnvironmentNames) DestroyRoot(sandbox, name);
        DestroyRoot(sandbox, "Ground");
        var envPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(EnvironmentPath);
        PrefabUtility.InstantiatePrefab(envPrefab, sandbox);
        EditorSceneManager.MarkSceneDirty(sandbox);
        EditorSceneManager.SaveScene(sandbox);

        EditorSceneManager.OpenScene(SampleScenePath, OpenSceneMode.Single);
        Debug.Log("SceneSyncSetup: done. Ground and SampleScene's rig overrides are applied to GameplayRig; " +
                  "camera/light/volume/RunManager now come from SceneEnvironment in both scenes.");
    }

    static GameObject FindRoot(Scene scene, string name)
    {
        foreach (GameObject go in scene.GetRootGameObjects())
            if (go.name == name) return go;
        return null;
    }

    static GameObject FindRootWith<T>(Scene scene) where T : Component
    {
        foreach (GameObject go in scene.GetRootGameObjects())
            if (go.GetComponent<T>() != null) return go;
        return null;
    }

    static GameObject FindRigInstance(Scene scene)
    {
        var rigAsset = AssetDatabase.LoadAssetAtPath<GameObject>(RigPath);
        foreach (GameObject go in scene.GetRootGameObjects())
            if (PrefabUtility.GetCorrespondingObjectFromSource(go) == rigAsset) return go;
        return null;
    }

    static void DestroyRoot(Scene scene, string name)
    {
        GameObject go = FindRoot(scene, name);
        if (go != null) Object.DestroyImmediate(go);
    }
}
