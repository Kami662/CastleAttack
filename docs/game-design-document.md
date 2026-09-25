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

A hand of cards drawn from a deck drives play (§4, "Built: hand & deck system"), and the player can now issue a global **order** to the horde — *focus the castle* (default) or *attack the towers* — the first slice of the unit-command system (§3, §4). Current input is keyboard for desktop testing only (number keys to play cards; C / T for orders) — see §8 for the actual touch-first input design. **Pacing (built 2026-09-24): a hybrid budget** — currency regenerates up to a holding cap, each encounter has a finite regeneration budget, and destroying a tower pays a bounty. This replaced the old fixed 100 pool. **Being replaced (decided 2026-09-25) by earned income ("plunder")**: damage pays, towns pay tribute, and there's no income timer (§3, "Encounter economy").

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

### Encounter economy (plunder) — decided and built 2026-09-25
**Replaces the hybrid budget** (built 2026-09-24). The hybrid budget's regeneration timer ("income stops in 37s") was confusing and was a system rule, not a player choice (pillar 2). Income is now **earned**: your army pays for itself by doing damage. Nothing drips in passively.
- **Start:** a starting pot (e.g. 100). No passive income.
- **Plunder:** every point of damage your monsters deal to a **tower** or the **castle** pays coins. A monster that reaches the castle deals its damage and is removed, so castle-damage plunder already pays for getting through the defenses; there's no separate payout for it.
- **Tower bounty:** a lump sum when a tower falls, **on top of the cap** (as before).
- **Holding cap:** stays. Plunder and tribute stop at the cap (overflow is wasted), so one giant stockpiled push is still limited.
- **Towns (new target type):** buildings **beside the road** with a **weak gun**. The Attack Towers order also targets them (it becomes "attack buildings", nearest first). Damaging a town pays **nothing**. Destroying it pays **tribute**: the town empties its treasury as a drip over time (e.g. 60 coins over 30s), then stops. Tribute has to be finite, or you could never go broke and trickling would work again. A detour to a town is a greed choice: units spent now for income later.
- **Growing the economy, by source:**
  - **Razing a tower:** bounty + **+cap** for the rest of the encounter.
  - **Castle HP milestones (75% / 50% / 25%):** **+plunder multiplier** (e.g. +25% each), so every breach makes the horde richer.
  - **Towns:** tribute (above).
  - **Between-castle rewards:** permanent **+cap** or **+plunder %** upgrades offered alongside cards (§3 "Alpha run rules").
  - **Economy cards** (post-alpha, §12.4): e.g. a **Looter** unit that earns double plunder, a **War Chest** card that raises the cap.
- **Lose:** can't afford any card in hand **and** nothing alive **and** no tribute still owed. There's no timer; you're simply broke. Win is unchanged (castle destroyed).
- **Why it works:** trickling loses on its own, because lone units die before dealing damage and so earn nothing. A rush that gets through pays for the next push. The rush-vs-trickle tension (§5) is now "does this push pay for itself?"
- **Card draw:** stays draw-on-play.
- **Target length:** still about **2 minutes per encounter**, now reached through tuning (castle HP, plunder rates) instead of a clock. A passive player simply doesn't earn.
- **First-pass numbers (tune in the Phase 1 balance pass):** start 100 · cap 200 (was 100 until 2026-09-25; the first tests lost most income at a cap equal to the start pot, §5) · plunder 1 coin per damage point · tower bounty 40 · +20 cap per razed tower · +25% plunder per castle milestone · town tribute 60 over 30s.
- **HUD needs:** current currency and cap; a "+N" pop-up on plunder, bounty and tribute (`BountyPopup` already exists); a tribute indicator while one is running.

### Armor — decided and built 2026-09-25
Towers and the castle are **armored**: every hit that lands on them is cut by a flat **armor** value, but never below **1 damage** per hit (`Armor.Reduce`, shared by `Tower` and `Castle`). **Towns are not armored** — they stay the soft, greedy target.
- **Why:** the first balance tests showed demolition was too easy because damage scales with unit count while a tower kills only one unit per second, so raising HP alone didn't help. Flat armor cuts weak hits much more than strong ones, so *which* units you play matters (a deck-building choice, pillar 2, not a system rule): a Grunt's 10 becomes 5 against a tower, a Brute's 30 becomes 25, a Sapper's 40 (×4 vs towers) becomes 35.
- **Floor of 1 (decided):** a hit that armor would reduce to nothing still chips for 1. Consequence: **armor does not slow Swarm at all** (a 1-damage hit stays 1), so a big swarm is still an effective building-breaker. If that stays a problem, the options are a floor of 0 (Swarm becomes a decoy) or a percentage floor (needs fractional damage) — see the balance pass.
- **Plunder is paid on damage actually dealt after armor**, so armor slows income as well as demolition. It does **not** fix the HP-times-rate coupling (§5): total plunder from a target is still its HP × the rate.
- **First numbers:** tower armor 5, castle armor 3, towns 0. Set per castle later via `CastleDefinition` (D4), so later castles can be tougher.
- **Status:** built and compiling with 0 errors; balance not measured yet. One smoke test (10 grunts under Attack Towers) razed both towns and one armored tower in ~30s, but the run was shared with someone else's plays, so it isn't a clean comparison. Re-test in the balance pass.

### Defenders (soldiers) — decided 2026-09-25, first pass built the same day
**Why:** the balance tests showed the defense only shoots, so unit numbers overwhelm it and demolition is too easy. Defenders that *block and fight* give the encounter real resistance in a way HP and armor can't. Pulled forward from post-alpha (§12.4) by Kevin's decision.
- **Who has them:** every tower and the castle. **Towns don't** (they keep their weak gun).
- **Two kinds:** **footmen** (melee interceptors) and, in **fewer numbers, archers** (ranged, stay near the building). **Changed 2026-09-25: archers are castle-only — towers send footmen only.** Fewer unit types to balance and read. **Counts (current, after the 2026-09-25 retest): tower 1 footman; castle 4 footmen + 1 archer** (they were 2 and 6 + 2 — too strong, nothing could win; §5).
  - **Footman (first pass):** 40 HP, 8 damage, 1 hit/s, speed 3.5. Runs at the nearest monster within its building's leash and fights it.
  - **Archer:** 20 HP, 6 damage, 1 shot/s, range 9, speed 3. Shoots from behind the footmen.
