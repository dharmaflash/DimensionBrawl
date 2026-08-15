# Legacy actor grammar sources

This folder contains animation-only source evidence used by isolated editor review scenes.

`C29_akaza.fbx` is an exact byte copy of
`ThePhantomKnowledge-1.0.0f3/Assets/Scenes/01_Master/C19-32/C29_akaza.fbx`.
The review setup loads only its animation clip and samples it onto DimensionBrawl's current
`Akaza_model` hierarchy. The source model geometry, materials, environment, audio, VFX, and
completed Timeline are not instantiated or shipped through a product scene.

The review scene and profile live under
`Assets/_Game/Editor/Review/Cinematics/C29AkazaLegacyActorGrammar/` and are deliberately
excluded from build settings.
