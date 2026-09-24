# Castle Attack! — Game Design Document

_Living document — update as decisions change. Last updated: 2026-09-23._

_Working title: **Castle Attack!** — a placeholder, not a final choice. See §10 for the naming shortlist. Repo: **https://github.com/Kami662/CastleAttack** (the Unity project folder is still `ElementalCardTD`; both rename easily later)._

_This file is a mirror. The source of truth is the claude.ai Project doc; re-sync this copy after a doc pass. A `CLAUDE.md` at the repo root carries the short version for Claude Code (§11)._

## 1. Concept

A **roguelite** tower-defense game with the roles reversed, designed touch-first — shipping on PC first, mobile after (see Platform below). The player is the **attacker**: instead of placing towers, they play monster cards from a deck and send waves of monsters down a path toward an AI-controlled castle. The castle defends itself with its own towers, which auto-target and damage the monsters as they approach. A run is a sequence of increasingly well-defended castles; losing ends the run and sends the player back to the start with some permanent progress kept.

- **Platform — decided 2026-09-24: PC first, mobile after, designed touch-first** (built in Unity, C#).
  - **Alpha / v1 ships on PC:** a **WebGL build on itch.io** (a link anyone can play in the browser — the best format for a portfolio piece) plus a **Windows** build.
  - **Mobile (Android, then iOS) comes after alpha.** Development and testing already happen on Windows, and phone builds add signing, installs and per-device performance work before anyone can play — worth doing once the game is fun, not before. iOS also needs a Mac.
  - **Why touch-first anyway:** a UI designed for touch works with a mouse, but a mouse-first UI (hover, small targets, right-click, keyboard) doesn't survive the move to phones. Following the touch-compatible rule (§8) keeps the later Android port to build settings + performance work, not a redesign.
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

A hand of cards drawn from a deck drives play (§4, "Built: hand & deck system"), and the player can now issue a global **order** to the horde — *focus the castle* (default) or *attack the towers* — the first slice of the unit-command system (§3, §4). Current input is keyboard for desktop testing only (number keys to play cards; C / T for orders) — see §8 for the actual touch-first input design. **Pacing is decided (2026-09-24): a hybrid budget** — currency regenerates up to a holding cap, each encounter has a finite regeneration budget, and destroying a tower pays a bounty (§3, "Encounter pacing"). Not built yet; the current build still uses the fixed 100 pool.

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

### Encounter pacing (hybrid budget) — decided 2026-09-24, not yet built
- **Regeneration:** currency ticks up at a steady rate, up to a **holding cap** — you can't bank more than the cap, so one giant stockpiled push is limited.
- **Encounter budget:** each encounter has a total amount regeneration can produce; once it's used up, regeneration stops. The budget **keeps draining while you sit at the cap** (overflow is wasted — use it or lose it), so encounters stay about 2 minutes even if someone plays passively.
- **Tower bounty:** destroying a tower pays a one-off bounty, **on top of both the cap and the budget** — aggression directly extends your encounter. This gives the Attack Towers order a clear payoff in the alpha and makes tearing down defenses feel earned (pillar 1). No bounty for castle damage for now (castle breach milestones were considered — a later option, tied to destructible castle parts).
- **Lose:** budget spent **and** no affordable card **and** nothing alive (the existing check plus "budget spent"). Win is unchanged (castle destroyed).
- **Card draw:** stays draw-on-play.
- **Target length:** about **2 minutes per encounter** → a 3-castle alpha run takes about 6–8 minutes.
- **Starting numbers (tune in the Phase 1 balance pass):** start 50 · +4/s · cap 100 · budget 360 (≈90s of regeneration) · tower bounty 40. Castle HP and tower counts will need to rise to match — roughly 5× more currency per encounter than today's 100.
- **Why this shape:** it keeps the validated rush-vs-trickle tension (§5), now as "save up vs. spend now"; the finite budget keeps the current lose condition working; and the per-encounter budget is the natural precursor of the run-wide wave budget below (post-alpha).
- **HUD needs:** current currency, the cap, and budget remaining.

### Alpha run rules — decided 2026-09-24, not yet built
- **One lost castle ends the run** — no second chance. A run is only 6–8 minutes, so starting over is cheap and every castle stays tense.
- **Nothing carries between castles except the deck** (no currency, no HP). The two run resources below are post-alpha.
- **After each cleared castle** (castles 1 and 2 — beating castle 3 wins the run, so the reward screen appears twice per run):
  - **Pick 1 of 3** new cards from the card pool, added to the run deck.
  - **Trade 1 (optional):** give up one card from your deck for a **random** different card from the pool — a gamble, mostly useful for getting rid of a card you don't want.
- **Win bonuses** (shown on the reward screen):
  - **All towers destroyed** → pick from **4** instead of 3.
  - **Won with budget left** (threshold TBD, e.g. ≥ 25%) → the offer includes a **rare** card. Needs a rarity tier on `CardDefinition`.
- **Castles:** 3 per run, always in the same order (1 → 2 → 3). Each has **its own layout** (its own single path, castle and tower placement) chosen by a `CastleDefinition` asset, which also sets **castle HP** and **tower strength** (HP / damage / range multipliers). **Pacing numbers stay global** — every castle has the same currency budget, so later castles' extra HP and tougher towers must still be beatable within it (check in the balance pass). Per-castle reward pools, random castle order and multi-path castles are post-alpha.
- **Card roster (decided 2026-09-24): 8 cards**, all using the one grunt model with stat/scale overrides. First-pass numbers, tuned against the new pacing in the Phase 1 balance pass:

  | Card | Role | Summons | First-pass stats | Cost |
  |---|---|---|---|---|
  | Lone Grunt | cheap chip damage | 1× | default grunt | 20 |
  | Grunt Rush | the validated rush | 5× | default grunt | 40 |
  | Big Push | big stream | 10× | default grunt | 60 |
  | Swarm | flood | 50× | HP 5, dmg 1, speed 4, scale 0.5 | 80 |
  | **Brute** | damage boss | 1× | HP 300, dmg 30, speed 1.5, scale 1.8 | 70 |
  | **Tank** | damage sponge — soaks tower fire so others slip past | 1× | HP 400, dmg 5, speed 1.2, scale 1.5 | 50 |
  | **Sapper** | tower breaker — starts a "demolition" deck (bounties, destroy-every-tower bonus) | 3× | HP 25, dmg 10, **×4 damage vs. towers** | 50 |
  | **Runner** | speed — races past towers, starts a "speed" deck (win-with-budget-left bonus) | 4× | HP 15, dmg 5, speed 7, scale 0.8 | 40 |

  Sapper needs one new card stat (bonus damage vs. towers), reset per life in `MonsterMover.OnSpawn` like the other overrides. On the field the three new unit types need a visual tell beyond size (a prop or color) — an art item.
- **Starter deck (decided 2026-09-24): 10 cards** — 4× Lone Grunt, 3× Grunt Rush, 2× Big Push, plus **1 wildcard slot**: a random pick from Sapper, Runner or Tank at the start of each run. Decks aim for **about 10 cards** so draws vary more (hands rarely repeat). At ~14 cards played per castle under the new pacing, you cycle the deck about once per castle, so a newly won card shows up once or twice per castle. The wildcard makes each run lean a different way from castle 1 — demolition (Sapper: Attack Towers + bounties), speed (Runner: the budget-left bonus) or soak (Tank) — while the 9 fixed cards keep balance predictable. It varies the starting deck, not the rules (pillar 3). Future starter decks (post-alpha) also target ~10.
- **Reward pool:** Swarm and Brute (**rare** — offered via the budget-left bonus), Tank, Runner and more Sappers (common). Swarm leaves the starter deck — it was only there for the pool stress test. With ~10-card decks each single reward matters less than in a 6-card deck, which is part of why the rares should feel strong and the trade step stays.
- **Why the bonuses:** they pull in opposite directions — thorough (spend units and time to raze everything) vs. fast (win before the budget runs down). That's a cheap preview of the horde-vs-wave tension the run resources will formalize; alpha playtests show whether the full system is worth building.

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
- **`CardDefinition.cs`** — **a ScriptableObject: a monster card as pure data** (name, description, currency `spawnCost`, `monsterPrefab`, `spawnCount`, `spawnInterval`). Authored via **Assets ▸ Create ▸ Castle Attack ▸ Card**. Also carries optional **per-card stat overrides** (`overrideStats` + `unitMaxHP` / `unitDamage` / `unitSpeed` / `unitScale`), so one prefab can be a boss on one card and a swarm on another (see "Built: per-card unit variety").
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
- **One-time editor utilities (deleted 2026-09-24, still in git history):** `GameplayRigSetup` (built the GameplayRig prefab + Sandbox scene), `TowerSetup` (placed the tower model and saved `Tower.prefab`), `SceneSyncSetup` (moved the environment into prefabs) and `SwarmCardSetup` (created the Swarm card and set the pool prewarm). New one-shot setup scripts go in `Assets/Editor/` under the **Castle Attack ▸ Setup** menu.

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
The interdependent gameplay/UI objects are grouped under one **`GameplayRig`** prefab (`Assets/Prefabs/GameplayRig.prefab`); `SampleScene` (canonical) and `Sandbox` (scratch) both instance it, so a wiring change is made **once, in the prefab**, and both scenes inherit it. Both scenes are in Build Settings. **Team rule: edit the prefab, not a scene's copy.** The **tower** (below) was later added into this prefab, so both scenes have it. (The leftover `TestMonster` that sat at Sandbox's scene root was removed on 2026-09-24.)

