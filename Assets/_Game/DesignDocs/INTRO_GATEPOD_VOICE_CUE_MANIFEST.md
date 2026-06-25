# Intro GatePod Voice Cue Manifest

Purpose: collect voice, breath, gasp, and short reaction audio for the Intro GatePod cutscene before the cues are placed on Timeline/Cinemachine shots.

Do not treat any line in this file as final dialogue until Master marks it approved.

## Target Import Path

Recommended Unity audio folder:

`Assets/_Game/Art/Audio/Voice/Cinematics/IntroGatePod/`

Recommended file naming:

`DB_VO_CIN_IntroGatePod_###_<short_description>.wav`

Example:

`DB_VO_CIN_IntroGatePod_010_wake_breath.wav`

## Status Labels

- `needed`: slot exists, no audio file yet
- `draft`: rough line or scratch recording exists
- `recorded`: audio file imported
- `placed`: assigned to Timeline/Cinemachine sequence
- `approved`: timing, mix, and performance accepted

## Cue List

| Cue ID | Status | Approx Time | Speaker | Type | Intended Beat | Script / Vocal Note | Audio File | Unity Path | Shot Link | Notes |
| --- | --- | ---: | --- | --- | --- | --- | --- | --- | --- | --- |
| VO_010_WAKE_BREATH | needed | TBD | Inori | breath | First sign of waking inside/near GatePod | TBD by Master | TBD | TBD | TBD | Keep very short; no dialogue unless needed. |
| VO_020_CONFUSION_REACTION | needed | TBD | Inori | reaction | She realizes the place is unfamiliar | TBD by Master | TBD | TBD | TBD | Avoid explanatory wording. |
| VO_030_INVASION_NOTICE | needed | TBD | Inori | reaction | She notices the invasion/threat direction | TBD by Master | TBD | TBD | TBD | Can be breath, gasp, or short line. |
| VO_040_WEAPON_DECISION | needed | TBD | Inori | reaction | She commits to picking up/using the weapon | TBD by Master | TBD | TBD | TBD | Performance should shift from confusion to resolve. |
| VO_050_HANDOFF_READY | needed | TBD | Inori | reaction | Final handoff into playable combat | TBD by Master | TBD | TBD | TBD | Should not fight the gameplay transition. |

## Placement Notes

- Keep voice cues independent from camera data until each shot is approved.
- When a shot is approved, set `Approx Time`, `Shot Link`, and `Unity Path`.
- Use the shortest acceptable vocal cue first; add full dialogue only when the scene truly needs it.
- Leave room for GatePod hum, invasion alarm, weapon pickup, and UI/gameplay handoff sounds.
