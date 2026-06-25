using DimensionBrawl.Presentation;
using UnityEditor;
using UnityEngine;

namespace DimensionBrawl.Editor
{
    public static class BuildResubmissionCinematicProfileSetup
    {
        public const string ProfileRoot = "Assets/_Game/DesignData/Profiles/Cinematics";
        private const string InoriRifleObjectName = "InoriRifle";
        private const string DragonSupportAttackStateName = "FlyStationarySpitFireBall";

        [MenuItem("DimensionBrawl/Cinematics/Rebuild Build Resubmission P0 Profiles")]
        public static void RebuildP0ProfilesMenu()
        {
            RebuildP0Profiles();
        }

        public static void RunBatchProfileGeneration()
        {
            RebuildP0Profiles();
        }

        public static void RebuildP0Profiles()
        {
            BuildResubmissionCinematicAnimationSetup.RebuildInoriCinematicP0Animations();
            EnsureFolder(ProfileRoot);
            ConfigureIntroAwakening(LoadOrCreate(ProfileRoot + "/DB_Cinematic_IntroAwakening.asset"));
            ConfigureGameplayHandoff(LoadOrCreate(ProfileRoot + "/DB_Cinematic_GameplayHandoff.asset"));
            ConfigureQteAssist(LoadOrCreate(ProfileRoot + "/DB_Cinematic_QTEAssist.asset"));
            ConfigureUltimateCutIn(LoadOrCreate(ProfileRoot + "/DB_Cinematic_UltimateCutIn.asset"));
            ConfigureDangerCue(LoadOrCreate(ProfileRoot + "/DB_Cinematic_DangerCue.asset"));
            ConfigureCombatTutorialOverlay(LoadOrCreate(ProfileRoot + "/DB_Cinematic_CombatTutorialOverlay.asset"));
            ConfigureBossIntro(LoadOrCreate(ProfileRoot + "/DB_Cinematic_BossIntro.asset"));
            ConfigurePhaseTransition(LoadOrCreate(ProfileRoot + "/DB_Cinematic_PhaseTransition.asset"));
            ConfigureBreakMoment(LoadOrCreate(ProfileRoot + "/DB_Cinematic_BreakMoment.asset"));
            ConfigureDialogueReactionBeat(LoadOrCreate(ProfileRoot + "/DB_Cinematic_DialogueReactionBeat.asset"));
            ConfigureResultBridge(LoadOrCreate(ProfileRoot + "/DB_Cinematic_ResultBridge.asset"));
            ConfigureSummonEntry(LoadOrCreate(ProfileRoot + "/DB_Cinematic_SummonEntry.asset"));
            ConfigureSummonFollowupHit(LoadOrCreate(ProfileRoot + "/DB_Cinematic_SummonFollowupHit.asset"));
            ConfigureSummonEmpower(LoadOrCreate(ProfileRoot + "/DB_Cinematic_SummonEmpower.asset"));
            ConfigureSummonRecall(LoadOrCreate(ProfileRoot + "/DB_Cinematic_SummonRecall.asset"));
            ConfigureBossSummonPressure(LoadOrCreate(ProfileRoot + "/DB_Cinematic_BossSummonPressure.asset"));
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Rebuilt build-resubmission P0/P1 cinematic sequence profiles.");
        }

        private static void ConfigureIntroAwakening(CinematicSequenceProfile profile)
        {
            profile.Configure(
                "intro_awakening",
                "Intro Awakening",
                CinematicSequenceProfile.SequenceCategory.IntroAwakening,
                "39-second opening vertical slice: capsule wake, system warning, body reveal, collapse establish, sword/gun pickup, enemy standoff, and gameplay back-view takeover. Kawaii candidates remain candidate notes until promoted/retargeted onto Inori.",
                39f,
                90,
                true,
                true,
                true,
                false,
                true,
                new[]
                {
                    ShotCamera("capsule_wakeup_first_person", CinematicSequenceProfile.ShotPurpose.NewInformation, CinematicSequenceProfile.CameraBlendKind.Ease, 0f, 4.6f, new Vector3(0f, 0.02f, -0.10f), 0f, -1.0f, -0.06f, -0.02f, new Vector3(0.24f, 1.48f, 1.62f), new Vector3(0f, 1.22f, 0.04f), 34f),
                    ShotCamera("system_warning_scan", CinematicSequenceProfile.ShotPurpose.MechanicConnection, CinematicSequenceProfile.CameraBlendKind.Reframe, 4.6f, 5.2f, new Vector3(-0.10f, 0.08f, -0.18f), 0.08f, 1.2f, -0.16f, 0.04f, new Vector3(-0.82f, 1.42f, 1.42f), new Vector3(0.04f, 1.12f, 0.08f), 34f),
                    ShotCamera("capsule_open_body_reveal", CinematicSequenceProfile.ShotPurpose.CharacterAction, CinematicSequenceProfile.CameraBlendKind.PullBack, 9.8f, 1.1f, new Vector3(0.08f, 0.08f, -0.26f), 0.12f, 2.0f, -0.22f, 0.08f, new Vector3(0.58f, 1.28f, 1.62f), new Vector3(0f, 1.16f, 0.12f), 32f),
                    ShotCamera("heaven_collapse_establishing", CinematicSequenceProfile.ShotPurpose.NewInformation, CinematicSequenceProfile.CameraBlendKind.PullBack, 15.6f, 5.0f, new Vector3(0.22f, 0.14f, -0.32f), -0.10f, 2.8f, -0.30f, 0.12f, new Vector3(2.15f, 1.72f, 3.24f), new Vector3(0f, 1.10f, 0.08f), 43f),
                    ShotCamera("sword_pickup_action", CinematicSequenceProfile.ShotPurpose.MechanicConnection, CinematicSequenceProfile.CameraBlendKind.PushIn, 20.6f, 4.6f, new Vector3(-0.14f, 0.04f, 0.10f), 0.06f, -1.8f, 0.14f, -0.01f, new Vector3(0.62f, 0.92f, 1.04f), new Vector3(0.02f, 0.72f, 0.10f), 29f),
                    ShotCamera("gun_pickup_action", CinematicSequenceProfile.ShotPurpose.MechanicConnection, CinematicSequenceProfile.CameraBlendKind.PushIn, 25.2f, 1.1f, new Vector3(0.10f, 0.05f, 0.14f), 0.10f, -2.0f, 0.16f, 0.02f, new Vector3(0.90f, 1.34f, 2.12f), new Vector3(0f, 1.04f, 0.16f), 36f),
                    ShotCamera("enemy_standoff_threat_direction", CinematicSequenceProfile.ShotPurpose.ThreatDirection, CinematicSequenceProfile.CameraBlendKind.Reframe, 30f, 5.2f, new Vector3(0f, 0.08f, -0.24f), 0.20f, 2.4f, -0.24f, 0.08f, new Vector3(1.18f, 1.34f, 2.08f), new Vector3(0f, 1.12f, 0.20f), 34f),
                    ShotCamera("gameplay_backview_takeover", CinematicSequenceProfile.ShotPurpose.GameplayHandoff, CinematicSequenceProfile.CameraBlendKind.GameplayMatch, 35.2f, 3.8f, new Vector3(0f, 0.03f, -0.10f), 0.04f, 0.6f, -0.08f, 0.02f, new Vector3(0f, 1.26f, -3.85f), new Vector3(0f, 1.18f, 0.45f), 34f)
                },
                new[]
                {
                    WeaponVisibility("intro_hide_rifle_until_pickup", 0f, false),
                    Body("inori_confused_idle", 0.3f, 6.6f, "CIN_IntroLookAtHands"),
                    Face("inori_surprised_wake", 1.2f, 2.5f, "Surprised"),
                    Face("inori_confused_scan", 6.0f, 4.0f, "Confused"),
                    Body("inori_body_reveal_surprised", 9.9f, 3.7f, "CIN_IntroSurprised"),
                    Face("inori_surprised_body_reveal", 9.9f, 3.2f, "Surprised"),
                    Body("inori_sword_pickup_candidate", 20.8f, 3.8f, "CIN_IntroPickUp"),
                    WeaponVisibility("intro_show_rifle_after_pickup", 25.2f, true),
                    Body("inori_rifle_ready", 26.6f, 3.4f, "CIN_CombatReady"),
                    Face("inori_resolve", 30.5f, 3.8f, "Angry"),
                    Body("inori_gameplay_ready", 35.2f, 3.6f, "CIN_CombatReady")
                },
                new[]
                {
                    Vfx("system_warning_glitch", 4.8f, CombatVfxCueId.ElitePhaseSwapSignal, 0.8f),
                    Vfx("capsule_open_flash", 9.8f, CombatVfxCueId.EliteSummonSignal, 0.9f),
                    Vfx("weapon_ready_muzzle_read", 27.6f, CombatVfxCueId.PlayerRangedMuzzleFlash, 0.65f),
                    Vfx("enemy_standoff_pressure", 31.0f, CombatVfxCueId.EnemyWindup, 1.0f)
                },
                new CinematicSequenceProfile.TutorialCue[0],
                Handoff(38.2f, "action_camera_controller"));
            EditorUtility.SetDirty(profile);
        }