**Update 2026-09-24 — environment moved into prefabs too.** Environment originally stayed per-scene, and it drifted: after the map was doubled, Sandbox kept the old ground size and camera, never got `RunManager`, and missed scene-only rig overrides (tower placement, the `PathVisualizer`). Fix: both scenes now contain **only prefab instances**. The ground moved into `GameplayRig` (its size follows the path layout the rig already owns), and a new **`SceneEnvironment`** prefab holds the camera, light, global volume and `RunManager`. `RunManager` sits inside it as a child and detaches itself before `DontDestroyOnLoad`, so it persists without dragging the camera/light along. Done by a one-shot editor script (`Castle Attack ▸ Setup ▸ 4. Sync scenes`). **Rules:** if both scenes need it, it goes in a prefab; no overrides on the scene instances — apply or revert them. Only per-scene Lighting settings (skybox, ambient) can't be prefabbed.

### Scene structure reference — what lives where
The quick answer to "where do I change X, and does it reach both scenes?"

| Lives in | Contents | Edit where | Reaches both scenes? |
|---|---|---|---|
| **`GameplayRig` prefab** | Managers (`GameManager`, `HandManager` + `HandDebugUI`, `MonsterSpawner`, `ProjectilePool`, `UnitCommander`, `GameOverManager`, `UIManager`), `Canvas` UI (currency, castle HP, game-over text, retry button), `EventSystem`, `Spawn_Marker`, `Path` + 5 waypoints + `PathVisualizer`, `Castle_Placeholder`, both towers (`Tower_Guard`, `Tower_Guard (1)` — nested `Tower.prefab`), `Ground` | Open the prefab (double-click it, or the arrow next to it in the Hierarchy) | Yes, automatically |
| **`SceneEnvironment` prefab** | `Main Camera`, `Directional Light`, `Global Volume` (post-processing), `RunManager` | Same — open the prefab | Yes, automatically |
| **Each scene file (`.unity`)** | **Lighting window ▸ Environment**: skybox, ambient light, fog, reflections; lightmap/baked-lighting settings. Sandbox may also hold its own scratch objects (none right now) | Window ▸ Rendering ▸ Lighting, **once per scene** | **No — change it in both scenes by hand** |
| **Project settings** | Build Settings scene list (`SampleScene` 0, `Sandbox` 1), Input System mode, URP asset | Edit ▸ Project Settings / File ▸ Build Profiles | Project-wide (saved on *project* save) |

