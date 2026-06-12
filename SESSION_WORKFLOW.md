# Session Workflow

Use this when starting a new AI-assisted work session.

## Start Of Session

1. Check `git status --short --branch`.
2. Read `CURRENT_STATE.md`.
3. Read the latest relevant entries in `DECISIONS.md`.
4. State the current task in one sentence.
5. Identify files that are allowed to change.

## During Work

- Prefer one small vertical result over broad groundwork.
- Stop before adding more than three new scripts.
- Stop before changing `Packages/` or `ProjectSettings/`.
- Stop before moving imported assets from `Assets/_Imported/`.
- Update the user before file edits.

## End Of Session

1. Show `git status --short --branch`.
2. Summarize changed files by purpose.
3. State verification performed or not performed.
4. Suggest a Conventional Commit message if changes are ready.

## Commit Rules

- Commit source/docs/config separately from large assets.
- Do not commit `Assets/_Imported/`.
- Do not commit Unity-generated solution/project files.
- Do not commit package or project setting changes unless they were intentional.

## If The Project Feels Messy

Do not refactor immediately. First write:

- what is messy,
- what behavior must remain unchanged,
- which files are affected,
- the smallest reversible cleanup step.

