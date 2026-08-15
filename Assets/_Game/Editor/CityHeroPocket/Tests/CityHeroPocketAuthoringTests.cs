using System;
using System.Linq;
using DimensionBrawl.LevelDesign;
using DimensionBrawl.Player;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace DimensionBrawl.Editor.CityHeroPocket.Tests
{
    public sealed class CityHeroPocketAuthoringTests
    {
        [TestCase(-0.019f, true)]
        [TestCase(-0.021f, false)]
        public void VisualQaSubjectFrameAllowanceIsNarrowAndFailClosed(
            float minimumY,
            bool expected)
        {
            var proof = new CityHeroPocketVisualQaCapture.ViewportProof
            {
                centerX = 0.5f,
                centerY = 0.45f,
                minimumDepth = 4f,
                minX = 0.25f,
                minY = minimumY,
                maxX = 0.72f,
                maxY = 0.6f,
                width = 0.47f,
                height = 0.6f - minimumY
            };

            Assert.That(
                CityHeroPocketVisualQaCapture.IsReadableSubjectProof(
                    proof,
                    CityHeroPocketVisualQaCapture.DefaultMinimumSubjectViewportY),
                Is.EqualTo(expected));
        }

        [Test]
        public void VisualQaSubjectFrameAllowanceDoesNotRelaxOtherEdges()
        {
            var proof = new CityHeroPocketVisualQaCapture.ViewportProof
            {
                centerX = 0.5f,
                centerY = 0.45f,
                minimumDepth = 4f,
                minX = 0.25f,
                minY = -0.019f,
                maxX = 0.72f,
                maxY = 1.021f,
                width = 0.47f,
                height = 1.07f
            };

            Assert.That(
                CityHeroPocketVisualQaCapture.IsReadableSubjectProof(
                    proof,
                    CityHeroPocketVisualQaCapture.DefaultMinimumSubjectViewportY),
                Is.False);
        }

        [Test]
        public void StaticAuthoringContractMatchesReviewedRecipe()
        {
            Assert.That(CityHeroPocketSceneSetup.LayoutRecipeSchema,
                Is.EqualTo("dimension-brawl-city-hero-pocket-layout/v1"));
            Assert.That(CityHeroPocketSceneSetup.LayoutBoundsEvidenceSha256,
                Is.EqualTo("597FE19CBAECB15A0487138704C7239A6A5953EF8DFFD77E6B88256FE0983909"));
            Assert.That(CityHeroPocketSceneSetup.LayoutRecipeJsonSha256,
                Is.EqualTo("79F1AE7CB76DDFDB751FA99B19453F60CDEE48DED8C76AF67F28E1B96A047552"));
            Assert.That(CityHeroPocketSceneSetup.ComputeTokyoModuleGoldenSha256(),
                Is.EqualTo(CityHeroPocketSceneSetup.TokyoModuleGoldenSha256),
                "Tokyo table drifted from the independently hashed authoritative JSON rows.");
            Assert.That(CityHeroPocketSceneSetup.RequiredTokyoPrefabPaths, Has.Length.EqualTo(24));
            Assert.That(CityHeroPocketSceneSetup.RequiredTokyoPrefabPaths.Distinct(
                    StringComparer.Ordinal).Count(),
                Is.EqualTo(24));
            Assert.That(CityHeroPocketSceneSetup.RequiredTokyoPrefabPaths.All(path =>
                    path.StartsWith(
                        CityHeroPocketSceneSetup.TokyoRoot + "/",
                        StringComparison.Ordinal)),
                Is.True);
            Assert.That(CityHeroPocketSceneSetup.TokyoModuleInstanceCount, Is.EqualTo(69));
            Assert.That(CityHeroPocketSceneSetup.TokyoModuleLod0RendererSlots, Is.EqualTo(80));
            Assert.That(CityHeroPocketSceneSetup.ProductLod0RendererSlots, Is.EqualTo(84));
            Assert.That(
                CityHeroPocketAuthoredPackValidator.CountSourceTokyoLod0RendererSlots(),
                Is.EqualTo(80),
                "The 24 promoted source prefabs no longer reproduce the corrected 80-slot Tokyo budget.");
            Assert.That(CityHeroPocketSceneSetup.RendererBudgetCorrection,
                Does.Contain("Showcase_Store_01"),
                "The independently audited renderer-budget correction must stay explicit.");
            Assert.That(CityHeroPocketSceneSetup.BoundaryLayerDeviation,
                Does.Contain("Ignore Raycast layer 2"),
                "The no-TagManager boundary-layer deviation must stay explicit.");
            Assert.That(CityHeroPocketAuthoredPackValidator.TemporaryLilToonShaderDebtRoot,
                Is.EqualTo("Assets/_Imported/AssetStore/lilToon/Shader/"),
                "Temporary Inori shader debt must stay narrow and explicit.");
            Assert.That(CityHeroPocketSceneSetup.ExitPortalPrefabPath,
                Is.EqualTo(
                    "Assets/_Game/Art/VFX/CombatCues/Prefabs/" +
                    "DB_VFX_PlayerSummonPreSpawnPortal.prefab"));
            Assert.That(CityHeroPocketSceneSetup.ExitTriggerPosition,
                Is.EqualTo(new Vector3(0f, 1f, 7.6f)));
            Assert.That(CityHeroPocketSceneSetup.ExitTriggerSize,
                Is.EqualTo(new Vector3(10.8f, 2f, 0.6f)));
            Assert.That(CityHeroPocketSceneSetup.ExitTriggerCenter,
                Is.EqualTo(new Vector3(0f, 0.05f, 0f)),
                "The exit trigger must stay clear of the road collider at y=0.");
            Assert.That(CityHeroPocketSceneSetup.TransitionFocusPosition,
                Is.EqualTo(new Vector3(0f, 2.8f, 10.55f)));
            Assert.That(CityHeroPocketSceneSetup.ExitCoverColor,
                Is.EqualTo(new Color(0.84f, 0.97f, 1f, 1f)));
            Assert.That(CityHeroPocketExitTransitionController.HudFadeFrameCount,
                Is.EqualTo(18));
            Assert.That(CityHeroPocketExitTransitionController.PortalGrowFrameCount,
                Is.EqualTo(42));
            Assert.That(CityHeroPocketExitTransitionController.CoverFadeStartFrame,
                Is.EqualTo(234));
            Assert.That(CityHeroPocketExitTransitionController.ExitReadyFrame,
                Is.EqualTo(294));
            Assert.That(
                CityHeroPocketExitTransitionController.InitialPortalScaleFactor,
                Is.EqualTo(0.08f));
            Assert.That(
                typeof(CityHeroPocketExitTransitionController).GetProperty(
                    nameof(CityHeroPocketExitTransitionController
                        .IgnoredLaneActionProjectileTriggerEnterCount)),
                Is.Not.Null,
                "G02/G03 requires a public expected-projectile traffic counter.");
            Assert.That(
                typeof(CityHeroPocketExitTransitionController).GetProperty(
                    nameof(CityHeroPocketExitTransitionController.RejectedTriggerEnterCount)),
                Is.Not.Null,
                "G03 requires a public wrong-collider trigger proof counter.");
            Assert.That(
                (int)PlayerInputLockSource.CityHeroPocketExitTransition,
                Is.EqualTo(1 << 9));
            Assert.That(
                PlayerInputLockMask.WithState(
                    PlayerInputLockSource.CinematicCue,
                    PlayerInputLockSource.CityHeroPocketExitTransition,
                    locked: true),
                Is.EqualTo(
                    PlayerInputLockSource.CinematicCue
                    | PlayerInputLockSource.CityHeroPocketExitTransition),
                "City exit input ownership must coexist with another cinematic cue.");
        }

        [Test]
        public void GeneratedOutputsExistAndPassFreshReloadValidator()
        {
            Assert.That(
                AssetDatabase.LoadAssetAtPath<SceneAsset>(CityHeroPocketSceneSetup.ScenePath),
                Is.Not.Null,
                "Committed CityHeroPocket scene output is missing; run the deterministic setup first.");
            Assert.That(
                AssetDatabase.LoadAssetAtPath<GameObject>(
                    CityHeroPocketSceneSetup.PlayerPrefabPath),
                Is.Not.Null,
                "Committed compact Inori prefab output is missing; run the deterministic setup first.");

            Assert.DoesNotThrow(
                CityHeroPocketAuthoredPackValidator.ValidateAuthoredOutputs,
                "Saved CityHeroPocket scene/prefab failed its reopen and dependency contract.");
        }

        [Test]
        public void CompactPlayerPrefabSourceRootIsCanonicalAndActive()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                CityHeroPocketSceneSetup.PlayerPrefabPath);
            Assert.That(prefab, Is.Not.Null,
                "Committed compact Inori prefab output is missing.");
            Assert.That(prefab.activeSelf, Is.True,
                "Compact prefab source root must be authored active.");
            Assert.That(prefab.transform.localPosition, Is.EqualTo(Vector3.zero));
            Assert.That(prefab.transform.localRotation, Is.EqualTo(Quaternion.identity));
            Assert.That(prefab.transform.localScale, Is.EqualTo(Vector3.one));
        }

        [Test]
        public void ValidationTemporarilyActivatesCityAndRestoresUnrelatedSceneSettings()
        {
            Assert.That(
                AssetDatabase.LoadAssetAtPath<SceneAsset>(CityHeroPocketSceneSetup.ScenePath),
                Is.Not.Null,
                "Committed CityHeroPocket scene output is missing.");

            SceneSetup[] previousSetup = EditorSceneManager.GetSceneManagerSetup();
            for (int sceneIndex = 0; sceneIndex < SceneManager.sceneCount; sceneIndex++)
            {
                Assert.That(
                    SceneManager.GetSceneAt(sceneIndex).isDirty,
                    Is.False,
                    "RenderSettings validation must not replace a user's dirty open scene.");
            }

            bool canRestorePreviousSetup = previousSetup.Length > 0
                && previousSetup.All(setup => !string.IsNullOrWhiteSpace(setup.path));
            Scene unrelated = EditorSceneManager.NewScene(
                NewSceneSetup.EmptyScene,
                NewSceneMode.Single);
            Assert.That(SceneManager.GetActiveScene().handle, Is.EqualTo(unrelated.handle));
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.81f, 0.13f, 0.57f, 1f);
            RenderSettings.fog = false;

            try
            {
                Assert.DoesNotThrow(
                    CityHeroPocketAuthoredPackValidator.ValidateAuthoredOutputs,
                    "Validator read RenderSettings from the unrelated active scene.");
                Assert.That(SceneManager.GetActiveScene().handle, Is.EqualTo(unrelated.handle),
                    "Validator did not restore the unrelated active scene.");
                Assert.That(RenderSettings.ambientMode, Is.EqualTo(AmbientMode.Flat));
                Assert.That(RenderSettings.ambientLight,
                    Is.EqualTo(new Color(0.81f, 0.13f, 0.57f, 1f)));
                Assert.That(RenderSettings.fog, Is.False);
            }
            finally
            {
                if (canRestorePreviousSetup)
                {
                    EditorSceneManager.RestoreSceneManagerSetup(previousSetup);
                }
                else
                {
                    EditorSceneManager.NewScene(
                        NewSceneSetup.EmptyScene,
                        NewSceneMode.Single);
                }
            }
        }
    }
}