**What still needs doing per scene:**
- Lighting window Environment changes (skybox/ambient/fog) — make them in **both** scenes. Currently both are identical Unity defaults. If these get customized (likely once the art direction lands), move them onto a small component on `SceneEnvironment` that applies them at startup, so they're prefab-owned too.
- Nothing else. Everything else is a prefab edit.

**Adding a new scene:** add a `GameplayRig` and a `SceneEnvironment` instance, both at position (0,0,0) with no overrides; copy the Lighting window Environment settings; add the scene to Build Settings.

**Checking for drift:** select a prefab instance in a scene → Inspector **Overrides** dropdown. It should say "No overrides" (the instance's own root position/name don't count). Anything listed there exists only in that scene — **Apply All** (push to the prefab) or **Revert All**.

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
| Tower Range | **12** (was 6; doubled with the map on 2026-09-24 — see below) |
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
| Swarm | 80 | 50× (HP 5, dmg 1, speed 4, scale 0.5) | 0.05s |

Starter deck in the current build: 3× Lone Grunt, 2× Grunt Rush, 1× Big Push, 1× Swarm (7 cards). **Planned for the alpha** (decided 2026-09-24, not yet applied): 4× Lone Grunt, 3× Grunt Rush, 2× Big Push + 1 random wildcard (Sapper, Runner or Tank) — 10 cards — with Swarm moving to the reward pool (§3 "Alpha run rules"). Cards may also carry stat overrides (off by default; Swarm uses them).

