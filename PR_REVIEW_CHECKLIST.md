# PR Review Checklist

Use this checklist before committing or merging AI-authored work.

## Scope

- The change matches the stated task.
- No unrelated project settings, package changes, generated files, or raw imported assets are included.
- `Assets/_Imported/` is not staged.

## Architecture

- No giant manager or all-in-one controller was introduced.
- No new legacy/fallback path was added without a documented reason and removal condition.
- Prefabs and scene objects remain authored and inspectable.
- Runtime generation is limited to short-lived gameplay instances.
- Any `Instantiate` usage has a clear owner, parent/lifetime policy, and cleanup path.
- No `Instantiate` calls occur in per-frame polling paths.

## Unity Safety

- `.meta` files are present for tracked assets and folders.
- No broken hardcoded absolute paths or machine-local paths.
- No package manifest change unless intentionally reviewed.
- No `ProjectSettings` change unless intentionally reviewed.

## Code Quality

- Serialized fields have clear ownership.
- Magic numbers are either named constants for local behavior or moved to data.
- Dependencies are explicit and narrow.
- New scripts are small enough to review.

## Verification

- Compile status is known.
- Manual test steps are written when behavior changes.
- The change summary says what was not included.
