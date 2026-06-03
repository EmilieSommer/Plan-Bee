# Plan Bee — Game Design Document

## Overview

**Genre:** 2D colony management strategy
**Engine:** Unity (2D)
**Reference:** Ant Colony: Wild Forest (Android game) — use this as the primary UI/UX and gameplay feel reference
**Perspective:** Top-down, zoomable. Player never controls individual bees.

The player manages a bee colony. Bees act autonomously based on their type. The player's job is to:
- Queue which bees to hatch
- Mark areas of the grid to expand the hive
- Survive a full in-game year (4 seasons)

**Win:** Survive all 4 seasons
**Loss:** Queen is killed OR the colony swarms (abandons the hive)
**Sandbox mode:** Survive indefinitely with no win condition

---

## Core Resources

| Resource | Produced by | Consumed by |
|----------|-------------|-------------|
| **Honey** | Foragers, House Bees | Colony survival (drain per tick), upgrades, winter reserve |
| **Pollen** | Foragers | Hatching eggs |
| **Beeswax** | House Bees | Building new hive tiles |

All three are always visible in the HUD. Running out of Honey during winter triggers starvation leading to swarm.

---

## Bee Types

All bees act **autonomously**. The player never directly controls a bee.

### Queen (NPC)
- 1 per hive. Her death = immediate game over (Defeat).
- Lays eggs into brood cells automatically.
- Lives in the Brood Chamber.
- Upgradeable (see Upgrades section).

### Nurse
- **Required** to hatch eggs. If there are no Nurses, the egg queue pauses entirely.
- Works inside the Brood Chamber.
- Tends queued eggs and reduces hatch timers.
- Player **starts with 1 Nurse**.

### Builder
- Constructs tiles the player has marked for building.
- Auto-paths to the nearest marked tile and builds it using Beeswax.
- If no Builders exist or Beeswax is 0, marked tiles remain unbuilt.

### Drone *(these are the defenders — male bees)*
- Station at the hive entrance and Drone Posts.
- Engage enemies that enter the hive.
- Have HP and attack stats (both upgradeable).
- Can be killed in combat.

### Forager
- Leaves the hive through the entrance.
- Returns after a **trip timer** carrying Honey and Pollen.
- Trip time is affected by: **bee upgrade level** + **current season**.
- Can be killed by Wasps/Hornets during transit.
- Cannot forage in **Winter** (trip disabled entirely).

### House Bee
- Stays inside the hive.
- Converts Pollen → Honey at a steady rate.
- Passive production; no player input needed.

---

## Egg Queue System

This is the primary player decision system.

- The queue has **3 slots**. Each slot holds **one bee type**.
- Each slot can hold up to **5 eggs** of that type.
- Both the number of slots and max stack size can be **increased via upgrades**.
- Each bee type has its own **hatch timer** (suggested base values below).
- **Nurses process the queue.** No Nurse = queue fully paused.
- If there is **no brood cell space** or **insufficient Pollen**, the Nurse waits until resources are available. The queue resumes automatically.

### Suggested base hatch timers
| Bee Type | Base hatch time |
|----------|----------------|
| Nurse | 45s |
| Builder | 60s |
| Drone | 30s |
| Forager | 90s |
| House Bee | 60s |

These are starting values — adjust through playtesting.

### Suggested base Pollen costs per egg
| Bee Type | Pollen cost |
|----------|------------|
| Nurse | 10 |
| Builder | 8 |
| Drone | 6 |
| Forager | 12 |
| House Bee | 10 |

---

## Hive Building System

- The hive is a **tile grid** (top-down).
- Player selects **Build mode** → picks a room type → clicks/drags to mark tiles on the grid.
- **Connection Rule:** You can ONLY build if the new tile is connected (adjacent) to an existing hive tile. Disconnected tiles cannot be built.
- **Room Placement:** You can build different hive rooms (Brood, Storage, etc.) directly in the Hive material.
- **Build Indicators:** When a build tool is selected, all valid tiles where you can currently click and build must be visually indicated/highlighted in some way.
- Builders automatically path to marked tiles and construct them, consuming Beeswax per tile.
- Construction progress is visible on the tile (e.g. progress bar or visual state).

### Tile Rendering Rules
- **Hive (Solid):** The base `Hive` tile is the `Center` tile. This should always be used if it is not bordering any other type of tile (do not use outer edges/tops for now).
- **Inner Rooms vs Hive:** Any inside room (like Brood, Storage, or InsideHive) that borders the solid `Hive` material must draw its appropriate border on that side.
- **Inner Rooms Merging:** Tile rooms should always use their `Center` tile unless they border `Hive` or border *another type* of zone. If they border a different zone type, they should use their **overlay** on that side so the two rooms merge smoothly together.