        private static void ConfigureGameplayHandoff(CinematicSequenceProfile profile)
        {
            profile.Configure(
                "gameplay_handoff",
                "Gameplay Handoff",
                CinematicSequenceProfile.SequenceCategory.GameplayHandoff,
                "Reusable short return from cinematic framing to the real action camera. It must remove input/camera dirt without a visible snap.",
                2.4f,
                80,
                true,
                true,
                false,
                false,
                true,
                new[]
                {
                    ShotCamera("combat_ready_rear_match", CinematicSequenceProfile.ShotPurpose.GameplayHandoff, CinematicSequenceProfile.CameraBlendKind.GameplayMatch, 0f, 1.2f, new Vector3(0f, 0.04f, -0.12f), 0.02f, 0.8f, -0.08f, 0.02f, new Vector3(0.72f, 1.32f, 2.05f), new Vector3(0f, 1.12f, 0.18f), 35f),
                    ShotCamera("input_return_settle", CinematicSequenceProfile.ShotPurpose.GameplayHandoff, CinematicSequenceProfile.CameraBlendKind.Ease, 1.2f, 1.2f, new Vector3(0f, 0.00f, -0.04f), 0f, 0.2f, -0.02f, 0f, new Vector3(0f, 1.22f, -4.05f), new Vector3(0f, 1.18f, 0.55f), 35f)
                },
                new[]
                {
                    WeaponVisibility("handoff_keep_rifle_visible", 0f, true),
                    Body("inori_backview_ready", 0f, 2.4f, "CIN_CombatReady")
                },
                new CinematicSequenceProfile.VfxCue[0],
                new CinematicSequenceProfile.TutorialCue[0],
                Handoff(1.9f, "action_camera_controller"));
            EditorUtility.SetDirty(profile);
        }

        private static void ConfigureQteAssist(CinematicSequenceProfile profile)
        {
            profile.Configure(
                "qte_assist",
                "QTE Assist",
                CinematicSequenceProfile.SequenceCategory.QteAssist,
                "Short assist/tag-in grammar: readable prompt, actor entry, support hit, VFX confirmation, and immediate return.",
                3.2f,
                78,
                true,
                true,
                false,
                false,
                true,
                new[]
                {
                    ShotCamera("qte_prompt_reframe", CinematicSequenceProfile.ShotPurpose.MechanicConnection, CinematicSequenceProfile.CameraBlendKind.Reframe, 0f, 0.55f, new Vector3(0f, 0.06f, -0.16f), 0.10f, 1.4f, -0.16f, 0.05f, new Vector3(0.34f, 1.32f, 1.28f), new Vector3(0f, 1.08f, 0.08f), 32f),
                    ShotCamera("assist_entry_dash", CinematicSequenceProfile.ShotPurpose.CharacterAction, CinematicSequenceProfile.CameraBlendKind.PushIn, 0.55f, 1.05f, new Vector3(-0.16f, 0.04f, 0.10f), 0.16f, -1.8f, 0.16f, 0.02f, new Vector3(-1.06f, 1.04f, 1.48f), new Vector3(0.08f, 0.96f, 0.25f), 34f),
                    ShotCamera("assist_hit_confirm", CinematicSequenceProfile.ShotPurpose.MechanicConnection, CinematicSequenceProfile.CameraBlendKind.Cut, 1.6f, 0.7f, new Vector3(0.10f, 0.04f, 0.18f), 0.10f, -2.2f, 0.18f, 0.01f, new Vector3(0.92f, 1.38f, 2.05f), new Vector3(0f, 1.18f, 0.18f), 34f),
                    ShotCamera("qte_return", CinematicSequenceProfile.ShotPurpose.GameplayHandoff, CinematicSequenceProfile.CameraBlendKind.GameplayMatch, 2.3f, 0.9f, new Vector3(0f, 0.02f, -0.08f), 0f, 0.4f, -0.06f, 0f, new Vector3(0f, 1.24f, -3.65f), new Vector3(0f, 1.16f, 0.45f), 34f)
                },
                new[]
                {
                    WeaponVisibility("qte_keep_rifle_visible", 0f, true),
                    Body("inori_assist_entry", 0.40f, 1.2f, "CIN_QTEEntryDash"),
                    Face("inori_qte_focus", 0.55f, 1.4f, "Angry"),
                    Body("inori_assist_fire", 1.55f, 0.8f, "CIN_QTEMagicShot"),
                    Body("inori_assist_recover", 2.3f, 0.9f, "CIN_CombatReady")
                },
                new[]
                {
                    Vfx("qte_hit_confirm", 1.55f, CombatVfxCueId.SummonFollowupHit, 1.15f)
                },
                new[]
                {
                    Tutorial("qte_button_prompt", CinematicSequenceProfile.TutorialCueKind.QtePrompt, 0f, 2.35f, "QTE", "QTE", true, new Vector2(0.72f, 0.72f))
                },
                Handoff(2.8f, "action_camera_controller"));
            EditorUtility.SetDirty(profile);
        }

        private static void ConfigureUltimateCutIn(CinematicSequenceProfile profile)
        {
            profile.Configure(
                "ultimate_cutin",
                "Ultimate Cut-In",
                CinematicSequenceProfile.SequenceCategory.UltimateCutIn,
                "High-impact but short combat cut-in: focus, charge, release, impact, cleanup. Uses promoted Inori body states, face presets, combat VFX, and authored shot poses.",
                4.0f,
                92,
                true,
                true,
                false,
                false,
                true,
                new[]
                {
                    ShotCamera("ultimate_focus_close", CinematicSequenceProfile.ShotPurpose.EmotionChange, CinematicSequenceProfile.CameraBlendKind.PushIn, 0f, 0.85f, new Vector3(-0.12f, 0.08f, -0.16f), 0.08f, -1.6f, 0.12f, 0.05f, new Vector3(0.66f, 1.38f, 1.28f), new Vector3(0.02f, 1.16f, 0.04f), 28f),
                    ShotCamera("ultimate_charge_arc", CinematicSequenceProfile.ShotPurpose.MechanicConnection, CinematicSequenceProfile.CameraBlendKind.Reframe, 0.85f, 1.05f, new Vector3(0.14f, 0.10f, -0.24f), 0.16f, 2.4f, -0.24f, 0.08f, new Vector3(-1.10f, 1.34f, 1.75f), new Vector3(0.06f, 1.12f, 0.20f), 32f),
                    ShotCamera("ultimate_release_hit", CinematicSequenceProfile.ShotPurpose.CharacterAction, CinematicSequenceProfile.CameraBlendKind.Cut, 1.9f, 0.85f, new Vector3(0.08f, 0.04f, 0.20f), 0.14f, -2.8f, 0.20f, 0.02f, new Vector3(0.82f, 1.24f, 1.85f), new Vector3(0.02f, 1.08f, 0.12f), 30f),
                    ShotCamera("ultimate_recover_handoff", CinematicSequenceProfile.ShotPurpose.GameplayHandoff, CinematicSequenceProfile.CameraBlendKind.GameplayMatch, 2.75f, 1.25f, new Vector3(0f, 0.03f, -0.10f), 0.03f, 0.6f, -0.08f, 0.01f, new Vector3(0f, 1.26f, -3.85f), new Vector3(0f, 1.18f, 0.45f), 34f)
                },
                new[]
                {
                    WeaponVisibility("ultimate_keep_rifle_visible", 0f, true),
                    Face("inori_calm_focus", 0f, 0.9f, "CalmEye"),
                    Body("inori_ultimate_charge", 0.55f, 1.3f, "CIN_QTEMagicShot"),
                    Face("inori_ultimate_release_expression", 1.45f, 1.4f, "Angry"),
                    Body("inori_ultimate_release", 1.9f, 0.75f, "CIN_UltimateRelease"),
                    Body("inori_ultimate_recover", 2.75f, 1.2f, "CIN_CombatReady")
                },
                new[]
                {
                    Vfx("ultimate_charge_signal", 0.65f, CombatVfxCueId.ElitePhaseSwapSignal, 1.05f),
                    Vfx("ultimate_impact_signal", 1.9f, CombatVfxCueId.SummonFollowupHit, 1.35f)
                },
                new CinematicSequenceProfile.TutorialCue[0],
                Handoff(3.45f, "action_camera_controller"));
            EditorUtility.SetDirty(profile);
        }

