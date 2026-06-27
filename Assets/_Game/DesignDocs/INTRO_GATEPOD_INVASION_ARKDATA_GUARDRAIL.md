# Intro GatePod Invasion ArkData Guardrail

This document is the hard checkpoint for the Intro GatePod invasion extension.
Do not implement or commit soldier, portal, air-raid, or camera timing changes
until the evidence checklist below is filled in a separate evidence note.

## Why This Exists

- The previous soldier entrance looked cheap because the scene was guided from
  a broad prompt and then arranged from generic second-by-second assumptions.
- ArkData was originally requested as the implementation basis, but the result
  did not provide usable authored camera/timeline data at the time.
- The current attempt must re-check ArkData deeply before falling back to Rina
  camera data or hand-authored timing.

## Source Priority

1. Current implemented cutscene facts: voice contents, capsule awakening, first
   person hand/scan beats, existing GatePod profile, Timeline, and Olympus stage.
2. Previous thread records: especially the cutscene trial-and-error chain and
   notes about what failed.
3. ArkData raw datasets: camera rows, timeline rows, actor motion rows, effect
   call rows, function-call timing, and duration distributions.
4. Current project assets: `SpecialSkillsEffectsPack` air-raid/portal/explosion
   assets, sci-fi soldier prefabs/controllers, sword assets, existing Inori
   cinematic body states.
5. Rina camera package: use only for the final third-person reveal/back-view
   camera path after the ArkData timing grammar has been checked.
6. Hand-authored timing: allowed only where the evidence note says ArkData has
   no matching authored data.

## Required Evidence Before Implementation

Create or update:

`Assets/_Game/DesignDocs/INTRO_GATEPOD_INVASION_ARKDATA_EVIDENCE.md`

That note must include:

- Dataset/table path or thread source.
- Row count or sample count.
- Relevant command/event names.
- Timing stats where possible: start gaps, duration median/average, camera shot
  length, event density per second, or frame offsets.
- Applicability: `direct`, `grammar-only`, or `not usable`.
- Decision: how the evidence affects the Intro GatePod invasion beats.

## No-Repetition Rules

- No generic PGS/R-style second-by-second scene layout without a cited source.
- No "three soldiers on an empty bridge" as the core invasion beat.
- No soldier spawn without a pre-spawn signal: air raid, portal open, warning
  sweep, muzzle/impact, or explosion cue.
- No final camera plan that reveals the protagonist face clearly before the
  intended reveal; use feet, hands, shoulder, silhouette, or back-view.
- No replacement of the existing capsule/voice/first-person sequence unless the
  user explicitly asks for it.
- No rifle-focused handoff if the requested beat is sword/melee confrontation.
- No unverified placeholder cubes as final VFX when a usable project asset exists.
- No implementation commit until the evidence note and beat table are written.

## Beat Table Requirements

Every beat must have these columns before code or scene generation changes:

| Beat | Time Range | Narrative Job | ArkData Evidence | Project Asset | Camera Rule | Actor/Animation |
| --- | --- | --- | --- | --- | --- | --- |

The time range may be tentative, but it must cite why that duration/density is
reasonable. If a duration is invented because no data exists, mark it as
`hand-authored fallback`.

## Commit Checkpoints

Commit only at these checkpoints:

1. Evidence and beat-plan checkpoint.
2. Runtime/schema implementation checkpoint, after compile validation.
3. Scene/profile/timeline generation checkpoint, after Unity validation.
4. Visual capture/polish checkpoint, after sample captures are reviewed.

Do not commit partially grounded implementation just because it compiles.

## Resume Checklist

When resuming this task, first check:

1. `git status --short`
2. Active goal and current plan
3. This guardrail document
4. The ArkData evidence note
5. Latest generated validation report and captures

If the evidence note does not exist or is incomplete, continue research instead
of editing Unity scene/profile/runtime code.
