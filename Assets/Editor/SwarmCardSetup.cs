using UnityEditor;
using UnityEngine;

/// <summary>
/// One-shot setup for the swarm card (GDD §3: "a swarm card that summons a
/// large number of weak units") — the first real stress test of the monster
/// pool. Creates Card_Swarm, adds one copy to StarterDeck, and points both
/// pools' prewarm at their prefabs so a 50-unit swarm reuses warm objects
/// instead of instantiating mid-fight. Uses the asset API rather than
/// hand-written YAML so Unity serializes every asset reference itself.
/// Disposable — delete after it has run once.
/// </summary>
public static class SwarmCardSetup
{
    const string CardPath = "Assets/ScriptableObjects/Card_Swarm.asset";
    const string TemplateCardPath = "Assets/ScriptableObjects/Card_LoneGrunt.asset";
    const string DeckPath = "Assets/ScriptableObjects/StarterDeck.asset";
    const string RigPath = "Assets/Prefabs/GameplayRig.prefab";
    const string ProjectilePath = "Assets/Prefabs/Projectile_Placeholder.prefab";

    [MenuItem("Castle Attack/Setup/5. Create swarm card")]
    static void Run()
    {
        if (EditorApplication.isPlaying)
        {
            Debug.LogError("SwarmCardSetup: exit Play mode first.");
            return;
        }
        if (AssetDatabase.LoadAssetAtPath<CardDefinition>(CardPath) != null)
        {
            Debug.LogError($"SwarmCardSetup: {CardPath} already exists — this has already run.");
            return;
        }

        var template = AssetDatabase.LoadAssetAtPath<CardDefinition>(TemplateCardPath);
        var deck = AssetDatabase.LoadAssetAtPath<DeckDefinition>(DeckPath);
        var projectile = AssetDatabase.LoadAssetAtPath<GameObject>(ProjectilePath);
        if (template == null || template.monsterPrefab == null || deck == null || projectile == null)
        {
            Debug.LogError("SwarmCardSetup: missing Card_LoneGrunt (or its monster prefab), StarterDeck, " +
                           "or Projectile_Placeholder. Nothing changed.");
            return;
        }

        // Pool prewarm first, so a failure here leaves no half-made card behind.
        GameObject rig = PrefabUtility.LoadPrefabContents(RigPath);
        try
        {
            var spawner = rig.GetComponentInChildren<MonsterSpawner>(true);
            var pool = rig.GetComponentInChildren<ProjectilePool>(true);
            if (spawner == null || pool == null)
            {
                Debug.LogError("SwarmCardSetup: GameplayRig has no MonsterSpawner or ProjectilePool. Nothing changed.");
                return;
            }
            spawner.prewarmPrefab = template.monsterPrefab;
            pool.prewarmPrefab = projectile;
            PrefabUtility.SaveAsPrefabAsset(rig, RigPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(rig);
        }

        // First-pass numbers — tune by feel (GDD §5).
        var card = ScriptableObject.CreateInstance<CardDefinition>();
        card.cardName = "Swarm";
        card.description = "Fifty tiny, fragile grunts in a flood. One tower hit kills each, but there are a lot of them.";
        card.spawnCost = 80;
        card.monsterPrefab = template.monsterPrefab;
        card.spawnCount = 50;
        card.spawnInterval = 0.05f;
        card.overrideStats = true;
        card.unitMaxHP = 5;
        card.unitDamage = 1;
        card.unitSpeed = 4f;
        card.unitScale = 0.5f;
        AssetDatabase.CreateAsset(card, CardPath);

        deck.cards.Add(card);
        EditorUtility.SetDirty(deck);
        AssetDatabase.SaveAssets();

        Debug.Log("SwarmCardSetup: done. Card_Swarm created and added to StarterDeck; " +
                  "MonsterSpawner now prewarms TestMonster, ProjectilePool prewarms Projectile_Placeholder.");
    }
}