        private static void ConfigureDangerCue(CinematicSequenceProfile profile)
        {
            profile.Configure(
                "danger_cue",
                "Danger Cue",
                CinematicSequenceProfile.SequenceCategory.DangerCue,
                "Short enemy/boss threat emphasis that preserves threat direction and returns control quickly.",
                1.45f,
                70,
                false,
                true,
                false,
                false,
                true,
                new[]
                {
                    ShotCamera("danger_threat_reframe", CinematicSequenceProfile.ShotPurpose.ThreatDirection, CinematicSequenceProfile.CameraBlendKind.Reframe, 0f, 0.55f, new Vector3(0f, 0.05f, -0.16f), 0.18f, 1.6f, -0.16f, 0.05f, new Vector3(0.78f, 1.32f, 1.40f), new Vector3(0f, 1.10f, 0.10f), 31f),
                    ShotCamera("danger_brace_return", CinematicSequenceProfile.ShotPurpose.GameplayHandoff, CinematicSequenceProfile.CameraBlendKind.Ease, 0.55f, 0.9f, new Vector3(0f, 0.02f, -0.06f), 0.02f, 0.4f, -0.04f, 0f, new Vector3(0.95f, 1.34f, -2.85f), new Vector3(0f, 1.12f, 1.20f), 39f)
                },
                new[]
                {
                    WeaponVisibility("danger_keep_rifle_visible", 0f, true),
                    Face("inori_danger_surprise", 0f, 0.45f, "Surprised"),
                    Body("inori_danger_evade_ready", 0.35f, 0.7f, "CIN_CombatReady"),
                    Face("inori_danger_resolve", 0.55f, 0.8f, "Angry")
                },
                new[]
                {
                    Vfx("danger_warning_signal", 0f, CombatVfxCueId.EnemyWindup, 0.95f)
                },
                new[]
                {
                    Tutorial("danger_warning_prompt", CinematicSequenceProfile.TutorialCueKind.WarningPrompt, 0f, 1.1f, "DANGER", "DANGER", true, new Vector2(0.28f, 0.70f))
                },
                Handoff(1.1f, "action_camera_controller"));
            EditorUtility.SetDirty(profile);
        }

        private static void ConfigureCombatTutorialOverlay(CinematicSequenceProfile profile)
        {
            profile.Configure(
                "combat_tutorial_overlay",
                "Combat Tutorial Overlay",
                CinematicSequenceProfile.SequenceCategory.CombatTutorialOverlay,
                "TutorialSystem-inspired mask/prompt sequence with large readable combat prompts. This profile records cue timing; final UI owner remains separate.",
                4.2f,
                65,
                false,
                false,
                false,
                false,
                true,
                new[]
                {
                    ShotCamera("tutorial_basic_attack_focus", CinematicSequenceProfile.ShotPurpose.MechanicConnection, CinematicSequenceProfile.CameraBlendKind.Ease, 0f, 1.0f, new Vector3(0f, 0.04f, -0.10f), 0.05f, 0.6f, -0.08f, 0.02f, new Vector3(0.68f, 1.30f, 1.72f), new Vector3(0f, 1.10f, 0.16f), 35f),
                    ShotCamera("tutorial_skill_focus", CinematicSequenceProfile.ShotPurpose.MechanicConnection, CinematicSequenceProfile.CameraBlendKind.Reframe, 1.4f, 1.0f, new Vector3(-0.08f, 0.04f, -0.12f), 0.08f, 0.8f, -0.10f, 0.02f, new Vector3(-0.72f, 1.28f, 1.84f), new Vector3(0f, 1.10f, 0.16f), 35f),
                    ShotCamera("tutorial_ultimate_ready_focus", CinematicSequenceProfile.ShotPurpose.MechanicConnection, CinematicSequenceProfile.CameraBlendKind.Ease, 2.8f, 1.0f, new Vector3(0.08f, 0.04f, -0.12f), 0.10f, 0.9f, -0.10f, 0.03f, new Vector3(0.82f, 1.34f, 1.92f), new Vector3(0f, 1.12f, 0.18f), 34f)
                },
                new[]
                {
                    WeaponVisibility("tutorial_keep_rifle_visible", 0f, true),
                    Body("inori_tutorial_ready", 0f, 4.2f, "CIN_CombatReady")
                },
                new[]
                {
                    Vfx("tutorial_skill_ready_signal", 1.4f, CombatVfxCueId.PlayerRangedMuzzleFlash, 0.55f)
                },
                new[]
                {
                    Tutorial("basic_attack_prompt", CinematicSequenceProfile.TutorialCueKind.ClickPrompt, 0f, 1.25f, "BASIC_ATTACK", "ATTACK", true, new Vector2(0.78f, 0.70f)),
                    Tutorial("skill_prompt", CinematicSequenceProfile.TutorialCueKind.SkillPrompt, 1.4f, 1.25f, "SKILL", "SKILL", true, new Vector2(0.78f, 0.66f)),
                    Tutorial("ultimate_prompt", CinematicSequenceProfile.TutorialCueKind.UltimatePrompt, 2.8f, 1.25f, "ULTIMATE", "ULT", true, new Vector2(0.78f, 0.62f))
                },
                Handoff(3.8f, "action_camera_controller"));
            EditorUtility.SetDirty(profile);
        }

        private static void ConfigureBossIntro(CinematicSequenceProfile profile)
        {
            profile.Configure(
                "boss_intro",
                "Boss Intro",
                CinematicSequenceProfile.SequenceCategory.BossIntro,
                "Reusable elite/boss entrance: threat reveal, Inori reaction, weapon-ready answer, and a clean return to combat framing.",
                5.6f,
                86,
                true,
                true,
                true,
                true,
                true,
                new[]
                {
                    ShotCamera("boss_shadow_reveal", CinematicSequenceProfile.ShotPurpose.NewInformation, CinematicSequenceProfile.CameraBlendKind.PullBack, 0f, 1.45f, new Vector3(0.18f, 0.12f, -0.22f), 0.24f, 3.0f, -0.26f, 0.12f, new Vector3(1.65f, 1.62f, 2.84f), new Vector3(0f, 1.20f, 0.22f), 38f),
                    ShotCamera("inori_boss_reaction", CinematicSequenceProfile.ShotPurpose.EmotionChange, CinematicSequenceProfile.CameraBlendKind.PushIn, 1.45f, 1.15f, new Vector3(-0.08f, 0.08f, -0.16f), 0.10f, -1.8f, 0.12f, 0.06f, new Vector3(0.62f, 1.38f, 1.50f), new Vector3(0f, 1.16f, 0.06f), 29f),
                    ShotCamera("boss_weapon_answer", CinematicSequenceProfile.ShotPurpose.CharacterAction, CinematicSequenceProfile.CameraBlendKind.Reframe, 2.60f, 1.55f, new Vector3(0.06f, 0.04f, -0.16f), 0.12f, -1.0f, -0.10f, 0.03f, new Vector3(0.46f, 1.36f, -2.82f), new Vector3(0f, 1.14f, 3.35f), 36f),
                    ShotCamera("boss_intro_handoff", CinematicSequenceProfile.ShotPurpose.GameplayHandoff, CinematicSequenceProfile.CameraBlendKind.GameplayMatch, 4.15f, 1.45f, new Vector3(0f, 0.03f, -0.10f), 0.03f, 0.5f, -0.06f, 0.01f, new Vector3(0f, 1.26f, -3.85f), new Vector3(0f, 1.18f, 0.55f), 35f)
                },
                new[]
                {
                    WeaponVisibility("boss_intro_keep_rifle_visible", 0f, true),
                    Face("inori_boss_surprised", 0.25f, 1.4f, "Surprised"),
                    Body("inori_boss_brace", 1.25f, 1.4f, "CIN_IntroSurprised"),
                    Face("inori_boss_resolve", 2.2f, 2.2f, "Angry"),
                    Body("inori_boss_ready", 2.35f, 0.55f, "CIN_BackViewProjectileAim"),
                    Body("inori_boss_fire", 2.9f, 1.05f, "CIN_BackViewProjectileBurst"),
                    Body("inori_boss_recover", 3.95f, 1.1f, "CIN_BackViewProjectileAim")
                },
                new[]
                {
                    Vfx("boss_aura_reveal", 0.25f, CombatVfxCueId.EliteAuraSignal, 1.25f),
                    Vfx("boss_pressure_windup", 2.15f, CombatVfxCueId.EnemyWindup, 1.08f),
                    Vfx("boss_answer_muzzle", 2.92f, CombatVfxCueId.PlayerRangedMuzzleFlash, 1.08f, new Vector3(0.12f, 1.08f, 0.85f)),
                    Vfx("boss_answer_impact", 3.16f, CombatVfxCueId.PlayerRangedProjectileImpact, 1.18f, new Vector3(0f, 1.02f, 4.35f))
                },
                new CinematicSequenceProfile.TutorialCue[0],
                Handoff(5.1f, "action_camera_controller"));
            EditorUtility.SetDirty(profile);
        }

