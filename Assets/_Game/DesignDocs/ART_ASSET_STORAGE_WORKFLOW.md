# Art Asset Storage Workflow

Last updated: 2026-06-15 KST

## Purpose

This project needs demo-scene-level stage quality without turning the gameplay Git repository into a raw Asset Store depot. The storage rule is therefore split into two layers:

1. Git tracks code, docs, authored scenes, gameplay data, and reviewed game-ready assets.
2. A separate art source depot tracks full raw packs, source textures, demo scenes, and heavy art references.

Until the art source depot is created, raw packs stay local-only under `Assets/_Imported/`.

## Current Decision

Use a hybrid workflow.

- Keep GitHub Git as the project/code collaboration repository.
- Keep `Assets/_Imported/` ignored and local-only.
- Do not add new Git LFS dependencies while the repository LFS budget is blocked.
- Use Unity Version Control, Perforce, or another deliberate art depot for full-fidelity raw stage assets before committing more demo-scene-scale source art.
- Promote only reviewed runtime slices into `Assets/_Game/`.

Git LFS is not the preferred next step right now because the account has already hit the LFS budget and the repository is already heavy with normal Git-tracked binary assets. Paid LFS can be revisited later, but it should be a deliberate repository policy change, not an emergency push fix.

## Asset Classes

### Runtime Game Asset

Committed to Git.

Examples:

- Authored gameplay prefabs.
- Authored scenes that reference `_Game` assets.
- ScriptableObject tuning data.
- Selected meshes, materials, textures, VFX prefabs, animation clips, and controllers required by a playable or reviewable scene.
- `.meta` files for every committed Unity asset.

Rules:

- Must live under `Assets/_Game/` or another documented game-owned folder.
- Must not reference `Assets/_Imported/`.
- Must be small enough to keep fresh clones practical.
- Must have a clear consumer scene, prefab, profile, or test.

### Art Source Asset

Not committed to Git by default.

Examples:

- Raw Asset Store packs.
- `.unitypackage`, `.zip`, `.psd`, `.tga`, `.exr`, `.blend`, and source export folders.
- Vendor demo scenes used as layout or lighting reference.
- Full-resolution source textures that are not directly needed by a runtime scene.

Rules:

- Keep under `Assets/_Imported/` until an art depot exists.
- Once an art depot exists, store these in that depot instead of Git.
- Do not use these paths directly from committed scenes or prefabs.

### Promoted Stage Slice

Committed to Git after review.

Examples:

- A selected terrain/material set converted into game-ready assets.
- A curated water/fog/particle layer used by one stage.
- A set of background cliffs, rocks, vegetation, and architectural props selected from a demo scene.

Rules:

- Promote into a named folder such as `Assets/_Game/Art/Environment/SpringIsles/`.
- Copy or create game-ready textures rather than raw source textures.
- Record the original source pack and demo-scene reference in the stage implementation notes.
- Prefer authored scene placement over runtime placement code.

## Demo-Scene-Quality Promotion Standard

Do not interpret "only promote what is needed" as "promote a tiny sample." Demo-scene-quality stage work needs whole composition layers:

- render profile and color grading,
- skybox, fog, ambient, and directional light setup,
- terrain or equivalent ground forms,
- water bodies and water particles where the source demo uses them,
- background cliffs/mountains/islands,
- foreground and midground vegetation density,
- rocks and architectural silhouettes,
- ambient particles such as leaves, petals, fog, and sun shafts,
- wind/shader global settings if the source demo relies on them,
- reflection probes, occlusion/static flags, or other reviewable performance helpers.

The correct compromise is to promote a coherent authored slice of the demo composition, not to copy the entire raw pack and not to rebuild the scene with primitives.

## Stage Art Workflow

1. Inspect the raw demo scene under `Assets/_Imported/`.
2. List the source layers that actually create the demo look.
3. Choose the stage composition slice.
4. Promote only the required meshes, materials, textures, prefabs, profiles, terrain data, and particles into `_Game`.
5. Build the stage as an authored scene using those promoted assets.
6. Validate that committed scenes and prefabs do not reference `_Imported/`.
7. Check large file impact before commit.
8. Commit stage structure and heavy promoted assets separately when possible.

## Pre-Commit Checks

Before committing art-heavy work:

- `git status --short --branch`
- Verify no file under `Assets/_Imported/` is tracked.
- Verify committed scene/prefab YAML has no `Assets/_Imported/` path dependency.
- Review files over 10 MB and justify them.
- Avoid adding raw source texture extensions unless the project has explicitly accepted an art depot or paid LFS plan.
- Keep package and project settings out of the commit unless the change was explicitly requested.

## Art Depot Setup Direction

Preferred near-term depot: Unity Version Control.

Reason:

- It is designed for Unity-style large binary assets and scene/prefab collaboration.
- It supports file locking and artist-friendly workflows better than plain Git.
- It avoids using the gameplay Git repository as the raw asset archive.

Fallbacks:

- Perforce is a strong professional option if hosting/admin overhead is acceptable.
- Paid Git LFS can work for a small team, but it should be paired with strict locking and file-size policy.
- Cloud-drive sharing is acceptable only as a temporary handoff for raw packs, not as the project source of truth.

## Boundary

This document does not authorize runtime scene generation, prefab builders, broad fallback paths, or direct `_Imported` references. It only defines where art source files belong and how reviewed stage slices become game-owned assets.