**Swarm card (2026-09-24, first pass — untuned):** 50 tiny, fragile units — one tower hit kills each, 1 damage each at the castle. First playtest on the doubled map: **Swarm alone won** (40 of 50 reached the castle, exactly its 40 HP) in 36s, 0 errors — a clean pool stress test, but too strong. The main reason is tower coverage, not the card: after the map doubled, `Tower_Guard` at (6,0,6) is exactly 6 units (its range) from the nearest path point, so it barely fires, and `Tower_Guard (1)` covers only ~10 of the path's 96 units. Retune tower range/placement for the bigger map before judging Swarm's numbers.

**Tower retune for the doubled map (2026-09-24):** decision — **double tower range only** (6 → 12 on `Tower.prefab`); monster/projectile speeds unchanged. Chosen over "scale everything ×2" (which would have reproduced the old balance exactly) and "move towers closer". Consequence: crossing the 96-unit path takes ~32s at speed 3 (was ~16s), and monsters spend about twice as long in range, so **towers are roughly 2× stronger than the validated v1 balance above** — this is a new balance point, to be re-judged by feel. Path coverage went from ~10 to ~45 of 96 units (each tower covers its own stretch, no overlap). Playtest results:
- **Swarm alone:** no longer wins — 29 of 50 got through (towers killed 21), castle left at 11/40.
- **Lone Grunt:** dies ~7s in, at the first tower — trickling still loses. ✅
- Swarm + Lone Grunt (the whole 100 currency) → loss at 11/40, 0 errors.
- **Rush test (Big Push + Grunt Rush, 15 grunts, all 100 currency, Focus Castle): lost.** Only 2 of 15 reached the castle (40 → 20 HP); the towers killed 13. The two cards were played ~4s apart, which spread the stream and favored the towers a little, but 2 of the needed 4 is not close. **At range 12, under the fixed 100 pool, the best affordable rush loses — towers beat every affordable combo.** Note that the hybrid budget's holding cap (100) also caps a single burst at 100, so under the new pacing a winning rush will have to be several overlapping bursts, or go through Attack Towers + bounties first.

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

## 8. Input / Controls (touch-first)

**Touch-compatible rule (applies to all UI, on every platform):** the game ships on PC first but is designed for touch (§1, Platform), so every screen must work identically with a finger or a mouse:
- Every action is **one tap/click on a big target** — no drag-only, double-click-only, long-press-only or right-click actions.
- **Nothing is hover-only.** Tooltips, costs, HP and order state are visible without a cursor.
- **Keyboard shortcuts are dev-only** (number keys, C/T, R) — never the only way to do something, and kept out of player builds.
- Design at phone aspect ratios (16:9 up to 20:9) with safe-area-aware anchoring, so PC and a later phone build share one layout.

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
11. **From here on, the build order is the alpha roadmap in §12** (phases 0–5 toward a first playable alpha). The items below are folded into it:
    - **Card hand UI** (real uGUI/TMP, replaces the placeholder) — girlfriend / UI-UX. §8.
    - **Pacing / currency model** — design-by-feel on the built hand.
    - ~~**Swarm card** — first real stress test of the pool (assign `prewarmPrefab`).~~ → **done 2026-09-24** (§5): 50× Swarm ran clean with both pools prewarmed; too strong until towers are retuned for the doubled map.
    - **Flying units** (§3) — new unit class that leaves the path; the incoming developer or the user.
    - **Clean tower re-export** from the artist (drops the Unity-side scale hacks).
    - **Tower-destruction VFX** — rubble/collapse + sound (art track).