        private static void ConfigurePhaseTransition(CinematicSequenceProfile profile)
        {
            profile.Configure(
                "phase_transition",
                "Phase Transition",
                CinematicSequenceProfile.SequenceCategory.PhaseTransition,
                "Reusable boss/elite phase swap: field shock, close emotional response, release beat, then combat handoff.",
                4.8f,
                84,
                true,
                true,
                true,
                true,
                true,
                new[]
                {
                    ShotCamera("phase_field_shock", CinematicSequenceProfile.ShotPurpose.Transition, CinematicSequenceProfile.CameraBlendKind.Reframe, 0f, 1.0f, new Vector3(0.12f, 0.08f, -0.18f), 0.14f, 2.2f, -0.18f, 0.08f, new Vector3(-1.20f, 1.46f, 2.34f), new Vector3(0f, 1.12f, 0.18f), 36f),
                    ShotCamera("phase_inori_focus", CinematicSequenceProfile.ShotPurpose.EmotionChange, CinematicSequenceProfile.CameraBlendKind.PushIn, 1.0f, 1.0f, new Vector3(-0.10f, 0.08f, -0.14f), 0.08f, -1.4f, 0.08f, 0.05f, new Vector3(0.56f, 1.36f, 1.42f), new Vector3(0f, 1.15f, 0.06f), 28f),
                    ShotCamera("phase_counter_release", CinematicSequenceProfile.ShotPurpose.CharacterAction, CinematicSequenceProfile.CameraBlendKind.Cut, 2.0f, 1.15f, new Vector3(0.10f, 0.04f, -0.16f), 0.12f, -1.2f, -0.10f, 0.02f, new Vector3(-0.36f, 1.34f, -2.62f), new Vector3(0f, 1.10f, 3.20f), 34f),
                    ShotCamera("phase_handoff", CinematicSequenceProfile.ShotPurpose.GameplayHandoff, CinematicSequenceProfile.CameraBlendKind.GameplayMatch, 3.15f, 1.3f, new Vector3(0f, 0.03f, -0.10f), 0.02f, 0.4f, -0.06f, 0.01f, new Vector3(0f, 1.25f, -3.70f), new Vector3(0f, 1.17f, 0.55f), 35f)
                },
                new[]
                {
                    WeaponVisibility("phase_keep_rifle_visible", 0f, true),
                    Face("phase_inori_shock", 0.15f, 1.0f, "Surprised"),
                    Body("phase_inori_stagger", 0.2f, 1.4f, "CIN_IntroStumble"),
                    Face("phase_inori_answer", 1.15f, 2.0f, "Angry"),
                    Body("phase_inori_release", 2.0f, 1.0f, "CIN_BackViewProjectileFire"),
                    Body("phase_inori_recover", 3.15f, 1.4f, "CIN_BackViewProjectileAim")
                },
                new[]
                {
                    Vfx("phase_swap_burst", 0f, CombatVfxCueId.ElitePhaseSwapSignal, 1.25f),
                    Vfx("phase_counter_muzzle", 2.03f, CombatVfxCueId.PlayerRangedMuzzleFlash, 1.0f, new Vector3(0.10f, 1.08f, 0.82f)),
                    Vfx("phase_counter_impact", 2.26f, CombatVfxCueId.PlayerRangedProjectileImpact, 1.16f, new Vector3(0f, 1.02f, 4.15f))
                },
                new CinematicSequenceProfile.TutorialCue[0],
                Handoff(4.25f, "action_camera_controller"));
            EditorUtility.SetDirty(profile);
        }

        private static void ConfigureBreakMoment(CinematicSequenceProfile profile)
        {
            profile.Configure(
                "break_moment",
                "Break Moment",
                CinematicSequenceProfile.SequenceCategory.BreakMoment,
                "Reusable guard/break payoff: pressure tells, Inori confirms the break, VFX hit read, and quick control return.",
                3.4f,
                82,
                true,
                true,
                false,
                true,
                true,
                new[]
                {
                    ShotCamera("break_pressure_tell", CinematicSequenceProfile.ShotPurpose.MechanicConnection, CinematicSequenceProfile.CameraBlendKind.PushIn, 0f, 0.75f, new Vector3(0.06f, 0.06f, -0.14f), 0.08f, 0.8f, -0.08f, 0.03f, new Vector3(0.18f, 1.30f, -3.05f), new Vector3(0f, 1.08f, 2.45f), 36f),
                    ShotCamera("break_hit_confirm", CinematicSequenceProfile.ShotPurpose.CharacterAction, CinematicSequenceProfile.CameraBlendKind.Cut, 0.75f, 0.95f, new Vector3(0.14f, 0.04f, -0.16f), 0.14f, -1.8f, -0.12f, 0.02f, new Vector3(0.34f, 1.28f, -2.38f), new Vector3(0f, 1.08f, 3.05f), 34f),
                    ShotCamera("break_reward_settle", CinematicSequenceProfile.ShotPurpose.GameplayHandoff, CinematicSequenceProfile.CameraBlendKind.Ease, 1.70f, 1.20f, new Vector3(0f, 0.03f, -0.08f), 0.02f, 0.4f, -0.06f, 0.01f, new Vector3(0f, 1.25f, -3.55f), new Vector3(0f, 1.15f, 0.55f), 34f)
                },
                new[]
                {
                    WeaponVisibility("break_keep_rifle_visible", 0f, true),
                    Face("break_inori_focus", 0f, 1.1f, "Angry"),
                    Body("break_inori_fire", 0.65f, 0.95f, "CIN_BackViewProjectileFire"),
                    Body("break_inori_ready", 1.7f, 1.6f, "CIN_BackViewProjectileAim"),
                    Face("break_inori_calm", 1.85f, 1.0f, "CalmEye")
                },
                new[]
                {
                    Vfx("break_window_open", 0.15f, CombatVfxCueId.SummonFollowupWindow, 1.0f),
                    Vfx("break_muzzle_flash", 0.74f, CombatVfxCueId.PlayerRangedMuzzleFlash, 1.02f, new Vector3(0.10f, 1.08f, 0.78f)),
                    Vfx("break_hit_vfx", 0.9f, CombatVfxCueId.PlayerRangedProjectileImpact, 1.2f, new Vector3(0f, 1.02f, 4.05f))
                },
                new CinematicSequenceProfile.TutorialCue[0],
                Handoff(2.85f, "action_camera_controller"));
            EditorUtility.SetDirty(profile);
        }

        private static void ConfigureDialogueReactionBeat(CinematicSequenceProfile profile)
        {
            profile.Configure(
                "dialogue_reaction_beat",
                "Dialogue Reaction Beat",
                CinematicSequenceProfile.SequenceCategory.DialogueReactionBeat,
                "Reusable short story reaction beat using Inori face presets, readable close framing, and a gentle return to gameplay-ready pose.",
                3.8f,
                76,
                true,
                true,
                true,
                true,
                true,
                new[]
                {
                    ShotCamera("dialogue_listen_close", CinematicSequenceProfile.ShotPurpose.EmotionChange, CinematicSequenceProfile.CameraBlendKind.Ease, 0f, 1.15f, new Vector3(-0.08f, 0.08f, -0.14f), 0.05f, -1.4f, 0.06f, 0.04f, new Vector3(0.50f, 1.38f, 1.34f), new Vector3(0f, 1.16f, 0.04f), 27f),
                    ShotCamera("dialogue_answer_shift", CinematicSequenceProfile.ShotPurpose.EmotionChange, CinematicSequenceProfile.CameraBlendKind.Reframe, 1.15f, 1.15f, new Vector3(0.08f, 0.06f, -0.12f), 0.07f, -0.8f, 0.08f, 0.04f, new Vector3(-0.54f, 1.34f, 1.40f), new Vector3(0f, 1.14f, 0.06f), 28f),
                    ShotCamera("dialogue_ready_return", CinematicSequenceProfile.ShotPurpose.GameplayHandoff, CinematicSequenceProfile.CameraBlendKind.Ease, 2.3f, 1.1f, new Vector3(0f, 0.03f, -0.08f), 0.02f, 0.6f, -0.05f, 0.01f, new Vector3(0f, 1.26f, -3.55f), new Vector3(0f, 1.15f, 0.55f), 34f)
                },
                new[]
                {
                    WeaponVisibility("dialogue_keep_rifle_visible", 0f, true),
                    Body("dialogue_inori_listen", 0f, 1.4f, "CIN_IntroLookAtHands"),
                    Face("dialogue_inori_confused", 0.15f, 1.1f, "Confused"),
                    Body("dialogue_inori_answer_shift", 1.15f, 1.0f, "CIN_IntroSurprised"),
                    Face("dialogue_inori_surprised", 1.15f, 0.8f, "Surprised"),
                    Body("dialogue_inori_ready", 2.2f, 1.5f, "CIN_BackViewProjectileAim"),
                    Face("dialogue_inori_resolve", 2.25f, 1.2f, "CalmEye")
                },
                new CinematicSequenceProfile.VfxCue[0],
                new CinematicSequenceProfile.TutorialCue[0],
                Handoff(3.35f, "action_camera_controller"));
            EditorUtility.SetDirty(profile);
        }

