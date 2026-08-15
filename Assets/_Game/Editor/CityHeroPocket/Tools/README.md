# City Hero Pocket asset curation

`curate_tokyo_street.py` turns a dependency-closed Tokyo Street staging
manifest into the bounded product kit used by City Hero Pocket. It is an
offline authoring tool; Unity never runs it during gameplay or builds.

The tool intentionally fails when:

- the output directory is non-empty;
- a source/meta hash or GUID differs from the reviewed manifest;
- a source GUID already exists in the destination project;
- the closure contains a package script, custom shader, demo scene, Flowers,
  `Roof_Wall_04`, or another explicitly excluded asset;
- the requested texture dimensions do not match the manifest;
- a generated PNG does not decode to the exact transformed pixels.

Texture policy:

- reviewed albedo maps remain at their authored 2048×2048 resolution;
- support and packed maps become 1024×1024 through a channel-preserving 2×2
  box filter;
- normal maps become 1024×1024 through vector averaging and renormalization;
- source TGA files become lossless PNG files while keeping their Unity GUID and
  importer metadata.

Run only against an isolated, rights-approved staging import. Always write to a
new external output folder first, validate that folder in the pinned Unity/URP
version, and only then promote its `Assets` subtree to the product repository.

Example:

```powershell
$python = '<bundled-python>/python.exe'
& $python curate_tokyo_street.py `
  --manifest '<evidence>/TOKYO_STREET_RICHER_24_CLOSURE.json' `
  --source-project-root '<isolated-staging-project>' `
  --output-root '<new-external-curated-output>' `
  --guid-scan-root '<DimensionBrawl>/Assets'
```

The output includes JSON/CSV provenance and a recursive `SHA256SUMS`. The
original package, its demo scenes, and excluded dependencies must never be
committed as part of this workflow.
