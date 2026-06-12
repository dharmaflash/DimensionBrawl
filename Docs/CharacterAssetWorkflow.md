## Character Asset Workflow

Last updated: 2026-06-09

### Why this exists

`Assets/Chibi_angel` is no longer the active authority player visual baseline, but it remains a tracked legacy / sample Unity asset folder. Fresh clones still need it through version control because older sample content and the archived Blue Archive import workflow still reference it from:

- `Assets/BlueArchiveSample/Scenes/BlueArchiveSampleScene.unity`
- legacy Blue Archive-style import assets under `Assets/Characters/BlueArchiveTest/...`
- any local experiments or archived scenes that still depend on the older chibi baseline

Manual folder handoff between PCs is no longer the expected workflow for tracked shared baselines. The current CombatGirls local test shell is a temporary exception during phase-1 stabilization.

### New PC setup

1. Clone the repository normally.
2. Open the Unity project after the clone finishes.
3. If a tracked shared runtime character asset is missing on a fresh clone, treat that as a repository problem and commit the missing asset instead of copying files by hand.
4. `Assets/CombatGirlsCharacterPack` is currently a local-only stabilization shell on this PC, so do not treat its absence on another clone as a shared baseline bug during phase 1.

### Unity project settings

Keep these settings enabled for team work:

- Version Control / Mode: `Visible Meta Files`
- Asset Serialization / Mode: `Force Text`

These settings make GUID-based references portable across machines and keep scenes/prefabs reviewable in Git.

### Rules for shared character assets

1. If a scene, prefab, script, or material references an asset at runtime, commit that asset and its `.meta` file.
2. In the current repository, `Assets/Chibi_angel` is still tracked directly in Git so fresh clones stay usable for legacy sample content even without Git LFS budget.
3. Do not hide shared runtime dependencies behind `.gitignore`. The current local CombatGirls test shell is a deliberate temporary exception during phase-1 stabilization, not the new shared baseline yet.
4. If an artist needs raw source or experimental exports, keep them separate from runtime assets. Only ignore them when nothing in the playable project references them.
5. Before merging character art changes, verify a clean clone can open the project without copying files by hand.

### Follow-up cleanup

The current setup is intentionally conservative: the whole `Assets/Chibi_angel` folder is still shareable so archived sample content and older workflow references keep opening reliably.

## 2026-06-09 - Local CombatGirls test shell (phase-1 stabilization)

- The current local player shell in Assets/_Game/Prefabs/Player/Player.prefab uses Assets/CombatGirlsCharacterPack/CombatGirl_Shield/Prefab/CombatGirls_Sword_Shield.prefab as the temporary runtime visual on this PC only.
- Assets/CombatGirlsCharacterPack remains ignored / local-only during phase-1 stabilization, so it is not yet the shared runtime dependency baseline for fresh clones.
- The active placeholder player should prefer the CombatGirls asset's native animation set for runtime presentation on this PC, with project gameplay scripts adapting to that shell instead of forcing the older temporary KAWAII clip contract.
- BattlePrototypeBuilder may keep a local CombatGirls-first path as long as it still retains a tracked fallback for shared clones.

Later, we can split it into:

- a tracked runtime folder for final FBX/material/texture/prefab assets
- a separate source or scratch folder for raw exports, experiments, and backups

If the team later enables enough Git LFS budget or moves character assets into a separate asset repository, we can revisit the storage strategy. Until then, collaboration reliability is more important than keeping these runtime assets local-only.

## 2026-06-07 - Blue Archive-style lead character workflow (`hayun` baseline)

This section records the workflow that actually worked for the `Assets/hayun` character import. Reuse this flow for the next main-character pass instead of re-learning it from scene experiments.

### Scope and intent

- This workflow is for a `Blue Archive-style chibi runtime import` using:
  - a Blender-authored mesh
  - one main color atlas (`Image_0.jpg`)
  - one normal map (`Image_2.jpg`)
  - one mostly-unused dark/emission map (`Image_3.jpg`)
  - Blue Archive sample shaders under `Assets/BlueArchiveSample`
- The current utility script is `hayun`-specific:
  - `Assets/Editor/FixImportedCharacterMaterials.cs`
  - menu: `Tools/Hayun/Fix Selected Imported Character`
  - menu: `Tools/Hayun/Apply Fixed Materials To Scene Mesh_0`

### Runtime source of truth

Treat these files as the authority after import:

1. `Assets/hayun/Mesh_0_assigned_unity.fbx`
   - mesh, UVs, material-slot assignment
2. `Assets/hayun/Image_0.jpg`
   - raw exported base atlas from Blender
3. `Assets/hayun/Image_0_BAStyle_Body.png`
4. `Assets/hayun/Image_0_BAStyle_Face.png`
5. `Assets/hayun/Image_0_BAStyle_Hair.png`
   - runtime-tuned per-part atlases