### Open questions
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
- ~~Pacing: fixed pool or regenerating income? Card-draw cadence?~~ → **hybrid budget**: regeneration up to a holding cap, a finite per-encounter budget, tower bounties paid on top; draw-on-play stays; ~2-minute encounters (§3 "Encounter pacing"). Decided 2026-09-24.
- ~~Starter deck size and contents?~~ → ~10 cards per deck for more draw variety; alpha starter deck 4× Lone Grunt, 3× Grunt Rush, 2× Big Push + a random wildcard (Sapper, Runner or Tank) per run for run-to-run variety; Swarm and Brute are rare rewards. Decided 2026-09-24.
- ~~Alpha card roster?~~ → 8 cards: the current 4 + Brute, Tank, Sapper, Runner (§3 "Alpha run rules"). Decided 2026-09-24.
- ~~Alpha target date?~~ → end of 2026, with a checkpoint on 19 October and a cut order if behind (§12.3). Decided 2026-09-24.
- ~~Scene flow between title, castles, rewards and run end?~~ → a menu scene (title + run-end, first scene) + the encounter scene reloaded per castle, with the reward screen as a panel between castles. Decided 2026-09-24.
- ~~Destruction feedback / fire and smoke scope?~~ → collapse moment + smoke/fire below half HP (towers and castle) + smoldering rubble, all in the alpha. Decided 2026-09-24.
- ~~How do castles differ, and in what order?~~ → own layout per castle (one path each), `CastleDefinition` sets castle HP + tower strength, fixed order 1 → 2 → 3, pacing global (§3 "Alpha run rules"). Decided 2026-09-24.
- ~~Who is the alpha for?~~ → private first (restricted itch.io link for playtesters), then a polished public build for the portfolio. Decided 2026-09-24.
- ~~Run stakes for the alpha / between-castle choice?~~ → one lost castle ends the run; reward = pick 1 of 3 + trade 1 card; bonuses for razing all towers (4 choices) and winning with budget left (a rare offered). Run resources post-alpha (§3 "Alpha run rules"). Decided 2026-09-24.
- ~~Mobile only, PC only, or both?~~ → **PC first** (WebGL on itch.io + Windows) for alpha/v1, **mobile after** (Android, then iOS); **designed touch-first** so the port stays cheap (§1 Platform, §8 rule). Decided 2026-09-24.
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
- **Edit the `GameplayRig` / `SceneEnvironment` prefabs, not a scene's copy** (§4). `SampleScene` is canonical; `Sandbox` is scratch. Scenes should contain only those prefab instances (plus Sandbox scratch objects) — anything both scenes need goes in a prefab, and scene-instance overrides get applied or reverted, never left.

### Cross-surface context (2026-09-22)
So every surface (claude.ai chat, **Claude Code**) and the incoming developer share the same context:
- The **claude.ai Project doc** is the source of truth for the design.
- This file is a copy committed to the game repo as **`docs/game-design-document.md`**, plus a **`CLAUDE.md`** at the repo root that Claude Code auto-reads at session start (project overview, architecture, conventions, status).
- **Re-sync this copy from the Project doc after a doc pass.** Memory (claude.ai) does *not* cross into Claude Code — the repo files carry the context instead.

### Repo status (2026-09-23)
- **SSH keys are set up** — pushes work without a personal access token.
- **Pushed through `4125272`:** data-driven cards, hand & deck, object pooling, GameplayRig + Sandbox.
- **Committed, not yet pushed:** this `docs/` copy + `CLAUDE.md`.
- **Update 2026-09-24:** everything above has since been committed and pushed (tower art and destructible towers in `341b314`; later work in `425ae6a`, `46d13c9`, `f9bd0ce` and after — see §12.5).
- **Known snag:** `Assets/Scripts/UnitCommader.cs` is misspelled — the class inside is `UnitCommander`, and Unity requires the filename to match the MonoBehaviour class name or the component cannot be added. Rename the file in the Unity Project window.
- **Cleanup:** `_to_delete/` in each repo root holds stale git lock files; delete when convenient.

## 12. Road to First Playable Alpha (working todo list, updated 2026-09-24)

The working checklist, organized around one milestone. §9 stays the place for design rationale and open questions; this is what to actually do next, in order. Owners: **[Kevin]** code/design · **[Art/UI]** 3D art + UI/UX · **[New dev]** programming support. Tick items as they land; fold decisions back into the relevant § once made.

### 12.1 What "first playable alpha" means
> Someone who has never seen the game opens an **itch.io link in their browser** (or runs the Windows build), starts a run, plays through **3 escalating castles** with the mouse alone (or touch), picks a new card after each castle, and reaches a run-win or run-loss screen — without anyone explaining the controls.

**In scope:** a decided pacing model · a card hand + order buttons (global Focus Castle / Attack Towers) that follow the touch-compatible rule (§8) · readable feedback (tower HP, tower destruction, castle hits) · a 3-castle run with a card reward between castles · title, reward and run-end screens · ~6 cards, 1 starter deck · WebGL + Windows builds that run a 50-unit swarm smoothly. Placeholder art is fine unless it hurts readability.

**Out of scope (post-alpha, §12.4):** mobile builds (Android, iOS), the two run resources, run modifiers, stun/status effects, flying units, per-card-group orders + Hold, castle-spawned defenders, meta-progression, save/load, extra starter decks, final art.

