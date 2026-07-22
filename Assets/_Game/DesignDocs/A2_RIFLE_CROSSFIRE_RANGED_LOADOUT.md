# A2 RifleCrossfire Ranged Loadout

Status: `FUNCTIONALLY VERIFIED / VISUAL REVIEW PENDING`

Date: 2026-07-20

Canonical product scope: the continuous Olympus Station segment in
`OlympusCorridorInvasionStage.unity`

## Outcome

A2 promotes one truthful physical-projectile enemy loadout into the canonical Station
encounter without admitting the candidate role framework or changing the route, terminal,
result, progression, or navigation owners.

The current Station roster is:

| Source ordinal | Spawn | Anchor | Archetype | Product behavior |
|---:|---|---|---|---|
| 0 | `add-left` | `Add_LeftLaneAnchor` | `SciFiSoldier.Melee` | Existing HeavyWindup melee participant |
| 1 | `add-right` | `Add_RightLaneAnchor` | `SciFiSoldier.Ranged` | New RifleCrossfire physical-projectile participant |

The exact A2 asset chain is:

```text
DB_Stage_OlympusStationCombat / add-right
  -> DB_Archetype_SciFiSoldier_Ranged
    -> PF_Enemy_SciFiSoldier_Ranged_RifleCrossfire
      -> DB_BasicSoldier_RifleCrossfireDeck
        -> DB_BasicSoldier_RifleCrossfire
          -> PF_EnemyProjectile_RifleCrossfire
```

The shared `DB_BasicSoldier_GeneralPatternDeck` and its Corridor consumers are unchanged.
The A2 soldier uses a dedicated one-entry deck and a dedicated hostile-orange projectile
prefab so product admission does not silently widen or reinterpret candidate inventory.

## Reviewed combat contract

`DB_BasicSoldier_RifleCrossfire` is an exact `ProjectileLine` profile with:

- actor type `SciFiSoldier.Ranged`;
- admitted distance interval `0..6.4 m`;
- damage `14`;
- hit stop `0.02 s`;
- hit response `FlashOnly`;
- control lock `None`.

The physical shot carries the exact firing enemy health as its source and Enemy as its
team. The projectile applies the profile's damage, hit-stop, response, and control policy;
those values are not replaced with projectile-driver defaults.

Windup locks a horizontal warning lane. AttackActive reuses that warned planar direction
rather than snapping to the target's later lateral position, while also retaining the
target elevation sampled during Windup. This keeps the attack laterally dodgeable and lets
the projectile hit a valid player above or below the shooter on the Olympus stairs.

The dedicated projectile explicitly owns a trigger sphere, kinematic/no-gravity rigidbody,
hit deactivation, visual alignment, and vertical travel. Sweep contacts are resolved
nearest-first. An active hostile `SummonPressureScreen` receives direct or overlapping
contact priority before a summon body or player health, consumes the shot exactly once,
and prevents same-frame pass-through damage.

## Ownership and lifecycle

Every admitted projectile-bearing ticket receives its own fixed `Projectiles` sibling
under the ticket root; the current Melee ticket has no projectile root or driver. The A2
Ranged projectile root is outside the moving soldier hierarchy, so shooter translation or
rotation after fire cannot drag or rotate a launched shot.

`BasicSoldierProjectileAttackDriver` owns at most three reusable projectile instances.
Owned and active counts remain observable, and every instance is parked synchronously on:

- normal hit, pressure-screen interception, miss, or lifetime expiry;
- soldier disable or death;
- ticket fault, boss/player terminal, run loss, or executor disable;
- continuous-scene unload; and
- Fail-to-Retry replacement.

The Station executor validates the exact projectile-driver lease before and after ticket
activation and during every active frame. A missing, disabled, reconfigured, reparented, or
over-capacity driver faults the whole plan, makes the affected hierarchy and projectiles
inert synchronously, and then follows the ordinary idempotent cleanup path.

## Verification ledger

The final rows below were run against the canonical project on 2026-07-21 KST
(2026-07-20 UTC) after the elevation and pressure-screen repairs.

