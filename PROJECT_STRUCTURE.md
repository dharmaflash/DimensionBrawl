# Project Structure

This document defines where work belongs. Do not invent new top-level folders without updating this document.

## Root

- `Assets/`: Unity assets.
- `Packages/`: Unity package manifest and lock file.
- `ProjectSettings/`: Unity project settings. Changes require explicit review.
- `Docs/`: human-facing process, asset, and workflow notes that are not Unity assets.

## Assets

- `Assets/_Game/`: all game-owned curated content and source.
- `Assets/_Imported/`: local-only raw asset packs. Ignored by Git.
- `Assets/Settings/`: Unity render/input/template settings created by project setup.
- `Assets/Scenes/`: default Unity scene area until game scenes are promoted under `_Game`.

## Assets/_Game

Planned ownership:

- `Art/`: curated game-ready art references, selected materials, selected animation clips, and wrappers around raw imports.
- `Prefabs/`: authored gameplay prefabs that can be placed or instantiated.
- `Scenes/`: authored playable and test scenes.
- `Scripts/`: game source code.
- `ScriptableObjects/`: data assets used by runtime systems.
- `DesignDocs/`: imported design research markdown.
- `DesignData/`: imported design research JSON.

Do not place raw asset store packs directly under `_Game`. Promote only selected assets or authored wrappers.

## Suggested Scripts Layout

Create these folders only when real scripts exist:

- `Scripts/Player`
- `Scripts/Enemies`
- `Scripts/Combat`
- `Scripts/Animation`
- `Scripts/Camera`
- `Scripts/Spawning`
- `Scripts/UI`
- `Scripts/Data`
- `Scripts/Editor`

## Suggested Prefabs Layout

Create these folders only when real prefabs exist:

- `Prefabs/Player`
- `Prefabs/Enemies`
- `Prefabs/Combat`
- `Prefabs/UI`

## Promotion Rule

An imported asset becomes game-owned only when it is selected, renamed if needed, wrapped, or configured for actual project use. Until then, leave it in `Assets/_Imported/`.