### 12.2 Decisions (all made 2026-09-24)
| # | Decision | Recommendation | Blocks |
|---|---|---|---|
| D1 | Pacing: fixed currency pool vs. regenerating income; card-draw cadence | ✅ **Decided 2026-09-24: hybrid budget** — regeneration up to a holding cap, finite per-encounter budget, tower bounty on top; draw-on-play; ~2-min encounters (§3 "Encounter pacing") | Balance pass, castle tuning |
| D2 | Alpha run length | ✅ **Decided 2026-09-24: 3 castles**, fixed order | Castle authoring |
| D3 | Run resources (horde strength + wave budget) in the alpha? | ✅ **Decided 2026-09-24: post-alpha.** One lost castle ends the run; only the deck carries over. Reward: pick 1 of 3 + trade 1 card; bonuses for razing every tower (4 choices) and winning with budget left (a rare in the offer) (§3 "Alpha run rules") | Run scope |
| D4 | How castles differ | ✅ **Decided 2026-09-24:** a per-castle layout prefab (path, castle, towers, ground, road, spawn portal) split out of `GameplayRig`, chosen by a `CastleDefinition` asset that also sets castle HP and tower strength. One path per castle; fixed order; pacing stays global (§3 "Alpha run rules") | Run structure |
| D5 | Scene flow | ✅ **Decided 2026-09-24: a menu scene + the encounter scene.** The menu scene (title + run-end) is the game's first scene; the encounter scene reloads once per castle with the next layout, and the reward screen is a panel inside it between castles. `RunManager` (persistent) carries the run. The encounter scenes still include `RunManager` via `SceneEnvironment`, so pressing Play in SampleScene/Sandbox keeps working without the menu | Run structure |
| D6 | First platform + orientation | ✅ **Decided 2026-09-24: PC first** — WebGL on itch.io + Windows; landscape. Android after alpha, iOS later. Designed touch-first (§8 rule) so the port stays cheap | Builds, UI design |
| D7 | Destruction feedback scope for alpha | ✅ **Decided 2026-09-24: the full set** — the collapse moment (rubble swap + dust/debris + camera shake + sound), smoke and fire once a tower or the castle drops below half HP, and destroyed towers keep smoldering so the battlefield shows what you've torn down (pillar 1) | Feedback work |

### 12.3 Checklist to alpha, in order
**Target: end of 2026** (decided 2026-09-24) — about 13 weeks from now, which is tight for three people and six phases. The dates below are a plan, not a promise.
- **Checkpoint on 19 October (end of Phase 1):** compare progress to the plan and re-plan honestly.
- **Art runs in parallel and must start early:** the Figma card/hand/HUD designs are needed when Phase 2 starts (20 Oct), and the castle kit needs to be well along when Phase 3 starts (10 Nov).
- **If behind, cut in this order:** castle ruins, props and ground texture → Tank card → smoldering rubble → the reward trade step → the win bonuses → a 2-castle run instead of 3. Everything else is the core of the alpha.

**Phase 0 — close out the current thread** · by 28 Sep
- [x] Rush test at tower range 12: **lost** — 2 of 15 grunts got through (§5). Decision (2026-09-24): keep range 12 for now and retune in the Phase 1 balance pass together with the new pacing, since tuning against the fixed pool that's being replaced would be thrown away. [Kevin]
- [ ] Commit + push the swarm card and tower retune. [Kevin]
- [x] Overlapping-projectile playtest — **passed** (2026-09-24). The normal layout never overlaps shots (1 shot/s, ≤ 0.6s flight, no shared tower coverage), so one tower's fire rate was raised to 5/s in Play mode only: several shots in flight per target, 12 grunts killed with constant overkill, no errors, spawning afterwards normal. `MonsterSpawner.Despawn` now also ignores (and warns about) despawning an already-pooled monster — the step behind the old corruption — and it never fired. [Kevin]
- [x] Cleanup (2026-09-24): `_to_delete/` in both repos (sent to the Recycle Bin), Sandbox's leftover `TestMonster` removed, and the one-shot editor scripts deleted (`GameplayRigSetup`, `TowerSetup`, `SceneSyncSetup`, `SwarmCardSetup` — still in git history). **Phase 0 complete.** [Kevin]

