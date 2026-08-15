# Tokyo Street — City Hero Pocket curation notice

## Source and entitlement

- Product: **Tokyo Street**
- Publisher: **Art Equilibrium**
- Unity Asset Store product ID: `228474`
- Product URL: <https://assetstore.unity.com/packages/3d/environments/urban/tokyo-street-228474>
- License: Standard Unity Asset Store EULA, Single Entity
- Entitled account: `ai-hyperion`
- Entitled organization: `dharmaflash1 (Personal)`
- Purchase date recorded by the entitled account: `2025-06-07`

The source package is not committed wholesale. Only a dependency-closed,
project-integrated subset is stored under the adjacent `TokyoStreet` folder.
Use and repository visibility must remain consistent with the recorded Asset
Store entitlement and EULA.

## Pinned source payload

- Outer package bytes: `3,470,082,571`
- Outer package SHA-256:
  `511A255925BAE543C823758D3D3BF72E22C65CFB0F69C29CF9F0D4A3096DC96B`
- Embedded URP package bytes: `1,118,988,726`
- Embedded URP package SHA-256:
  `6349857613A2E33373427DB73C08A04BE8ABA6B8938F1359E3C83036E3598C24`
- Package-local readme SHA-256:
  `BEA16D0A9C31C13F9DE4CDD1796BE1F1448E00BACA998C0477D7CF4E230A333C`

Unity Package Manager displayed `Import 1.1`, while Unity's product API and the
downloaded payload inventory match the current version `1.2` upload (version ID
`946093`, 1,343 file assets). The package is therefore identified by the hashes
above rather than by the anomalous UI label.

## Product curation

- Reviewed seed prefabs: 24
- Dependency-closed source assets: 169
- Textures: 95
- Authored 2048×2048 albedo maps retained: 24
- Support/packed/normal maps reduced to 1024×1024: 71
- Original TGA files converted to lossless PNG while preserving Unity GUIDs and
  importer settings
- Normal maps downsampled in vector space and renormalized
- Product asset/meta payload: `225,079,936` bytes before Unity library import

Deliberately excluded:

- demo scenes and demo lighting/post-processing;
- `Door.cs`, `SimpleCameraController.cs`, and all package scripts;
- package-local custom shaders;
- Flowers assets;
- `Roof_Wall_04.prefab`, whose second material slot is null;
- `Wall_Door_04.prefab`, because it directly depends on `Door.cs`;
- complete preassembled House prefabs and other dependencies outside the
  reviewed closure.

The curated kit is intended for the original City Hero Pocket layout, lighting,
post-processing, gameplay, and capture work. It must not be presented or
redistributed as a standalone Tokyo Street asset package.

## Reproducibility evidence

External evidence root:

`D:/DimensionBrawl_PV/00_staging/TokyoStreetAdmission`

- Rights/admission Markdown SHA-256:
  `BBA2E39A4B588696E8B479D835AFA77D1C2844DE487C10C6E76C029BF6AB3A57`
- Rights/admission JSON SHA-256:
  `98FE08A5D1EEB174AA3F9E056D325C503649A0D4FFD92AAE9D6D164BF526BA60`
- Richer-24 closure manifest SHA-256:
  `619329E023C139B8BCB4E1328A0DCEDF4D46454F2BC2437B59231E78070F5A9D`
- Curated product report SHA-256:
  `1BA8CD04E8D78F20EF2FF6CB968A177C7DD83BFEACC61F0B6FC043AD4E0401A6`
- Curated product `SHA256SUMS` SHA-256:
  `2EB372EBCE08BA8E5D39CAF44F3F94BD044E09B21A19A73845D3D918B146A483`
- Unity 6000.3.5f2/URP 17.3 validation report SHA-256:
  `C6A67CC5E2A081BA72EA848D7FE6EA2F001CEBB352C648E3F0D821326F5E2E23`
- Unity validation log SHA-256:
  `2B1479232AC6AD9D07E36AF981693AFF9E324D1278D0755988F4DDBF2AB5CD9C`
- Rendered 24-prefab contact sheet SHA-256:
  `FECDD1D5C00A80831B0D4FE12874EEE5158A9E318569D717C6431BBA3F60772D`

The curation tool and operating instructions live at
`Assets/_Game/Editor/CityHeroPocket/Tools`. The pinned Unity validation passed
with 169 assets, 95 PNG textures, and 24 rendered seed prefabs. GUID conflicts,
external `Assets/` dependencies, missing scripts, null materials, unsupported or
error shaders, magenta pixels, black tiles, and invisible tiles were all zero.
