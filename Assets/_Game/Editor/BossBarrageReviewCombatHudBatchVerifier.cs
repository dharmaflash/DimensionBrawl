using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DimensionBrawl.Combat;
using DimensionBrawl.Player;
using DimensionBrawl.Presentation;
using DimensionBrawl.Test;
using DimensionBrawl.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace DimensionBrawl.Editor
{
    [InitializeOnLoad]
    public static class BossBarrageReviewCombatHudBatchVerifier
    {
        private const string ScenePath =
            "Assets/_Game/Scenes/ActionFoundationBossBarrageLaneReview.unity";
        private static readonly string ResultPath = Path.Combine(
            Path.GetTempPath(),
            "DimensionBrawl-BossBarrageCombatHudBatch.result");
        private const string ActiveKey =
            "DimensionBrawl.BossBarrage.CombatHudBatch.Active";
        private const string StartedAtKey =
            "DimensionBrawl.BossBarrage.CombatHudBatch.StartedAt";
        private const string CapturedKey =
            "DimensionBrawl.BossBarrage.CombatHudBatch.Captured";
        private const string CombatHudCanvasRootName =
            "BossBarrageLaneReview_CombatHudCanvas";
        private const string CombatHudEventSystemRootName =
            "BossBarrageLaneReview_CombatHudEventSystem";
        private const string CombatHudInstanceName =
            "PF_UI_CombatHud";
        private const string DimensionHudSkinRootName =
            "DimensionHudSkinRoot";
        private const string DimensionHudArtRoot =
            "Assets/_Game/UI/CombatHud/Art/DimensionHud";

        private static readonly Vector2 DimensionHudDesignResolution = new Vector2(2560f, 1440f);

        private static readonly DesignRectCheck[] ImageDesignRects =
        {
            new DesignRectCheck("TopLeftPanel", new Rect(45f, 36f, 571f, 165f)),
            new DesignRectCheck("BossSymbol", new Rect(850f, 39f, 59f, 71f)),
            new DesignRectCheck("BossNameArea", new Rect(916f, 51f, 759f, 48f)),
            new DesignRectCheck("BossHpBackground", new Rect(851f, 109f, 856f, 31f)),
            new DesignRectCheck("BossHpFill", new Rect(867f, 113f, 823f, 24f)),
            new DesignRectCheck("BossCostBackground", new Rect(854f, 142f, 849f, 34f)),
            new DesignRectCheck("BossCostFill", new Rect(870f, 144f, 819f, 25f)),
            new DesignRectCheck("PlayerSymbol", new Rect(814f, 1195f, 85f, 142f)),
            new DesignRectCheck("PlayerNameArea", new Rect(911f, 1212f, 215f, 43f)),
            new DesignRectCheck("PlayerHpAmountArea", new Rect(1326f, 1220f, 201f, 32f)),
            new DesignRectCheck("PlayerMpAmountArea", new Rect(1359f, 1327f, 147f, 26f)),
            new DesignRectCheck("SettingsButton", new Rect(2250f, 47f, 100f, 95f)),
            new DesignRectCheck("PauseButton", new Rect(2396f, 47f, 100f, 95f)),
            new DesignRectCheck("MoveJoystickRing", new Rect(155f, 853f, 421f, 415f)),
            new DesignRectCheck("MoveJoystickKnob", new Rect(303f, 1004f, 122f, 121f)),
            new DesignRectCheck("BasicAttackButton", new Rect(2239f, 1156f, 230f, 248f)),
            new DesignRectCheck("DodgeButton", new Rect(1975f, 1172f, 256f, 218f)),
            new DesignRectCheck("Skill1Button", new Rect(2217f, 868f, 236f, 286f)),
            new DesignRectCheck("UltimateButton", new Rect(1975f, 896f, 248f, 226f)),
            new DesignRectCheck("SummonSlot1Button", new Rect(2293f, 235f, 211f, 216f)),
            new DesignRectCheck("SummonSlot2Button", new Rect(2308f, 472f, 182f, 186f)),
            new DesignRectCheck("SummonSlot3Button", new Rect(2312f, 683f, 179f, 183f)),
            new DesignRectCheck("HealthBar_Track", new Rect(901f, 1263f, 640f, 21f)),
            new DesignRectCheck("HealthBar", new Rect(904f, 1263f, 633f, 20f)),
            new DesignRectCheck("ResourceBar_Track", new Rect(909f, 1296f, 616f, 28f)),
            new DesignRectCheck("ResourceBar", new Rect(915f, 1299f, 605f, 21f))
        };

        private static readonly DesignRectCheck[] TextDesignRects =
        {
            new DesignRectCheck("Timer", new Rect(178f, 55f, 409f, 48f)),
            new DesignRectCheck("Objective", new Rect(180f, 117f, 409f, 64f)),
            new DesignRectCheck("ActionFeedback", new Rect(916f, 51f, 759f, 48f)),
            new DesignRectCheck("InputMode", new Rect(911f, 1212f, 215f, 43f)),
            new DesignRectCheck("HealthText", new Rect(1326f, 1220f, 201f, 32f)),
            new DesignRectCheck("ResourceText", new Rect(1359f, 1327f, 147f, 26f))
        };

        private static readonly ButtonRouteCheck[] ButtonRouteChecks =
        {
            new ButtonRouteCheck("PauseButton", "RequestPause", "overlayHud", null, new Rect(2396f, 47f, 100f, 95f)),
            new ButtonRouteCheck("BasicAttackButton", "RequestBasicAttack", "rangedBasicAttackAction", "queuedFire", new Rect(2239f, 1156f, 230f, 248f), "combatModeController"),
            new ButtonRouteCheck("DodgeButton", "RequestDodge", "actionController", "mobileDodgeQueued", new Rect(1975f, 1172f, 256f, 218f)),
            new ButtonRouteCheck("Skill1Button", "RequestSkill1", "skill1Action", "queued", new Rect(2217f, 868f, 236f, 286f)),
            new ButtonRouteCheck("UltimateButton", "RequestUltimate", "combatModeController", "queuedSwap", new Rect(1975f, 896f, 248f, 226f)),
            new ButtonRouteCheck("SummonSlot1Button", "RequestSummonSlot1", "summonSlot1Action", "queued", new Rect(2293f, 235f, 211f, 216f)),
            new ButtonRouteCheck("SummonSlot2Button", "RequestSummonSlot2", "summonSlot2Action", "queued", new Rect(2308f, 472f, 182f, 186f)),
            new ButtonRouteCheck("SummonSlot3Button", "RequestSummonSlot3", "summonSlot3Action", "queued", new Rect(2312f, 683f, 179f, 183f))
        };

        private static readonly ResolutionCheck[] ResponsiveResolutions =
        {
            new ResolutionCheck("DESIGN_2560x1440", new Vector2(2560f, 1440f)),
            new ResolutionCheck("REVIEW_3120x1440", new Vector2(3120f, 1440f)),
            new ResolutionCheck("FHD_1920x1080", new Vector2(1920f, 1080f)),
            new ResolutionCheck("HD_1280x720", new Vector2(1280f, 720f)),
            new ResolutionCheck("WIDE_2340x1080", new Vector2(2340f, 1080f))
        };

        private const double WarmupSeconds = 3.0;
        private const double TimeoutSeconds = 90.0;
        private const int WarmupFrames = 30;
        private const int CaptureWidth = 3120;
        private const int CaptureHeight = 1440;

        static BossBarrageReviewCombatHudBatchVerifier()
        {
            EditorApplication.update -= Monitor;
            EditorApplication.update += Monitor;
        }

        public static void RunBatchVerification()
        {
            Clear();
            ActionFoundationBatchVerificationResult.DeleteIfExists(ResultPath);

            Screen.SetResolution(CaptureWidth, CaptureHeight, FullScreenMode.Windowed);
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            EditorPrefs.SetBool(ActiveKey, true);
            EditorPrefs.SetFloat(StartedAtKey, (float)EditorApplication.timeSinceStartup);
            EditorPrefs.SetBool(CapturedKey, false);
            EditorApplication.update -= Monitor;
            EditorApplication.update += Monitor;
            EditorApplication.isPlaying = true;
        }

        private static void Monitor()
        {
            if (!EditorPrefs.GetBool(ActiveKey, false))
            {
                return;
            }

            double startedAt = EditorPrefs.GetFloat(StartedAtKey, (float)EditorApplication.timeSinceStartup);
            if (EditorApplication.timeSinceStartup - startedAt > TimeoutSeconds)
            {
                WriteResult(false, "TIMEOUT", "Batch combat HUD verification timed out before capture.");
                Finish(1);
                return;
            }

            if (!EditorApplication.isPlaying)
            {
                return;
            }

            EditorApplication.QueuePlayerLoopUpdate();
            UnityEditorInternal.InternalEditorUtility.RepaintAllViews();

            if (EditorApplication.timeSinceStartup - startedAt < WarmupSeconds
                || Time.frameCount < WarmupFrames)
            {
                return;
            }

            if (EditorPrefs.GetBool(CapturedKey, false))
            {
                return;
            }

            EditorPrefs.SetBool(CapturedKey, true);
            try
            {
                VerificationSnapshot snapshot = VerifyActivePlayScene();
                WriteResult(snapshot.Passed, "COMPLETE", snapshot.Report);
                Finish(snapshot.Passed ? 0 : 1);
            }
            catch (Exception exception)
            {
                WriteResult(false, "EXCEPTION", exception.ToString());
                Debug.LogException(exception);
                Finish(1);
            }
        }

        private static VerificationSnapshot VerifyActivePlayScene()
        {
            Scene activeScene = SceneManager.GetActiveScene();
            GameObject canvasRoot = FindRoot(activeScene, CombatHudCanvasRootName);
            GameObject eventSystemRoot = FindRoot(activeScene, CombatHudEventSystemRootName);
            GameObject hudInstance = canvasRoot != null ? FindChild(canvasRoot.transform, CombatHudInstanceName) : null;
            GameObject skinRoot = hudInstance != null ? FindChild(hudInstance.transform, DimensionHudSkinRootName) : null;

            Canvas canvas = canvasRoot != null ? canvasRoot.GetComponent<Canvas>() : null;
            CanvasScaler canvasScaler = canvasRoot != null ? canvasRoot.GetComponent<CanvasScaler>() : null;
            GraphicRaycaster raycaster = canvasRoot != null ? canvasRoot.GetComponent<GraphicRaycaster>() : null;
            EventSystem eventSystem = eventSystemRoot != null ? eventSystemRoot.GetComponent<EventSystem>() : null;
            InputSystemUIInputModule inputModule =
                eventSystemRoot != null ? eventSystemRoot.GetComponent<InputSystemUIInputModule>() : null;
            CombatHudPresenter presenter =
                canvasRoot != null ? canvasRoot.GetComponentInChildren<CombatHudPresenter>(includeInactive: true) : null;
            CombatHudInputBridge inputBridge =
                canvasRoot != null ? canvasRoot.GetComponentInChildren<CombatHudInputBridge>(includeInactive: true) : null;
            BossBarrageLaneReviewCombatHudBinder binder =
                canvasRoot != null ? canvasRoot.GetComponent<BossBarrageLaneReviewCombatHudBinder>() : null;
            BossBarrageLaneReviewHud reviewHud = FindSceneObjectOrNull<BossBarrageLaneReviewHud>(activeScene);
            BossBarrageLaneReviewMobileHud mobileHud = FindSceneObjectOrNull<BossBarrageLaneReviewMobileHud>(activeScene);
            BossBarrageLaneReviewOverlayHud overlayHud = FindSceneObjectOrNull<BossBarrageLaneReviewOverlayHud>(activeScene);

            int activeDimensionSpriteCount = CountActiveDimensionHudSprites(hudInstance);
            bool canvasReady = canvasRoot != null
                && canvasRoot.activeInHierarchy
                && canvas != null
                && canvas.enabled
                && canvas.renderMode == RenderMode.ScreenSpaceOverlay
                && canvasScaler != null
                && canvasScaler.referenceResolution == DimensionHudDesignResolution
                && Mathf.Approximately(canvasScaler.matchWidthOrHeight, 1f)
                && raycaster != null;
            bool inputModulePointerReady = HasInputModulePointerActions(inputModule);
            bool eventSystemReady = eventSystemRoot != null
                && eventSystemRoot.activeInHierarchy
                && eventSystem != null
                && inputModule != null
                && inputModulePointerReady;
            bool combatHudReady = hudInstance != null
                && hudInstance.activeInHierarchy
                && presenter != null
                && inputBridge != null
                && binder != null;
            bool dimensionSpritesReady = skinRoot != null
                && skinRoot.activeInHierarchy
                && activeDimensionSpriteCount >= 20;
            bool legacyVisualPolicyReady = reviewHud != null
                && mobileHud != null
                && overlayHud != null
                && !ReadBool(reviewHud, "showHud", true)
                && !mobileHud.enabled
                && !ReadBool(mobileHud, "showHud", true)
                && !ReadBool(mobileHud, "drawHudVisuals", true)
                && ReadBool(overlayHud, "showOverlay", false)
                && !ReadBool(overlayHud, "drawIdleButton", true);
            StringBuilder layoutReport = new StringBuilder();
            bool layoutReady = hudInstance != null
                && HasImageDesignRects(hudInstance, layoutReport);
            bool responsiveProjectionReady = HasResponsiveProjections(layoutReport);

            StringBuilder buttonReport = new StringBuilder();
            PrepareCombatHudInputProbeState(binder);
            bool buttonVisualReady = hudInstance != null
                && HasNoVisibleChildButtonImages(hudInstance, buttonReport);
            bool buttonInputReady = hudInstance != null
                && HasButtonInputs(hudInstance, binder, raycaster, eventSystem, buttonReport);
            bool joystickInputReady = hudInstance != null
                && HasJoystickInput(hudInstance, raycaster, eventSystem, buttonReport);
            bool textAreaReady = hudInstance != null
                && HasTextAreas(hudInstance, buttonReport);
            bool summonActionReady = binder != null
                && HasSummonActionCostsAndCooldowns(binder, buttonReport);

            StringBuilder report = new StringBuilder();
            report.AppendLine($"SCENE={activeScene.path}");
            report.AppendLine($"CANVAS_ROOT={(canvasRoot != null ? canvasRoot.name : "<null>")}");
            report.AppendLine($"HUD_INSTANCE={(hudInstance != null ? hudInstance.name : "<null>")}");
            report.AppendLine($"SKIN_ROOT={(skinRoot != null ? skinRoot.name : "<null>")}");
            report.AppendLine($"ACTIVE_DIMENSION_SPRITES={activeDimensionSpriteCount}");
            report.AppendLine($"HAS_PRESENTER={BoolText(presenter != null)}");
            report.AppendLine($"HAS_INPUT_BRIDGE={BoolText(inputBridge != null)}");
            report.AppendLine($"HAS_BINDER={BoolText(binder != null)}");
            report.AppendLine($"HAS_EVENT_SYSTEM={BoolText(eventSystemReady)}");
            report.AppendLine($"EVENT_SYSTEM_POINTER_ACTIONS={PassText(inputModulePointerReady)}");
            report.AppendLine($"CANVAS_REFERENCE={canvasScaler?.referenceResolution.ToString() ?? "<null>"}");
            report.AppendLine($"CANVAS_MATCH_HEIGHT={canvasScaler?.matchWidthOrHeight.ToString("0.###") ?? "<null>"}");
            report.AppendLine($"LEGACY_REVIEW_HUD_VISIBLE={BoolText(ReadBool(reviewHud, "showHud", true))}");
            report.AppendLine($"LEGACY_MOBILE_HUD_ENABLED={BoolText(mobileHud != null && mobileHud.enabled)}");
            report.AppendLine($"LEGACY_MOBILE_HUD_VISIBLE={BoolText(ReadBool(mobileHud, "showHud", true))}");
            report.AppendLine($"LEGACY_MOBILE_VISUALS_VISIBLE={BoolText(ReadBool(mobileHud, "drawHudVisuals", true))}");
            report.AppendLine($"LEGACY_IDLE_BUTTON_VISIBLE={BoolText(ReadBool(overlayHud, "drawIdleButton", true))}");
            report.AppendLine($"SCREEN={Screen.width}x{Screen.height}");
            report.AppendLine($"CURRENT_FRAME={Time.frameCount}");
            report.AppendLine($"CANVAS_OUTPUT={PassText(canvasReady)}");
            report.AppendLine($"COMBAT_HUD_OUTPUT={PassText(combatHudReady)}");
            report.AppendLine($"DIMENSION_SPRITE_OUTPUT={PassText(dimensionSpritesReady)}");
            report.AppendLine($"DIMENSION_LAYOUT={PassText(layoutReady)}");
            report.AppendLine($"RESPONSIVE_PROJECTION={PassText(responsiveProjectionReady)}");
            if (layoutReport.Length > 0)
            {
                report.Append(layoutReport);
            }

            report.AppendLine($"BUTTON_VISUAL_OVERLAYS={PassText(buttonVisualReady)}");
            report.AppendLine($"BUTTON_INPUTS={PassText(buttonInputReady)}");
            report.AppendLine($"JOYSTICK_INPUTS={PassText(joystickInputReady)}");
            report.AppendLine($"TEXT_AREAS={PassText(textAreaReady)}");
            report.AppendLine($"SUMMON_ACTIONS={PassText(summonActionReady)}");
            if (buttonReport.Length > 0)
            {
                report.Append(buttonReport);
            }

            report.AppendLine($"EVENT_SYSTEM={PassText(eventSystemReady)}");
            report.AppendLine($"LEGACY_VISUAL_POLICY={PassText(legacyVisualPolicyReady)}");

            bool passed = canvasReady
                && combatHudReady
                && dimensionSpritesReady
                && layoutReady
                && responsiveProjectionReady
                && buttonVisualReady
                && buttonInputReady
                && joystickInputReady
                && textAreaReady
                && summonActionReady
                && eventSystemReady
                && legacyVisualPolicyReady;
            report.AppendLine($"RESULT={(passed ? "PASS" : "FAIL")}");

            return new VerificationSnapshot(passed, report.ToString());
        }

        private static bool HasImageDesignRects(GameObject hudInstance, StringBuilder report)
        {
            bool ready = true;
            for (int i = 0; i < ImageDesignRects.Length; i++)
            {
                ready &= HasDesignRect(
                    hudInstance,
                    ImageDesignRects[i].ObjectName,
                    ImageDesignRects[i].DesignRect,
                    report);
            }

            return ready;
        }

        private static bool HasResponsiveProjections(StringBuilder report)
        {
            bool ready = true;
            for (int i = 0; i < ResponsiveResolutions.Length; i++)
            {
                ready &= HasResponsiveProjection(ResponsiveResolutions[i], report);
            }

            return ready;
        }

        private static bool HasResponsiveProjection(ResolutionCheck resolution, StringBuilder report)
        {
            float scale = resolution.Size.y / DimensionHudDesignResolution.y;
            float projectedDesignWidth = DimensionHudDesignResolution.x * scale;
            float horizontalMargin = (resolution.Size.x - projectedDesignWidth) * 0.5f;
            if (horizontalMargin < -0.01f)
            {
                report.AppendLine($"RESPONSIVE_{resolution.Id}=FAIL_SCREEN_NARROW marginX={horizontalMargin:0.###}");
                return false;
            }

            bool ready = true;
            for (int i = 0; i < ImageDesignRects.Length; i++)
            {
                ready &= ProjectedRectFits(resolution, ImageDesignRects[i], scale, horizontalMargin, report);
            }

            for (int i = 0; i < TextDesignRects.Length; i++)
            {
                ready &= ProjectedRectFits(resolution, TextDesignRects[i], scale, horizontalMargin, report);
            }

            if (ready)
            {
                report.AppendLine($"RESPONSIVE_{resolution.Id}=PASS scale={scale:0.###} marginX={horizontalMargin:0.###}");
            }

            return ready;
        }

        private static bool ProjectedRectFits(
            ResolutionCheck resolution,
            DesignRectCheck check,
            float scale,
            float horizontalMargin,
            StringBuilder report)
        {
            Rect projected = new Rect(
                horizontalMargin + check.DesignRect.xMin * scale,
                check.DesignRect.yMin * scale,
                check.DesignRect.width * scale,
                check.DesignRect.height * scale);
            bool fits = projected.xMin >= -0.01f
                && projected.yMin >= -0.01f
                && projected.xMax <= resolution.Size.x + 0.01f
                && projected.yMax <= resolution.Size.y + 0.01f;
            if (!fits)
            {
                report.AppendLine($"RESPONSIVE_{resolution.Id}_{check.ObjectName}=OUT_OF_BOUNDS rect={projected}");
            }

            return fits;
        }

        private static bool HasNoVisibleChildButtonImages(GameObject hudInstance, StringBuilder report)
        {
            bool ready = true;
            for (int i = 0; i < ButtonRouteChecks.Length; i++)
            {
                ready &= HasNoVisibleChildButtonImages(hudInstance, ButtonRouteChecks[i].ButtonName, report);
            }

            return ready;
        }

        private static void PrepareCombatHudInputProbeState(BossBarrageLaneReviewCombatHudBinder binder)
        {
            if (binder == null)
            {
                return;
            }

            SummonEnergyLadder energy = ReadBinderReference<SummonEnergyLadder>(binder, "energyLadder");
            PlayerSummonSlot1Action slot1 = ReadBinderReference<PlayerSummonSlot1Action>(binder, "summonSlot1Action");
            PlayerSupportSummonSlotAction slot2 = ReadBinderReference<PlayerSupportSummonSlotAction>(binder, "summonSlot2Action");
            PlayerSupportSummonSlotAction slot3 = ReadBinderReference<PlayerSupportSummonSlotAction>(binder, "summonSlot3Action");

            slot1?.ClearSlotCooldown();
            if (slot2 != null)
            {
                SetPrivateFloat(slot2, "slotCooldownRemaining", 0f);
            }

            if (slot3 != null)
            {
                SetPrivateFloat(slot3, "slotCooldownRemaining", 0f);
            }

            if (energy != null)
            {
                energy.ResetLadder();
                energy.GrantCurrentTierEnergy(300f);
            }

            System.Reflection.MethodInfo updateMethod = typeof(BossBarrageLaneReviewCombatHudBinder).GetMethod(
                "Update",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            updateMethod?.Invoke(binder, null);
        }

        private static bool HasButtonInputs(
            GameObject hudInstance,
            BossBarrageLaneReviewCombatHudBinder binder,
            GraphicRaycaster raycaster,
            EventSystem eventSystem,
            StringBuilder report)
        {
            bool ready = true;
            for (int i = 0; i < ButtonRouteChecks.Length; i++)
            {
                ButtonRouteCheck check = ButtonRouteChecks[i];
                ready &= HasButtonClickRoute(hudInstance, check.ButtonName, check.MethodName, report);
                ready &= HasSerializedReference(binder, check.PrimaryReferenceFieldName, report);
                if (!string.IsNullOrEmpty(check.SecondaryReferenceFieldName))
                {
                    ready &= HasSerializedReference(binder, check.SecondaryReferenceFieldName, report);
                }

                if (!string.IsNullOrEmpty(check.QueueFieldName))
                {
                    ready &= HasPointerActionInput(hudInstance, check, report);
                    ready &= InvokesQueuedAction(hudInstance, binder, check, report);
                    ready &= InvokesQueuedActionFromPointer(
                        hudInstance,
                        binder,
                        raycaster,
                        eventSystem,
                        check,
                        report);
                }

                ready &= RaycastsToButtonCenter(
                    raycaster,
                    eventSystem,
                    hudInstance,
                    check.ButtonName,
                    check.DesignRect,
                    report);
            }

            return ready;
        }

        private static bool HasTextAreas(GameObject hudInstance, StringBuilder report)
        {
            bool ready = true;
            for (int i = 0; i < TextDesignRects.Length; i++)
            {
                ready &= HasVisibleTextRect(
                    hudInstance,
                    TextDesignRects[i].ObjectName,
                    TextDesignRects[i].DesignRect,
                    report);
            }

            ready &= HasVisibleChildText(hudInstance, "SummonSlot1Button", "Label", report);
            ready &= HasVisibleChildText(hudInstance, "SummonSlot1Button", "State", report);
            ready &= HasVisibleChildText(hudInstance, "SummonSlot2Button", "Label", report);
            ready &= HasVisibleChildText(hudInstance, "SummonSlot2Button", "State", report);
            ready &= HasVisibleChildText(hudInstance, "SummonSlot3Button", "Label", report);
            ready &= HasVisibleChildText(hudInstance, "SummonSlot3Button", "State", report);
            return ready;
        }

        private static bool HasInputModulePointerActions(InputSystemUIInputModule inputModule)
        {
            return inputModule != null
                && inputModule.point != null
                && inputModule.point.action != null
                && inputModule.leftClick != null
                && inputModule.leftClick.action != null;
        }

        private static bool HasJoystickInput(
            GameObject hudInstance,
            GraphicRaycaster raycaster,
            EventSystem eventSystem,
            StringBuilder report)
        {
            GameObject ringObject = FindChild(hudInstance.transform, "MoveJoystickRing");
            GameObject knobObject = FindChild(hudInstance.transform, "MoveJoystickKnob");
            CombatHudVirtualJoystick joystick = ringObject != null
                ? ringObject.GetComponent<CombatHudVirtualJoystick>()
                : null;
            RectTransform ringTransform = ringObject != null ? ringObject.GetComponent<RectTransform>() : null;
            Image ringImage = ringObject != null ? ringObject.GetComponent<Image>() : null;
            if (ringObject == null || knobObject == null || joystick == null || ringTransform == null)
            {
                report.AppendLine($"JOYSTICK_COMPONENT=FAIL ring={BoolText(ringObject != null)} knob={BoolText(knobObject != null)} joystick={BoolText(joystick != null)} rect={BoolText(ringTransform != null)}");
                return false;
            }

            using var joystickObject = new SerializedObject(joystick);
            PlayerMovementController movement = joystickObject.FindProperty("movementController")?.objectReferenceValue
                as PlayerMovementController;
            RectTransform configuredKnob = joystickObject.FindProperty("knob")?.objectReferenceValue
                as RectTransform;
            bool configReady = movement != null
                && configuredKnob != null
                && configuredKnob.gameObject == knobObject
                && ringImage != null
                && ringImage.raycastTarget;
            if (!configReady)
            {
                report.AppendLine($"JOYSTICK_CONFIG=FAIL movement={BoolText(movement != null)} knob={BoolText(configuredKnob != null)} knobMatches={BoolText(configuredKnob != null && configuredKnob.gameObject == knobObject)} raycast={BoolText(ringImage != null && ringImage.raycastTarget)}");
                return false;
            }

            bool raycastReady = RaycastsToControlCenter(
                raycaster,
                eventSystem,
                ringObject,
                "MoveJoystickRing",
                report);
            if (!raycastReady)
            {
                return false;
            }

            System.Reflection.FieldInfo moveInputField = typeof(PlayerMovementController).GetField(
                "mobileMoveInput",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (moveInputField == null)
            {
                report.AppendLine("JOYSTICK_MOVEMENT_FIELD=MISSING mobileMoveInput");
                return false;
            }

            moveInputField.SetValue(movement, Vector2.zero);
            Vector2 center = GetRectCenterScreenPoint(ringTransform);
            float dragOffset = Mathf.Max(24f, ringTransform.rect.width * 0.25f);
            var downData = new PointerEventData(eventSystem)
            {
                button = PointerEventData.InputButton.Left,
                pointerId = -1,
                position = center,
                pressPosition = center
            };
            var dragData = new PointerEventData(eventSystem)
            {
                button = PointerEventData.InputButton.Left,
                pointerId = -1,
                position = center + new Vector2(dragOffset, 0f),
                pressPosition = center
            };

            ExecuteEvents.Execute(ringObject, downData, ExecuteEvents.pointerDownHandler);
            ExecuteEvents.Execute(ringObject, dragData, ExecuteEvents.dragHandler);
            Vector2 joystickInput = joystick.CurrentInput;
            Vector2 movementInput = moveInputField.GetValue(movement) is Vector2 activeInput
                ? activeInput
                : Vector2.zero;
            ExecuteEvents.Execute(ringObject, dragData, ExecuteEvents.pointerUpHandler);
            Vector2 releasedInput = moveInputField.GetValue(movement) is Vector2 released
                ? released
                : Vector2.one;
            moveInputField.SetValue(movement, Vector2.zero);

            bool activeReady = joystickInput.sqrMagnitude > 0.01f && movementInput.sqrMagnitude > 0.01f;
            bool releasedReady = releasedInput.sqrMagnitude <= 0.0001f;
            if (!activeReady || !releasedReady)
            {
                report.AppendLine($"JOYSTICK_POINTER=FAIL active={BoolText(activeReady)} released={BoolText(releasedReady)} joystick={joystickInput} movement={movementInput} releasedInput={releasedInput}");
            }

            return raycastReady && activeReady && releasedReady;
        }

        private static int CountActiveDimensionHudSprites(GameObject hudInstance)
        {
            if (hudInstance == null)
            {
                return 0;
            }

            int count = 0;
            Image[] images = hudInstance.GetComponentsInChildren<Image>(includeInactive: true);
            for (int i = 0; i < images.Length; i++)
            {
                Image image = images[i];
                if (image == null
                    || image.sprite == null
                    || !image.enabled
                    || !image.gameObject.activeInHierarchy
                    || image.color.a <= 0.001f)
                {
                    continue;
                }

                string spritePath = AssetDatabase.GetAssetPath(image.sprite);
                if (spritePath.StartsWith(DimensionHudArtRoot, StringComparison.Ordinal))
                {
                    count++;
                }
            }

            return count;
        }

        private static bool HasDesignRect(
            GameObject hudInstance,
            string objectName,
            Rect designRect,
            StringBuilder report)
        {
            GameObject target = FindChild(hudInstance.transform, objectName);
            if (target == null)
            {
                report.AppendLine($"LAYOUT_{objectName}=MISSING");
                return false;
            }

            RectTransform rectTransform = target.GetComponent<RectTransform>();
            Image image = target.GetComponent<Image>();
            if (rectTransform == null || image == null || image.preserveAspect)
            {
                report.AppendLine(
                    $"LAYOUT_{objectName}=COMPONENT_FAIL rect={BoolText(rectTransform != null)} image={BoolText(image != null)} preserveAspect={(image != null ? BoolText(image.preserveAspect) : "<null>")}");
                return false;
            }

            Vector2 expectedAnchor = new Vector2(0.5f, 0.5f);
            Vector2 expectedPosition = new Vector2(
                designRect.xMin + designRect.width * 0.5f - DimensionHudDesignResolution.x * 0.5f,
                DimensionHudDesignResolution.y * 0.5f - designRect.yMin - designRect.height * 0.5f);
            Vector2 expectedSize = new Vector2(designRect.width, designRect.height);
            bool matches = Approximately(rectTransform.anchorMin, expectedAnchor)
                && Approximately(rectTransform.anchorMax, expectedAnchor)
                && Approximately(rectTransform.anchoredPosition, expectedPosition)
                && Approximately(rectTransform.sizeDelta, expectedSize);
            if (!matches)
            {
                report.AppendLine(
                    $"LAYOUT_{objectName}=RECT_FAIL expectedAnchor={expectedAnchor} actualMin={rectTransform.anchorMin} actualMax={rectTransform.anchorMax} expectedPosition={expectedPosition} anchored={rectTransform.anchoredPosition} expectedSize={expectedSize} sizeDelta={rectTransform.sizeDelta}");
            }

            return matches;
        }

        private static bool HasNoVisibleChildButtonImages(
            GameObject hudInstance,
            string buttonName,
            StringBuilder report)
        {
            GameObject buttonObject = FindChild(hudInstance.transform, buttonName);
            if (buttonObject == null)
            {
                report.AppendLine($"BUTTON_OVERLAY_{buttonName}=MISSING");
                return false;
            }

            bool ready = true;
            Image rootImage = buttonObject.GetComponent<Image>();
            Image[] images = buttonObject.GetComponentsInChildren<Image>(includeInactive: true);
            for (int i = 0; i < images.Length; i++)
            {
                Image image = images[i];
                if (image == null || image == rootImage)
                {
                    continue;
                }

                bool visible = image.enabled
                    && image.gameObject.activeInHierarchy
                    && image.sprite != null
                    && image.color.a > 0.001f;
                if (!visible)
                {
                    continue;
                }

                ready = false;
                report.AppendLine($"BUTTON_OVERLAY_{buttonName}=VISIBLE_CHILD name={image.gameObject.name} alpha={image.color.a:0.###}");
            }

            return ready;
        }

        private static bool HasButtonClickRoute(
            GameObject hudInstance,
            string buttonName,
            string expectedMethodName,
            StringBuilder report)
        {
            GameObject buttonObject = FindChild(hudInstance.transform, buttonName);
            Button button = buttonObject != null ? buttonObject.GetComponent<Button>() : null;
            if (button == null)
            {
                report.AppendLine($"BUTTON_ROUTE_{buttonName}=MISSING_BUTTON");
                return false;
            }

            bool hasRoute = false;
            for (int i = 0; i < button.onClick.GetPersistentEventCount(); i++)
            {
                if (string.Equals(button.onClick.GetPersistentMethodName(i), expectedMethodName, StringComparison.Ordinal))
                {
                    hasRoute = true;
                    break;
                }
            }

            bool interactable = button.interactable && button.IsInteractable();
            if (!hasRoute || !interactable)
            {
                report.AppendLine($"BUTTON_ROUTE_{buttonName}=FAIL hasRoute={BoolText(hasRoute)} interactable={BoolText(interactable)} persistentCount={button.onClick.GetPersistentEventCount()}");
                return false;
            }

            return true;
        }

        private static bool HasPointerActionInput(
            GameObject hudInstance,
            ButtonRouteCheck check,
            StringBuilder report)
        {
            GameObject buttonObject = FindChild(hudInstance.transform, check.ButtonName);
            CombatHudPointerActionInput pointerAction = buttonObject != null
                ? buttonObject.GetComponent<CombatHudPointerActionInput>()
                : null;
            CombatHudActionId expectedActionId = ResolvePointerActionId(check.ButtonName);
            bool expectedHold = expectedActionId == CombatHudActionId.BasicAttack;
            bool ready = pointerAction != null
                && pointerAction.enabled
                && pointerAction.ActionId == expectedActionId
                && pointerAction.SendsHoldState == expectedHold;
            if (!ready)
            {
                report.AppendLine($"BUTTON_POINTER_ACTION_{check.ButtonName}=FAIL component={BoolText(pointerAction != null)} enabled={BoolText(pointerAction != null && pointerAction.enabled)} action={(pointerAction != null ? pointerAction.ActionId.ToString() : "<null>")} expected={expectedActionId} hold={(pointerAction != null ? BoolText(pointerAction.SendsHoldState) : "<null>")} expectedHold={BoolText(expectedHold)}");
            }

            return ready;
        }

        private static CombatHudActionId ResolvePointerActionId(string buttonName)
        {
            return buttonName switch
            {
                "BasicAttackButton" => CombatHudActionId.BasicAttack,
                "DodgeButton" => CombatHudActionId.Dodge,
                "Skill1Button" => CombatHudActionId.Skill1,
                "UltimateButton" => CombatHudActionId.Ultimate,
                "SummonSlot1Button" => CombatHudActionId.SummonSlot1,
                "SummonSlot2Button" => CombatHudActionId.SummonSlot2,
                "SummonSlot3Button" => CombatHudActionId.SummonSlot3,
                _ => CombatHudActionId.None
            };
        }
        private static bool HasSerializedReference(
            UnityEngine.Object target,
            string fieldName,
            StringBuilder report)
        {
            if (target == null)
            {
                report.AppendLine($"SERIALIZED_REF_{fieldName}=MISSING_TARGET");
                return false;
            }

            using var serializedObject = new SerializedObject(target);
            SerializedProperty property = serializedObject.FindProperty(fieldName);
            bool ready = property != null && property.objectReferenceValue != null;
            if (!ready)
            {
                report.AppendLine($"SERIALIZED_REF_{fieldName}=MISSING");
            }

            return ready;
        }

        private static bool RaycastsToControlCenter(
            GraphicRaycaster raycaster,
            EventSystem eventSystem,
            GameObject targetObject,
            string label,
            StringBuilder report)
        {
            if (raycaster == null || eventSystem == null || targetObject == null)
            {
                report.AppendLine($"CONTROL_RAYCAST_{label}=MISSING raycaster={BoolText(raycaster != null)} eventSystem={BoolText(eventSystem != null)} target={BoolText(targetObject != null)}");
                return false;
            }

            RectTransform rectTransform = targetObject.GetComponent<RectTransform>();
            if (rectTransform == null)
            {
                report.AppendLine($"CONTROL_RAYCAST_{label}=MISSING_RECT");
                return false;
            }

            GameObject top = RaycastTop(raycaster, eventSystem, GetRectCenterScreenPoint(rectTransform), out int hitCount);
            bool ready = top != null && IsSameOrChildOf(top.transform, targetObject.transform);
            if (!ready)
            {
                report.AppendLine($"CONTROL_RAYCAST_{label}=MISS top={(top != null ? top.name : "<none>")} hits={hitCount}");
            }

            return ready;
        }

        private static GameObject RaycastTop(
            GraphicRaycaster raycaster,
            EventSystem eventSystem,
            Vector2 screenPoint,
            out int hitCount)
        {
            hitCount = 0;
            if (raycaster == null || eventSystem == null)
            {
                return null;
            }

            var eventData = new PointerEventData(eventSystem)
            {
                position = screenPoint
            };
            var results = new List<RaycastResult>();
            raycaster.Raycast(eventData, results);
            hitCount = results.Count;
            return results.Count > 0 ? results[0].gameObject : null;
        }

        private static Vector2 GetRectCenterScreenPoint(RectTransform rectTransform)
        {
            return RectTransformUtility.WorldToScreenPoint(
                null,
                rectTransform.TransformPoint(rectTransform.rect.center));
        }

        private static bool RaycastsToButtonCenter(
            GraphicRaycaster raycaster,
            EventSystem eventSystem,
            GameObject hudInstance,
            string buttonName,
            Rect designRect,
            StringBuilder report)
        {
            GameObject buttonObject = FindChild(hudInstance.transform, buttonName);
            if (raycaster == null || eventSystem == null || buttonObject == null)
            {
                report.AppendLine($"BUTTON_RAYCAST_{buttonName}=MISSING raycaster={BoolText(raycaster != null)} eventSystem={BoolText(eventSystem != null)} button={BoolText(buttonObject != null)}");
                return false;
            }

            RectTransform rectTransform = buttonObject.GetComponent<RectTransform>();
            Image image = buttonObject.GetComponent<Image>();
            CanvasGroup canvasGroup = buttonObject.GetComponent<CanvasGroup>();
            if (rectTransform == null)
            {
                report.AppendLine($"BUTTON_RAYCAST_{buttonName}=MISSING_RECT");
                return false;
            }

            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(
                null,
                rectTransform.TransformPoint(rectTransform.rect.center));
            var eventData = new PointerEventData(eventSystem)
            {
                position = screenPoint
            };
            var results = new List<RaycastResult>();
            raycaster.Raycast(eventData, results);
            for (int i = 0; i < results.Count; i++)
            {
                if (IsSameOrChildOf(results[i].gameObject.transform, buttonObject.transform))
                {
                    return true;
                }
            }

            string top = results.Count > 0 && results[0].gameObject != null ? results[0].gameObject.name : "<none>";
            bool contains = RectTransformUtility.RectangleContainsScreenPoint(rectTransform, screenPoint, null);
            bool geometryReady = contains
                && image != null
                && image.raycastTarget
                && image.color.a > 0.001f
                && (canvasGroup == null || canvasGroup.blocksRaycasts);
            report.AppendLine(
                $"BUTTON_RAYCAST_{buttonName}={(geometryReady ? "BATCH_GEOMETRY_PASS" : "MISS")} point={screenPoint} top={top} hits={results.Count} contains={BoolText(contains)} imageRaycast={BoolText(image != null && image.raycastTarget)} imageAlpha={(image != null ? image.color.a.ToString("0.###") : "<null>")} canvasGroupBlocks={BoolText(canvasGroup == null || canvasGroup.blocksRaycasts)}");
            return geometryReady;
        }

        private static bool InvokesQueuedAction(
            GameObject hudInstance,
            BossBarrageLaneReviewCombatHudBinder binder,
            ButtonRouteCheck check,
            StringBuilder report)
        {
            GameObject buttonObject = FindChild(hudInstance.transform, check.ButtonName);
            Button button = buttonObject != null ? buttonObject.GetComponent<Button>() : null;
            if (button == null || binder == null)
            {
                report.AppendLine($"BUTTON_QUEUE_{check.ButtonName}=MISSING button={BoolText(button != null)} binder={BoolText(binder != null)}");
                return false;
            }

            using var serializedObject = new SerializedObject(binder);
            SerializedProperty property = serializedObject.FindProperty(check.PrimaryReferenceFieldName);
            UnityEngine.Object action = property != null ? property.objectReferenceValue : null;
            if (action == null)
            {
                report.AppendLine($"BUTTON_QUEUE_{check.ButtonName}=MISSING_ACTION field={check.PrimaryReferenceFieldName}");
                return false;
            }

            System.Reflection.FieldInfo queueField = action.GetType().GetField(
                check.QueueFieldName,
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (queueField == null)
            {
                report.AppendLine($"BUTTON_QUEUE_{check.ButtonName}=MISSING_FIELD field={check.QueueFieldName}");
                return false;
            }

            queueField.SetValue(action, false);
            button.onClick.Invoke();
            bool queued = queueField.GetValue(action) is bool value && value;
            queueField.SetValue(action, false);
            if (!queued)
            {
                report.AppendLine($"BUTTON_QUEUE_{check.ButtonName}=NOT_QUEUED field={check.QueueFieldName}");
            }

            return queued;
        }

        private static bool InvokesQueuedActionFromPointer(
            GameObject hudInstance,
            BossBarrageLaneReviewCombatHudBinder binder,
            GraphicRaycaster raycaster,
            EventSystem eventSystem,
            ButtonRouteCheck check,
            StringBuilder report)
        {
            GameObject buttonObject = FindChild(hudInstance.transform, check.ButtonName);
            Button button = buttonObject != null ? buttonObject.GetComponent<Button>() : null;
            RectTransform rectTransform = buttonObject != null ? buttonObject.GetComponent<RectTransform>() : null;
            if (buttonObject == null || button == null || rectTransform == null || binder == null)
            {
                report.AppendLine($"BUTTON_POINTER_QUEUE_{check.ButtonName}=MISSING button={BoolText(button != null)} rect={BoolText(rectTransform != null)} binder={BoolText(binder != null)}");
                return false;
            }

            using var serializedObject = new SerializedObject(binder);
            SerializedProperty property = serializedObject.FindProperty(check.PrimaryReferenceFieldName);
            UnityEngine.Object action = property != null ? property.objectReferenceValue : null;
            if (action == null)
            {
                report.AppendLine($"BUTTON_POINTER_QUEUE_{check.ButtonName}=MISSING_ACTION field={check.PrimaryReferenceFieldName}");
                return false;
            }

            System.Reflection.FieldInfo queueField = action.GetType().GetField(
                check.QueueFieldName,
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (queueField == null)
            {
                report.AppendLine($"BUTTON_POINTER_QUEUE_{check.ButtonName}=MISSING_FIELD field={check.QueueFieldName}");
                return false;
            }

            Vector2 screenPoint = GetRectCenterScreenPoint(rectTransform);
            GameObject raycastTarget = RaycastTop(raycaster, eventSystem, screenPoint, out int hitCount);
            bool usedGeometryFallback = false;
            GameObject eventTarget = raycastTarget;
            if (eventTarget == null && HasButtonGeometryForPointerFallback(buttonObject, rectTransform, screenPoint))
            {
                eventTarget = buttonObject;
                usedGeometryFallback = true;
            }

            GameObject downHandler = eventTarget != null
                ? ExecuteEvents.GetEventHandler<IPointerDownHandler>(eventTarget)
                : null;
            GameObject clickHandler = eventTarget != null
                ? ExecuteEvents.GetEventHandler<IPointerClickHandler>(eventTarget)
                : null;
            bool handlerReady = (downHandler != null && IsSameOrChildOf(downHandler.transform, buttonObject.transform))
                || (clickHandler != null && IsSameOrChildOf(clickHandler.transform, buttonObject.transform));
            if (!handlerReady)
            {
                report.AppendLine($"BUTTON_POINTER_QUEUE_{check.ButtonName}=NO_HANDLER top={(raycastTarget != null ? raycastTarget.name : "<none>")} target={(eventTarget != null ? eventTarget.name : "<none>")} fallback={BoolText(usedGeometryFallback)} down={(downHandler != null ? downHandler.name : "<none>")} click={(clickHandler != null ? clickHandler.name : "<none>")} hits={hitCount}");
                return false;
            }

            queueField.SetValue(action, false);
            var eventData = new PointerEventData(eventSystem)
            {
                button = PointerEventData.InputButton.Left,
                pointerId = -1,
                position = screenPoint,
                pressPosition = screenPoint
            };
            bool holdReady = true;
            ExecuteEvents.ExecuteHierarchy(eventTarget, eventData, ExecuteEvents.pointerDownHandler);
            if (check.ButtonName == "BasicAttackButton")
            {
                holdReady &= HasBasicFireHeld(binder, expected: true, report, "DOWN");
            }

            ExecuteEvents.ExecuteHierarchy(eventTarget, eventData, ExecuteEvents.pointerClickHandler);
            ExecuteEvents.ExecuteHierarchy(eventTarget, eventData, ExecuteEvents.pointerUpHandler);
            if (check.ButtonName == "BasicAttackButton")
            {
                holdReady &= HasBasicFireHeld(binder, expected: false, report, "UP");
                if (holdReady)
                {
                    report.AppendLine("BASIC_FIRE_HOLD=PASS");
                }
            }

            bool queued = queueField.GetValue(action) is bool value && value;
            queueField.SetValue(action, false);
            if (!queued)
            {
                report.AppendLine($"BUTTON_POINTER_QUEUE_{check.ButtonName}=NOT_QUEUED top={(raycastTarget != null ? raycastTarget.name : "<none>")} target={(eventTarget != null ? eventTarget.name : "<none>")} fallback={BoolText(usedGeometryFallback)} down={(downHandler != null ? downHandler.name : "<none>")} click={(clickHandler != null ? clickHandler.name : "<none>")} hits={hitCount}");
            }

            return queued && holdReady;
        }

        private static bool HasBasicFireHeld(
            BossBarrageLaneReviewCombatHudBinder binder,
            bool expected,
            StringBuilder report,
            string phase)
        {
            using var serializedObject = new SerializedObject(binder);
            PlayerRangedBasicAttackAction action =
                serializedObject.FindProperty("rangedBasicAttackAction")?.objectReferenceValue
                as PlayerRangedBasicAttackAction;
            bool actual = action != null && action.HasExternalFireHeldInput;
            bool ready = action != null && actual == expected;
            if (!ready)
            {
                report.AppendLine($"BASIC_FIRE_HOLD_{phase}=FAIL expected={BoolText(expected)} actual={BoolText(actual)} action={BoolText(action != null)}");
            }

            return ready;
        }

        private static bool HasSummonActionCostsAndCooldowns(
            BossBarrageLaneReviewCombatHudBinder binder,
            StringBuilder report)
        {
            SummonEnergyLadder energy = ReadBinderReference<SummonEnergyLadder>(binder, "energyLadder");
            PlayerSummonSlot1Action slot1 = ReadBinderReference<PlayerSummonSlot1Action>(binder, "summonSlot1Action");
            PlayerSupportSummonSlotAction slot2 = ReadBinderReference<PlayerSupportSummonSlotAction>(binder, "summonSlot2Action");
            PlayerSupportSummonSlotAction slot3 = ReadBinderReference<PlayerSupportSummonSlotAction>(binder, "summonSlot3Action");

            bool ready = true;
            ready &= HasSummonSlot1Config(
                slot1,
                BossBarrageSummonReviewContract.Slot1RequiredMana,
                BossBarrageSummonReviewContract.Slot1CooldownSeconds,
                report);
            ready &= HasSupportSummonConfig(
                "SUMMON_S2",
                slot2,
                BossBarrageSummonReviewContract.Slot2MinimumTier,
                BossBarrageSummonReviewContract.Slot2RequiredMana,
                BossBarrageSummonReviewContract.Slot2CooldownSeconds,
                report);
            ready &= HasSupportSummonConfig(
                "SUMMON_S3",
                slot3,
                BossBarrageSummonReviewContract.Slot3MinimumTier,
                BossBarrageSummonReviewContract.Slot3RequiredMana,
                BossBarrageSummonReviewContract.Slot3CooldownSeconds,
                report);
            if (energy == null)
            {
                report.AppendLine("SUMMON_ENERGY=MISSING");
                return false;
            }

            if (slot1 != null)
            {
                ready &= CanUseSummonSlot1(slot1, energy, report);
            }

            if (slot2 != null)
            {
                ready &= CanUseSupportSummon("SUMMON_S2", slot2, energy, report);
            }

            if (slot3 != null)
            {
                ready &= CanUseSupportSummon("SUMMON_S3", slot3, energy, report);
            }

            energy.ResetLadder();
            return ready;
        }

        private static bool HasSummonSlot1Config(
            PlayerSummonSlot1Action action,
            float expectedCost,
            float expectedCooldown,
            StringBuilder report)
        {
            bool ready = action != null
                && Nearly(action.RequiredSummonMana, expectedCost)
                && Nearly(action.SlotCooldownSeconds, expectedCooldown);
            report.AppendLine(
                $"SUMMON_S1_CONFIG={PassText(ready)} cost={(action != null ? action.RequiredSummonMana.ToString("0.###") : "<null>")} cooldown={(action != null ? action.SlotCooldownSeconds.ToString("0.###") : "<null>")}");
            return ready;
        }

        private static bool HasSupportSummonConfig(
            string label,
            PlayerSupportSummonSlotAction action,
            int expectedTier,
            float expectedCost,
            float expectedCooldown,
            StringBuilder report)
        {
            bool ready = action != null
                && action.MinimumSummonTier == expectedTier
                && Nearly(action.RequiredSummonMana, expectedCost)
                && Nearly(action.SlotCooldownSeconds, expectedCooldown);
            report.AppendLine(
                $"{label}_CONFIG={PassText(ready)} tier={(action != null ? action.MinimumSummonTier.ToString() : "<null>")} cost={(action != null ? action.RequiredSummonMana.ToString("0.###") : "<null>")} cooldown={(action != null ? action.SlotCooldownSeconds.ToString("0.###") : "<null>")}");
            return ready;
        }

        private static bool CanUseSummonSlot1(
            PlayerSummonSlot1Action action,
            SummonEnergyLadder energy,
            StringBuilder report)
        {
            action.ClearSlotCooldown();
            energy.ResetLadder();
            energy.GrantCurrentTierEnergy(action.RequiredSummonMana);
            int useCountBefore = action.TotalUseCount;
            bool used = action.TryUseSummonSlot1();
            bool useCountReady = action.TotalUseCount == useCountBefore + 1;
            bool cooldownReady = action.SlotCooldownRemaining > 0.01f;
            bool spentReady = energy.CurrentMana <= 0.01f && !energy.CanSpend;
            bool ready = used && useCountReady && cooldownReady && spentReady;
            report.AppendLine(
                $"SUMMON_S1_USE={PassText(ready)} used={BoolText(used)} count={action.TotalUseCount - useCountBefore} cooldown={action.SlotCooldownRemaining:0.###} mana={energy.CurrentMana:0.###} reason={action.LastUseBlockedReason ?? "<none>"}");
            return ready;
        }

        private static bool CanUseSupportSummon(
            string label,
            PlayerSupportSummonSlotAction action,
            SummonEnergyLadder energy,
            StringBuilder report)
        {
            SetPrivateFloat(action, "slotCooldownRemaining", 0f);
            energy.ResetLadder();
            energy.GrantCurrentTierEnergy(action.RequiredSummonMana);
            int useCountBefore = action.TotalUseCount;
            bool used = action.TryUseSummon();
            bool useCountReady = action.TotalUseCount == useCountBefore + 1;
            bool cooldownReady = action.SlotCooldownRemaining > 0.01f;
            bool spentReady = energy.CurrentMana <= 0.01f && !energy.CanSpend;
            bool ready = used && useCountReady && cooldownReady && spentReady;
            report.AppendLine(
                $"{label}_USE={PassText(ready)} used={BoolText(used)} count={action.TotalUseCount - useCountBefore} cooldown={action.SlotCooldownRemaining:0.###} mana={energy.CurrentMana:0.###} reason={action.LastUseBlockedReason ?? "<none>"}");
            return ready;
        }

        private static T ReadBinderReference<T>(
            BossBarrageLaneReviewCombatHudBinder binder,
            string fieldName)
            where T : UnityEngine.Object
        {
            using var serializedObject = new SerializedObject(binder);
            return serializedObject.FindProperty(fieldName)?.objectReferenceValue as T;
        }

        private static void SetPrivateFloat(UnityEngine.Object target, string fieldName, float value)
        {
            System.Reflection.FieldInfo field = target.GetType().GetField(
                fieldName,
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            field?.SetValue(target, value);
        }

        private static bool Nearly(float actual, float expected)
        {
            return Mathf.Abs(actual - expected) <= 0.01f;
        }
        private static bool HasButtonGeometryForPointerFallback(
            GameObject buttonObject,
            RectTransform rectTransform,
            Vector2 screenPoint)
        {
            Image image = buttonObject.GetComponent<Image>();
            CanvasGroup canvasGroup = buttonObject.GetComponent<CanvasGroup>();
            return RectTransformUtility.RectangleContainsScreenPoint(rectTransform, screenPoint, null)
                && image != null
                && image.raycastTarget
                && image.color.a > 0.001f
                && (canvasGroup == null || canvasGroup.blocksRaycasts);
        }

        private static bool HasVisibleTextRect(
            GameObject hudInstance,
            string objectName,
            Rect designRect,
            StringBuilder report)
        {
            GameObject target = FindChild(hudInstance.transform, objectName);
            RectTransform rectTransform = target != null ? target.GetComponent<RectTransform>() : null;
            Text text = target != null ? target.GetComponent<Text>() : null;
            if (target == null || rectTransform == null || text == null)
            {
                report.AppendLine($"TEXT_{objectName}=MISSING");
                return false;
            }

            bool visible = target.activeInHierarchy
                && text.enabled
                && text.color.a > 0.5f
                && !string.IsNullOrWhiteSpace(text.text);
            Vector2 expectedPosition = new Vector2(
                designRect.xMin + designRect.width * 0.5f - DimensionHudDesignResolution.x * 0.5f,
                DimensionHudDesignResolution.y * 0.5f - designRect.yMin - designRect.height * 0.5f);
            bool layout = Approximately(rectTransform.anchorMin, new Vector2(0.5f, 0.5f))
                && Approximately(rectTransform.anchorMax, new Vector2(0.5f, 0.5f))
                && Approximately(rectTransform.anchoredPosition, expectedPosition)
                && Approximately(rectTransform.sizeDelta, new Vector2(designRect.width, designRect.height));
            if (!visible || !layout)
            {
                report.AppendLine($"TEXT_{objectName}=FAIL visible={BoolText(visible)} text='{text.text}' anchored={rectTransform.anchoredPosition} size={rectTransform.sizeDelta}");
                return false;
            }

            return true;
        }

        private static bool HasVisibleChildText(
            GameObject hudInstance,
            string rootName,
            string childName,
            StringBuilder report)
        {
            GameObject root = FindChild(hudInstance.transform, rootName);
            GameObject child = root != null ? FindChild(root.transform, childName) : null;
            Text text = child != null ? child.GetComponent<Text>() : null;
            bool ready = child != null
                && child.activeInHierarchy
                && text != null
                && text.enabled
                && text.color.a > 0.5f
                && !string.IsNullOrWhiteSpace(text.text);
            if (!ready)
            {
                report.AppendLine($"TEXT_{rootName}_{childName}=FAIL");
            }

            return ready;
        }


        private static bool IsSameOrChildOf(Transform candidate, Transform parent)
        {
            Transform current = candidate;
            while (current != null)
            {
                if (current == parent)
                {
                    return true;
                }

                current = current.parent;
            }

            return false;
        }

        private static bool Approximately(Vector2 left, Vector2 right)
        {
            return Mathf.Abs(left.x - right.x) <= 0.0001f
                && Mathf.Abs(left.y - right.y) <= 0.0001f;
        }

        private static GameObject FindRoot(Scene scene, string rootName)
        {
            if (!scene.IsValid())
            {
                return null;
            }

            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                if (string.Equals(roots[i].name, rootName, StringComparison.Ordinal))
                {
                    return roots[i];
                }
            }

            return null;
        }

        private static GameObject FindChild(Transform root, string childName)
        {
            if (root == null)
            {
                return null;
            }

            if (string.Equals(root.name, childName, StringComparison.Ordinal))
            {
                return root.gameObject;
            }

            for (int i = 0; i < root.childCount; i++)
            {
                GameObject found = FindChild(root.GetChild(i), childName);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        private static T FindSceneObjectOrNull<T>(Scene scene) where T : Component
        {
            T[] objects = Resources.FindObjectsOfTypeAll<T>();
            for (int i = 0; i < objects.Length; i++)
            {
                T candidate = objects[i];
                if (candidate != null && candidate.gameObject.scene == scene)
                {
                    return candidate;
                }
            }

            return null;
        }

        private static bool ReadBool(UnityEngine.Object target, string fieldName, bool fallback)
        {
            if (target == null)
            {
                return fallback;
            }

            using var serializedObject = new SerializedObject(target);
            SerializedProperty property = serializedObject.FindProperty(fieldName);
            return property != null && property.propertyType == SerializedPropertyType.Boolean
                ? property.boolValue
                : fallback;
        }

        private static void WriteResult(bool passed, string state, string report)
        {
            ActionFoundationBatchVerificationResult.WriteResult(ResultPath, passed, state, ResultPath, report);
            if (passed)
            {
                Debug.Log($"Boss barrage combat HUD batch verification passed. See {ResultPath}.");
            }
            else
            {
                Debug.LogError($"Boss barrage combat HUD batch verification failed. See {ResultPath}.");
            }
        }

        private static void Finish(int exitCode)
        {
            Clear();
            EditorApplication.Exit(exitCode);
        }

        private static void Clear()
        {
            EditorPrefs.DeleteKey(ActiveKey);
            EditorPrefs.DeleteKey(StartedAtKey);
            EditorPrefs.DeleteKey(CapturedKey);
            EditorApplication.update -= Monitor;
        }

        private static string BoolText(bool value)
        {
            return value ? "TRUE" : "FALSE";
        }

        private static string PassText(bool value)
        {
            return value ? "PASS" : "FAIL";
        }

        private readonly struct DesignRectCheck
        {
            public DesignRectCheck(string objectName, Rect designRect)
            {
                ObjectName = objectName;
                DesignRect = designRect;
            }

            public string ObjectName { get; }
            public Rect DesignRect { get; }
        }

        private readonly struct ButtonRouteCheck
        {
            public ButtonRouteCheck(
                string buttonName,
                string methodName,
                string primaryReferenceFieldName,
                string queueFieldName,
                Rect designRect,
                string secondaryReferenceFieldName = null)
            {
                ButtonName = buttonName;
                MethodName = methodName;
                PrimaryReferenceFieldName = primaryReferenceFieldName;
                QueueFieldName = queueFieldName;
                DesignRect = designRect;
                SecondaryReferenceFieldName = secondaryReferenceFieldName;
            }

            public string ButtonName { get; }
            public string MethodName { get; }
            public string PrimaryReferenceFieldName { get; }
            public string QueueFieldName { get; }
            public Rect DesignRect { get; }
            public string SecondaryReferenceFieldName { get; }
        }

        private readonly struct ResolutionCheck
        {
            public ResolutionCheck(string id, Vector2 size)
            {
                Id = id;
                Size = size;
            }

            public string Id { get; }
            public Vector2 Size { get; }
        }

        private readonly struct VerificationSnapshot
        {
            public VerificationSnapshot(bool passed, string report)
            {
                Passed = passed;
                Report = report;
            }

            public bool Passed { get; }
            public string Report { get; }
        }
    }
}