**Phase 1 — the encounter feels complete (desktop is fine)** · 29 Sep – 19 Oct
- [ ] D1: implement the hybrid budget (§3 "Encounter pacing") — regeneration + holding cap + encounter budget in `GameManager`; tower bounty paid from `Tower`'s destroy path; `GameOverManager` lose check gains "budget spent"; HUD shows currency, cap and budget left; a "+40" bounty pop-up where a tower falls; `[EncounterEnd]` log adds budget left and bounties earned. [Kevin]
- [ ] Balance pass on one encounter with the new pacing; record in §5. Tune tower strength (range 12 currently beats every affordable rush — §5) together with the cap, regeneration, budget and bounty. **Explicit targets:** a well-timed rush wins, trickling loses, Swarm needs support. Then check castles 2 and 3 are still beatable within the same budget. [Kevin]
- [ ] World-space tower HP bars (so Attack Towers progress is visible) + a hit flash when the castle takes damage. [Kevin; bar style: Art/UI]
- [ ] D7: destruction feedback — replace "hide on death" with the rubble model + dust/debris burst + small camera shake + sound; smoke and fire once a tower or the castle is below half HP (a threshold hook in `Tower.TakeDamage` / `Castle.TakeDamage`); destroyed towers keep a smoldering loop on the rubble. [Art/UI: rubble, particles · Kevin: hook-up]
- [ ] New cards from the roster (§3 "Alpha run rules"): **Brute**, **Tank** and **Runner** as pure data (existing overrides). [New dev — good first task: data + playtest]
- [ ] **Sapper**: add a "bonus damage vs. towers" card stat (reset per life in `MonsterMover.OnSpawn`), then the card. [Kevin or new dev]
- [ ] Starter deck → 10 cards: 4× Lone Grunt, 3× Grunt Rush, 2× Big Push + a wildcard slot (`DeckDefinition` gains a wildcard options list — Sapper, Runner, Tank — and `RunManager` picks one when a run starts; outside a run, e.g. pressing Play in Sandbox, pick at encounter start). Plus a `CardPool` asset with Swarm and Brute (rare) plus Tank, Runner and Sapper (common) — needs the `rarity` field, shared with the Phase 3 reward screen. [Kevin]
- [ ] Placeholder SFX: tower shot, hit, monster death, castle hit, tower collapse. [Art/UI + Kevin]

**Phase 2 — touch UI** · 20 Oct – 9 Nov
- [ ] Figma: card hand, HUD (currency, castle HP, active order), order buttons, win/lose panels — at phone landscape size, following the touch-compatible rule (§8) even though the alpha ships on PC. [Art/UI]
- [ ] Card hand in uGUI + TMP: tap to play, cost + affordability state, played card animates out, next draws in; delete `HandDebugUI`. [Kevin: logic · Art/UI: layout/styling]
- [ ] On-screen **Focus Castle / Attack Towers** buttons showing the active order; C/T stay as dev-only shortcuts. [Kevin]
- [ ] Safe-area-aware layout, checked at 16:9, 19.5:9 and 20:9 in the Game view. [Art/UI + Kevin]

**Phase 3 — minimal run** · 10 Nov – 7 Dec
- [ ] D4: split the per-castle layout (path + waypoints, castle, towers, ground, road, spawn portal) out of `GameplayRig` into a layout prefab; add `CastleDefinition` (`[CreateAssetMenu(menuName = "Castle Attack/Castle")]`) with the layout prefab, castle HP and tower HP/damage/range multipliers. The encounter scene loads the current castle's layout from `RunManager`; Sandbox gets a castle picker for testing one castle directly. Update the §4 "Scene structure reference" table to match. [Kevin]
- [ ] Author 3 escalating castles — e.g. 1 = today's (2 towers, 40 HP); 2 = 3 towers, more HP; 3 = 4 tougher towers. Tune by play. [Kevin: layout · Art/UI: visual pass later]
- [ ] `RunState` gains current castle index + run deck (starts from `StarterDeck`, grows); `HandManager` takes the run deck instead of a fixed `DeckDefinition`. [Kevin]
- [ ] Encounter reports its result to `RunManager`: win → reward → next castle; loss → run over. `GameOverManager` stays the single arbiter, with run-level outcomes added in a fixed priority order (§4 warning). [Kevin]
- [ ] Reward screen (§3 "Alpha run rules"): pick 1 of 3 from a `CardPool` asset (4 with the all-towers bonus; a rare included with the budget-left bonus) + optional trade of 1 deck card. Needs a `rarity` field on `CardDefinition`. [Kevin: logic · Art/UI: screen]
- [ ] **Player-visible path** — a road/trail along each castle's waypoints, plus a spawn portal. Today the path only exists as an editor gizmo; players can't see where monsters will walk. [Art/UI: look · Kevin: placement per layout]
- [ ] "Castle N of 3" indicator (HUD or a short transition card between castles). [Art/UI + Kevin]
- [ ] D5: a **menu scene** with the title screen ("Start Run") and the run-end screen (castles cleared, win/loss, "New Run"), first in Build Settings; it holds a `SceneEnvironment` instance so `RunManager` exists from the start. Retry now means a new run; the R-key scene reload stays as a dev tool. Update the §4 "Scene structure reference" table. [Kevin + Art/UI]

