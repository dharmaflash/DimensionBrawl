using System.Collections;
using System.Linq;
using DimensionBrawl.Presentation;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace DimensionBrawl.Tests
{
    public sealed class GameplayLookStateControllerPlayModeTests
    {
        private const string CorridorScenePath =
            "Assets/_Game/Scenes/OlympusCorridorInvasionStage.unity";
        private const string StationScenePath =
            "Assets/_Game/Scenes/OlympusStationCombatStage.unity";

        [UnityTearDown]
        public IEnumerator RestoreNeutralScene()
        {
            Time.timeScale = 1f;
            Scene loadedProductScene = SceneManager.GetSceneByPath(StationScenePath);
            if (!loadedProductScene.IsValid() || !loadedProductScene.isLoaded)
            {
                loadedProductScene = SceneManager.GetSceneByPath(CorridorScenePath);
            }

            if (!loadedProductScene.IsValid() || !loadedProductScene.isLoaded)
            {
                yield break;
            }

            Scene neutral = SceneManager.CreateScene("GameplayLookStateTestNeutral");
            SceneManager.SetActiveScene(neutral);
            AsyncOperation unload = SceneManager.UnloadSceneAsync(loadedProductScene);
            while (unload != null && !unload.isDone)
            {
                yield return null;
            }
        }

        [UnityTest]
        public IEnumerator ExclusivePriorityRestoresLowerLeaseThenGameplayBase()
        {
            Fixture fixture = CreateFixture(immediate: true);
            try
            {
                Assert.That(fixture.Base.weight, Is.EqualTo(0.73f).Within(0.0001f));
                Assert.That(fixture.Character.weight, Is.Zero);
                Assert.That(fixture.PhaseTwo.weight, Is.Zero);
                Assert.That(fixture.Controller.CurrentState, Is.EqualTo(GameplayLookState.GameplayBase));

                Assert.That(
                    fixture.Controller.TryAcquire(
                        GameplayLookState.CharacterFocus,
                        fixture.CharacterOwner,
                        out GameplayLookStateController.LookLease characterLease),
                    Is.True);
                Assert.That(characterLease.IsValid, Is.True);
                Assert.That(fixture.Controller.CurrentState, Is.EqualTo(GameplayLookState.CharacterFocus));
                Assert.That(fixture.Character.weight, Is.EqualTo(1f));

                Assert.That(
                    fixture.Controller.TryAcquire(
                        GameplayLookState.Phase2Cinematic,
                        fixture.PhaseTwoOwner,
                        out GameplayLookStateController.LookLease phaseTwoLease),
                    Is.True);
                Assert.That(fixture.Controller.CurrentState, Is.EqualTo(GameplayLookState.Phase2Cinematic));
                Assert.That(fixture.Character.weight, Is.Zero);
                Assert.That(fixture.PhaseTwo.weight, Is.EqualTo(1f));

                phaseTwoLease.Dispose();
                phaseTwoLease.Dispose();
                Assert.That(fixture.Controller.CurrentState, Is.EqualTo(GameplayLookState.CharacterFocus));
                Assert.That(fixture.Character.weight, Is.EqualTo(1f));
                Assert.That(fixture.PhaseTwo.weight, Is.Zero);

                characterLease.Dispose();
                Assert.That(fixture.Controller.CurrentState, Is.EqualTo(GameplayLookState.GameplayBase));
                Assert.That(fixture.Controller.ActiveLeaseCount, Is.Zero);
                Assert.That(fixture.Character.weight, Is.Zero);
                Assert.That(fixture.Base.weight, Is.EqualTo(0.73f).Within(0.0001f));
            }
            finally
            {
                fixture.Dispose();
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator DestroyedOwnerAndDisabledControllerResetEveryOverlay()
        {
            Fixture fixture = CreateFixture(immediate: true);
            try
            {
                Assert.That(
                    fixture.Controller.TryAcquire(
                        GameplayLookState.CharacterFocus,
                        fixture.CharacterOwner,
                        out GameplayLookStateController.LookLease lease),
                    Is.True);
                Assert.That(fixture.Character.weight, Is.EqualTo(1f));

                Object.Destroy(fixture.CharacterOwner);
                yield return null;
                Assert.That(fixture.Controller.CurrentState, Is.EqualTo(GameplayLookState.GameplayBase));
                Assert.That(fixture.Controller.ActiveLeaseCount, Is.Zero);
                Assert.That(fixture.Character.weight, Is.Zero);
                Assert.That(lease.IsValid, Is.False);

                Assert.That(
                    fixture.Controller.TryAcquire(
                        GameplayLookState.Phase2Cinematic,
                        fixture.PhaseTwoOwner,
                        out GameplayLookStateController.LookLease phaseTwoLease),
                    Is.True);
                fixture.Controller.enabled = false;
                Assert.That(fixture.PhaseTwo.weight, Is.Zero);
                Assert.That(fixture.Controller.CurrentState, Is.EqualTo(GameplayLookState.GameplayBase));
                Assert.That(phaseTwoLease.IsValid, Is.False);
            }
            finally
            {
                fixture.Dispose();
            }
        }

        [UnityTest]
        public IEnumerator BlendUsesUnscaledTimeAndUnboundStatesFailClosed()
        {
            Fixture fixture = CreateFixture(immediate: false);
            float previousTimeScale = Time.timeScale;
            try
            {
                Assert.That(
                    fixture.Controller.TryAcquire(
                        GameplayLookState.Finisher,
                        fixture.CharacterOwner,
                        out _),
                    Is.False);
                Assert.That(
                    fixture.Controller.TryAcquire(
                        GameplayLookState.GameplayBase,
                        fixture.CharacterOwner,
                        out _),
                    Is.False);

                Time.timeScale = 0f;
                Assert.That(
                    fixture.Controller.TryAcquire(
                        GameplayLookState.CharacterFocus,
                        fixture.CharacterOwner,
                        out GameplayLookStateController.LookLease lease),
                    Is.True);

                float deadline = Time.realtimeSinceStartup + 1f;
                while (fixture.Character.weight < 0.999f
                    && Time.realtimeSinceStartup < deadline)
                {
                    yield return null;
                }

                Assert.That(fixture.Character.weight, Is.EqualTo(1f).Within(0.001f));
                Assert.That(fixture.Base.weight, Is.EqualTo(0.73f).Within(0.0001f));
                lease.Dispose();

                deadline = Time.realtimeSinceStartup + 1f;
                while (fixture.Character.weight > 0.001f
                    && Time.realtimeSinceStartup < deadline)
                {
                    yield return null;
                }

                Assert.That(fixture.Character.weight, Is.Zero.Within(0.001f));
            }
            finally
            {
                Time.timeScale = previousTimeScale;
                fixture.Dispose();
            }
        }

        [UnityTest]
        public IEnumerator SameStateOwnersGenerationAndBulkReleaseAreIsolated()
        {
            Fixture fixture = CreateFixture(immediate: true);
            GameObject secondOwner = new GameObject("SecondCharacterOwner");
            try
            {
                Assert.That(
                    fixture.Controller.TryAcquire(
                        GameplayLookState.CharacterFocus,
                        fixture.CharacterOwner,
                        out GameplayLookStateController.LookLease first),
                    Is.True);
                Assert.That(
                    fixture.Controller.TryAcquire(
                        GameplayLookState.CharacterFocus,
                        secondOwner,
                        out GameplayLookStateController.LookLease second),
                    Is.True);
                first.Dispose();
                Assert.That(fixture.Controller.CurrentState, Is.EqualTo(GameplayLookState.CharacterFocus));
                Assert.That(fixture.Controller.ActiveLeaseCount, Is.EqualTo(1));

                Assert.That(fixture.Controller.ReleaseAllOwnedBy(fixture.CharacterOwner), Is.Zero);
                Assert.That(fixture.Controller.ReleaseAllOwnedBy(secondOwner), Is.EqualTo(1));
                Assert.That(fixture.Controller.ReleaseAllOwnedBy(secondOwner), Is.Zero);
                Assert.That(fixture.Controller.CurrentState, Is.EqualTo(GameplayLookState.GameplayBase));
                Assert.That(second.IsValid, Is.False);

                Assert.That(
                    fixture.Controller.TryAcquire(
                        GameplayLookState.Phase2Cinematic,
                        fixture.PhaseTwoOwner,
                        out GameplayLookStateController.LookLease oldGeneration),
                    Is.True);
                fixture.Controller.enabled = false;
                fixture.Controller.enabled = true;
                Assert.That(
                    fixture.Controller.TryAcquire(
                        GameplayLookState.CharacterFocus,
                        fixture.CharacterOwner,
                        out GameplayLookStateController.LookLease newGeneration),
                    Is.True);
                oldGeneration.Dispose();
                Assert.That(newGeneration.IsValid, Is.True);
                Assert.That(fixture.Controller.CurrentState, Is.EqualTo(GameplayLookState.CharacterFocus));
                newGeneration.Dispose();
            }
            finally
            {
                Object.DestroyImmediate(secondOwner);
                fixture.Dispose();
            }

            yield return null;
        }

        [Test]
        public void InvalidOrInstantiatedProfilesFailClosedWithoutMutatingProfiles()
        {
            Fixture fixture = CreateFixture(immediate: true);
            try
            {
                VolumeProfile originalProfile = fixture.Character.sharedProfile;
                int originalComponentCount = originalProfile.components.Count;

                Assert.That(
                    fixture.Controller.TryAcquire(
                        GameplayLookState.CharacterFocus,
                        fixture.CharacterOwner,
                        out GameplayLookStateController.LookLease validLease),
                    Is.True);
                validLease.Dispose();
                Assert.That(fixture.Character.sharedProfile, Is.SameAs(originalProfile));
                Assert.That(fixture.Character.HasInstantiatedProfile(), Is.False);
                Assert.That(originalProfile.components.Count, Is.EqualTo(originalComponentCount));

                fixture.Character.sharedProfile = null;
                Assert.That(
                    fixture.Controller.TryAcquire(
                        GameplayLookState.CharacterFocus,
                        fixture.CharacterOwner,
                        out _),
                    Is.False);
                fixture.Character.sharedProfile = originalProfile;

                VolumeProfile runtimeClone = fixture.Character.profile;
                Assert.That(fixture.Character.HasInstantiatedProfile(), Is.True);
                Assert.That(
                    fixture.Controller.TryAcquire(
                        GameplayLookState.CharacterFocus,
                        fixture.CharacterOwner,
                        out _),
                    Is.False);
                fixture.Character.profile = null;
                Object.DestroyImmediate(runtimeClone);

                Assert.That(fixture.Character.sharedProfile, Is.SameAs(originalProfile));
                Assert.That(fixture.Character.HasInstantiatedProfile(), Is.False);
                Assert.That(originalProfile.components.Count, Is.EqualTo(originalComponentCount));

                fixture.Character.isGlobal = false;
                Assert.That(
                    fixture.Controller.TryAcquire(
                        GameplayLookState.CharacterFocus,
                        fixture.CharacterOwner,
                        out _),
                    Is.False);
                fixture.Character.isGlobal = true;

                fixture.Character.priority = fixture.Base.priority - 1f;
                Assert.That(
                    fixture.Controller.TryAcquire(
                        GameplayLookState.CharacterFocus,
                        fixture.CharacterOwner,
                        out _),
                    Is.False);
                fixture.Character.priority = 95f;

                fixture.Controller.Configure(
                    fixture.Base,
                    new[]
                    {
                        new GameplayLookStateController.OverlayBinding(
                            GameplayLookState.CharacterFocus,
                            fixture.Character,
                            0f,
                            0f),
                        new GameplayLookStateController.OverlayBinding(
                            GameplayLookState.Phase2Cinematic,
                            fixture.Character,
                            0f,
                            0f),
                    });
                Assert.That(
                    fixture.Controller.TryAcquire(
                        GameplayLookState.CharacterFocus,
                        fixture.CharacterOwner,
                        out _),
                    Is.False);
            }
            finally
            {
                fixture.Dispose();
            }
        }

        [UnityTest]
        public IEnumerator DestroyingControllerResetsExternalOverlayVolumes()
        {
            GameObject controllerObject = new GameObject("ExternalLookController");
            GameObject volumeRoot = new GameObject("ExternalLookVolumes");
            GameObject owner = new GameObject("ExternalLookOwner");
            Volume baseVolume = CreateVolume(
                volumeRoot.transform,
                "Base",
                1f,
                40f,
                out VolumeProfile baseProfile);
            Volume overlay = CreateVolume(
                volumeRoot.transform,
                "Overlay",
                0f,
                95f,
                out VolumeProfile overlayProfile);
            try
            {
                GameplayLookStateController controller =
                    controllerObject.AddComponent<GameplayLookStateController>();
                controller.Configure(
                    baseVolume,
                    new[]
                    {
                        new GameplayLookStateController.OverlayBinding(
                            GameplayLookState.CharacterFocus,
                            overlay,
                            0f,
                            0f),
                    });
                Assert.That(
                    controller.TryAcquire(
                        GameplayLookState.CharacterFocus,
                        owner,
                        out GameplayLookStateController.LookLease lease),
                    Is.True);
                Assert.That(overlay.weight, Is.EqualTo(1f));

                Object.DestroyImmediate(controllerObject);
                Assert.That(overlay.weight, Is.Zero);
                Assert.That(lease.IsValid, Is.False);
                lease.Dispose();
            }
            finally
            {
                Object.DestroyImmediate(volumeRoot);
                Object.DestroyImmediate(owner);
                Object.DestroyImmediate(baseProfile);
                Object.DestroyImmediate(overlayProfile);
                if (controllerObject != null)
                {
                    Object.DestroyImmediate(controllerObject);
                }
            }

            yield return null;
        }

        [UnityTest]
        [Timeout(45000)]
        public IEnumerator AuthoredOlympusScenesStartAtGameplayBaseAndExcludePresentationDof()
        {
            EditorSceneManager.LoadSceneInPlayMode(
                CorridorScenePath,
                new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;
            yield return null;
            ValidateAuthoredScene(SceneManager.GetSceneByPath(CorridorScenePath), expectPhaseTwo: false);

            EditorSceneManager.LoadSceneInPlayMode(
                StationScenePath,
                new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;
            yield return null;
            Scene station = SceneManager.GetSceneByPath(StationScenePath);
            ValidateAuthoredScene(station, expectPhaseTwo: true);

            GameplayLookStateController controller = station.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<GameplayLookStateController>(true))
                .Single();
            Volume characterFocus = RequireNamedVolume(station, "InoriPresentation_WarmPostProcess");
            Volume phaseTwo = RequireNamedVolume(station, "AkazaPhase2_SourceSoftPostVolume");
            GameObject transitionRoot = RequireNamedObject(
                station,
                "OlympusStation_AkazaPhase2TransitionRig");
            transitionRoot.SetActive(true);
            yield return null;
            Assert.That(controller.CurrentState, Is.EqualTo(GameplayLookState.Phase2Cinematic));
            Assert.That(controller.ActiveLeaseCount, Is.EqualTo(1));
            Assert.That(phaseTwo.weight, Is.EqualTo(1f).Within(0.0001f));
            Assert.That(characterFocus.weight, Is.Zero.Within(0.0001f));

            transitionRoot.SetActive(false);
            yield return null;
            Assert.That(controller.CurrentState, Is.EqualTo(GameplayLookState.GameplayBase));
            Assert.That(controller.ActiveLeaseCount, Is.Zero);
            Assert.That(phaseTwo.weight, Is.Zero.Within(0.0001f));
            Assert.That(characterFocus.weight, Is.Zero.Within(0.0001f));
        }

        private static Fixture CreateFixture(bool immediate)
        {
            GameObject root = new GameObject("GameplayLookStateTestRoot");
            Volume baseVolume = CreateVolume(root.transform, "Base", 0.73f, 40f, out VolumeProfile baseProfile);
            Volume character = CreateVolume(root.transform, "Character", 0.91f, 95f, out VolumeProfile characterProfile);
            Volume phaseTwo = CreateVolume(root.transform, "PhaseTwo", 0.87f, 220f, out VolumeProfile phaseTwoProfile);
            GameObject characterOwner = new GameObject("CharacterOwner");
            GameObject phaseTwoOwner = new GameObject("PhaseTwoOwner");
            GameplayLookStateController controller =
                root.AddComponent<GameplayLookStateController>();
            float duration = immediate ? 0f : 0.04f;
            controller.Configure(
                baseVolume,
                new[]
                {
                    new GameplayLookStateController.OverlayBinding(
                        GameplayLookState.CharacterFocus,
                        character,
                        duration,
                        duration),
                    new GameplayLookStateController.OverlayBinding(
                        GameplayLookState.Phase2Cinematic,
                        phaseTwo,
                        duration,
                        duration),
                });

            return new Fixture(
                root,
                characterOwner,
                phaseTwoOwner,
                controller,
                baseVolume,
                character,
                phaseTwo,
                new[] { baseProfile, characterProfile, phaseTwoProfile });
        }

        private static void ValidateAuthoredScene(Scene scene, bool expectPhaseTwo)
        {
            Assert.That(scene.IsValid() && scene.isLoaded, Is.True);
            GameplayLookStateController[] controllers = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<GameplayLookStateController>(true))
                .ToArray();
            Assert.That(controllers, Has.Length.EqualTo(1));
            GameplayLookStateController controller = controllers[0];
            Assert.That(controller.CurrentState, Is.EqualTo(GameplayLookState.GameplayBase));
            Assert.That(controller.ActiveLeaseCount, Is.Zero);

            Volume gameplayBase = RequireNamedVolume(scene, "OlympusCorridor_GlobalPostProcess");
            Volume characterFocus = RequireNamedVolume(scene, "InoriPresentation_WarmPostProcess");
            Assert.That(controller.GameplayBaseVolume, Is.SameAs(gameplayBase));
            Assert.That(gameplayBase.weight, Is.EqualTo(1f).Within(0.0001f));
            Assert.That(characterFocus.weight, Is.Zero.Within(0.0001f));
            Assert.That(
                controller.GetOverlayVolume(GameplayLookState.CharacterFocus),
                Is.SameAs(characterFocus));
            Assert.That(gameplayBase.sharedProfile, Is.Not.Null);
            Assert.That(
                gameplayBase.sharedProfile.TryGet(out DepthOfField baseDepthOfField),
                Is.True);
            Assert.That(baseDepthOfField.active, Is.False);

            if (!expectPhaseTwo)
            {
                Assert.That(controller.HasBinding(GameplayLookState.Phase2Cinematic), Is.False);
                return;
            }

            Volume phaseTwo = RequireNamedVolume(scene, "AkazaPhase2_SourceSoftPostVolume");
            Assert.That(phaseTwo.weight, Is.Zero.Within(0.0001f));
            Assert.That(
                controller.GetOverlayVolume(GameplayLookState.Phase2Cinematic),
                Is.SameAs(phaseTwo));
            Assert.That(phaseTwo.sharedProfile, Is.Not.Null);
            Assert.That(phaseTwo.sharedProfile.TryGet(out DepthOfField _), Is.False);

            AkazaPhase2CinematicLookDriver[] drivers = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<AkazaPhase2CinematicLookDriver>(true))
                .ToArray();
            Assert.That(drivers, Has.Length.EqualTo(1));
            Assert.That(drivers[0].LookStateController, Is.SameAs(controller));
            Assert.That(drivers[0].transform.root.gameObject.activeSelf, Is.False);
        }

        private static Volume RequireNamedVolume(Scene scene, string objectName)
        {
            Volume[] matches = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<Volume>(true))
                .Where(volume => volume.name == objectName)
                .ToArray();
            Assert.That(matches, Has.Length.EqualTo(1), objectName);
            return matches[0];
        }

        private static GameObject RequireNamedObject(Scene scene, string objectName)
        {
            GameObject[] matches = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<Transform>(true))
                .Where(transform => transform.name == objectName)
                .Select(transform => transform.gameObject)
                .ToArray();
            Assert.That(matches, Has.Length.EqualTo(1), objectName);
            return matches[0];
        }

        private static Volume CreateVolume(
            Transform parent,
            string name,
            float weight,
            float priority,
            out VolumeProfile profile)
        {
            GameObject child = new GameObject(name);
            child.transform.SetParent(parent, false);
            Volume volume = child.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.weight = weight;
            volume.priority = priority;
            profile = ScriptableObject.CreateInstance<VolumeProfile>();
            volume.sharedProfile = profile;
            return volume;
        }

        private sealed class Fixture
        {
            public Fixture(
                GameObject root,
                GameObject characterOwner,
                GameObject phaseTwoOwner,
                GameplayLookStateController controller,
                Volume baseVolume,
                Volume character,
                Volume phaseTwo,
                VolumeProfile[] profiles)
            {
                Root = root;
                CharacterOwner = characterOwner;
                PhaseTwoOwner = phaseTwoOwner;
                Controller = controller;
                Base = baseVolume;
                Character = character;
                PhaseTwo = phaseTwo;
                Profiles = profiles;
            }

            public GameObject Root { get; }
            public GameObject CharacterOwner { get; }
            public GameObject PhaseTwoOwner { get; }
            public GameplayLookStateController Controller { get; }
            public Volume Base { get; }
            public Volume Character { get; }
            public Volume PhaseTwo { get; }
            public VolumeProfile[] Profiles { get; }

            public void Dispose()
            {
                Time.timeScale = 1f;
                if (Profiles != null)
                {
                    foreach (VolumeProfile profile in Profiles)
                    {
                        if (profile != null)
                        {
                            Object.DestroyImmediate(profile);
                        }
                    }
                }

                Object.DestroyImmediate(Root);
                if (CharacterOwner != null)
                {
                    Object.DestroyImmediate(CharacterOwner);
                }

                if (PhaseTwoOwner != null)
                {
                    Object.DestroyImmediate(PhaseTwoOwner);
                }
            }
        }
    }
}