- **Monsters fight back:** a monster with a defender in reach (its attack range) stops and fights it instead of walking on. That is what makes footmen real blockers: they slow a rush and soak damage, which is also what gives Tank, Brute and Sapper a job.
- **Finite reserve (decided):** each building holds a fixed number of soldiers and releases them a couple at a time while a monster is within its alert range. Soldiers that die are gone; there's no respawn. **Current — tower:** 1 footman, up to 2 out at a time. **Castle:** 4 footmen + 1 archer, 4 out at a time (first pass was tower 2 footmen + 1 archer, castle 6 + 2). If a building falls, its unreleased reserve is lost; soldiers already out keep fighting.
- **No plunder for killing soldiers (decided):** they're a cost, not income, so there's nothing to farm and income stays tied to buildings and the castle.
- **Not affected by armor** (armor is a building/castle stat). Soldiers aren't pooled: the counts are small (tens), unlike monsters.
- **Placeholder look:** blue capsules (archers smaller and lighter); the real models are on the art list.
- **Smoke test (2026-09-25, SampleScene, 0 errors):** the reserves deploy (a tower released 2 footmen + 1 archer within ~5s of the first monster arriving, then had nothing left), and soldiers block: 7 grunts under Attack Towers razed a town but the armored tower held for 45s+ and only fell to a following Big Push (~55s after the first wave). Balance not measured — coin totals are still high (cap reached), so retune HP/armor/payouts next. No monster fights soldiers *except* when adjacent, and archers are not yet visibly distinct in play. The `DefenderPost` logs each release (`[name] released a Footman. Left in reserve: …`).
- **Tuning knobs:** all on the `DefenderPost` component (reserve, out at once, release interval, alert range, leash) and the two defender prefabs (HP, damage, speed, range). Per-castle values later via `CastleDefinition`.

### Stun (test build) — 2026-09-25
A minimal **tower stun**, built to test the idea; the general status-effect system (§4 "Anticipated") stays post-alpha (§12.4).
- **Effect:** a stunned tower (or town) **stops shooting**, and its `DefenderPost` **releases only footmen** while stunned (no archers, since archers are the "shooting" half of the building). Soldiers already out keep fighting; the castle's gun can't be stunned yet.
- **Who stuns:** the **Stunner** card (`stunSeconds` on `CardDefinition`): each hit a Stunner lands on a tower stuns it for the card's duration (4s).
- **Anti stun-lock:** a stun never refreshes while active, and the tower is **immune for 3s after** a stun ends (`Tower.stunImmunitySeconds`), so a crowd of Stunners can't hold a tower down for good. A stunned tower is therefore silent about 4 of every 7 seconds at best.
- **Feedback (placeholder):** a light-blue "STUNNED" pop-up and a log line.
- **Tested (2026-09-25, SampleScene, 0 errors):** stun applies (`Town_1 stunned for 4s`, in three runs) and the Sapper razes a town in ~2s. **Footmen-only rule verified** with a dev key (**Y**, editor-only: stuns every standing tower for 5s): `Tower_Guard` was stunned, released two footmen tagged "(stunned: footmen only)" while stunned, and released its archer only after the stun ended. **Not yet seen:** a real Stunner reaching an armored tower — in three runs the Stunners (20 HP each) were killed by the tower's soldiers first, so a Stunner on its own can't get through a defended tower; it needs the horde around it. The gun actually stopping is by code path (`Tower.Update` returns while stunned), not confirmed by eye.
- **Resolved limit (2026-09-25):** orders used to be global, which is why a lone Stunner couldn't do its job. Orders are now **per group** (§3 "Unit Commands"), so Stunners can stun a tower while the rest of the horde runs past it to the castle. That combination is not tested yet.
- **Test deck:** the starter deck currently holds 2 Sappers and 2 Stunners on top of the 7 original cards (11 total) for testing; the real alpha deck is decided in §3 "Alpha run rules".

### Alarm (escalating defense) — decided and built 2026-09-25
**Why:** nothing got worse the longer an encounter ran, so speed was worth nothing and the fast-win / blitz bonuses had nothing to push against. The alarm is the alpha-sized version of the post-alpha wave budget (§3 "Run Resources"): slow, split attacks get harder, and the direct route becomes faster but riskier.
- **The clock:** the alarm starts when the **first card is played** and rises **one level every 30s**, up to **level 4** (`GameManager.alarmStepSeconds`, `alarmMaxLevel`). It's a static, `GameManager.AlarmLevel`, reset each encounter. The HUD shows "Alarm N" once it's above 0.
- **Effects per level (first pass):**
  - **Towers repair** themselves: **+4 HP/s per level**, but only after **4 seconds without being hit** — so hit-and-run and long pauses between attacks cost you, while a continuous assault is unaffected (`Tower.repairPerAlarmLevel`, `repairDelaySeconds`). Applies to towns too; the castle itself doesn't repair.
  - **Soldier posts release faster:** the wait between releases shrinks **25% per level** (`DefenderPost.alarmReleaseSpeedup`).
  - **The castle calls for reinforcements:** **+1 footman** (was +2 until the 2026-09-25 retest) added to its reserve each level (`reinforcementsPerAlarmLevel`; towers 0). This is a deliberate exception to "no respawn" (§3 "Defenders"): time is the one thing that refills the reserve.
- **Tested (2026-09-25, SampleScene):** level 1 fired at 30s with "Alarm 1" on the HUD and `Castle_Placeholder calls for reinforcements: +2 footmen`. Not yet measured: the repair rate against real attacks, or whether 30s per level suits a ~2-minute encounter (level 4 arrives at 120s).
- **Watch out:** repair means a re-damaged tower pays plunder again (plunder is per damage dealt), so a tower can in principle be "farmed" between repairs. The alarm makes waiting costly enough that this is probably fine, but check it in the balance pass; the loot-value fix (§5) would close it.
- **Run modifiers later:** the step length, level cap and per-level effects are exactly the "allowances and drain rates" that run modifiers can vary (pillar 3).

