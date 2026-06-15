# Spring Isles Demo Adaptation Plan

Last updated: 2026-06-15 KST

## Goal

Raise `ActionFoundationStageBreakGateReview.unity` toward the ToonScapes Spring Isles demo-scene quality while preserving the project rules:

- raw packs stay local-only under `Assets/_Imported/`,
- committed scenes should reference promoted `_Game` assets only,
- stage art should be authored and inspectable in Unity,
- gameplay route readability must stay stronger than decoration.

## Source Demo Findings

The source day demo at `Assets/_Imported/AssetStore/ToonScapes/Spring Isles/Demo/Demo_Scene_Day.unity` gets its look from a full authored environment stack, not a single shader.

Key source layers:

- one main camera with a global Volume profile,
- one directional light,
- linear fog and skybox lighting,
- four Terrain objects,
- water groups,
- background cliffs and mountains,
- about 23,000 prefab instances,
- heavy vegetation, rock, bamboo, plant, building-prop, and water layering,
- ambient particles such as sun shafts, fog, blowing leaves, and blowing petals,
- WindManager shader globals for vegetation motion,
- reflection probe groups and occlusion setup.

Current review scene gap:

- far fewer promoted prefab instances,
- no dedicated Spring Isles Volume Profile applied,
- no WindManager-equivalent shader global setup,
- sparse water/particle layering,
- limited background depth,
- route-combat layout exists but art composition is not yet demo-grade.

## Reference Data Boundary

`C:\Ark\SubcultureGameData\games\aether-gazer\enemies-stages` is useful for stage family, difficulty bucket, cost, level, monster catalogue, affix, and stage-reference context. The 2026-06-15 readable join notes explicitly state that direct per-map spawn or wave placement files were not found in that public Lua config snapshot.

For `S1-1 Break Gate`, treat `Assets/_Game/DesignDocs/LINEAR_STAGE_DESIGN_FOUNDATION.md` as the route-placement authority: `EntryRead -> BasicPressure -> BreakGate -> Relief -> FinalStand`. Use `C:\Ark` data as pacing and metadata evidence, not as a direct placement source. The Spring Isles dressing should support those authored pockets with readable terrain, water, background depth, progression gates, and collision-safe walkable surfaces.

## Adaptation Passes

### Pass 1: Visual Baseline

Purpose: make the scene read like Spring Isles before adding density.

Promote or author:

- Spring Isles stage Volume Profile based on the source sunny profile.
- Stage fog, skybox, ambient, and directional light values based on the source demo.
- Reflection probe placement for the combat route.
- WindManager-equivalent scene object or component, if needed for promoted vegetation shaders.

Acceptance:

- The scene no longer looks like a generic grey test arena.
- Lighting and color grading match the source pack direction.
- Vegetation can receive the same wind shader globals as the demo.

### Pass 2: Terrain And Water Foundation

Purpose: replace flat test-ground feeling with a coherent island route.

Promote or author:

- selected terrain data or terrain-equivalent ground forms,
- ground materials and terrain layers needed by the route,
- water bodies, waterfall elements, and water particles,
- collision-safe walkable route surfaces.

Acceptance:

- The player no longer starts on a visually flat platform.
- The route has stable collision and does not drop the player.
- Water and terrain frame the route without hiding enemy reads.

### Pass 3: Background And Silhouette

Purpose: build distant depth without overloading the combat lane.

Promote or author:

- background cliffs,
- distant mountains or islands,
- large silhouette props,
- sky-facing composition anchors.

Acceptance:

- The camera sees layered distance, not empty sky or black walls.
- Background geometry frames the route and boss-direction intent.
- Combat space remains readable on mobile landscape framing.

### Pass 4: Foreground And Midground Density

Purpose: approach demo-scene richness with curated density.

Promote or author:

- bamboo clusters,
- grass and plant patches,
- bushes/shrubs,
- rocks and stone blocks,
- architectural route accents such as walls, stairs, gates, lanterns, platforms, or bridges.

Acceptance:

- The route has repeated visual language similar to the demo.
- Density is placed by authored scene groups, not runtime generation.
- Navigation, enemy silhouettes, warning cues, and camera visibility remain readable.

### Pass 5: Ambient Motion

Purpose: recover the living demo-scene feel.

Promote or author:

- fog particles,
- sun shafts,
- blowing leaves,
- blowing petals,
- water particles,
- mobile-safe particle counts and culling ranges.

Acceptance:

- Ambient motion is visible but does not confuse combat VFX.
- Particles are presentation-only and do not decide gameplay.

### Pass 6: Performance And Source-Control Review

Purpose: keep the stage practical for mobile and collaboration.

Checks:

- no committed `_Imported` references,
- no accidental raw source texture copies,
- large file review,
- mobile camera readability,
- route-collision sanity,
- Unity validation menu or PlayMode test where available.

## First Implementation Recommendation

Start with Pass 1 and Pass 2 together. The current scene will not reach demo direction through decoration density alone; it needs the source demo's lighting, postprocess, terrain/water foundation, and wind setup first.

After that, add background and density in controlled authored groups.

## Out Of Scope

- Runtime stage generator.
- Runtime prefab builder.
- Full raw demo scene copy into Git.
- Production encounter spawning.
- Reward/progression/stage-select logic.
- Boss implementation.