        private static void ConfigureResultBridge(CinematicSequenceProfile profile)
        {
            profile.Configure(
                "result_bridge",
                "Result Bridge",
                CinematicSequenceProfile.SequenceCategory.ResultBridge,
                "Reusable end-of-fight bridge: final stance, impact cleanup, calm expression, and handoff to result UI.",
                4.2f,
                72,
                true,
                true,
                true,
                true,
                true,
                new[]
                {
                    ShotCamera("result_final_impact", CinematicSequenceProfile.ShotPurpose.CharacterAction, CinematicSequenceProfile.CameraBlendKind.Cut, 0f, 0.8f, new Vector3(0.12f, 0.04f, -0.16f), 0.12f, -1.2f, -0.12f, 0.02f, new Vector3(0.30f, 1.26f, -2.45f), new Vector3(0f, 1.08f, 3.15f), 34f),
                    ShotCamera("result_inori_settle", CinematicSequenceProfile.ShotPurpose.EmotionChange, CinematicSequenceProfile.CameraBlendKind.PullBack, 0.8f, 1.35f, new Vector3(0f, 0.06f, -0.16f), 0.03f, 1.0f, -0.08f, 0.03f, new Vector3(1.20f, 1.46f, -3.10f), new Vector3(0.10f, 1.08f, 1.85f), 40f),
                    ShotCamera("result_ui_handoff", CinematicSequenceProfile.ShotPurpose.GameplayHandoff, CinematicSequenceProfile.CameraBlendKind.Ease, 2.15f, 1.45f, new Vector3(0f, 0.02f, -0.08f), 0.01f, 0.4f, -0.04f, 0.01f, new Vector3(0.65f, 1.32f, -3.45f), new Vector3(0f, 1.12f, 0.85f), 38f)
                },
                new[]
                {
                    WeaponVisibility("result_keep_rifle_visible", 0f, true),
                    Body("result_inori_release", 0f, 0.8f, "CIN_BackViewProjectileBurst"),
                    Face("result_inori_intense", 0f, 0.9f, "Angry"),
                    Body("result_inori_recover", 0.8f, 1.5f, "CIN_ResultSettle"),
                    Face("result_inori_calm", 1.4f, 1.8f, "CalmEye"),
                    Body("result_inori_ready", 2.15f, 1.8f, "CIN_BackViewProjectileAim")
                },
                new[]
                {
                    Vfx("result_final_muzzle", 0.02f, CombatVfxCueId.PlayerRangedMuzzleFlash, 1.05f, new Vector3(0.12f, 1.08f, 0.85f)),
                    Vfx("result_final_hit", 0f, CombatVfxCueId.EnemyDeath, 1.05f, new Vector3(0f, 1.05f, 4.2f)),
                    Vfx("result_clear_signal", 1.35f, CombatVfxCueId.PocketCleared, 1.0f)
                },
                new CinematicSequenceProfile.TutorialCue[0],
                Handoff(CinematicSequenceProfile.GameplayReturnMode.ResultUi, 3.6f, "result_ui", 0.1f, restoreCamera: false));
            EditorUtility.SetDirty(profile);
        }

        private static void ConfigureSummonEntry(CinematicSequenceProfile profile)
        {
            profile.Configure(
                "summon_entry",
                "Summon Entry",
                CinematicSequenceProfile.SequenceCategory.SummonEntry,
                "Reusable summon/assist entrance: summoning signal, Inori command beat, proxy impact, and handoff back to action.",
                4.4f,
                80,
                true,
                true,
                false,
                true,
                true,
                new[]
                {
                    ShotCamera("summon_signal_start", CinematicSequenceProfile.ShotPurpose.MechanicConnection, CinematicSequenceProfile.CameraBlendKind.Reframe, 0f, 0.95f, new Vector3(0.10f, 0.06f, -0.14f), 0.12f, 1.2f, -0.10f, 0.04f, new Vector3(-2.85f, 1.92f, -2.85f), new Vector3(3.75f, 1.70f, 3.15f), 56f),
                    ShotCamera("summon_command_close", CinematicSequenceProfile.ShotPurpose.CharacterAction, CinematicSequenceProfile.CameraBlendKind.PushIn, 0.95f, 1.0f, new Vector3(-0.08f, 0.08f, -0.14f), 0.08f, -1.4f, 0.08f, 0.04f, new Vector3(1.45f, 1.54f, -2.35f), new Vector3(0.15f, 1.12f, 2.70f), 36f),
                    ShotCamera("summon_proxy_hit", CinematicSequenceProfile.ShotPurpose.MechanicConnection, CinematicSequenceProfile.CameraBlendKind.Cut, 1.95f, 1.0f, new Vector3(0.14f, 0.04f, -0.16f), 0.14f, -1.2f, -0.12f, 0.02f, new Vector3(2.10f, 1.44f, -1.65f), new Vector3(0.35f, 1.14f, 3.65f), 36f),
                    ShotCamera("summon_handoff", CinematicSequenceProfile.ShotPurpose.GameplayHandoff, CinematicSequenceProfile.CameraBlendKind.GameplayMatch, 2.95f, 1.2f, new Vector3(0f, 0.03f, -0.08f), 0.02f, 0.4f, -0.05f, 0.01f, new Vector3(0f, 1.26f, -3.70f), new Vector3(0f, 1.17f, 0.55f), 35f)
                },
                new[]
                {
                    WeaponVisibility("summon_keep_rifle_visible", 0f, true),
                    Face("summon_inori_command", 0.45f, 1.3f, "Angry"),
                    Body("summon_inori_cast", 0.8f, 1.05f, "CIN_BackViewProjectileCharge"),
                    Body("summon_inori_proxy_fire", 1.9f, 0.9f, "CIN_BackViewProjectileBurst"),
                    Body("summon_inori_recover", 2.6f, 1.6f, "CIN_BackViewProjectileAim"),
                    Face("summon_inori_calm", 2.8f, 1.0f, "CalmEye"),
                    ActorVisibility("summon_support_dragon_visible", CinematicSequenceProfile.ActorRole.Environment, 0f, true),
                    ActorBody("summon_support_dragon_fire", CinematicSequenceProfile.ActorRole.Environment, 0.2f, 1.5f, DragonSupportAttackStateName),
                    BodyTrigger("summon_proxy_manifest", CinematicSequenceProfile.ActorRole.Summon, 0.05f, "EliteSummonPackage"),
                    BodyTrigger("summon_proxy_attack", CinematicSequenceProfile.ActorRole.Summon, 1.95f, "Attack"),
                    ActorVisibility("summon_support_dragon_hide", CinematicSequenceProfile.ActorRole.Environment, 4.15f, false)
                },
                new[]
                {
                    Vfx("summon_entry_signal", 0f, CombatVfxCueId.EliteSummonSignal, 1.2f, new Vector3(1.05f, 0.05f, 2.25f)),
                    Vfx("summon_proxy_window", 0.95f, CombatVfxCueId.SummonBlockOpportunity, 1.0f, new Vector3(1.05f, 0.65f, 2.45f)),
                    Vfx("summon_proxy_muzzle", 1.96f, CombatVfxCueId.PlayerRangedMuzzleFlash, 1.02f, new Vector3(0.12f, 1.08f, 0.85f)),
                    Vfx("summon_proxy_hit_vfx", 2.12f, CombatVfxCueId.PlayerRangedProjectileImpact, 1.15f, new Vector3(0f, 1.02f, 4.05f))
                },
                new CinematicSequenceProfile.TutorialCue[0],
                Handoff(3.95f, "action_camera_controller"));
            EditorUtility.SetDirty(profile);
        }