### Alpha run rules — decided 2026-09-24, not yet built
- **One lost castle ends the run** — no second chance. A run is only 6–8 minutes, so starting over is cheap and every castle stays tense.
- **Only the deck and economy upgrades carry between castles** (no currency, no HP). The economy upgrades are the +cap / +plunder % picked on the reward screen (updated 2026-09-25). The two run resources below are post-alpha.
- **After each cleared castle** (castles 1 and 2 — beating castle 3 wins the run, so the reward screen appears twice per run):
  - **Pick 1 of 3** from the card pool, added to the run deck. The offer can also include an **economy upgrade** (+cap or +plunder %) in place of a card (decided 2026-09-25).
  - **Trade 1 (optional):** give up one card from your deck for a **random** different card from the pool — a gamble, mostly useful for getting rid of a card you don't want.
- **Win bonuses** (shown on the reward screen):
  - **All towers destroyed** → **+1 card choice** (4 instead of 3).
  - **All towns destroyed** → the offer includes a **rare** card. Needs a rarity tier on `CardDefinition`. (Was "won with budget left", which no longer exists; changed 2026-09-25.)
  - **Fast win** (won within the castle's time target) → **+1 card choice**. *(Added 2026-09-25.)*
  - **Blitz** (won without destroying any tower or town) → **+1 card choice**. *(Added 2026-09-25.)*
  - **They stack:** each of all-towers / fast win / blitz adds one choice (3 base, at most 5, because blitz and all-towers exclude each other). The time target is set per castle (`CastleDefinition`); first guess ~90s (75% of the 2-minute encounter target), tuned in the balance pass.
  - **Why fast and blitz exist (2026-09-25):** demolition pays twice — income (bounty, cap, tribute) and fewer defenders shooting your later units — while going straight for the castle pays only in time, and with no income timer time was worth nothing. These two bonuses give the direct route a reward; the post-alpha wave budget (§3 "Run Resources") is the systemic version of the same idea.
- **Castles:** 3 per run, always in the same order (1 → 2 → 3). Each has **its own layout** (its own single path, castle and tower placement) chosen by a `CastleDefinition` asset, which also sets **castle HP**, **tower strength** (HP / damage / range multipliers) and the **castle's own gun** (range / damage / fire rate — every castle defends itself too, not just its towers). The layout also places the castle's **towns**. **Economy numbers stay global** — every castle uses the same starting pot, plunder rate and cap, so later castles' extra HP and tougher towers must still be beatable with them (check in the balance pass). Per-castle reward pools, random castle order and multi-path castles are post-alpha.
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
  | **Runner** | speed — races past towers and raids towns fast, starts a "raider" deck (all-towns bonus) | 4× | HP 15, dmg 5, speed 7, scale 0.8 | 40 |

  Sapper needs one new card stat (bonus damage vs. towers), reset per life in `MonsterMover.OnSpawn` like the other overrides. On the field the three new unit types need a visual tell beyond size (a prop or color) — an art item.
- **Starter deck (decided 2026-09-24): 10 cards** — 4× Lone Grunt, 3× Grunt Rush, 2× Big Push, plus **1 wildcard slot**: a random pick from Sapper, Runner or Tank at the start of each run. Decks aim for **about 10 cards** so draws vary more (hands rarely repeat). At ~14 cards played per castle under the new pacing, you cycle the deck about once per castle, so a newly won card shows up once or twice per castle. The wildcard makes each run lean a different way from castle 1 — demolition (Sapper: Attack Towers + bounties), raiding (Runner: towns + the all-towns bonus) or soak (Tank) — while the 9 fixed cards keep balance predictable. It varies the starting deck, not the rules (pillar 3). Future starter decks (post-alpha) also target ~10.
- **Reward pool:** Swarm and Brute (**rare** — offered via the all-towns bonus), Tank, Runner and more Sappers (common). Swarm leaves the starter deck — it was only there for the pool stress test. With ~10-card decks each single reward matters less than in a 6-card deck, which is part of why the rares should feel strong and the trade step stays.
- **Why the bonuses:** they reward three different routes. Razing every tower is the demolition route (units spent on defenses), razing every town is the raiding route (units spent on detours for income), and the fast win / blitz bonuses are the direct route (go straight for the castle and win quickly, leaving everything else standing). Alpha playtests show whether the greedy routes feel worth it. (Before 2026-09-25 the second bonus was "won with budget left", a speed reward that disappeared along with the budget.)

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

**v1 built (2026-09-22):** a single **global** order — Focus Castle / Attack Towers — toggled by dev keys (C / T). Monsters on "attack towers" divert to the nearest standing tower and chip its HP; on "focus castle" they path to the castle.

**Per-group orders + Halt — decided and built 2026-09-25 (pulled forward from post-alpha; first pass):**
- **Every card play is a group.** The units one card summons form a `UnitGroup` and follow that group's own order, which can be changed at any time — including for units already walking. This replaces the global order.
- **Three orders:** **Focus Castle**, **Attack Towers** (which also targets towns), and **Halt** — the group stops in place. A halted group still fights any defender that comes into reach, and towers still shoot it (halting in range is a choice with a cost). Giving the group another order resumes it.
- **New groups** start with a default order (initially Focus Castle) that the player can change; on the placeholder panel that's the "New groups" row (dev keys C / T / H set it), so the usual one-tap flow is: set the default once, play cards.
- **Interaction (first pass, placeholder IMGUI, `UnitCommander.OnGUI`):** a panel with a row per live group ("Sapper (3 alive)") and one big button per order; rows disappear when the group is dead. **The real UI (Phase 2):** tap the card icon that summoned a group → tap an order button (GDD §8). Every action stays one tap.
- **Tested (2026-09-25, SampleScene, 0 errors):** two groups with different orders at once (a Grunt Rush on Towers razing a town while a Big Push walked to the castle), Halt stopping a group mid-walk (the count held at 8 alive) and Castle resuming it, and groups cleaning up when their units are dead.
- **Why:** the solved opening "Attack Towers first, then the castle" came from the single global order; per-group orders let Sapper/Stunner groups work the towers while the rest run the castle, and give the player real, repeated decisions. Not built yet: showing a group's position or health, and stagger control.

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
| **`GameplayRig` prefab** | Managers (`GameManager`, `HandManager` + `HandDebugUI`, `MonsterSpawner`, `ProjectilePool`, `UnitCommander`, `GameOverManager`, `UIManager`), `Canvas` UI (currency, castle HP, game-over text, retry button), `EventSystem`, `Spawn_Marker`, `Path` + 5 waypoints + `PathVisualizer`, `Castle_Placeholder`, both towers (`Tower_Guard`, `Tower_Guard (1)` — nested `Tower.prefab`), two towns (`Town_1`, `Town_2` — nested `Town.prefab`, a placeholder), `Ground` | Open the prefab (double-click it, or the arrow next to it in the Hierarchy) | Yes, automatically |
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
| Castle Max HP | **100** (was 40 until 2026-09-25 — raised with the plunder economy and the cap 200: 10 grunt arrivals instead of 4, so Swarm alone (max 50 damage) can't win, and the castle pays more plunder. Not yet re-tested) |
| Tower Range | **12** (was 6; doubled with the map on 2026-09-24 — see below) |
| Tower HP / armor | **250 / 5** (was 100 / 0 until 2026-09-25). Town HP / armor: 150 / 0 (was 60) |
| Castle armor | **3** (added 2026-09-25; every hit is cut by armor, min 1 — §3 "Armor") |
| Castle gun range | **8** (was 6; set 2026-09-25 — a shorter-range last line of defense) |
| Tower Fire Rate | 1 shot/sec |
| Targets per shot | **1** for every tower, the castle gun and towns. `Tower.targetsPerShot` (max 4) supports volleys — built 2026-09-25 at 2, then set back to **1** the same day (the 15-grunt direct rush got 0 through at 2). Kept as a knob for the future: Kevin wants castles to get **2–3 different tower types** later (post-alpha), which is what it's for |
| Tower Damage | 15 per hit |
| Monster Max HP | 30 |
| Monster damage to castle | 10 |
| Currency (plunder, built 2026-09-25) | start 100 · cap 200 (+20 per razed tower; was 100 until the first tests) · 1 coin per damage dealt · tower bounty 40 (above the cap) · +25% plunder at castle 75/50/25% HP. No passive income. Replaced the hybrid budget (start 50 · +4/s · budget 360), which replaced a fixed 100 pool |
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

**Cap raised to 200 and castle HP raised from 40 to 100 (2026-09-25),** in response to the two tests below. The numbers in those two tests (their "lost at the cap" totals, the 40 HP castle, the ~90s wins) were measured at a cap of 100 and 40 HP.

**Balance pass, first re-test at cap 200 / castle 100 (2026-09-25, Sandbox, 0 errors).** Start 100, bounty 40, +20 cap per tower, tribute 60/30s, plunder 1 per damage:

| Run | Play | Result |
|---|---|---|
| 1 | **Focus Castle rush**: Big Push + Grunt Rush at once (15 grunts, all 100 coins), then 2 Lone Grunts from the plunder | **Lose** in 66s. Only 4 of 15 arrived: castle 100 → 60. Plunder 42, none lost at the cap |
| 2 | **Attack Towers with one Swarm** (80 coins), then Focus Castle with Grunt Rush + Big Push + 3 Lone Grunts | **Win** in 74s. The Swarm alone razed both towers and both towns in ~25s and left **260 coins against a cap of 240**. Ended at 240/240 with **154 lost at the cap**; plunder 225, bounties 80, tribute 75 |
| 3 | **Trickle**: single Lone Grunts, then one Grunt Rush | **Lose** in 65s. Castle untouched (100/100), plunder 0 |
| — | Swarm alone | Can't win: 50 units × 1 damage = 50 max, castle has 100 |

**Reading:** trickling loses ✅ and Swarm needs support ✅ (it can't finish a 100 HP castle), but **a rush alone loses and demolition is far too strong.** One 80-coin Swarm wipes out every building and turns a 100-coin start into 260+, so once you open with it the encounter is decided and the wallet sits at the cap. Two causes: (1) a swarm is 50 damage per second against 100 HP towers and 60 HP towns, while a tower kills only one unit per second, and (2) the building payouts stack (each tower = 40 bounty + 20 cap; each town = 60 tribute), so demolition pays back several times what it costs. The ~74s win is also under the ~2-minute target. **Next tuning steps, one at a time:** make buildings sturdier (tower HP 100 → ~250, town HP 60 → ~150, so demolition costs real units), then trim the payouts if the wallet still fills, then re-tune castle HP for the 2-minute target. Not observed: the unaffordable-hand soft-lock.

**Fifth re-test — soldier counts reduced (2026-09-25, SampleScene, 0 errors; same unsaved Starting Currency 150 override, so 150 coins).** Tower reserve 2 → 1 footman; castle 6 + 2 → 4 footmen + 1 archer; alarm reinforcements +2 → +1 per level. Everything else as in the fourth test.

| Route | Play | Result |
|---|---|---|
| **Stun + Sapper on towers, then castle waves** (per-group orders) | Two Stunner groups + a Sapper on Towers; then Grunt Rush + 2 Lone Grunts, then Big Push + Grunt Rush, then Swarm + Sapper, then Grunt Rush + Lone Grunt, all on Castle | **Win**, 217s, with **5 coins left**. The Stunners stunned `Town_1` twice, the Sapper razed it, then a tower (+40 bounty, cap 220) and `Town_2` fell within ~25s. The castle fell in waves (100 → 37 → 1 → 0) with the plunder rate at ×1.75 and alarm 4 (120s+). Plunder 300 (**189 lost at the cap**), bounties 40, tribute only 15 of 120 (cap full), all-towns bonus earned, towers 1/2 |
| **Trickle** | Two single Lone Grunts, 15s apart | Stopped after two units: both died with **0 plunder**, castle untouched, currency 110 (trickling earns nothing, as intended) |
| Direct rush | not re-run — the dealt hand (2 Lone Grunts + Sapper) couldn't make one | — |

**Reading:** a **win is possible again and it's close**: the combo (Stun + Sapper on the towers, then castle waves) works as designed and the castle nearly held at 1 HP, which is the tension we want. Per-group orders are what made it winnable: without them the Stunners never lived to stun a tower. **Open:** (1) the win took 217s, well over the ~2-minute target, mostly my waiting between waves, so re-run it at pace to see how much the alarm (level 4 from 120s) bites a real player; (2) the wallet still sits at the cap (**189 plunder + 105 tribute lost**), so the loot-value/cap work in the balance pass is still needed; (3) the fast-win (≤90s) and blitz bonuses are still out of reach, since the direct rush is untested at these counts and the last real one lost 3-of-15. Next: a proper direct rush with a rush-shaped hand (Big Push + Grunt Rush), and a re-check of the fast route.

**Fourth re-test — after the cuts, per-group orders and the alarm (2026-09-25, SampleScene, 0 errors; the scene's GameManager had an unsaved Starting Currency 150 override, so these runs started with 150 coins, not 100).** Current defense: tower HP 250 / armor 5 / 1 target per shot, tower reserve 2 footmen, castle HP 100 / armor 3 with 6 footmen + 2 archers (+2 footmen per alarm level), towns HP 150.

| Route | Play | Result |
|---|---|---|
| Direct rush | Big Push + Sapper + Stunner (15 units), all Focus Castle | **Lose**, 48s. 3 of 15 got through (castle 100 → 79), plunder 21 |
| Demolition | Sapper (3) on Towers, Grunt Rush on Castle; then Grunt Rush + 2 Lone Grunts on Towers | **Lose.** The Sapper razed `Town_1` (+60 tribute) and died; both towers released their footmen and every later wave died without razing a tower. Alarm reached 2 |
| Stun + rush | 2 Stunner groups on Towers, Swarm (50) on Castle | **Lose**, 159s. **First real Stunner stun on an armored tower** (`Tower_Guard (1) stunned for 4s`), but the Swarm did **0 damage** to the castle: its soldiers (6 footmen + 2 archers, +2 per alarm level, alarm 4 by 120s) killed it. Plunder 153, tribute 59 |

**Reading:** **no route wins on the starting coins.** Per-group orders work and the Stunner finally does its job, but the defense is now stronger than any affordable hand: soldiers kill small groups before they reach anything except the soft town, and the castle's reserve plus alarm reinforcements stop even a 50-unit swarm. Every earlier version of the encounter was too easy; this one is too hard, and the win-bonus routes (fast win, blitz) are unreachable. **Suggested next dials, one at a time:** tower reserve 2 → 1 footman; castle reserve 6+2 → 4+1 and alarm reinforcements +2 → +1; tower armor 5 → 3; footman HP 40 → 25. Then re-run the same three routes plus a trickle. Target: a good direct rush *can* win but usually doesn't, demolition wins slowly and safely, trickling loses.

**Third re-test — armor + defenders + 2 targets per shot (2026-09-25, SampleScene, 0 errors).** A **Focus Castle rush of 15 grunts** (Big Push + Grunt Rush, all 100 coins) now **loses outright: 0 of 15 reach the castle** (castle 100/100), in 32s. Before these changes 4 of 15 got through. Swing from "too easy" to "the direct route is impossible": the two towers on the way (each firing at 2 monsters per volley) plus their soldiers stop a cheap rush completely. **That undoes the point of the speed and blitz bonuses** — going straight for the castle can no longer win on the starting 100 coins, so those bonuses can't be earned. Next tuning should pull the defense back on the *direct-route* side while keeping demolition costly, e.g. fewer soldiers per tower, castle-only soldiers, a smaller alert range so soldiers only join fights near their building, or 1 target per shot with the soldiers kept. Re-test the four routes (rush, demolition, trickle, swarm) after each change.

**Second re-test, buildings buffed (2026-09-25, SampleScene, 0 errors):** tower HP 100 → 250, town HP 60 → 150 (`Tower.prefab`, `Town.prefab`). A single **Big Push** (60 coins, 10 grunts) under Attack Towers still razed one tower and both towns within ~25s and left **240 coins against a cap of 220**. The full win (slow manual play, 170s — not representative) ended: plunder earned 281, **348 lost at the cap**, bounties 40, tribute 54, bonuses logged as "all towns (rare)", towers razed 1/2, towns 2/2, 3 card choices.
- **Finding: HP and income are coupled.** Plunder is 1 coin per damage point, so a sturdier building *pays more*: a 250 HP tower now yields 250 coins of plunder (was 100), plus 40 bounty and +20 cap. The HP buff made the wallet problem worse, not better. Damage scales with unit count while a tower only kills one unit per second, so HP alone doesn't stop demolition either.
- **Finding: currency isn't the bottleneck, card tempo is.** Income dwarfs costs (348 coins wasted at the cap), because the hand (3 cards, draw-on-play) limits how fast coins can be spent. So more income never changes play; what matters is that buildings cost units and time to raze.
- **Proposed fix (not built):** stop paying per point of damage and give every target a fixed **loot value** paid in proportion to the damage dealt (`damage / maxHP × lootValue`). HP then only sets how long a target takes to raze, and income is tuned separately from HP. Castle and towers get a loot value; towns still pay nothing until destroyed. Then buff HP freely, and trim the bounty, cap growth and tribute if the wallet still fills.
- **Win-bonus logging is live:** the `[EncounterEnd] WIN` line now lists the bonuses earned and card choices (fast win ≤ `fastWinSeconds` = 90, blitz, all towers, all towns).

**First run with plunder (2026-09-25, Sandbox, 0 errors):** Attack Towers with Grunt Rush + Swarm + Big Push, then Focus Castle → **won in 95.6s**, right at the ~2-minute target. Both towers razed (+80 bounty, cap 100 → 140); the castle milestones took the plunder rate to ×1.75. Totals: **175 plunder earned, 80 lost at the cap**, 80 bounties, 55 coins left. Findings for the balance pass:
- **The cap bites hard early.** One Grunt Rush (40) on the first tower took currency 60 → 100 (the cap) within seconds, and the rest of that tower's 100 plunder was wasted. With the start at the cap, a big tower kill is mostly lost unless you spend while fighting. Options: a higher cap, a lower start, or fewer coins per damage on towers.
- **Possible soft-lock.** With no passive income, a hand of only unaffordable cards (say 20 coins vs. hand of 40/60/60) is a loss even if a cheaper card is in the draw pile, because cards are drawn only when one is played. Not hit yet, but likely in play. Candidate fix: a free redraw of the hand when nothing in it is affordable but the deck holds an affordable card.
- Attack Towers is now clearly the strong opening (towers pay damage + bounty + cap); check that a Focus Castle rush still wins, and that trickling loses.

**Towns test (2026-09-25, Sandbox, 0 errors):** one Grunt Rush under Attack Towers razed `Town_1` in about 3s with no income from the damage, then a 60-coin tribute started (`Tribute +2/s` on the HUD, "Tribute +60" pop-up). The rush then went on to raze `Tower_Guard (1)` (+40 bounty, cap → 120) while the tribute was still running. A later Focus Castle Big Push died to the towers; a Swarm then won it in 88.9s with 1 town razed. End log: **tribute earned 34 of 60, plunder 46, 134 coins lost at the cap.** The cap was full while the tribute dripped, so over half of it was wasted. This reinforces the earlier finding: with the start pot at the cap, the cap wastes most income. In the balance pass, either raise the cap well above the start pot or lower the start; the tribute drip in particular is only worth anything if there's room under the cap when it runs.

**First run with the hybrid budget (2026-09-24, Sandbox; superseded by plunder):** Attack Towers + steady card play destroyed both guard towers (+80 in bounties) and won in **52s** with 153 of 360 budget left — well under the ~2-minute target, so the encounter is now too easy that way. The Phase 1 balance pass starts from here.
- **The castle also shoots.** `Castle_Placeholder` carries its own `Tower` component (range **6** — it never got the range-12 change, which was on `Tower.prefab` only), so there are three shooters, not two. Until 2026-09-24 that gun also counted as an attackable tower: Attack Towers monsters could "destroy" it, which hid the entire castle and made the encounter unwinnable (and paid a bounty). Fixed — a `Tower` on the castle is now the castle's own gun: it keeps shooting but can't be targeted or destroyed separately. **Decided 2026-09-24: castles keep their own defense.** Each castle's gun stats (range, damage, fire rate) come from its `CastleDefinition`, alongside tower strength. **Castle gun range set to 8** (2026-09-25): shorter than the towers' 12, so it works as a last line of defense close to the castle rather than a third tower.

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
- ~~Pacing: fixed pool or regenerating income? Card-draw cadence?~~ → **earned income ("plunder")**: a starting pot, coins per damage to towers and the castle, tower bounties, towns that pay finite tribute when destroyed, a holding cap, and ways to raise cap and plunder rate; no passive income and no timer; draw-on-play stays; ~2-minute encounters (§3 "Encounter economy"). Decided 2026-09-25. It replaces the hybrid budget (decided and built 2026-09-24), whose income timer was confusing.
- ~~Starter deck size and contents?~~ → ~10 cards per deck for more draw variety; alpha starter deck 4× Lone Grunt, 3× Grunt Rush, 2× Big Push + a random wildcard (Sapper, Runner or Tank) per run for run-to-run variety; Swarm and Brute are rare rewards. Decided 2026-09-24.
- ~~Alpha card roster?~~ → 8 cards: the current 4 + Brute, Tank, Sapper, Runner (§3 "Alpha run rules"). Decided 2026-09-24.
- ~~Alpha target date?~~ → end of 2026, with a checkpoint on 19 October and a cut order if behind (§12.3). Decided 2026-09-24.
- ~~Should castles shoot?~~ → yes, every castle has its own defense; its gun stats are set per castle by `CastleDefinition`. Decided 2026-09-24.
- ~~Scene flow between title, castles, rewards and run end?~~ → a menu scene (title + run-end, first scene) + the encounter scene reloaded per castle, with the reward screen as a panel between castles. Decided 2026-09-24.
- ~~Destruction feedback / fire and smoke scope?~~ → collapse moment + smoke/fire below half HP (towers and castle) + smoldering rubble, all in the alpha. Decided 2026-09-24.
- ~~How do castles differ, and in what order?~~ → own layout per castle (one path each), `CastleDefinition` sets castle HP + tower strength, fixed order 1 → 2 → 3, pacing global (§3 "Alpha run rules"). Decided 2026-09-24.
- ~~Who is the alpha for?~~ → private first (restricted itch.io link for playtesters), then a polished public build for the portfolio. Decided 2026-09-24.
- ~~Run stakes for the alpha / between-castle choice?~~ → one lost castle ends the run; reward = pick 1 of 3 + trade 1 card; bonuses for razing all towers (+1 choice), razing all towns (a rare offered — was "won with budget left" until 2026-09-25), and, added 2026-09-25, a fast win and a blitz (no building destroyed), each +1 choice. Run resources post-alpha (§3 "Alpha run rules"). Decided 2026-09-24.
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

**Out of scope (post-alpha, §12.4):** mobile builds (Android, iOS), the two run resources, run modifiers, stun/status effects, flying units, meta-progression, save/load, extra starter decks, final art.

### 12.2 Decisions (all made 2026-09-24)
| # | Decision | Recommendation | Blocks |
|---|---|---|---|
| D1 | Pacing: fixed currency pool vs. regenerating income; card-draw cadence | ✅ **Re-decided 2026-09-25: earned income ("plunder")** — a starting pot; damage to towers and the castle pays; tower bounty; towns (weak gun, beside the road) pay finite tribute when destroyed; a holding cap raised by razing towers; castle milestones raise the plunder rate; reward-screen economy upgrades; no passive income, no timer; draw-on-play; ~2-min encounters (§3 "Encounter economy"). Replaces the hybrid budget (2026-09-24) | Balance pass, castle tuning |
| D2 | Alpha run length | ✅ **Decided 2026-09-24: 3 castles**, fixed order | Castle authoring |
| D3 | Run resources (horde strength + wave budget) in the alpha? | ✅ **Decided 2026-09-24: post-alpha.** One lost castle ends the run; only the deck (and, since 2026-09-25, reward-screen economy upgrades) carries over. Reward: pick 1 of 3 + trade 1 card; bonuses for razing every tower (4 choices) razing every town (a rare in the offer; was "budget left" until 2026-09-25), and a fast win / blitz (+1 choice each, added 2026-09-25) (§3 "Alpha run rules") | Run scope |
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
- [x] D1: hybrid budget built (2026-09-24) — regeneration + holding cap + encounter budget in `GameManager` (all five numbers in the Inspector); `Tower.Destroyed` event → `GameManager` pays the bounty; lose requires "budget spent"; HUD shows currency / cap and budget seconds left; `BountyPopup` shows a floating "+40"; `[EncounterEnd]` logs budget left and bounties earned. Tested: regeneration, cap, bounty above the cap, the "+40" pop-up, the lose timing and a win. Also fixed the castle-gun bug found while testing (§5). *Superseded 2026-09-25 by plunder (next item); the cap, bounty and `BountyPopup` carry over.* [Kevin]
- [x] **D1 re-decided — plunder economy** (§3 "Encounter economy"), built 2026-09-25: `Tower.Damaged` / `Castle.Damaged` events (damage actually dealt, overkill excluded) → `GameManager` pays plunder at the multiplier, up to the cap (a fraction carries to the next hit; what doesn't fit is counted as "lost at the cap"); +cap per razed tower (the bounty still goes above the cap); castle HP milestones at 75/50/25% each add +25% plunder; the regeneration and budget are gone; the lose check is now "no affordable card + nothing alive" (the "no tribute owed" part joins with towns); HUD shows currency / cap and the plunder rate once raised; batched "+N" plunder pop-ups (pale green) and a milestone banner. All numbers are Inspector fields. Tested in Sandbox, 0 errors: plunder, the cap, bounty over the cap, cap growth, milestones and a win (see §5). The "no tribute owed" lose clause came with towns (next item). [Kevin]
- [x] **Towns**, built 2026-09-25: a `Tower` with `isTown` ticked (chosen over a separate `Town` component: it reuses the HP, gun, `StandingTowers` registry and destroy flow, and Attack Towers targets it with no `MonsterMover` change). `GameManager` pays nothing for damage to a town and no bounty or cap for destroying one; it opens a **tribute stream** (60 coins over 30s, one per razed town, several can run at once) and the lose check waits while any stream is open. HUD shows `Tribute +2/s` while one runs; a "Tribute +60" pop-up marks the kill. `Assets/Prefabs/Town.prefab` is a **placeholder of three cubes** (weak gun: range 8, 5 dmg, 0.5 shots/s, 60 HP); two instances sit in `GameplayRig` at (-18, -12) and (-6, 12), 6 units off the road. Tested in Sandbox, 0 errors (see §5). Still to do: the real model + rubble (art list), and "attack buildings" wording once the on-screen order buttons exist (Phase 2). [Kevin · Art/UI: town model + rubble]
- [ ] Balance pass on one encounter with the new economy; record in §5. Tune tower strength (range 12 currently beats every affordable rush — §5) together with the starting pot, plunder rate, cap, bounty, cap-per-tower, milestone bonus and town tribute. **Explicit targets:** a well-timed rush wins, trickling loses, Swarm needs support. Then check castles 2 and 3 are still beatable with the same economy numbers. [Kevin]
- [x] **Defenders** (§3 "Defenders", pulled forward from post-alpha on 2026-09-25): `Defender` (footman / archer, `Active` registry), `DefenderPost` on every tower and the castle (finite reserve, released while a monster is in alert range), and `MonsterMover` now fights a defender in reach. Placeholder blue capsules; `Assets/Prefabs/Defender_Footman.prefab` and `Defender_Archer.prefab`. First pass, balance not measured. **Cost to the schedule:** roughly a week of Phase 1; if the 19 October checkpoint is behind, this is the first thing to trim back (fewer soldiers) before cutting anything on the §12.3 cut list. [Kevin · Art/UI: soldier models]
- [ ] World-space tower HP bars (so Attack Towers progress is visible) + a hit flash when the castle takes damage. [Kevin; bar style: Art/UI]
- [ ] D7: destruction feedback — replace "hide on death" with the rubble model + dust/debris burst + small camera shake + sound; smoke and fire once a tower or the castle is below half HP (a threshold hook in `Tower.TakeDamage` / `Castle.TakeDamage`); destroyed towers keep a smoldering loop on the rubble. [Art/UI: rubble, particles · Kevin: hook-up]
- [ ] New cards from the roster (§3 "Alpha run rules"): **Brute**, **Tank** and **Runner** as pure data (existing overrides). [New dev — good first task: data + playtest]
- [x] **Sapper**, built 2026-09-25: `CardDefinition.towerDamageMultiplier` (reset per life in `MonsterMover.OnSpawn`, applied to hits on towers and towns, not the castle) and `Card_Sapper.asset` (50 coins, 3×, HP 25, dmg 10, ×4 vs. towers). Tested: three Sappers razed the unarmored `Town_1` in ~2s. Not yet tested against an armored, defended tower (armor 5 makes each hit 35). [Kevin or new dev]
- [x] **Stunner (test unit), built 2026-09-25** — a minimal tower stun pulled forward to test the idea (§3 "Stun (test build)"). `Card_Stunner.asset`: 40 coins, 2×, HP 20, dmg 3, speed 3.5, stuns a tower for 4s per hit. [Kevin]
- [ ] Starter deck → 10 cards: 4× Lone Grunt, 3× Grunt Rush, 2× Big Push + a wildcard slot (`DeckDefinition` gains a wildcard options list — Sapper, Runner, Tank — and `RunManager` picks one when a run starts; outside a run, e.g. pressing Play in Sandbox, pick at encounter start). Plus a `CardPool` asset with Swarm and Brute (rare) plus Tank, Runner and Sapper (common) — needs the `rarity` field, shared with the Phase 3 reward screen. [Kevin]
- [ ] Placeholder SFX: tower shot, hit, monster death, castle hit, tower collapse. [Art/UI + Kevin]

**Phase 2 — touch UI** · 20 Oct – 9 Nov
- [ ] Figma: card hand, HUD (currency, castle HP, active order), order buttons, win/lose panels — at phone landscape size, following the touch-compatible rule (§8) even though the alpha ships on PC. [Art/UI]
- [ ] Card hand in uGUI + TMP: tap to play, cost + affordability state, played card animates out, next draws in; delete `HandDebugUI`. [Kevin: logic · Art/UI: layout/styling]
- [ ] On-screen **per-group orders** (Focus Castle / Attack Towers / Halt): tap a card's icon to select its group, then tap an order button; a "new groups" default. Replaces the placeholder IMGUI panel in `UnitCommander.OnGUI`; C/T/H stay as dev-only shortcuts. The logic is built (§3 "Unit Commands"); this is the real touch UI. [Kevin: logic · Art/UI: design]
- [x] **Per-group orders + Halt** and the **alarm** (2026-09-25, pulled forward from post-alpha — §3 "Unit Commands", §3 "Alarm"): `UnitGroup` per card play, three orders, placeholder panel, alarm clock with tower repair, faster releases and castle reinforcements. Both tested in SampleScene. [Kevin]
- [ ] Safe-area-aware layout, checked at 16:9, 19.5:9 and 20:9 in the Game view. [Art/UI + Kevin]

**Phase 3 — minimal run** · 10 Nov – 7 Dec
- [ ] D4: split the per-castle layout (path + waypoints, castle, towers, towns, ground, road, spawn portal) out of `GameplayRig` into a layout prefab; add `CastleDefinition` (`[CreateAssetMenu(menuName = "Castle Attack/Castle")]`) with the layout prefab, castle HP, tower HP/damage/range multipliers and the castle gun's range/damage/fire rate. The encounter scene loads the current castle's layout from `RunManager`; Sandbox gets a castle picker for testing one castle directly. Update the §4 "Scene structure reference" table to match. [Kevin]
- [ ] Author 3 escalating castles — e.g. 1 = today's (2 towers, 2 towns, 100 HP); 2 = 3 towers, more HP; 3 = 4 tougher towers — each with 1–3 towns. Tune by play. [Kevin: layout · Art/UI: visual pass later]
- [ ] `RunState` gains current castle index + run deck (starts from `StarterDeck`, grows); `HandManager` takes the run deck instead of a fixed `DeckDefinition`. [Kevin]
- [ ] Encounter reports its result to `RunManager`: win → reward → next castle; loss → run over. `GameOverManager` stays the single arbiter, with run-level outcomes added in a fixed priority order (§4 warning). [Kevin]
- [ ] Reward screen (§3 "Alpha run rules"): pick 1 of 3 from a `CardPool` asset (+1 choice each for the all-towers, fast-win and blitz bonuses — they stack, max 5; a rare included with the all-towns bonus), with economy upgrades (+cap / +plunder %) mixed into the offer, + optional trade of 1 deck card. Needs a `rarity` field on `CardDefinition`; `RunState` keeps the picked upgrades. [Kevin: logic · Art/UI: screen]
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
- **Economy cards** — e.g. a **Looter** unit (double plunder) and a **War Chest** card (+cap), so greed vs. force becomes a deck choice (§3 "Encounter economy"). Decided 2026-09-25 to come right after the alpha; Phase 1's card list is already full.
- **Run modifiers** — start with the 3 named (blitz/siege/balanced) before authoring more (§7 tuning risk).
- **Stun** — build the general status-effect system (§4 "Anticipated") first; stun is its first effect. *(A minimal, tower-only stun was built as a test on 2026-09-25 — §3 "Stun (test build)". It doesn't replace the general system: no stun on monsters or defenders.)*
- **Flying units** — build the path-blocking obstacle first, so flyer balance is tuned against real friction.
- **Per-card-group orders + Hold** — *pulled into the alpha on 2026-09-25 and built (§3 "Unit Commands"); only the real touch UI remains, in Phase 2.*
- Destructible castle parts; castle progression within an encounter. *(Castle-spawned defenders were pulled into the alpha on 2026-09-25 — §3 "Defenders". What stays post-alpha: defender variety per castle, defenders that can be stunned, and elite/hero defenders.)*
- **Mobile port** — Android first: build profile, performance pass on a mid-range phone (swarm sizes, draw calls), store setup; then iOS (needs a Mac). Mid-run resume becomes important here — mobile OSes kill backgrounded apps (§4 save/load).
- Meta-progression; save/load + mid-run resume (§4 stance); more starter decks; final art.
- *(Only if skybox/ambient get customized)* move Lighting-window settings onto a component on `SceneEnvironment` (§4 "Scene structure reference").

### 12.5 Done log
- **2026-09-23** — pooling fix (`MonsterMover.IsAlive`: stale projectile references were damaging pooled monsters twice); `sqrMagnitude` range checks; `ProjectilePool`; `RunManager` + `RunState` shell; card identity on monsters (`MonsterMover.SourceCard`); `[EncounterEnd]` logging. Commit `425ae6a`.
- **2026-09-23/24** — map doubled; `PathVisualizer`; scenes synced into `GameplayRig` + new `SceneEnvironment` prefab. Commit `46d13c9`.
- **2026-09-24** — Swarm card + `unitScale` override; both pools prewarmed; tower range 6 → 12 (§5).
- **2026-09-24** — Phase 0 done (rush test, overlapping-projectile playtest, cleanup). Phase 1 started: hybrid budget built; castle-gun bug fixed.
- **2026-09-25** — castle gun range 6 → 8. D1 re-decided: the hybrid budget's income timer was confusing, so it's replaced by earned income (plunder, towns, economy growth). Plunder, cap growth, castle milestones and towns (finite tribute; placeholder cube model, two in `GameplayRig`) built and tested; art list gained Town + Town rubble and the coin pop-ups.
- **Incident note:** a hand-edited `ProjectilePool.prewarmPrefab` reference in `GameplayRig.prefab`'s YAML caused an `InvalidCastException` on scene start (fixed by clearing it + adding a try/catch around `Instantiate` in both `ProjectilePool` and `MonsterSpawner`'s `CreateNew`). Lesson: prefab asset references get assigned via the Inspector or the editor API, not hand-written YAML. *Root cause, found later:* a field referencing a prefab needs the fileID of the prefab's **root GameObject** (e.g. `TestMonster` = `1640811771736085792`); the hand-written `100100000` points at the prefab asset itself, which isn't a `GameObject` — hence the invalid cast.
