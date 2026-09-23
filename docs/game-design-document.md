# Castle Attack! — Game Design Document

_Living document — update as decisions change. Last updated: 2026-09-23._

_Working title: **Castle Attack!** — a placeholder, not a final choice. See §10 for the naming shortlist. Repo: **https://github.com/Kami662/CastleAttack** (the Unity project folder is still `ElementalCardTD`; both rename easily later)._

_This file is a mirror. The source of truth is the claude.ai Project doc; re-sync this copy after a doc pass. A `CLAUDE.md` at the repo root carries the short version for Claude Code (§11)._

## 1. Concept

A mobile **roguelite** tower-defense game with the roles reversed. The player is the **attacker**: instead of placing towers, they play monster cards from a deck and send waves of monsters down a path toward an AI-controlled castle. The castle defends itself with its own towers, which auto-target and damage the monsters as they approach. A run is a sequence of increasingly well-defended castles; losing ends the run and sends the player back to the start with some permanent progress kept.

- **Platform:** Mobile (built in Unity, C#)
- **Purpose:** First game project, built as a CV/portfolio piece
- **Engine:** Unity 6000.6.1f1, Universal Render Pipeline (URP)
- **Theme (v1):** Elemental / magic
- **Structure:** run-based roguelite (see §3, Run Structure)
- **Team:** small team, not solo — the user (programming/design), their girlfriend (3D art + UI/UX), and another developer joining soon (programming support, ~week of 2026-09-22)
- **Timeline:** no fixed deadline — the user is comfortable with a 1-2 year development timeline, so the full feature scope (§3) is a genuine target, not something to be cut down for time pressure.

### Design pillar: the player is the monster, and should feel like it
A guiding principle that has come up repeatedly and should steer ambiguous decisions: the player is playing as the attacking horde, and the game should make destruction feel *earned and satisfying*. When a choice is between "mechanically tidier" and "more satisfying to have accomplished," lean toward the latter. This directly drove the destructible-towers decision (§3) and should inform feedback, art, and audio work too.

### Design pillar: constraints should be player choices, not system rules
Where a design tension can be resolved either by a global rule or by giving the player a tool that changes the situation, prefer the tool. The stun mechanic (§3) is the worked example: rather than deciding globally whether towers are destructible *or* suppressible, towers are destructible and suppression is a card the player can choose to bring. This keeps decisions in the player's hands and gives decks distinct identities.

### Design pillar: vary the pressure, not the rules
Run-to-run variety should come from changing *how much* pressure each constraint applies, not from changing what the constraints are. A player who has learned the system should never have to re-learn it; they should have to re-plan. This is why run modifiers adjust allowances and drain rates rather than swapping mechanics in and out (§3, Run Resources).

## 2. Core Loop (v1 — "level 1")

The single-battle loop, which is fully implemented and tuned (see §5). In the roguelite structure this is one **encounter**; a run strings several together (§3).

1. Player plays a card from their hand to summon monsters (each card costs currency; currency starts at a fixed pool).
2. Each summoned monster walks a fixed waypoint path toward the castle.
3. The castle's tower auto-targets the closest monster in range and fires a **visible projectile** at it (§4).
4. A monster that reaches the castle deals damage to it and is removed.
5. The encounter ends when either:
   - **Win:** the castle's HP reaches 0.
   - **Lose:** the player can no longer afford to play any card in hand and no monsters remain alive on the field.
6. On either outcome the game freezes and a **Retry** action reloads the encounter (see §4).

A hand of cards drawn from a deck drives play (§4, "Built: hand & deck system"), and the player can now issue a global **order** to the horde — *focus the castle* (default) or *attack the towers* — the first slice of the unit-command system (§3, §4). Current input is keyboard for desktop testing only (number keys to play cards; C / T for orders) — see §8 for the actual mobile input design. **The key open pacing question is whether currency regenerates during an encounter (a fixed pool spent down vs. a regenerating income), which — together with the card-draw cadence — defines the moment-to-moment feel (§9).**

## 3. Longer-term Design (planned, not yet built)

### Run Structure (roguelite) — decided
- **Decision: the game is run-based, in the roguelite tradition**, not a level-select campaign. A run is a sequence of castle encounters of escalating difficulty. Losing ends the run; the player starts over, keeping some permanent progress (the "lite" in roguelite).
- **Two layers of win condition** now exist and should not be confused:
  - **Encounter win:** destroy this castle before the run's resources run out.
  - **Run win:** complete the full sequence of castles — the actual goal of a session.
- **In-run deck growth is the centrepiece.** The player begins a run with a small fixed **starter deck** and *acquires cards as the run progresses* — typically by choosing one of a few offered cards after each cleared castle. This is the main reason a run feels different each time, and it is where the run's sense of building momentum comes from.
  - This supersedes the earlier framing of "pre-made decks" as the whole story: pre-made decks remain, but as **starter** decks that define a run's opening identity, not as the fixed deck for the entire run.
  - It also largely dissolves the deferred deck-builder question (see Roadmap): the player builds a deck *by playing*, which is more interesting than assembling one in a menu beforehand. A menu deck builder may never be needed.
- **Between-castle choices are the run's texture.** After each cleared castle the player makes a meaningful choice — a card to add, a permanent buff, a resource trade. Exact menu of options undecided (§9).
- **Escalation:** later castles in a run have more towers, tougher towers, and — on some — castle-spawned defending units (see Castle Defenses).

### Run Resources — decided
**Decision: two persistent resources deplete across a run — horde strength and a run-wide wave budget.** Both always exist; what varies between runs is how much of each you start with and how fast each drains (see Run Modifiers below).

- **Horde strength** — a persistent army pool that depletes as monsters die and replenishes only *partially* between castles. This is the cost of losing units.
- **Run-wide wave budget** — a pool of waves (or time) shared across the whole run rather than reset per castle. Clearing a castle quickly banks the remainder for later castles. This is the cost of being slow.
- The run ends in defeat when either resource is exhausted before the final castle falls.

**Why two rather than one — they pull in opposite directions.** This is the point, and it is what makes the pair worth the extra complexity:
- The **wave budget** rewards committing hard and ending encounters fast: rush the castle, spend big, move on.
- **Horde strength** rewards restraint and efficiency: don't overspend units, don't feed the towers, preserve the army.

A player cannot maximise both. Every encounter becomes a negotiation between "end this quickly" and "end this cheaply" — which is exactly the rush-versus-trickle tension already validated in v1 playtesting (§5), promoted from a single-battle decision to the spine of the whole run.

**Replenishment must stay partial.** If horde strength fully restores between castles it stops being attrition and becomes a per-encounter resource, and the run-level tension evaporates. Partial restoration (a flat amount, a percentage, or a between-castle reward option) is what makes losses carry forward. Rates undecided (§9).

**Balance risk — one resource becoming decoration.** The failure mode for a two-resource design is that one of them is never the thing that actually ends runs, leaving it as flavour the player ignores. The tuning target is that **runs end for both reasons across a population of runs**. If playtesting shows one resource never binds, either tighten it or cut it; two resources are only worth it while both genuinely bite.

### Run Modifiers (run-to-run variety) — decided in shape, undecided in detail
Rather than randomising *which* resources exist, each run randomises **starting allowances and drain rates**. The rules stay constant; the pressure profile changes.

- A "blitz" run: generous horde strength, tight wave budget — favours aggressive, high-commitment play.
- A "siege" run: generous waves, scarce horde — favours patient, efficient play where every unit lost matters.
- A "balanced" run: neither constraint dominant.

**Why this shape:** randomising which mechanics are active forces the player to re-learn the system each run, which reads as arbitrary. Randomising how hard each known constraint presses asks them to *re-plan* instead — the same skills apply, but the right strategy differs. That is the difference between variety and noise.

**Practical advantages:** it is numbers rather than systems, so it is cheap to implement and cheap to author; each modifier is a small data asset. It also adds meaningful run variety without any new art, which directly offsets the content-variety risk in §7. The modifier should be **visible to the player at run start**, so it informs the starter-deck choice rather than surprising them later.

### Castles as resource-pressure design
The two resources also give a design axis for making castles feel different without new models:
- A **tower-dense castle** punishes losses and drains horde strength.
- A **high-HP fortress** with light defenses drains the wave budget instead — it is not dangerous, it is slow.
- A castle with **spawning defenders** drains both.

### Card & Hand System
- **Starter decks are pre-made.** The player begins a run with a fixed, designer-built starter deck (e.g. "Inferno Horde", "Glacier Wall" as elemental themes), then grows it during the run (see Run Structure). The data for this exists: a `DeckDefinition` asset is a starter deck (§4).
  - Pre-made starters keep balance tractable — each is tuned as a coherent whole and gives a run a recognisable opening identity — while the in-run acquisitions provide variety.
  - The deck is drawn from randomly: the player draws a hand of **X cards** (default 3, tuned by feel) during an encounter.
- Cards vary in what they summon, not just raw stats — e.g.:
  - A big, strong **boss card** — one powerful, high-cost unit.
  - A **swarm card** that summons a large number of weak units (e.g. 50 small units).
  - A **mid-tier card** that summons a moderate number of stronger units (e.g. 10 elite units).
  - A **stun/disruptor card** — summons units that disable towers rather than out-damaging them.
- The **data shape for all of these is built** — see §4. A card's summon is `spawnCount` × `spawnInterval` on a `CardDefinition` asset, so boss/elite/swarm are all the same asset type with different numbers. **Resolved 2026-09-22:** cards can now also **override unit stats** (`overrideStats` + HP/damage/speed on `CardDefinition`), so units differ in *strength* too, from the same monster prefab — a real boss vs. a real swarm. See §4, "Built: per-card unit variety."
- **Cards have a second cost dimension.** With horde strength as a run resource, a card that summons 50 units is not just expensive in encounter currency — it is expensive in horde if those units die. Swarm cards become high-risk/high-tempo (great for the wave budget, brutal on horde), while efficient elite cards become the attrition-friendly option.
- **Mulligan:** the player can redraw their opening hand (exact rules still undecided).

### Unit Commands
Once monsters are summoned, the player can issue orders to direct their behavior instead of units only auto-pathing to the castle. Planned commands:
- **Focus castle** — prioritize attacking/pathing straight to the castle.
- **Attack towers** — prioritize destroying the castle's defensive towers instead of rushing past them.
- **Hold position** — stop and hold ground rather than advancing.

**v1 built (2026-09-22, integrating):** a single **global** order — Focus Castle (default) / Attack Towers — toggled by dev keys (C / T). Monsters on "attack towers" divert to the nearest standing tower and chip its HP; on "focus castle" they path to the castle. Per-card-group scope and the Hold command come later. See §4.

Commands can eventually be issued at two scopes: **all units at once** (built), or **a specific unit type/subset** (e.g. only one card's summons — the mobile design, §8).

The commands interact directly with the run resources: "attack towers" spends horde strength to save future horde strength, while "focus castle" spends waves to preserve units. Commands are how the player expresses which resource they are choosing to conserve.

### Tower Destructibility
- **Decision: towers are permanently destructible** — within an encounter. A destroyed tower stays destroyed for the rest of that castle; each new castle in a run starts with its own intact defenses.
  - **Built 2026-09-22/23 (integrating):** `Tower` now has HP and is destroyed when monsters attack it down (it hides itself — placeholder for the collapse). See §4.
  - **Rationale:** the "attack towers" command has to feel like a real accomplishment. A rebuild timer would technically balance the board better, but it undercuts the core fantasy of tearing the defenses down. Satisfaction wins here.
  - **Why the "kill every tower first" degenerate strategy is handled:** clearing towers costs both run resources — the units spent (horde) and the waves taken (budget). An all-out demolition is only correct when it saves more than it costs.
  - **Feedback requirement:** the destruction moment needs to land visually and audibly — collapse animation, persistent rubble, screen shake, a distinct sound. Currently the tower just hides on death; the VFX/audio is an explicit art task, not a byproduct of the model work.

### Stun & Status Effects
- **Decision: some monsters/cards can stun towers.** A stunned tower stops firing for a short duration, then resumes. This exists alongside permanent destruction rather than replacing it.
- **Why this matters:** it converts a global system rule ("destructible *or* suppressible?") into a **player choice made at deck and play time**. Both answers coexist:
  - **Destroy it** — expensive in units and waves, but permanent for this encounter.
  - **Stun it** — cheap and fast, but temporary; opens a *window* to capitalise on immediately.
  Stun is the horde-efficient answer and destruction is the wave-efficient one, so the choice maps onto which run resource is currently scarce.
- **Knock-on benefits:** deck identity (stunners make a tempo deck, their absence makes a demolition deck); skill expression through timing; and a natural fit with a lightning/shock element.
- **Balance caution — stun-locking.** If stuns stack or chain freely, several stun units can keep a tower permanently disabled, which collapses back into "destruction, but cheaper." Needs a guard: diminishing returns, a short immunity window after recovery, or a cap on concurrent stun sources per tower.

### Castle Defenses
- **Every castle always has towers/buildings for self-defense** — a baseline rule across all castles. v1's single tower is the minimal case.
- Some castles go further and **spawn defending units** of their own, giving the AI defender active reinforcements. A natural escalation step for later castles in a run, and a counterweight to permanent tower destruction.

### Flying units — planned (roadmap)
A unit class that can **leave the waypoint path** once ordered and head straight for the target (castle or tower), rather than following the ground route.
- **Weaker than ground units** — how much weaker depends on the flyer type (a light scout vs. a heavier flyer), so it's a tradeoff, not a strict upgrade.
- **The point:** later the ground path will be **blocked by environmental assets** (walls, scenery), so **only flyers can bypass it**. That turns "do I bring flyers?" into a real deck-building decision against certain castle layouts.
- **Needs:** a flying flag on the monster (ignore waypoints, straight-line move to target), balancing (lower HP/damage), and a hook into the command system (ordering flyers to bypass). Tracked as a build task.

### Other Planned Systems
- **Castle progression (within an encounter):** the castle gains upgrades from kills — possibly elemental resistance tied to the monster type it last killed.
- **Meta-progression (between runs) — the "lite".** Something must persist across runs so repeated attempts feel like progress. Likely: new cards unlocked into the acquisition pool, new starter decks, new monster types. Undecided (§9).
- **Destructible castle property:** destroying parts of the castle grants bonus currency mid-encounter.
- **Castle variety:** a run needs a set of castle layouts varying in tower count, placement, HP profile, and whether they spawn defenders.

### Roadmap (deferred, explicitly not now)
- **Menu deck builder** — largely superseded by in-run deck growth. May never be needed; revisit only if in-run acquisition proves insufficient.

## 4. Systems & Architecture

### Scripts (all in `Assets/Scripts/`)
- **`GameManager.cs`** — owns currency and card-play input. `PlayCardFromHand(index)`: affordability check → deduct the card's cost → `MonsterSpawner.SpawnCard(...)` → tell `HandManager` to consume the card + redraw. Number keys 1-5 play hand slots (dev shortcut). Owns `spawnPoint`/`waypoints`. Exposes `CanPlayAnyCard` = `HandManager.AnyAffordable(currency)` for the lose-check. Does not instantiate monsters or hold a prefab — the card carries it.
- **`CardDefinition.cs`** — **a ScriptableObject: a monster card as pure data** (name, description, currency `spawnCost`, `monsterPrefab`, `spawnCount`, `spawnInterval`). Authored via **Assets ▸ Create ▸ Castle Attack ▸ Card**. Also carries optional **per-card stat overrides** (`overrideStats` + `unitMaxHP` / `unitDamage` / `unitSpeed`), so one prefab can be a boss on one card and a swarm on another (see "Built: per-card unit variety").
- **`DeckDefinition.cs`** — a ScriptableObject: a deck as an ordered `List<CardDefinition>` (duplicates allowed = multiple copies). A starter deck in concrete form. Authored via **Assets ▸ Create ▸ Castle Attack ▸ Deck**.
- **`HandManager.cs`** — the runtime card state for an encounter: a shuffled draw pile built from a `DeckDefinition`, the hand, and a discard pile. Draw-on-play; reshuffles the discard back in when the draw pile empties. Pure card bookkeeping — spends no currency and spawns nothing. Its input is "a list of `CardDefinition`s", so `RunManager` can later feed it the run's grown deck instead of a fixed asset — the small swap that connects it to the roguelite layer.
- **`HandDebugUI.cs`** — a **throwaway** IMGUI (`OnGUI`) placeholder hand: a row of buttons along the screen bottom, greyed when unaffordable, tap to play. Dev-only; delete it and its GameObject when the real uGUI/TMP hand ships (§8). Nothing else depends on it.
- **`MonsterSpawner.cs`** — **the single point of monster creation and removal, owner of the live-monster registry, and the object pool.** `Spawn()` reuses a pooled monster (or instantiates when the pool is empty), `SpawnCard()` summons a whole card's group staggered by `spawnInterval` via a coroutine (and applies a card's stat override to each summon), `Despawn()` deactivates and returns a monster to its pool. Pools are kept per-prefab; optional `prewarmPrefab`/`prewarmCount` pre-instantiate inactive monsters at startup. Plus a static registry (`ActiveMonsters`) that monsters keep current.
- **`MonsterMover.cs`** — moves a monster along its waypoints; has HP and `TakeDamage()`. Self-registers with the spawner in `OnEnable`/`OnDisable`. All death paths route through a `Die()` helper → `MonsterSpawner.Despawn(this)`. `OnSpawn(waypoints)` resets per-life state (HP, waypoint index, `enabled`, **and restores the prefab's default stats**) on every spawn — the pooling-correctness hook; `ApplyStats()` then re-applies a card's stat override. **Reads `UnitCommander.Current`:** on the *AttackTowers* order it diverts to the nearest standing `Tower` and chips its HP (`attackRange`, `attacksPerSecond`) instead of following the path, reverting to the path when no towers remain. `SourcePrefab` records which pool it returns to.
- **`Castle.cs`** — has HP and `TakeDamage()`; sets a public `isDestroyed` flag when HP reaches 0.
- **`Tower.cs`** — on a fire-rate timer, finds the closest monster **by iterating `MonsterSpawner.ActiveMonsters`** (not scanning the scene) and fires a **`Projectile`** at it (falls back to instant-hit if none is assigned). **Now destructible:** has `maxHealth` + `TakeDamage()` + an `IsDestroyed` state (hides itself on death — placeholder for rubble/collapse). Keeps a **static `StandingTowers` registry** so attacking monsters find the nearest one. Still needs a stunned state.
- **`Projectile.cs`** — the tower's shot: a placeholder sphere that flies to its target and deals damage on arrival; fizzles at the last known spot if the target dies mid-flight. Swap the mesh for an arrow later without touching the script. Not pooled yet (low volume).
- **`UnitCommander.cs`** — holds the horde's single **global order** (`FocusCastle` / `AttackTowers`) as a static, toggled by dev keys C / T. The mobile per-card-group selection (§8) will replace the keys later without changing how `MonsterMover` reads the order. Resets to FocusCastle on scene load.
- **`UIManager.cs`** — pushes live currency/castle-HP values to TextMeshPro text every frame.
- **`GameOverManager.cs`** — the single authority on encounter win/lose state (below). Its lose-check reads `GameManager.CanPlayAnyCard` and `MonsterSpawner.ActiveMonsters.Count`. Also owns `RestartGame()`.
- **`Assets/Editor/GameplayRigSetup.cs`** — one-time **editor** utility (menu **Castle Attack ▸ Setup**) that built the GameplayRig prefab + Sandbox scene. Editor-only; safe to delete.
- **`Assets/Editor/TowerSetup.cs`** — one-time **editor** utility (menu **Castle Attack ▸ Setup ▸ 3**) that placed the tower model with `Tower.cs` and saved `Tower.prefab`. Editor-only; safe to delete.

### Design decision: centralized win/lose arbitration
Originally, `GameManager` (lose) and `Castle` (win) each independently decided game-over state in their own `Update()`. Because Unity does not guarantee execution order between scripts, this produced a real race condition: in some frames the lose-check fired and locked in `gameOver = true` before the win-check ran, showing "YOU LOSE" even though the castle had just been destroyed.

**Fix:** `GameOverManager` is the single source of truth. Every frame it checks win *before* lose:
1. If `castle.isDestroyed` → show win, stop.
2. Else if the player can't play any card and no monsters remain alive → show lose.

Win always takes priority, and the dependency on script execution order is gone. `GameManager` and `Castle` only expose state now.

**Note for the run structure:** this arbitration will need extending. Encounter-level lose is no longer the only failure — the run also ends when horde strength or the wave budget is exhausted. The same principle applies: one authority checks all end conditions in a defined priority order. The bug that produced this rule is exactly the bug that recurs if run-level conditions get scattered.

### Built: encounter retry (debug/playtest loop)
**Done.** `GameOverManager.RestartGame()` resets `Time.timeScale` to 1 and reloads the active scene. Reachable two ways: a **RETRY button** on the game-over screen (a disabled-by-default `RetryButton` under the Canvas, wired to `RestartGame()`), and the **R key** while game-over is up (faster for iteration). Both tested.

This is deliberately the *debug* loop, not the player-facing post-run flow — once the run structure exists, a player-facing "retry" means starting a new run, while this scene reload stays useful for tuning a single encounter.

### Built: spawner + live-monster registry — done 2026-09-18
All monster creation and removal routes through **`MonsterSpawner`**, and nothing scans the scene for monsters any more.

- **`MonsterSpawner.Spawn(prefab, position, waypoints)`** — the only place monsters are created; pool-backed.
- **`MonsterSpawner.Despawn(mover)`** — the only place monsters are removed; returns to pool.
- **Registry:** a `static List<MonsterMover>` exposed read-only as `ActiveMonsters`. Monsters `Register`/`Unregister` in `OnEnable`/`OnDisable` — stays correct when a pooled object toggles active/inactive. Static so registration never depends on `Instance`. `Tower` iterates it defensively.
- **Consumers switched off `FindObjectsByType`:** `Tower` and `GameOverManager` read the registry now. Zero remaining scene-scan deprecation warnings.

The registry is also the natural hook for horde-strength accounting later (the spawner owns every birth and death), and `Tower.StandingTowers` mirrors the same pattern for towers.

### Built: card system (data-driven) — done 2026-09-18
The prototype plays a **card**, not a hardcoded spawn. `CardDefinition` is a ScriptableObject; `MonsterSpawner.SpawnCard` runs a coroutine that summons `spawnCount` monsters staggered by `spawnInterval`; every summon goes through `Spawn`, so the registry and pooling apply automatically. This is the data layer the hand & deck sit on.

### Built: hand & deck system — done 2026-09-18
Play is driven by a **hand of cards drawn from a deck**, with a placeholder UI. `DeckDefinition` → `HandManager` builds a shuffled draw pile, draws an opening hand (`handSize`, default 3), and on each play discards + draws a replacement (draw-on-play); reshuffles the discard when the draw pile empties. `GameManager.PlayCardFromHand(index)` is the play authority. `HandDebugUI` is a throwaway IMGUI hand (greyed when unaffordable) until the real uGUI hand ships. Cards authored: `Card_LoneGrunt` (1×/20), `SwarmCard_Grunts` "Grunt Rush" (5×/40), `Card_BigPush` (10×/60); `StarterDeck` = 3× Lone Grunt, 2× Grunt Rush, 1× Big Push. Verified in play. **Deferred:** the pacing model (currency regen? timed draw?) — decide by feel (§9).

### Built: object pooling — done 2026-09-18
Monsters are reused instead of instantiated/destroyed — no per-unit churn or GC hitch once the pool is warm. `MonsterSpawner` keeps one pool per prefab; `Spawn` reuses/instantiates, `Despawn` deactivates + enqueues. **`MonsterMover.OnSpawn` is the correctness core:** `Start()`/field initializers don't run on a reused object, so `OnSpawn` re-sets HP, waypoint index, `enabled`, **and now the default stats** on every spawn — anything that becomes per-life state (status effects, buffs, stat overrides) must reset there. Verified: a Grunt Rush (5×) after a death spawned exactly 5 (1 reused + 4 new), all full-HP from the spawn point; 0 errors.

### Built: shared GameplayRig prefab + Sandbox scene — done 2026-09-18
The interdependent gameplay/UI objects are grouped under one **`GameplayRig`** prefab (`Assets/Prefabs/GameplayRig.prefab`); `SampleScene` (canonical) and `Sandbox` (scratch) both instance it, so a wiring change is made **once, in the prefab**, and both scenes inherit it. Environment (camera, light, ground, volume) stays per-scene. Both scenes are in Build Settings. **Team rule: edit the prefab, not a scene's copy.** The **tower** (below) was later added into this prefab, so both scenes have it. (A leftover inactive `TestMonster` still sits at scene root in both scenes — harmless; clean up whenever.)

### Built: gf's tower model in-game — done 2026-09-19
The first real **art asset** replaces a gray-box: the girlfriend's Blender tower stands beside the path as a working defensive `Tower` (its `Tower.cs` fires at monsters). It's a nested prefab inside the shared `GameplayRig`, so both scenes have it; `Assets/Prefabs/Tower.prefab` is the reusable tower.

**FBX import gotcha — worth remembering, it cost real time:** the export arrived with the *whole Blender scene* baked in — a **camera**, a **light**, six **backdrop planes**, and a **~100× object scale**. In Unity the embedded camera **rendered over the game view** (it looked like the tower was filling the screen), the light threw a harsh spotlight, and the un-applied scale made the model enormous. Unity-side fixes applied: model import **Import Cameras off**, **Import Lights off**, and a **Scale Factor** (~0.35 as a stopgap). The real fix is a clean re-export — recipe now in the art repo README (§11): **select only the mesh, apply all transforms, tick "Selected Objects", mesh-only.** A properly-exported mesh needs none of these Unity-side hacks and should sit at Scale Factor 1. A clean re-export from the artist is a pending follow-up.

### Built: per-card unit variety — done 2026-09-22
`CardDefinition` gained optional stat overrides (`overrideStats` + `unitMaxHP` / `unitDamage` / `unitSpeed`). When on, `MonsterSpawner.SpawnCard` calls `MonsterMover.ApplyStats(...)` on each summon, so the **same monster prefab** is a tough boss (high HP/damage, slow) on one card and a weak swarm on another — cards differ in unit *strength*, not just count. **Pooling-safe:** `MonsterMover` caches the prefab's defaults in `Awake`, and `OnSpawn` restores them every spawn *before* any override is applied, so a reused monster never keeps a previous card's boss stats. This resolves the "same-prefab" limitation noted in §3.

### Built (integrating): destructible towers + visible projectile + unit commands — 2026-09-22/23
The "destruction should feel earned" pillar starts here. Code delivered; being pasted/tested in the editor.
- **`Tower` is destructible:** `maxHealth` (100 first-pass) + `TakeDamage()` + `IsDestroyed`; hides on death (placeholder for the collapse/rubble + sound). A static `StandingTowers` registry lets attackers find the nearest.
- **Visible projectile:** `Projectile.cs` — the tower spawns a placeholder sphere that flies to the target and deals damage on arrival (swap the mesh for an arrow later). Instant-hit fallback if unassigned. Not pooled yet.
- **Unit commands (v1):** `UnitCommander` holds one global order — **FocusCastle** (default) or **AttackTowers** — on dev keys **C** / **T**. `MonsterMover` reads it: on AttackTowers it moves to the nearest standing tower and hits it (`attackRange` 1.5, `attacksPerSecond` 1, dealing its `damage` per hit); when no towers remain it returns to the path.
- **Scope decision:** one global toggle for now; the mobile design is per-card-group selection (§8), which will replace the keys without changing how monsters read the order.
- **Editor setup:** a placeholder projectile prefab (sphere + `Projectile`, no collider) is assigned to `Tower.prefab`; a `UnitCommander` GameObject lives in the `GameplayRig` prefab. Numbers are first-pass, to be tuned.

### Anticipated: run state must outlive the encounter scene
The roguelite structure introduces state spanning multiple encounters — deck, run progress, horde strength, remaining wave budget, active run modifier, meta-progression. That needs a persistent `RunManager` (via `DontDestroyOnLoad` or a bootstrap scene), with `GameManager` handling only the current encounter. `HandManager` is already shaped for this: feed it the run's deck instead of a fixed `DeckDefinition`. Drawing this line early is the difference between "the encounter scene owns everything" (today) and "the encounter scene is handed a context and reports a result" (what the roguelite needs). This boundary also makes save/load cheap — see the next decision.

### Design decision: save/load — design for it now, build it later (2026-09-18)
**Decision: implement no save system yet, but make the two cheap choices that keep adding it later an afternoon's work rather than a painful refactor.** Nothing stable to save today. Two free-now choices: (1) **separate data from behaviour** — `RunManager`'s state lives in a plain serializable POCO (`RunState`/`PlayerData`), so "saving" is just JSON to `Application.persistentDataPath`; (2) **version the save format from the first write** (`int saveVersion`) to avoid painful migrations later. What needs saving, and when: settings (trivial, `PlayerPrefs`, anytime); meta-progression (build saving when it exists); mid-run resume (matters on mobile — OS kills backgrounded apps — prioritise once runs exist). Do **not** use `PlayerPrefs` for structured run/meta state; serialize a POCO to JSON.

### Anticipated: a general status-effect system
Stun is the first status effect, not the last — slows, DoT, armor reduction follow. Worth a small general system (effect type, duration, source) rather than a hardcoded `isStunned` bool on `Tower`. One place for the anti-stun-lock guard, and one place to clear effects when a pooled object recycles — which `MonsterMover.OnSpawn` already establishes as the reset point.

### Other technical notes
- **FBX import (see the tower):** for any Blender export, turn **Import Cameras/Lights off** on the model importer, and expect to set a **Scale Factor** unless the artist applied transforms. Best fixed at the source (§11 export recipe).
- **Input System:** Active Input Handling = "Both", required for `Input.GetKeyDown`.
- **TextMeshPro:** anchor/pivot must match; a wide left-aligned box pushes centred text off-screen — check TMP alignment, not just the Rect Transform.
- **`Time.timeScale` persists across scene loads.** Game-over sets `0f`; any reload must set `1f` or the new scene comes up frozen.
- **Reloading a scene from a UI Button** throws `MissingReferenceException` unless the EventSystem selection is cleared first — `RestartGame()` calls `EventSystem.current.SetSelectedGameObject(null)` before `LoadScene`.
- **A scene must be in Build Settings** to `LoadScene` it. `SampleScene` (0) and `Sandbox` (1) are registered.
- **Unity doesn't always auto-detect externally edited scripts** — `Ctrl+R` forces the reimport. External `.meta` edits (e.g. import settings) may need a right-click **Reimport**, not just Refresh.
- **`[CreateAssetMenu(menuName = "Castle Attack/...")]`** — the pattern for every future data asset (run modifiers, castle definitions).
- **Unity 6 deprecations:** `FindObjectOfType` → `FindAnyObjectByType`; `FindFirstObjectByType` is *also* deprecated.
- **Pooling reset lives in `MonsterMover.OnSpawn`.** Any new per-life state (buffs, effects, stat overrides, animation state) must reset there — `Start()` won't run on a reused object.
- **`ProjectSettings/*.asset` (Build Settings)** only flush on a project save (File ▸ Save Project / quit), not a scene save.

## 5. Current v1 Balance (tuned via playtesting)

| Parameter | Value |
|---|---|
| Castle Max HP | **40** |
| Tower Range | 6 |
| Tower Fire Rate | 1 shot/sec |
| Tower Damage | 15 per hit |
| Monster Max HP | 30 |
| Monster damage to castle | 10 |
| Starting Currency | 100 |
| Hand size | 3 |

**Cards (per-asset data):**

| Card | Cost | Summons | Interval |
|---|---|---|---|
| Lone Grunt | 20 | 1× | — |
| Grunt Rush | 40 | 5× | 0.3s |
| Big Push | 60 | 10× | 0.2s |

Starter deck: 3× Lone Grunt, 2× Grunt Rush, 1× Big Push (6 cards). Cards may also carry stat overrides (off by default).

**Towers & attacking them (new, first-pass — untuned):** Tower `maxHealth` 100; fires a visible projectile (speed ~20). A monster ordered onto a tower: `attackRange` 1.5, `attacksPerSecond` 1, dealing its `damage` per hit. So one grunt (10 dmg) takes ~10s to fell a 100-HP tower alone; a group is much faster.

**Playtested behavior (core loop):**
- A single monster spawned alone dies to the tower (2 hits) before reaching the castle — trickling is a guaranteed loss.
- Spawning 5 at once overwhelms the tower's 1-kill/sec rate: it kills 1, the other 4 get through for 40 damage — exactly enough to destroy a 40 HP castle. Now the "Grunt Rush" card; re-confirmed after pooling. (With the corner defensive tower added, fewer get through — the extra tower visibly strengthens the defense.)

**This result is load-bearing.** The rush-versus-trickle tension it validated is precisely what the two-resource run economy generalises. Numbers are a baseline; the *shape* is the target.

Parameters still to tune: tower HP, monster attack rate/range, stun duration & stun-lock thresholds, starting horde strength, horde replenishment, run wave budget, per-modifier allowances, difficulty curve. **Currency income rate is undecided pending the pacing decision (§9)** — currently a fixed pool, no regeneration.

## 6. Visual Direction

- **Style:** semi-3D — low-poly 3D models, fixed/angled camera, 2D UI overlay.
- **First art in-game (2026-09-19):** the girlfriend's Blender **tower** model replaced the gray-box tower (§4). Everything else is still placeholder — cube castle, capsule monster, IMGUI hand, sphere projectile.
- **Art plan:** castle first in Blender (low-poly, modular pieces, flat shading, bold palette, camera angle decided before modeling) since it sets the visual language. Then one monster archetype. Then UI/UX once the palette exists. See the castle-design-brief doc.
- **Modularity matters more now.** A roguelite needs several visually distinct castles — build a reusable kit (walls, towers, gates, keeps) recombined into layouts. The single highest-leverage art decision for run variety.
- **Destruction states are a modeling requirement.** Towers need intact and destroyed/rubble states, ideally a collapse. Currently a destroyed tower just hides; the **rubble/collapse + screen-shake + sound is a real art/VFX task** (the "destruction should feel earned" pillar). A modular kit makes each destroyed variant cheap.
- **Stunned must read instantly** and differently from destroyed — temporary language (arcs, sparks, a duration ring) vs. permanent (rubble, smoke, missing geometry).
- **Swarm units read as a mass, not 50 individuals.** Silhouette and colour over detail, low poly, minimal unique animation.
- **Deck identity:** each starter deck is an art package (shared palette + creature family). Each card also needs card art/frame for the hand UI.
- **Source art stays out of the game repo.** Working `.blend`/`.psd` live in the art repo; only exported game-ready `.fbx` + compressed textures are committed. Follow the **FBX export recipe** (§11) — the tower import proved why.
- **Who's doing what:** the girlfriend does 3D art and UI/UX. The **card hand UI** is the concrete, ready-to-design first task now that a working hand exists to design against (§8).

## 7. Design Risks / Recommendations

- **Content variety is the main risk, and it lands hardest on art.** Roguelites live or die on runs feeling different — several castle layouts, a deep card pool, enough monster types. Four load-bearing mitigations: a **modular castle kit** (§6), **data-driven cards/decks with per-card stat overrides** (in place — §4), **run modifiers** (§3, variety via numbers not art), and **castles as resource-pressure profiles** (§3).
- **Two-resource tuning is harder than one.** Right design, but double the balance surface and a risk one resource never binds. Instrument playtests to record *why* each run ended.
- **Scope.** With a dedicated artist, a second programmer, and no deadline, the full list is reasonable. Keep a rough build order (§9).
- **Task/role split.** Spawner/registry, first card, hand/deck, object pooling, shared-rig/Sandbox, the **tower art asset**, and **per-card unit variety** are done (§4); **destructible towers + unit commands + projectile** are integrating. Good next pieces: the **card hand UI** (girlfriend), the **currency/pacing model**, the **swarm card**, and **flying units** (§3). Art track: modular castle kit → first monster archetype → UI/UX, plus tower-destruction VFX. The incoming developer can take the swarm card or flying units.
- **Tower targeting — decided.** Monsters ordered to attack towers target the **nearest** one (built as nearest standing tower). Player-designated targeting is a later refinement.
- **Save/load — design for it now, build later (§4).**
- **Data-driven content — done (§4).** Reuse the `CreateAssetMenu` SO pattern for run modifiers and castle definitions.

## 8. Mobile Input / Controls

- **Decision: card-group selection, not free-form unit picking.** Units are selected for commands by tapping the icon of the card that summoned them — reuses the card as the selection handle, avoids fiddly drag-select, supports both command scopes. (The current build uses a single global order on dev keys; this per-card scope replaces it, §4.)
- Playing a card to summon works via direct tap.
- The command flow (tap card icon → tap a command button) needs prototyping — a UI/UX pass candidate.
- **No tower-tapping in the first version**, since tower targeting is automatic.
- Stun windows are time-critical — remaining stun duration must be legible.
- **Two run resources must be readable at a glance.** Horde strength and remaining wave budget both need persistent, instantly-scannable readouts.
- **The card hand UI is the immediate, ready-to-design UI/UX task.** A working hand exists behind the IMGUI placeholder (§4), so the real hand — cards, cost/affordability, tap to play, played card animating out and the next drawing in — can be designed against something real.
- **Other new screens the roguelite requires:** run-start (starter deck choice + active modifier), between-castle reward/choice, run-summary on death, meta-progression.

## 9. Build Order & Open Questions

### Agreed build order
1. ~~**Source control + Unity merge setup + Git LFS**~~ → **done** (§11).
2. ~~**Debug retry loop**~~ → **done** (§4).
3. ~~**Spawner + live-monster registry**~~ → **done 2026-09-18** (§4).
4. ~~**First card, end to end (data layer)**~~ → **done 2026-09-18** (§4).
5. ~~**Hand & deck**~~ → **done 2026-09-18** (§4).
6. ~~**Object pooling**~~ → **done 2026-09-18** (§4).
7. ~~**Shared GameplayRig prefab + Sandbox scene**~~ → **done 2026-09-18** (§4).
8. ~~**gf's tower model in-game**~~ → **done 2026-09-19** (§4). First art asset; also surfaced the FBX-export recipe (§11).
9. ~~**Per-card unit variety**~~ → **done 2026-09-22** (§4). Stat overrides on `CardDefinition`.
10. **Integrating: destructible towers + visible projectile + unit commands** (§4) — `Tower` HP/registry, `Projectile`, `UnitCommander` global toggle, monster attack-tower behavior. Code delivered 2026-09-22/23; being pasted + tested.
11. **Next, in parallel:**
    - **Card hand UI** (real uGUI/TMP, replaces the placeholder) — girlfriend / UI-UX. §8.
    - **Pacing / currency model** — design-by-feel on the built hand.
    - **Swarm card** — first real stress test of the pool (assign `prewarmPrefab`).
    - **Flying units** (§3) — new unit class that leaves the path; the incoming developer or the user.
    - **Clean tower re-export** from the artist (drops the Unity-side scale hacks).
    - **Tower-destruction VFX** — rubble/collapse + sound (art track).

### Open questions
- **Pacing / currency-regeneration model + card-draw cadence.** Currently fixed pool + draw-on-play. Does currency regenerate (Clash-Royale income)? Do cards draw on a timer? Decide by *playing*. (Lean, unconfirmed: income + small hand + draw-on-play.)
- Hand size and mulligan rules (default 3; tune by feel).
- **Flying units:** HP/damage balance vs. ground units, and how "order flyers to bypass the blocked path" fits the command system.
- **Tower HP and monster attack rates** — first-pass 100 HP / 1 hit-per-sec; tune once destructible towers are in play.
- **Projectile:** pool it? (currently Instantiate/Destroy — fine at low volume; revisit if towers/shots multiply.)
- Starting values/drain rates for horde strength and the wave budget; partial replenishment rate.
- The set of run modifiers and how much they skew allowances.
- Run length: how many castles, difficulty curve.
- What the between-castle choice offers, and how many options.
- What persists between runs — the "lite".
- Whether the wave budget is literally waves or elapsed time.
- Stun tuning: duration, on-hit vs. area, anti-stun-lock guard; whether stun works on spawned defenders.
- How many starter decks ship initially, and their identities.
- Full list of unit commands beyond the three named (per-card scope + Hold still to build).
- What castle progression looks like within an encounter.
- Overall card pool size and balance.
- Save/load *implementation* details (stance decided, §4).
- Final game name (§10).
- Where shared cloud storage for source art lives (§11).

### Resolved
- ~~Starting deck: random, or fixed pre-built?~~ → **pre-made starter decks** (a `DeckDefinition`), grown during the run.
- ~~Towers destructible, suppressible, or rebuilding?~~ → **permanently destructible** within an encounter (built 2026-09-22/23); the run economy balances it.
- ~~Should suppression/stun exist instead of destruction?~~ → **both**; destruction default, stun a card.
- ~~Which tower do monsters attack?~~ → **nearest** standing tower.
- ~~Cards differ only in count (same-prefab limitation)?~~ → **per-card stat overrides on `CardDefinition`** (done 2026-09-22) — units differ in strength too.
- ~~Unit-command scope for v1?~~ → **one global FocusCastle/AttackTowers toggle** now (dev keys C/T); per-card-group selection later (§8).
- ~~Do tower shots need to be visible?~~ → **yes** — a placeholder `Projectile` now, arrow art later.
- ~~Pool objects, and when?~~ → **done 2026-09-18** — spawner + registry, then the pool, before the swarm card.
- ~~A separate test scene, and how to keep it in sync?~~ → **shared `GameplayRig` prefab** (done 2026-09-18).
- ~~Level-based or run-based?~~ → **run-based roguelite**; in-run deck growth the centrepiece.
- ~~What depletes across a run?~~ → **both horde strength and a run-wide wave budget**.
- ~~Randomise the resources themselves?~~ → **no — randomise allowances/drain rates** (run modifiers).
- ~~Cards/decks hardcoded or data?~~ → **ScriptableObject assets** (done, §4).
- ~~How does the hand refill?~~ → **draw-on-play**; currency income/timed draw is the open pacing question.
- ~~Build save/load now or later?~~ → **design for it now, implement later** (§4).

## 10. Naming (undecided)

"Castle Attack!" is a working title — clear but generic, hard to search, describes the genre not the twist.

**Availability checked (2026-09-17):**
- **Overrun** — ruled out (existing mobile TD games + a Steam title).
- **Hordebound** — clear (only a small itch.io project). Echoes horde-strength.
- **Monstertide** — clear.
- **Gatecrasher** — crowded.
- **Hand of Ruin** — free but reads derivative of *Hand of Fate*.

**Current shortlist:** Hordebound, then Monstertide. Still to do: search the App Store / Play Store apps directly and check domains. A two-part name keeps store keywords without a clumsy title — e.g. *Hordebound — Monster Deck Siege* (title = icon label ≤ ~12 chars; subtitle carries search terms).

## 11. Source Control & Asset Pipeline — set up 2026-09-17

**Repos:** game code at https://github.com/Kami662/CastleAttack, source art at https://github.com/Kami662/CastleAttackBlenderFiles (both private, MIT licensed). Initial commit `fd4743b` — 175 files; `Library/`, `Temp/`, `Logs/`, `UserSettings/` and generated IDE files excluded.

### Onboarding: one command
After cloning either repo, run **`setup-dev.ps1`** (Windows) / **`setup-dev.sh`** (macOS/Linux) from the repo root. It locates Unity (game repo), registers the SmartMerge driver, and initialises Git LFS — both fail *silently* otherwise (SmartMerge's driver definition lives in the un-committed `.git/config`; LFS-less clones yield pointer text instead of assets). **The incoming developer must run it before their first commit/clone.** Verified on the user's machine (Unity 6000.6.1f1, LFS 3.7.1). Run from **PowerShell**, not cmd.

### Git LFS
Set up in **both** repos before any art landed. GitHub's LFS quota is **per account, not per repo** — both share **1 GB storage / 1 GB bandwidth per month** (bandwidth counts every clone/pull). Data pack: $5/month for 50 GB. The game repo tracks `*.fbx` via LFS, so **art commits are done from the user's own machine** (git-lfs isn't available in the assistant's sandbox). The art repo tracks `.blend`/`.psd`/`.fbx`/textures/audio, with a Blender `.gitignore` and a `source/{castle,monsters,props}` + `exports/` + `reference/` layout.

### Art pipeline rule + FBX export recipe
**Source art stays out of the game repo** — working `.blend`/`.psd` live in the art repo; only game-ready `.fbx` + compressed textures go into `Assets/`. Commit at meaningful points, not every save (LFS quota is forever); `File → Clean Up → Purge Unused Data` first.

**FBX export recipe (added 2026-09-19, written into the art repo README)** — after the first tower import baked in a camera, a light, six backdrop planes, and a ~100× scale, which hijacked Unity's game view and mis-sized the model. Export clean instead:
1. **Select only the mesh(es)** you want in-game — nothing else.
2. **Object → Apply → All Transforms** (`Ctrl+A`) so scale bakes to 1.
3. **File → Export → FBX**, tick **Limit to → Selected Objects**.
4. **Include → Object Types → Mesh** only.
Unity-side safety net (the programmer): on the model importer, untick **Import Cameras** and **Import Lights**, set a sensible **Scale Factor**. A clean export needs none of this and sits at Scale Factor 1.

### Working conventions
- Don't edit the same scene simultaneously; prefer prefabs over scene objects for shared things; commit `.meta` files with their assets; never commit `Library/`.
- **Edit the `GameplayRig` prefab, not a scene's copy** (§4). `SampleScene` is canonical; `Sandbox` is scratch.

### Cross-surface context (2026-09-22)
So every surface (claude.ai chat, **Claude Code**) and the incoming developer share the same context:
- The **claude.ai Project doc** is the source of truth for the design.
- This file is a copy committed to the game repo as **`docs/game-design-document.md`**, plus a **`CLAUDE.md`** at the repo root that Claude Code auto-reads at session start (project overview, architecture, conventions, status).
- **Re-sync this copy from the Project doc after a doc pass.** Memory (claude.ai) does *not* cross into Claude Code — the repo files carry the context instead.

### Repo status (2026-09-23)
- **SSH keys are set up** — pushes work without a personal access token.
- **Pushed through `4125272`:** data-driven cards, hand & deck, object pooling, GameplayRig + Sandbox.
- **Committed, not yet pushed:** this `docs/` copy + `CLAUDE.md`.
- **Still uncommitted in the working tree:** the girlfriend's tower art (`Assets/Art/`), `Tower.prefab`, `TowerSetup.cs`, per-card unit variety (`CardDefinition` / `MonsterSpawner` / `MonsterMover`), and the destructible-tower / projectile / unit-command scripts. These need a commit from Kevin's machine (LFS handles the `.fbx`).
- **Known snag:** `Assets/Scripts/UnitCommader.cs` is misspelled — the class inside is `UnitCommander`, and Unity requires the filename to match the MonoBehaviour class name or the component cannot be added. Rename the file in the Unity Project window.
- **Cleanup:** `_to_delete/` in each repo root holds stale git lock files; delete when convenient.