### Room Types

| Room | Effect |
|------|--------|
| **Brood Chamber** | Queen lays here; required for egg queue to function; most protected |
| **Honey Storage** | Increases maximum Honey capacity |
| **Pollen Store** | Increases maximum Pollen capacity; should be placed adjacent to Brood Chamber |
| **Drone Post** | Increases Drone effectiveness; Drones stationed here defend that section |

New room types unlock via the upgrade/progression system.

### Starting hive
- Built mainly with **Hive** tiles.
- An entrance formed by **2 Inside Hive** tiles.
- A small **2x2 Brooding Chamber** inside the hive.
- Small seed resources: ~100 Honey, ~50 Pollen, ~30 Beeswax

---

## Day Cycle

- Time passes in real-time. Recommend adding **pause** and **×2 speed** controls.
- Each in-game day, a **threat event** occurs (enemy wave or catastrophe).
- At end of day, a **summary screen** shows:
  - Honey earned
  - Beeswax earned
  - Bees killed
  - Bees remaining (by type)

---

## Season System

A **season clock** is always visible. 4 seasons per year.

| Season | Key effects |
|--------|-------------|
| **Spring** | Low threats; good foraging. Ramp-up period. |
| **Summer** | Wasps, Hornets, Ants. High combat pressure. |
| **Autumn** | Bears, Robber Bees. Structural threats. Prepare Honey reserves. |
| **Winter** | Foraging **disabled**. Heavy Honey drain. Mice. Pure survival. |

Forager trip timer increases in Autumn. Foragers cannot leave in Winter. Running out of Honey in Winter causes bees to die from starvation, eventually triggering the Swarm loss condition.

---

## Enemy System

All enemies **physically enter the screen** and move through the hive. They either:
- **Kill bees** (attack bee entities)
- **Damage tiles** (attack hive structure, reducing HP until tile breaks)

Challenge scales with: colony size + chosen difficulty + current season + days elapsed.

| Enemy | Season | Behavior |
|-------|---------|----------|
| **Varroa Mites** | All year | Spread across brood tiles; debuff/weaken bees over time |
| **Small Hive Beetles** | Warm | Damage comb tiles; contaminate Honey stores |
| **Wax Moths** | Summer/Autumn | Destroy brood tiles and wax structure |
| **Mice** | Winter | Eat Honey; damage tiles; enter seeking warmth |
| **Ants** | Summer | Mass wave attack; steal Pollen and brood |
| **Wasps/Hornets** | Late Summer | Kill Foragers in transit; raid entrance |
| **Robber Bees** | Any (weak hive) | Steal Honey; scale in strength with colony weakness |
| **Bears** | Autumn | Destroy large sections of hive tiles |
| **Skunks** | Spring/Summer | Attack entrance; kill Drones |

**Suggested MVP enemy set for first playable build:** Wasps, Varroa Mites, Robber Bees, Mice, Ants.

---

## Win / Loss Conditions

| Outcome | Trigger |
|---------|---------|
| **Win** | Survive all 4 seasons |
| **Defeat** | Queen is killed by an enemy |
| **Swarm** | Colony drops below a survival threshold → all remaining bees abandon hive → restart |
| **Sandbox** | Optional mode: no win state, survive indefinitely |

---

## Progression & Upgrades

Upgrades are **persistent** — they carry over between runs (not roguelite).
Purchased with Honey (or a separate meta-currency earned per run — to be decided).

### Queen Upgrades
| Upgrade | Effect |
|---------|--------|
| Resourceful | Eggs cost less Pollen |
| Mother | Eggs cost more Pollen but hatch with better base stats |
| Pheromones *(later)* | Passive calm effect — TBD |

### Nurse Upgrades
| Upgrade | Effect |
|---------|--------|
| More Loving | Faster hatch timers |
| Strict | Hatched bees have better combat stats |

### Forager Upgrades
| Upgrade | Effect |
|---------|--------|
| Sturdy Wings | Carry more resources per trip |
| Lighter | Shorter trip time |
| Salsa Lessons | Better waggle dance communication → more Pollen per trip |

### Drone Upgrades
| Upgrade | Effect |
|---------|--------|
| Stronger | More attack damage |
| Bulky | More HP |