        private static void ConfigureSummonFollowupHit(CinematicSequenceProfile profile)
        {
            profile.Configure(
                "summon_followup_hit",
                "Summon Follow-up Hit",
                CinematicSequenceProfile.SequenceCategory.SummonFollowupHit,
                "Reusable summon follow-up payoff: Inori gives the command, the frontline summon attacks, dragon support crosses the flank, and the camera returns to action.",
                3.25f,
                78,
                true,
                true,
                false,
                true,
                true,
                new[]
                {
                    ShotCamera("summon_followup_command", CinematicSequenceProfile.ShotPurpose.MechanicConnection, CinematicSequenceProfile.CameraBlendKind.Reframe, 0f, 0.65f, new Vector3(0.08f, 0.06f, -0.14f), 0.10f, 1.0f, -0.10f, 0.04f, new Vector3(-1.95f, 1.62f, -2.65f), new Vector3(1.25f, 1.18f, 3.05f), 42f),
                    ShotCamera("summon_followup_clash", CinematicSequenceProfile.ShotPurpose.CharacterAction, CinematicSequenceProfile.CameraBlendKind.Cut, 0.65f, 1.05f, new Vector3(0.14f, 0.04f, -0.16f), 0.16f, -1.4f, -0.12f, 0.02f, new Vector3(1.20f, 1.40f, -2.10f), new Vector3(0.45f, 1.08f, 3.95f), 46f),
                    ShotCamera("summon_followup_dragon_cross", CinematicSequenceProfile.ShotPurpose.ThreatDirection, CinematicSequenceProfile.CameraBlendKind.Reframe, 1.70f, 0.8f, new Vector3(-0.10f, 0.08f, -0.18f), 0.18f, 2.0f, -0.18f, 0.08f, new Vector3(-3.05f, 1.86f, -1.95f), new Vector3(4.10f, 1.92f, 3.25f), 54f),
                    ShotCamera("summon_followup_handoff", CinematicSequenceProfile.ShotPurpose.GameplayHandoff, CinematicSequenceProfile.CameraBlendKind.GameplayMatch, 2.50f, 0.75f, new Vector3(0f, 0.03f, -0.08f), 0.02f, 0.4f, -0.05f, 0.01f, new Vector3(0f, 1.26f, -3.70f), new Vector3(0f, 1.17f, 0.55f), 35f)
                },
                new[]
                {
                    WeaponVisibility("summon_followup_keep_rifle_visible", 0f, true),
                    Face("summon_followup_inori_command", 0f, 1.0f, "Angry"),
                    Body("summon_followup_inori_signal", 0f, 0.85f, "CIN_BackViewProjectileCharge"),
                    Body("summon_followup_inori_release", 0.66f, 0.95f, "CIN_BackViewProjectileFire"),
                    Body("summon_followup_inori_recover", 1.75f, 1.2f, "CIN_BackViewProjectileAim"),
                    Face("summon_followup_inori_calm", 2.05f, 1.0f, "CalmEye"),
                    ActorVisibility("summon_followup_dragon_visible", CinematicSequenceProfile.ActorRole.Environment, 0f, true),
                    BodyTrigger("summon_followup_proxy_attack", CinematicSequenceProfile.ActorRole.Summon, 0.62f, "Attack"),
                    ActorBody("summon_followup_dragon_fire", CinematicSequenceProfile.ActorRole.Environment, 1.45f, 1.0f, DragonSupportAttackStateName),
                    ActorVisibility("summon_followup_dragon_hide", CinematicSequenceProfile.ActorRole.Environment, 3.15f, false)
                },
                new[]
                {
                    Vfx("summon_followup_window", 0f, CombatVfxCueId.SummonFollowupWindow, 1.08f, new Vector3(0.95f, 0.72f, 2.35f)),
                    Vfx("summon_followup_muzzle", 0.68f, CombatVfxCueId.PlayerRangedMuzzleFlash, 1.04f, new Vector3(0.12f, 1.08f, 0.82f)),
                    Vfx("summon_followup_hit", 0.92f, CombatVfxCueId.SummonFollowupHit, 1.24f, new Vector3(0.05f, 1.04f, 4.10f)),
                    Vfx("summon_followup_dragon_flash", 1.72f, CombatVfxCueId.EliteSummonSignal, 0.82f, new Vector3(3.40f, 1.62f, 3.15f))
                },
                new CinematicSequenceProfile.TutorialCue[0],
                Handoff(2.95f, "action_camera_controller"));
            EditorUtility.SetDirty(profile);
        }

        private static void ConfigureSummonEmpower(CinematicSequenceProfile profile)
        {
            profile.Configure(
                "summon_empower",
                "Summon Empower",
                CinematicSequenceProfile.SequenceCategory.SummonEmpower,
                "Reusable summon empower beat: Inori channels energy into the frontline summon, the summon holds the lane, dragon support amplifies the read, and control returns cleanly.",
                3.45f,
                74,
                true,
                true,
                true,
                true,
                true,
                new[]
                {
                    ShotCamera("summon_empower_channel", CinematicSequenceProfile.ShotPurpose.MechanicConnection, CinematicSequenceProfile.CameraBlendKind.Reframe, 0f, 0.85f, new Vector3(0.08f, 0.06f, -0.14f), 0.10f, 1.2f, -0.10f, 0.04f, new Vector3(-1.65f, 1.54f, -2.80f), new Vector3(0.85f, 1.18f, 2.95f), 40f),
                    ShotCamera("summon_empower_transfer", CinematicSequenceProfile.ShotPurpose.CharacterAction, CinematicSequenceProfile.CameraBlendKind.PushIn, 0.85f, 0.95f, new Vector3(-0.08f, 0.08f, -0.16f), 0.12f, -1.2f, -0.10f, 0.05f, new Vector3(0.85f, 1.34f, -2.15f), new Vector3(0.20f, 1.10f, 3.25f), 36f),
                    ShotCamera("summon_empower_hold", CinematicSequenceProfile.ShotPurpose.ThreatDirection, CinematicSequenceProfile.CameraBlendKind.Reframe, 1.80f, 0.8f, new Vector3(0.10f, 0.04f, -0.12f), 0.14f, 1.6f, -0.12f, 0.03f, new Vector3(2.35f, 1.42f, -1.95f), new Vector3(0.45f, 1.18f, 3.80f), 40f),
                    ShotCamera("summon_empower_handoff", CinematicSequenceProfile.ShotPurpose.GameplayHandoff, CinematicSequenceProfile.CameraBlendKind.GameplayMatch, 2.60f, 0.85f, new Vector3(0f, 0.03f, -0.08f), 0.02f, 0.4f, -0.05f, 0.01f, new Vector3(0f, 1.26f, -3.70f), new Vector3(0f, 1.17f, 0.55f), 35f)
                },
                new[]
                {
                    WeaponVisibility("summon_empower_keep_rifle_visible", 0f, true),
                    Face("summon_empower_inori_focus", 0f, 1.2f, "Angry"),
                    Body("summon_empower_inori_channel", 0f, 1.05f, "CIN_BackViewProjectileCharge"),
                    Body("summon_empower_inori_transfer", 0.95f, 1.0f, "CIN_BackViewProjectileFire"),
                    Body("summon_empower_inori_ready", 2.05f, 1.1f, "CIN_BackViewProjectileAim"),
                    Face("summon_empower_inori_calm", 2.25f, 0.8f, "CalmEye"),
                    ActorVisibility("summon_empower_dragon_visible", CinematicSequenceProfile.ActorRole.Environment, 0f, true),
                    BodyTrigger("summon_empower_proxy_manifest", CinematicSequenceProfile.ActorRole.Summon, 0.10f, "EliteSummonPackage"),
                    BodyTrigger("summon_empower_proxy_attack", CinematicSequenceProfile.ActorRole.Summon, 1.70f, "Attack"),
                    ActorBody("summon_empower_dragon_fire", CinematicSequenceProfile.ActorRole.Environment, 1.35f, 1.0f, DragonSupportAttackStateName),
                    ActorVisibility("summon_empower_dragon_hide", CinematicSequenceProfile.ActorRole.Environment, 3.35f, false)
                },
                new[]
                {
                    Vfx("summon_empower_channel_ring", 0f, CombatVfxCueId.SummonFollowupWindow, 1.10f, new Vector3(0.85f, 0.55f, 2.35f)),
                    Vfx("summon_empower_transfer_flash", 0.90f, CombatVfxCueId.EliteAuraSignal, 0.90f, new Vector3(0.80f, 0.95f, 2.75f)),
                    Vfx("summon_empower_guard", 1.82f, CombatVfxCueId.EliteShieldSignal, 0.84f, new Vector3(0.30f, 1.05f, 3.65f)),
                    Vfx("summon_empower_release", 2.15f, CombatVfxCueId.PlayerRangedMuzzleFlash, 0.92f, new Vector3(0.12f, 1.08f, 0.82f))
                },
                new CinematicSequenceProfile.TutorialCue[0],
                Handoff(3.05f, "action_camera_controller"));
            EditorUtility.SetDirty(profile);
        }

