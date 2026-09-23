using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// One-time helper: drop the girlfriend's tower model into the scene as a
/// working defensive Tower (has Tower.cs, positioned beside the path corner),
/// and save it as a reusable prefab. Editor-only; safe to delete after use.
/// </summary>
public static class TowerSetup
{
    const string ModelPath  = "Assets/Art/Models/tower_for_project.fbx";
    const string PrefabPath = "Assets/Prefabs/Tower.prefab";

    [MenuItem("Castle Attack/Setup/3. Add Tower From Model")]
    public static void AddTower()
    {
        AssetDatabase.Refresh();

        var model = AssetDatabase.LoadAssetAtPath<GameObject>(ModelPath);
        if (model == null)
        {
            EditorUtility.DisplayDialog("Model not found",
                "Could not load " + ModelPath + " — make sure the FBX has imported.", "OK");
            return;
        }

        // Instantiate the model into the open scene.
        var go = (GameObject)PrefabUtility.InstantiatePrefab(model);
        go.name = "Tower_Guard";
        go.transform.position = new Vector3(3f, 0f, 3f); // beside the (0,0,0) path corner, within range 6
        if (go.GetComponent<Tower>() == null) go.AddComponent<Tower>();

        // Save as its own prefab (a variant of the model) for reuse.
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        var prefab = PrefabUtility.SaveAsPrefabAssetAndConnect(go, PrefabPath, InteractionMode.UserAction);

        Selection.activeGameObject = go;
        SceneView.FrameLastActiveSceneView();
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log(prefab != null
            ? "TowerSetup: placed 'Tower_Guard' at (3,0,3) with Tower.cs, saved " + PrefabPath +
              ". Now check its scale/orientation in the scene."
            : "TowerSetup: instantiated in scene but prefab save failed.");
    }
}