| Gate | Evidence | Result |
|---|---|---|
| Rifle projectile contract, warned-line dodge, elevation, miss/reuse/cleanup | `C:\tmp\DimensionBrawl-A2-RifleSmoke-Elevation2.xml` | 3/3 passed |
| Full summon lane, energy, projectile-policy, and pressure-screen class | `C:\tmp\DimensionBrawl-A2-SummonLane-Full.xml` | 124/124 passed |
| Full stage-run route class, including actual Station ranged hit and driver-loss cleanup | `C:\tmp\DimensionBrawl-A2-StageRunRoute-Final3.xml` | 33/33 passed |
| Full canonical UI route, authoring, and Retry class | `C:\tmp\DimensionBrawl-A2-CanonicalUi-Final.xml` | 34/34 passed |
| Mobile runtime hot path | `C:\tmp\DimensionBrawl-A2-MobileHotPath-Final.xml` | 7/7 passed |
| Final enemy prefab contract validation | `C:\tmp\DimensionBrawl-A2-EnemyValidate-Final3.log` | `PASS` |
| Final playable definition/scene/route validation | `C:\tmp\DimensionBrawl-A2-StageValidator-Final2.log` | `PASS` |

The five non-overlapping final test classes total 201/201. Focused diagnostic reruns are
not added to that total.

The actual Station integration proof kills the Melee ticket first, observes the Ranged
ticket and boss remain active, positions the player at a physical mid-range distance, and
observes an exact source/damage/hit-stop/response/control-policy hit from the Ranged ticket.
Only the Ranged death then completes the independent Add plan.

Accepted product identities remain unchanged:

- policy digest: `f18fc51e2b65ae7e11b7e26866adc29f1f994c95be3591f2806bb846cd0bcaf2`;
- route digest: `878dac821103cdca2d2ad29a3fab8bce27109e9a5c1d551b14eccb736fd252d0`;
- result/progression join digest: `d389c587a17c29cb8e1df60222442ff4339f32fa5435b3586e8f49aa43461d71`.

## ArkData structural evidence and copy boundary

Reviewed material under `\\DESKTOP-69817L3\ArkData\SubcultureGameData` informed only the
general responsibility split already recorded in the current gap diagnosis:

```text
stage spawn row -> stable archetype identity -> promoted prefab -> reviewed profile/deck
```

PGR, HI3, and Aether Gazer structural notes support keeping stage composition, enemy
identity, behavior/loadout data, and instance lifetime as separate owners. DimensionBrawl
implements that separation with a direct Unity asset reference plus a matching stable ID.

All ArkData remains `PRIVATE REFERENCE / REVIEW NEEDED`. A2 copies no foreign code, asset,
identifier, stage row, balance value, timing, text, layout, art, audio, or implementation
detail. Every exact A2 name, value, ownership rule, and test fixture is a local
DimensionBrawl product decision.

## Explicit limits and known blockers

- Visual acceptance is still pending. Functional tests do not certify hostile-orange
  readability on a mobile device, stair/elevation readability, warning-lane clarity, or
  boss + Melee + Ranged combat clutter.
- The one-entry deck admits `0..6.4 m`. It proves physical mid-range pressure but does not
  enforce minimum standoff or claim a guaranteed backline-shooter behavior.
- The current loadout reuses existing generic cues. It does not claim dedicated rifle
  audio, trail, camera treatment, or final presentation polish.
- Product-unused/candidate-inventory mixed decks still have a separate admission defect:
  setup and executor projectile requirements are derived from the starting profile rather
  than every deck row. In particular, the promoted but product-unused Elite deck can select
  `ClosePunish` without a projectile driver and therefore perform a zero-damage attack.
  The canonical A2 one-entry deck is not affected. Any future mixed-deck promotion must
  close this validator/runtime gate first.
- A2 does not admit the role-candidate layer, Encounter/Wave grammar, required-defeat
  objective, second catalog row, reward, save, economy, or service system.

## Visual review checklist

Before changing status to visually accepted, inspect actual mobile-landscape play for:

1. orange projectile separation from player/summon effects and the environment;
2. warning-lane and shot agreement after a lateral dodge;
3. shooter/player height differences on the station stairs;
4. target selection and silhouette readability with boss, Melee, and Ranged alive; and
5. whether missing rifle-specific audio/trail feedback is acceptable or becomes the next
   small presentation slice.

## Next bounded priority

A2 closes the reviewed-loadout prerequisite for a second stage. B0-1 has since admitted a
truthful one-row entry/final route and rejects malformed topology before a run exists while
preserving the accepted Olympus identities. B0-2 truthful one-row facts/result commit and
B0-3 neutral bootstrap/fact/result/recovery adapters are now implemented and verified.
Start B0-4 catalog/build plumbing before B1,
the compact second playable stage. Add a minimal required-defeat owner only if that stage's
actual design requires ordinary-enemy roster completion; CF-01 remains review-only.