        private static void ConfigureSummonRecall(CinematicSequenceProfile profile)
        {
            profile.Configure(
                "summon_recall",
                "Summon Recall",
                CinematicSequenceProfile.SequenceCategory.SummonRecall,
                "Reusable summon recall beat: Inori calls the summon back, the field collapses inward, dragon support exits, and the camera settles to gameplay.",
                3.35f,
                70,
                true,
                true,
                true,
                true,
                true,
                new[]
                {
                    ShotCamera("summon_recall_signal", CinematicSequenceProfile.ShotPurpose.MechanicConnection, CinematicSequenceProfile.CameraBlendKind.Reframe, 0f, 0.75f, new Vector3(0.08f, 0.06f, -0.14f), 0.10f, 1.0f, -0.10f, 0.04f, new Vector3(-1.35f, 1.44f, -2.70f), new Vector3(0.45f, 1.10f, 2.85f), 38f),
                    ShotCamera("summon_recall_collapse", CinematicSequenceProfile.ShotPurpose.Transition, CinematicSequenceProfile.CameraBlendKind.PullBack, 0.75f, 1.05f, new Vector3(-0.11f, 0.08f, -0.15f), 0.12f, 1.55f, -0.15f, 0.05f, new Vector3(-0.95f, 1.50f, -2.45f), new Vector3(0.55f, 1.10f, 3.15f), 40f),
                    ShotCamera("summon_recall_dragon_exit", CinematicSequenceProfile.ShotPurpose.ThreatDirection, CinematicSequenceProfile.CameraBlendKind.Reframe, 1.80f, 0.75f, new Vector3(-0.10f, 0.08f, -0.18f), 0.18f, 1.6f, -0.18f, 0.08f, new Vector3(-2.65f, 1.80f, -1.85f), new Vector3(3.95f, 1.80f, 3.05f), 50f),
                    ShotCamera("summon_recall_handoff", CinematicSequenceProfile.ShotPurpose.GameplayHandoff, CinematicSequenceProfile.CameraBlendKind.GameplayMatch, 2.55f, 0.8f, new Vector3(0f, 0.03f, -0.08f), 0.02f, 0.4f, -0.05f, 0.01f, new Vector3(0f, 1.26f, -3.70f), new Vector3(0f, 1.17f, 0.55f), 35f)
                },
                new[]
                {
                    WeaponVisibility("summon_recall_keep_rifle_visible", 0f, true),
                    Face("summon_recall_inori_signal", 0f, 1.0f, "CalmEye"),
                    Body("summon_recall_inori_ready_signal", 0f, 0.9f, "CIN_BackViewProjectileAim"),
                    Body("summon_recall_inori_close", 0.82f, 1.0f, "CIN_BackViewProjectileRecover"),
                    Body("summon_recall_inori_ready", 1.90f, 1.0f, "CIN_CombatReady"),
                    Face("summon_recall_inori_calm", 1.95f, 0.9f, "CalmEye"),
                    ActorVisibility("summon_recall_dragon_visible", CinematicSequenceProfile.ActorRole.Environment, 0f, true),
                    BodyTrigger("summon_recall_proxy_manifest", CinematicSequenceProfile.ActorRole.Summon, 0.05f, "EliteSummonPackage"),
                    BodyTrigger("summon_recall_proxy_attack", CinematicSequenceProfile.ActorRole.Summon, 0.72f, "Attack"),
                    ActorBody("summon_recall_dragon_fire", CinematicSequenceProfile.ActorRole.Environment, 1.35f, 0.85f, DragonSupportAttackStateName),
                    ActorVisibility("summon_recall_dragon_hide", CinematicSequenceProfile.ActorRole.Environment, 2.65f, false)
                },
                new[]
                {
                    Vfx("summon_recall_ring", 0f, CombatVfxCueId.EliteSummonSignal, 0.88f, new Vector3(0.70f, 0.50f, 2.25f)),
                    Vfx("summon_recall_collapse", 0.82f, CombatVfxCueId.SummonBlockOpportunity, 0.92f, new Vector3(0.42f, 0.62f, 2.95f)),
                    Vfx("summon_recall_exit_flash", 1.72f, CombatVfxCueId.PocketCleared, 0.82f, new Vector3(2.90f, 1.50f, 3.10f))
                },
                new CinematicSequenceProfile.TutorialCue[0],
                Handoff(2.95f, "action_camera_controller"));
            EditorUtility.SetDirty(profile);
        }

        private static void ConfigureBossSummonPressure(CinematicSequenceProfile profile)
        {
            profile.Configure(
                "boss_summon_pressure",
                "Boss Summon Pressure",
                CinematicSequenceProfile.SequenceCategory.BossSummonPressure,
                "Reusable boss-summon pressure beat: the frontline summon catches the lane pressure, Inori answers from back-view, dragon support marks the flank, and control returns to action.",
                3.55f,
                76,
                true,
                true,
                false,
                true,
                true,
                new[]
                {
                    ShotCamera("boss_summon_pressure_wall", CinematicSequenceProfile.ShotPurpose.MechanicConnection, CinematicSequenceProfile.CameraBlendKind.Reframe, 0f, 0.75f, new Vector3(0.08f, 0.06f, -0.14f), 0.10f, 1.5f, -0.12f, 0.04f, new Vector3(-1.75f, 1.52f, -2.55f), new Vector3(0.65f, 1.12f, 3.20f), 42f),
                    ShotCamera("boss_summon_pressure_guard", CinematicSequenceProfile.ShotPurpose.ThreatDirection, CinematicSequenceProfile.CameraBlendKind.Cut, 0.75f, 0.95f, new Vector3(0.12f, 0.05f, -0.16f), 0.14f, -1.4f, -0.12f, 0.03f, new Vector3(0.95f, 1.36f, -2.05f), new Vector3(0.15f, 1.08f, 3.55f), 38f),
                    ShotCamera("boss_summon_pressure_crack", CinematicSequenceProfile.ShotPurpose.CharacterAction, CinematicSequenceProfile.CameraBlendKind.PushIn, 1.70f, 0.85f, new Vector3(0.10f, 0.04f, -0.14f), 0.12f, -1.2f, -0.10f, 0.02f, new Vector3(2.25f, 1.42f, -1.70f), new Vector3(0.35f, 1.10f, 3.85f), 42f),
                    ShotCamera("boss_summon_pressure_handoff", CinematicSequenceProfile.ShotPurpose.GameplayHandoff, CinematicSequenceProfile.CameraBlendKind.GameplayMatch, 2.55f, 0.85f, new Vector3(0f, 0.03f, -0.08f), 0.02f, 0.4f, -0.05f, 0.01f, new Vector3(0f, 1.26f, -3.70f), new Vector3(0f, 1.17f, 0.55f), 35f)
                },
                new[]
                {
                    WeaponVisibility("boss_summon_pressure_keep_rifle_visible", 0f, true),
                    Face("boss_summon_pressure_inori_focus", 0f, 1.15f, "Angry"),
                    Body("boss_summon_pressure_inori_charge", 0f, 0.85f, "CIN_BackViewProjectileCharge"),
                    Body("boss_summon_pressure_inori_fire", 0.82f, 0.95f, "CIN_BackViewProjectileFire"),
                    Body("boss_summon_pressure_inori_ready", 1.85f, 1.1f, "CIN_BackViewProjectileAim"),
                    Face("boss_summon_pressure_inori_calm", 2.10f, 0.9f, "CalmEye"),
                    ActorVisibility("boss_summon_pressure_dragon_visible", CinematicSequenceProfile.ActorRole.Environment, 0f, true),
                    BodyTrigger("boss_summon_pressure_proxy_manifest", CinematicSequenceProfile.ActorRole.Summon, 0.05f, "EliteSummonPackage"),
                    BodyTrigger("boss_summon_pressure_proxy_guard", CinematicSequenceProfile.ActorRole.Summon, 0.62f, "Attack"),
                    ActorBody("boss_summon_pressure_dragon_fire", CinematicSequenceProfile.ActorRole.Environment, 1.35f, 1.0f, DragonSupportAttackStateName),
                    ActorVisibility("boss_summon_pressure_dragon_hide", CinematicSequenceProfile.ActorRole.Environment, 3.35f, false)
                },
                new[]
                {
                    Vfx("boss_summon_pressure_window", 0f, CombatVfxCueId.SummonFollowupWindow, 1.08f, new Vector3(0.92f, 0.58f, 2.35f)),
                    Vfx("boss_summon_pressure_guard", 0.72f, CombatVfxCueId.EliteShieldSignal, 1.02f, new Vector3(0.35f, 1.04f, 3.40f)),
                    Vfx("boss_summon_pressure_answer", 1.05f, CombatVfxCueId.PlayerRangedMuzzleFlash, 0.94f, new Vector3(0.12f, 1.08f, 0.82f)),
                    Vfx("boss_summon_pressure_crack", 1.68f, CombatVfxCueId.PlayerRangedProjectileImpact, 1.18f, new Vector3(0.05f, 1.02f, 4.05f)),
                    Vfx("boss_summon_pressure_dragon_mark", 1.92f, CombatVfxCueId.EliteAuraSignal, 0.72f, new Vector3(3.10f, 1.58f, 3.15f))
                },
                new CinematicSequenceProfile.TutorialCue[0],
                Handoff(3.10f, "action_camera_controller"));
            EditorUtility.SetDirty(profile);
        }