**Phase 4 — shippable PC builds** · 8 – 14 Dec
- [ ] Windows build profile; a full run works outside the Editor. [New dev — good candidate]
- [ ] WebGL build profile: compression set up (itch.io serves Brotli/gzip builds), check download size and load time. [New dev + Kevin]
- [ ] itch.io page (restricted/private link for playtesters first); upload the WebGL build. [Kevin]
- [ ] Input pass: a full run with the mouse only, no keyboard; dev keys kept out of player builds. If a touchscreen laptop is around, one run by touch too. [Kevin]
- [ ] Performance in the browser and on Windows: 50-unit swarm + projectiles; Profiler for GC spikes and draw calls (shared materials, SRP Batcher). WebGL is the tighter budget. [Kevin + New dev]
- [ ] *(Optional)* Android smoke test: build installs on a phone and a run is playable by touch — no performance work, just proof the touch rule held. [New dev]

**Phase 5 — alpha polish + playtest** · 15 – 31 Dec
- [ ] Art swaps with the biggest readability payoff: castle model (castle first, §6), one monster, card frame. [Art/UI]
- [ ] Clean tower re-export (drops the Unity-side scale hacks, §4). [Art/UI]
- [ ] Bug bash: a full run with a clean Console. [everyone]
- [ ] Playtest with 3–5 people who haven't seen it, via the itch.io link; note where they get confused; collect `[EncounterEnd]` logs (a Windows build keeps a `Player.log`; WebGL needs the browser console or an in-game export). [Kevin]
- [ ] Tag the build `alpha-0.1` in git. The itch.io page stays a private/restricted link for playtesters. [Kevin]
- [ ] After playtest feedback: a polished public build and itch.io page for the portfolio/CV (decided 2026-09-24: private first, public later). [Kevin + Art/UI]

### 12.4 After alpha (explicitly deferred)
- **Two run resources** — horde strength + run-wide wave budget (§3). Open: waves-cleared vs. elapsed time (leaning waves-cleared — ties to the spawn/despawn events the economy already uses); partial replenishment rate.
- **Run modifiers** — start with the 3 named (blitz/siege/balanced) before authoring more (§7 tuning risk).
- **Stun** — build the general status-effect system (§4 "Anticipated") first; stun is its first effect.
- **Flying units** — build the path-blocking obstacle first, so flyer balance is tuned against real friction.
- **Per-card-group orders + Hold** (§8) — `MonsterMover.SourceCard` is already threaded through for this.
- Castle-spawned defenders; destructible castle parts; castle progression within an encounter.
- **Mobile port** — Android first: build profile, performance pass on a mid-range phone (swarm sizes, draw calls), store setup; then iOS (needs a Mac). Mid-run resume becomes important here — mobile OSes kill backgrounded apps (§4 save/load).
- Meta-progression; save/load + mid-run resume (§4 stance); more starter decks; final art.
- *(Only if skybox/ambient get customized)* move Lighting-window settings onto a component on `SceneEnvironment` (§4 "Scene structure reference").

### 12.5 Done log
- **2026-09-23** — pooling fix (`MonsterMover.IsAlive`: stale projectile references were damaging pooled monsters twice); `sqrMagnitude` range checks; `ProjectilePool`; `RunManager` + `RunState` shell; card identity on monsters (`MonsterMover.SourceCard`); `[EncounterEnd]` logging. Commit `425ae6a`.
- **2026-09-23/24** — map doubled; `PathVisualizer`; scenes synced into `GameplayRig` + new `SceneEnvironment` prefab. Commit `46d13c9`.
- **2026-09-24** — Swarm card + `unitScale` override; both pools prewarmed; tower range 6 → 12 (§5).
- **Incident note:** a hand-edited `ProjectilePool.prewarmPrefab` reference in `GameplayRig.prefab`'s YAML caused an `InvalidCastException` on scene start (fixed by clearing it + adding a try/catch around `Instantiate` in both `ProjectilePool` and `MonsterSpawner`'s `CreateNew`). Lesson: prefab asset references get assigned via the Inspector or the editor API, not hand-written YAML. *Root cause, found later:* a field referencing a prefab needs the fileID of the prefab's **root GameObject** (e.g. `TestMonster` = `1640811771736085792`); the hand-written `100100000` points at the prefab asset itself, which isn't a `GameObject` — hence the invalid cast.