6. `Assets/hayun/Mesh_0_Body_Fixed.mat`
7. `Assets/hayun/Mesh_0_Face_Fixed.mat`
8. `Assets/hayun/Mesh_0_Hair_Fixed.mat`
   - runtime-tuned final materials

The scene instance is a consumer of those assets, not the authority.

### Blender export contract

Before export:

1. Split the character into at least these material regions:
   - `Body`
   - `Face`
   - `Hair`
2. Make sure the actual polygons are assigned to the intended material slots.
3. Save the edited atlas image itself, not only the Blender paint state.

Export/update these files together:

- `Mesh_0_assigned_unity.fbx`
- `Image_0.jpg`
- `Image_2.jpg`
- `Image_3.jpg`

Important:

- If you changed `material-slot assignment` in Blender, re-export and reimport the `FBX`.
- Updating only `Image_0.jpg` does **not** fix wrong face/hair/body assignment.

### Unity import flow

1. Put the exported files in a dedicated folder such as `Assets/hayun`.
2. Make sure the Blue Archive sample shaders already exist under `Assets/BlueArchiveSample`.
3. Select the imported character asset or scene object.
4. Run `Tools/Hayun/Fix Selected Imported Character` once.
   - This generates or refreshes:
     - `Mesh_0_*_Fixed.mat`
     - `Image_0_BAStyle_*.png`
   - It also remaps the imported FBX material slots.
5. Place the character in the scene if needed.
6. Run `Tools/Hayun/Apply Fixed Materials To Scene Mesh_0` to reapply the current fixed materials to the scene copy.

### Safe tuning workflow

After the first generation pass:

1. Edit `Mesh_0_*_Fixed.mat` for shader-value tuning.
   - outline width
   - skin brightness
   - multiply/shadow tint
   - hair/cloth balance
2. Edit `Image_0_BAStyle_*.png` for pixel-level cleanup.
   - clothing specks
   - face atlas cleanup
   - hair color fills
3. Reapply with `Tools/Hayun/Apply Fixed Materials To Scene Mesh_0`.

Do **not** use the scene object as the long-term tuning surface.

Do **not** rerun `Fix Selected Imported Character` casually after manual tuning, because that path recreates materials and can overwrite shader values.

### What each layer is responsible for

- `Blender / FBX`
  - mesh shape
  - UVs
  - face/body/hair polygon assignment
- `Image_0.jpg`
  - raw exported color atlas
- `Image_0_BAStyle_*.png`
  - runtime split/tuned atlases for body/face/hair
- `Mesh_0_*_Fixed.mat`
  - final runtime look in Unity
- scene object
  - placement only

### Known pitfalls from the `hayun` pass

#### 1. White or uncolored hair patches

If part of the hair stays white after you "repainted the texture", the problem may still be the `FBX material assignment`, not the atlas.

Symptom:

- edited pixels exist in `Image_0.jpg`
- but the wrong region still renders as face/body/empty in Unity

Fix:

- re-export `Mesh_0_assigned_unity.fbx`
- reimport the FBX in Unity
- then reapply fixed materials

#### 2. Manual material tuning keeps disappearing

Cause:

- editing the scene instance instead of the fixed material asset
- or rerunning `Fix Selected Imported Character`, which resets material defaults before rebuilding

Rule:

- tune `Mesh_0_*_Fixed.mat`
- then use `Apply Fixed Materials To Scene Mesh_0`

#### 3. Distant shimmer / glitter on cloth or hair

Two different causes appeared:

1. Tiny bright atlas pixels on dark cloth/hair regions
2. Packed UV bleed when enabling mipmaps on dense BA-style atlases

Current rule:

- clean bright specks in the relevant `Image_0_BAStyle_*.png`
- keep the current `BAStyle` color atlases on `mipmapEnabled = false`
- do not blindly copy a mipmap fix from another character if the atlas packing is different

#### 4. Face tweaks are easy to overdo

The face shader is bright and can wash out subtle edits.

Practical rule:

- stabilize the base face first
- avoid broad fake face-shadow experiments
- avoid blush until the base face and mouth line are stable
- if blush is needed later, keep it as a very small lateral cheek edit, not a center-face wash

#### 5. Scene debugging should use Unity runtime inspection when possible

For this pass, local Unity MCP checks were useful for:

- asset refresh
- running the menu items
- capturing scene screenshots
- confirming whether the result changed before more edits

That is safer than stacking blind material tweaks.

### What to commit

Commit runtime-relevant assets:

- final FBX
- final textures used at runtime
- final fixed materials
- manifests / notes / `.meta` files
- scene changes only if the scene actually depends on the placed character instance

Do not treat local scratch and backup files as required runtime assets by default.

### Recommended next improvement

The current utility is `hayun`-specific. If the same pipeline is reused for another major character, prefer one of these follow-ups:

1. duplicate the workflow carefully for a new character folder and constants
2. or refactor `FixImportedCharacterMaterials.cs` into a generic `character import utility` with configurable folder/name inputs

Until that happens, assume the current script is hardcoded for `Assets/hayun`.