        private static CinematicSequenceProfile.CameraCue Camera(
            string cueId,
            CinematicSequenceProfile.ShotPurpose purpose,
            CinematicSequenceProfile.CameraBlendKind blendKind,
            float startSeconds,
            float durationSeconds,
            Vector3 localOffset,
            float planarDirectionOffset,
            float fieldOfViewDelta,
            float cameraDistanceDelta,
            float focusHeightDelta)
        {
            return new CinematicSequenceProfile.CameraCue(
                cueId,
                purpose,
                blendKind,
                startSeconds,
                durationSeconds,
                localOffset,
                planarDirectionOffset,
                fieldOfViewDelta,
                cameraDistanceDelta,
                focusHeightDelta);
        }

        private static CinematicSequenceProfile.CameraCue ShotCamera(
            string cueId,
            CinematicSequenceProfile.ShotPurpose purpose,
            CinematicSequenceProfile.CameraBlendKind blendKind,
            float startSeconds,
            float durationSeconds,
            Vector3 localOffset,
            float planarDirectionOffset,
            float fieldOfViewDelta,
            float cameraDistanceDelta,
            float focusHeightDelta,
            Vector3 cameraLocalPosition,
            Vector3 lookAtLocalPosition,
            float fieldOfView)
        {
            return new CinematicSequenceProfile.CameraCue(
                cueId,
                purpose,
                blendKind,
                startSeconds,
                durationSeconds,
                localOffset,
                planarDirectionOffset,
                fieldOfViewDelta,
                cameraDistanceDelta,
                focusHeightDelta,
                cameraLocalPosition,
                lookAtLocalPosition,
                fieldOfView);
        }

        private static CinematicSequenceProfile.ActorCue Body(string cueId, float startSeconds, float durationSeconds, string stateName)
        {
            return new CinematicSequenceProfile.ActorCue(
                cueId,
                CinematicSequenceProfile.ActorRole.Inori,
                CinematicSequenceProfile.ActorCueKind.BodyState,
                startSeconds,
                durationSeconds,
                stateName);
        }

        private static CinematicSequenceProfile.ActorCue ActorBody(
            string cueId,
            CinematicSequenceProfile.ActorRole role,
            float startSeconds,
            float durationSeconds,
            string stateName)
        {
            return new CinematicSequenceProfile.ActorCue(
                cueId,
                role,
                CinematicSequenceProfile.ActorCueKind.BodyState,
                startSeconds,
                durationSeconds,
                stateName);
        }

        private static CinematicSequenceProfile.ActorCue BodyTrigger(
            string cueId,
            CinematicSequenceProfile.ActorRole role,
            float startSeconds,
            string triggerName)
        {
            return new CinematicSequenceProfile.ActorCue(
                cueId,
                role,
                CinematicSequenceProfile.ActorCueKind.BodyTrigger,
                startSeconds,
                0f,
                string.Empty,
                triggerName);
        }

        private static CinematicSequenceProfile.ActorCue Face(string cueId, float startSeconds, float durationSeconds, string stateName)
        {
            return new CinematicSequenceProfile.ActorCue(
                cueId,
                CinematicSequenceProfile.ActorRole.Inori,
                CinematicSequenceProfile.ActorCueKind.FaceState,
                startSeconds,
                durationSeconds,
                string.Empty,
                string.Empty,
                stateName);
        }

        private static CinematicSequenceProfile.ActorCue WeaponVisibility(string cueId, float startSeconds, bool visible)
        {
            return new CinematicSequenceProfile.ActorCue(
                cueId,
                CinematicSequenceProfile.ActorRole.Inori,
                CinematicSequenceProfile.ActorCueKind.WeaponVisibility,
                startSeconds,
                0f,
                string.Empty,
                socketPath: InoriRifleObjectName,
                requireSocket: true,
                objectActive: visible);
        }

        private static CinematicSequenceProfile.ActorCue ActorVisibility(
            string cueId,
            CinematicSequenceProfile.ActorRole role,
            float startSeconds,
            bool visible)
        {
            return new CinematicSequenceProfile.ActorCue(
                cueId,
                role,
                CinematicSequenceProfile.ActorCueKind.WeaponVisibility,
                startSeconds,
                0f,
                string.Empty,
                requireSocket: false,
                objectActive: visible);
        }

        private static CinematicSequenceProfile.VfxCue Vfx(
            string cueId,
            float startSeconds,
            CombatVfxCueId cueIdValue,
            float intensity)
        {
            return new CinematicSequenceProfile.VfxCue(cueId, startSeconds, 0f, cueIdValue, intensity);
        }

        private static CinematicSequenceProfile.VfxCue Vfx(
            string cueId,
            float startSeconds,
            CombatVfxCueId cueIdValue,
            float intensity,
            Vector3 localOffset)
        {
            return new CinematicSequenceProfile.VfxCue(cueId, startSeconds, 0f, cueIdValue, localOffset, intensity);
        }

        private static CinematicSequenceProfile.TutorialCue Tutorial(
            string cueId,
            CinematicSequenceProfile.TutorialCueKind cueKind,
            float startSeconds,
            float durationSeconds,
            string promptKey,
            string guideText,
            bool requireLargeText,
            Vector2 screenAnchor)
        {
            return new CinematicSequenceProfile.TutorialCue(
                cueId,
                cueKind,
                startSeconds,
                durationSeconds,
                promptKey,
                guideText,
                requireLargeText,
                screenAnchor);
        }

        private static CinematicSequenceProfile.GameplayHandoffCue Handoff(float startSeconds, string targetId)
        {
            return Handoff(
                CinematicSequenceProfile.GameplayReturnMode.ActionCameraController,
                startSeconds,
                targetId,
                0.08f);
        }

        private static CinematicSequenceProfile.GameplayHandoffCue Handoff(
            CinematicSequenceProfile.GameplayReturnMode returnMode,
            float startSeconds,
            string targetId,
            float inputReleaseDelaySeconds,
            bool restoreHud = true,
            bool restoreTimeScale = true,
            bool restoreCamera = true)
        {
            return new CinematicSequenceProfile.GameplayHandoffCue(
                returnMode,
                startSeconds,
                targetId,
                inputReleaseDelaySeconds,
                restoreHud,
                restoreTimeScale,
                restoreCamera);
        }

        private static CinematicSequenceProfile LoadOrCreate(string assetPath)
        {
            CinematicSequenceProfile profile = AssetDatabase.LoadAssetAtPath<CinematicSequenceProfile>(assetPath);
            if (profile != null)
            {
                return profile;
            }

            profile = ScriptableObject.CreateInstance<CinematicSequenceProfile>();
            AssetDatabase.CreateAsset(profile, assetPath);
            return profile;
        }

        private static void EnsureFolder(string folderPath)
        {
            if (AssetDatabase.IsValidFolder(folderPath))
            {
                return;
            }

            int separatorIndex = folderPath.LastIndexOf('/');
            string parent = folderPath.Substring(0, separatorIndex);
            string name = folderPath.Substring(separatorIndex + 1);
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, name);
        }
    }
}