### Egg Queue Upgrades
| Upgrade | Effect |
|---------|--------|
| +1 Slot | Add a 4th (then 5th) queue slot |
| +Stack Size | Increase max eggs per slot beyond 5 |

### Architecture Upgrades
| Upgrade | Effect |
|---------|--------|
| Reinforced Foundations | Hive tiles have more HP (vs Bears, Beetles) |
| Cold Resistance | Reduces Honey drain during Winter |
| Clean | Slows Varroa Mite spread speed |

---

## HUD / UI Elements

Always visible:
- Honey / Pollen / Beeswax resource counters
- Season clock
- Bee count by type (e.g. Nurses: 3, Drones: 5, Foragers: 2...)
- Egg queue panel (3 slots, type icon, stack count, timer bar per slot)

Accessible via buttons:
- Build menu (tile type picker)
- Upgrade screen
- Speed controls (pause / ×1 / ×2)

---

## Implementation Order (Phases)

### Phase 1 — Foundation
- [ ] 2D Unity project setup (scenes, folders, namespaces)
- [ ] Tile grid system (tilemap; mark-to-build; adjacency validation)
- [ ] Top-down camera with zoom
- [ ] Resource manager (Honey, Pollen, Beeswax) + HUD

### Phase 2 — Egg Queue
- [ ] Egg queue UI: 3 slots, bee type picker, stack counter, hatch timer bar
- [ ] Hatch timer logic; pause on: no Nurse, no brood space, no Pollen
- [ ] Bee entity spawning on hatch complete
- [ ] Starting state: 1 Nurse, 3×3 Brood Chamber, seed resources

### Phase 3 — Bee Automation
- [ ] Bee base class: state machine (idle → working → returning)
- [ ] Nurse: stay in brood chamber, tick down hatch timers
- [ ] Forager: exit entrance → trip timer → return with resources; killed if Wasp present
- [ ] House Bee: passively convert Pollen → Honey
- [ ] Builder: path to marked zone → consume Beeswax → complete tile
- [ ] Drone: station at entrance/Drone Post → engage enemies

### Phase 4 — Building System
- [ ] Build menu UI
- [ ] Tile marking tool (click/drag; adjacency check)
- [ ] Builder assignment to build zones; construction progress on tile
- [ ] Room effects activate on completion

### Phase 5 — Day Cycle & Seasons
- [ ] Day timer; pause and ×2 speed controls
- [ ] End-of-day summary screen
- [ ] Season clock; 4-season year cycle
- [ ] Seasonal modifiers (forager timer, winter Honey drain, enemy spawn weights)

### Phase 6 — Enemies (MVP)
- [ ] Enemy base class (pathfind into hive, attack bee or tile)
- [ ] Wasps/Hornets
- [ ] Varroa Mites
- [ ] Robber Bees
- [ ] Mice
- [ ] Ants
- [ ] Remaining enemies (Beetles, Moths, Bears, Skunks) in later pass

### Phase 7 — Win / Loss
- [ ] Queen death → Defeat screen
- [ ] Swarm threshold logic → Swarm screen → restart
- [ ] Year complete → Win screen
- [ ] Sandbox mode toggle

### Phase 8 — Upgrades
- [ ] Persistent upgrade data (survives restarts; saved to disk)
- [ ] Upgrade screen UI
- [ ] All bee upgrades wired into their systems
- [ ] Architecture upgrades wired into hive/enemy systems
- [ ] Egg queue slot and stack upgrades

### Phase 9 — Polish
- [ ] Bee movement and role animations
- [ ] Enemy enter/attack animations
- [ ] Seasonal visual changes (snow, flowers, etc.)
- [ ] Sound design
- [ ] Difficulty settings

---

## Open Questions (Decide Before Building)

1. **Time controls:** Pause only, or also ×2 speed?
2. **Upgrade currency:** Honey only, or separate meta-currency earned per run?
3. **Forager base trip times:** Suggested: Spring 60s → Summer 75s → Autumn 100s → Winter disabled. Adjust?
4. **Enemy pathfinding:** Straight to Queen/brood, or swarm entrance and spread inward?
5. **Drone Post:** Does building one automatically pull Drones to it, or does the player assign them?
6. **Sandbox vs Year-Survival:** Both in v1, or year-survival first?
7. **Starting difficulty:** How many days before first serious enemy wave?
8. **Swarm threshold:** What exact condition triggers a swarm? (e.g. fewer than 3 bees total? Honey at 0 for 3 days?)

---

*Reference game: Ant Colony (Android). When in doubt about feel or UI, check how that game handles it.*
