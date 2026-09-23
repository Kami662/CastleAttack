# Castle Attack! — project context

## What this is
A mobile **roguelite tower-defense with reversed roles**, Unity 6000.6.1f1 + URP, C#.
The player is the **attacker**: play monster cards from a hand, send waves down a
waypoint path at an AI-defended castle. Portfolio/CV project. Team of 3: Kevin
(programming/design), his girlfriend (3D art + UI/UX), and a second dev joining.
No deadline — 1–2 year horizon, so scope is real, not aspirational.

Repo: `CastleAttack` (the Unity project folder is still named `ElementalCardTD`).
Full design doc: **`docs/game-design-document.md`** — read it before making design
decisions. It is mirrored from a claude.ai Project doc, which is the source of truth.

## Design pillars (use these to break ties)
1. **The player is the monster, and should feel like it.** When "tidier" fights
   "more satisfying to have accomplished", pick satisfying. This is why towers are
   permanently destructible instead of rebuilding.
2. **Constraints should be player choices, not system rules.** Prefer giving the
   player a tool over adding a global rule.
3. **Vary the pressure, not the rules.** Run-to-run variety comes from changing
   allowances/drain rates, never from swapping mechanics in and out.

## Architecture — the rules that matter
- **Data-driven content via ScriptableObjects.** `CardDefinition` (cost, prefab,
  spawnCount, spawnInterval, optional stat overrides) and `DeckDefinition`.
  Always `[CreateAssetMenu(menuName = "Castle Attack/...")]`. Any new content type
  (run modifiers, castle definitions) follows this pattern.
- **Single authorities, no scene scanning.**
  - `MonsterSpawner` is the *only* place monsters are created or removed, owns the
    object pool, and owns the static `ActiveMonsters` registry.
  - `Tower` owns a static `StandingTowers` registry (same pattern).
  - `GameOverManager` is the single arbiter of win/lose, checked in a fixed priority
    order (win before lose). This exists because scattering the checks caused a real
    script-execution-order race that showed "YOU LOSE" on a winning frame. Don't
    reintroduce distributed end-condition checks.
  - Nothing uses `FindObjectsByType` for monsters/towers.
- **Pooling reset lives in `MonsterMover.OnSpawn`.** `Start()` and field initializers
  do NOT run on a reused object. Any new per-life state (buffs, status effects, stat
  overrides, animation state) MUST be reset there or it leaks between lives.
- **Shared `GameplayRig` prefab.** Managers + UI + spawn point/path/castle/tower live
  in `Assets/Prefabs/GameplayRig.prefab`, instanced by both `SampleScene` (canonical)
  and `Sandbox` (scratch). **Edit the prefab, not a scene's copy.**
- **Editor automation** for fiddly setup: `Assets/Editor/*.cs` with
  `[MenuItem("Castle Attack/Setup/...")]`. These are one-shot and disposable.

## Current scripts (Assets/Scripts/)
GameManager, CardDefinition, DeckDefinition, HandManager, HandDebugUI (throwaway
IMGUI hand), MonsterSpawner, MonsterMover, Castle, Tower, Projectile, UnitCommander,
UIManager, GameOverManager. Editor/: GameplayRigSetup, TowerSetup.

## State
**Done:** spawner + registry, data-driven cards, hand/deck (draw-on-play, reshuffle),
object pooling, shared GameplayRig + Sandbox, retry loop (button + R key),
the girlfriend's tower model in-game, per-card unit stat overrides.

**In flight:** destructible towers (Tower HP 100 / `TakeDamage` / `IsDestroyed`),
visible `Projectile` (placeholder sphere, arrow art later, not pooled),
`UnitCommander` global order FocusCastle/AttackTowers on dev keys C/T, with
`MonsterMover` diverting to the nearest standing tower on AttackTowers.

**Next:** real uGUI card hand UI (replaces HandDebugUI), currency/pacing model
(decide by feel — fixed pool vs. regenerating income is the big open question),
swarm card as a pool stress test, **flying units** (bypass the waypoint path;
weaker than ground units; the ground path gets blocked by environment later so
flyers become a deck-building decision), tower-destruction VFX.

## Conventions / gotchas
- Unity 6: `FindObjectOfType` and `FindFirstObjectByType` are both deprecated →
  `FindAnyObjectByType`.
- `Time.timeScale` survives scene loads — any reload must reset it to 1.
- Reloading a scene from a UI Button needs `EventSystem.current.SetSelectedGameObject(null)`
  first or you get a `MissingReferenceException`.
- `ProjectSettings/*.asset` (Build Settings) only flush on a *project* save, not a
  scene save.
- Input System is set to "Both" — legacy `Input.GetKeyDown` works.
- **FBX imports:** untick Import Cameras and Import Lights, check Scale Factor. A
  Blender export that didn't tick "Selected Objects" brings the whole scene in —
  an embedded camera will render over your game view. Export recipe is in the art
  repo README and in §11 of the design doc.
- UI is uGUI + TextMeshPro. Mobile is the target; keyboard input is dev-only.
- Git LFS tracks `*.fbx`; pushes and art commits run from Kevin's machine.
- Windows + PowerShell 5 — no `&&` between commands.
- Run `setup-dev.ps1` / `setup-dev.sh` once after cloning (Unity SmartMerge + LFS).

## How Kevin wants to work
Show code changes for review before they're applied — he pastes them in himself.
Editor/scene/prefab work is his. Keep explanations short; he's learning Unity as he
goes, so say *why* a pattern exists when it's non-obvious.
