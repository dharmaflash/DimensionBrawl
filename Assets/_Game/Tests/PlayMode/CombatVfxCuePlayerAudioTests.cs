using System.Collections;
using DimensionBrawl.Combat;
using DimensionBrawl.Presentation;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace DimensionBrawl.Tests
{
    public sealed class CombatVfxCuePlayerAudioTests
    {
        private const string CombatVfxCueProfilePath =
            "Assets/_Game/DesignData/Profiles/ActionFoundation/DB_CombatVfxCues_ActionFoundation.asset";
        private static readonly string[] PlayerRangedGunshotClipPaths =
        {
            "Assets/_Game/Art/Audio/SFX/Guns/DB_SFX_PlayerRanged_Gunshot_01.wav",
            "Assets/_Game/Art/Audio/SFX/Guns/DB_SFX_PlayerRanged_Gunshot_02.wav",
            "Assets/_Game/Art/Audio/SFX/Guns/DB_SFX_PlayerRanged_Gunshot_03.wav",
            "Assets/_Game/Art/Audio/SFX/Guns/DB_SFX_PlayerRanged_Gunshot_04.wav",
            "Assets/_Game/Art/Audio/SFX/Guns/DB_SFX_PlayerRanged_Gunshot_05.wav",
        };
        private const string BossBarrageProjectilePrefabPath =
            "Assets/_Game/Prefabs/Combat/PF_BossBarrageProjectile_NeedleLock.prefab";
        private const string Skill1ProjectilePrefabPath =
            "Assets/_Game/Prefabs/Combat/PF_PlayerSkill1Projectile_LaneBolt.prefab";
        private const string SummonSlot1ProjectilePrefabPath =
            "Assets/_Game/Prefabs/Combat/PF_SummonSlot1Projectile_AssistBolt.prefab";
        private const string SummonSlot2ProjectilePrefabPath =
            "Assets/_Game/Prefabs/Combat/PF_SummonSlot2Projectile_MarksmanBolt.prefab";
        private const string SummonSlot3ProjectilePrefabPath =
            "Assets/_Game/Prefabs/Combat/PF_SummonSlot3Projectile_VanguardBolt.prefab";

        [UnityTest]
        public IEnumerator CombatVfxCuePlayerRestartsAudioSourcesInsideCuePrefab()
        {
            GameObject owner = new GameObject("CombatVfxCuePlayerAudioTestOwner");
            GameObject prefab = new GameObject("CombatVfxCuePlayerAudioTestPrefab");
            AudioClip clip = AudioClip.Create("CombatVfxCuePlayerAudioTestClip", 44100, 1, 44100, false);
            try
            {
                owner.AddComponent<AudioListener>();
                GameObject audioObject = new GameObject("AudioCue");
                audioObject.transform.SetParent(prefab.transform, worldPositionStays: false);
                AudioSource source = audioObject.AddComponent<AudioSource>();
                source.clip = clip;
                source.playOnAwake = false;
                source.loop = false;
                source.spatialBlend = 0f;

                CombatVfxCueProfile profile = CreateSingleCueProfile(prefab);
                CombatVfxCuePlayer cuePlayer = owner.AddComponent<CombatVfxCuePlayer>();
                SetObjectReference(cuePlayer, "profile", profile);

                cuePlayer.PlayCue(CombatVfxCueId.PlayerRangedMuzzleFlash, owner.transform, Vector3.forward);
                yield return null;

                Transform instance = owner.transform.Find(prefab.name);
                Assert.IsNotNull(instance, "Parented combat cue instance should be spawned under the anchor.");
                AudioSource playedSource = instance.GetComponentInChildren<AudioSource>(true);
                Assert.IsNotNull(playedSource, "Spawned combat cue should keep the prefab AudioSource.");
                Assert.IsTrue(playedSource.isPlaying, "CombatVfxCuePlayer should explicitly restart AudioSources when a cue plays.");
            }
            finally
            {
                Object.Destroy(owner);
                Object.Destroy(prefab);
                Object.Destroy(clip);
            }
        }

        [UnityTest]
        public IEnumerator CombatVfxCuePlayerPlaysRandomizedAudioBankInsideCuePrefab()
        {
            GameObject owner = new GameObject("CombatVfxCuePlayerRandomAudioTestOwner");
            GameObject prefab = new GameObject("CombatVfxCuePlayerRandomAudioTestPrefab");
            AudioClip firstClip = AudioClip.Create("CombatVfxCuePlayerRandomAudioClipA", 44100, 1, 44100, false);
            AudioClip secondClip = AudioClip.Create("CombatVfxCuePlayerRandomAudioClipB", 44100, 1, 44100, false);
            try
            {
                owner.AddComponent<AudioListener>();
                GameObject audioObject = new GameObject("RandomAudioCue");
                audioObject.transform.SetParent(prefab.transform, worldPositionStays: false);
                AudioSource source = audioObject.AddComponent<AudioSource>();
                source.playOnAwake = false;
                source.loop = false;
                source.spatialBlend = 0f;
                CombatVfxCueAudioRandomizer randomizer = audioObject.AddComponent<CombatVfxCueAudioRandomizer>();
                randomizer.Configure(source, new[] { firstClip, secondClip }, 0.68f, 0.96f, 1.04f, 0.92f, 1.06f);

                CombatVfxCueProfile profile = CreateSingleCueProfile(prefab);
                CombatVfxCuePlayer cuePlayer = owner.AddComponent<CombatVfxCuePlayer>();
                SetObjectReference(cuePlayer, "profile", profile);

                cuePlayer.PlayCue(CombatVfxCueId.PlayerRangedMuzzleFlash, owner.transform, Vector3.forward);
                yield return null;

                Transform instance = owner.transform.Find(prefab.name);
                Assert.IsNotNull(instance, "Parented combat cue instance should be spawned under the anchor.");
                AudioSource playedSource = instance.GetComponentInChildren<AudioSource>(true);
                Assert.IsNotNull(playedSource, "Spawned combat cue should keep the prefab AudioSource.");
                Assert.IsTrue(playedSource.isPlaying, "CombatVfxCuePlayer should play randomized AudioSources when a cue plays.");
                Assert.Contains(playedSource.clip, new[] { firstClip, secondClip });
                Assert.That(playedSource.pitch, Is.InRange(0.96f, 1.04f));
                Assert.That(playedSource.volume, Is.InRange(0.68f * 0.92f, 0.68f * 1.06f));
            }
            finally
            {
                Object.Destroy(owner);
                Object.Destroy(prefab);
                Object.Destroy(firstClip);
                Object.Destroy(secondClip);
            }
        }

        [Test]
        public void PlayerRangedMuzzleCueUsesReviewedGunshotSfxBank()
        {
            CombatVfxCueProfile profile = AssetDatabase.LoadAssetAtPath<CombatVfxCueProfile>(CombatVfxCueProfilePath);
            Assert.IsNotNull(profile, $"Missing combat VFX cue profile at {CombatVfxCueProfilePath}.");

            AssertCueHasReviewedGunshotAudioBank(profile, CombatVfxCueId.PlayerRangedMuzzleFlash, PlayerRangedGunshotClipPaths);
            AssertCueHasNoAuthoredAudio(profile, CombatVfxCueId.PlayerRangedProjectileImpact);
        }

        [Test]
        public void CombatStateCuesDoNotCarryTemporarySfx()
        {
            CombatVfxCueProfile profile = AssetDatabase.LoadAssetAtPath<CombatVfxCueProfile>(CombatVfxCueProfilePath);
            Assert.IsNotNull(profile, $"Missing combat VFX cue profile at {CombatVfxCueProfilePath}.");

            AssertCueHasNoAuthoredAudio(profile, CombatVfxCueId.EnemyHit);
            AssertCueHasNoAuthoredAudio(profile, CombatVfxCueId.EnemyDeath);
            AssertCueHasNoAuthoredAudio(profile, CombatVfxCueId.PlayerDamaged);
            AssertCueHasNoAuthoredAudio(profile, CombatVfxCueId.PlayerCritical);
            AssertCueHasNoAuthoredAudio(profile, CombatVfxCueId.EliteShieldSignal);
            AssertCueHasNoAuthoredAudio(profile, CombatVfxCueId.EliteArmorBreakSignal);
            AssertCueHasNoAuthoredAudio(profile, CombatVfxCueId.EliteSummonSignal);
            AssertCueHasNoAuthoredAudio(profile, CombatVfxCueId.SummonFollowupWindow);
            AssertCueHasNoAuthoredAudio(profile, CombatVfxCueId.SummonBlockOpportunity);
            AssertCueHasNoAuthoredAudio(profile, CombatVfxCueId.SummonSlot2BeamLock);
            AssertCueHasNoAuthoredAudio(profile, CombatVfxCueId.SummonSlot2BeamFire);
            AssertCueHasNoAuthoredAudio(profile, CombatVfxCueId.SummonSlot2BeamHit);
            AssertCueHasNoAuthoredAudio(profile, CombatVfxCueId.SummonSlot3ShieldRaise);
            AssertCueHasNoAuthoredAudio(profile, CombatVfxCueId.SummonSlot3ShieldHit);
            AssertCueHasNoAuthoredAudio(profile, CombatVfxCueId.PocketCleared);
            AssertCueHasNoAuthoredAudio(profile, CombatVfxCueId.SummonFollowupMissed);
            AssertCueHasNoAuthoredAudio(profile, CombatVfxCueId.PocketFailed);
        }

        [Test]
        public void PocketFollowupHitCueDoesNotCarryTemporarySfx()
        {
            CombatVfxCueProfile profile = AssetDatabase.LoadAssetAtPath<CombatVfxCueProfile>(CombatVfxCueProfilePath);
            Assert.IsNotNull(profile, $"Missing combat VFX cue profile at {CombatVfxCueProfilePath}.");

            AssertCueHasNoAuthoredAudio(profile, CombatVfxCueId.SummonFollowupHit);
        }

        [UnityTest]
        public IEnumerator LaneActionProjectileRestartsAudioSourcesOnConfigure()
        {
            GameObject listener = new GameObject("LaneActionProjectileAudioListener");
            GameObject projectileObject = new GameObject("LaneActionProjectileAudioTest");
            AudioClip clip = CreateTestClip("LaneActionProjectileAudioClip");
            try
            {
                listener.AddComponent<AudioListener>();
                AudioSource source = AddOneShotAudio(projectileObject, clip);
                LaneActionProjectile projectile = projectileObject.AddComponent<LaneActionProjectile>();
                projectileObject.SetActive(false);

                projectile.Configure(null, DamageTeam.Player, 0f, Vector3.forward, 1f, 1f, 0.1f);
                yield return null;

                Assert.IsTrue(source.isPlaying, "LaneActionProjectile should restart authored projectile AudioSources when fired.");

                projectile.Deactivate();
                yield return null;

                Assert.IsFalse(source.isPlaying, "LaneActionProjectile should stop authored projectile AudioSources when deactivated.");
            }
            finally
            {
                Object.Destroy(listener);
                Object.Destroy(projectileObject);
                Object.Destroy(clip);
            }
        }

        [UnityTest]
        public IEnumerator BossBarrageProjectileRestartsAudioSourcesOnConfigure()
        {
            GameObject listener = new GameObject("BossBarrageProjectileAudioListener");
            GameObject projectileObject = new GameObject("BossBarrageProjectileAudioTest");
            AudioClip clip = CreateTestClip("BossBarrageProjectileAudioClip");
            try
            {
                listener.AddComponent<AudioListener>();
                AudioSource source = AddOneShotAudio(projectileObject, clip);
                BossBarrageProjectile projectile = projectileObject.AddComponent<BossBarrageProjectile>();
                projectileObject.SetActive(false);

                projectile.Configure(null, DamageTeam.Enemy, 0f, Vector3.back, 1f, 1f, 0.1f);
                yield return null;

                Assert.IsTrue(source.isPlaying, "BossBarrageProjectile should restart authored projectile AudioSources when fired.");

                projectile.Deactivate();
                yield return null;

                Assert.IsFalse(source.isPlaying, "BossBarrageProjectile should stop authored projectile AudioSources when deactivated.");
            }
            finally
            {
                Object.Destroy(listener);
                Object.Destroy(projectileObject);
                Object.Destroy(clip);
            }
        }

        [Test]
        public void PromotedProjectilePrefabsDoNotCarryTemporarySfx()
        {
            AssertProjectilePrefabHasNoAuthoredAudio(BossBarrageProjectilePrefabPath);
            AssertProjectilePrefabHasNoAuthoredAudio(Skill1ProjectilePrefabPath);
            AssertProjectilePrefabHasNoAuthoredAudio(SummonSlot1ProjectilePrefabPath);
            AssertProjectilePrefabHasNoAuthoredAudio(SummonSlot2ProjectilePrefabPath);
            AssertProjectilePrefabHasNoAuthoredAudio(SummonSlot3ProjectilePrefabPath);
        }

        private static CombatVfxCueProfile CreateSingleCueProfile(GameObject prefab)
        {
            CombatVfxCueProfile profile = ScriptableObject.CreateInstance<CombatVfxCueProfile>();
            SerializedObject serializedObject = new SerializedObject(profile);
            SerializedProperty cues = serializedObject.FindProperty("cues");
            cues.arraySize = 1;

            SerializedProperty cue = cues.GetArrayElementAtIndex(0);
            cue.FindPropertyRelative("cueId").enumValueIndex = (int)CombatVfxCueId.PlayerRangedMuzzleFlash;
            cue.FindPropertyRelative("prefab").objectReferenceValue = prefab;
            cue.FindPropertyRelative("localPositionOffset").vector3Value = Vector3.zero;
            cue.FindPropertyRelative("localEulerOffset").vector3Value = Vector3.zero;
            cue.FindPropertyRelative("localScale").vector3Value = Vector3.one;
            cue.FindPropertyRelative("lifetimeSeconds").floatValue = 0.5f;
            cue.FindPropertyRelative("prewarmCount").intValue = 0;
            cue.FindPropertyRelative("parentToAnchor").boolValue = true;
            cue.FindPropertyRelative("alignForwardToDirection").boolValue = false;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            return profile;
        }

        private static AudioClip CreateTestClip(string name)
        {
            return AudioClip.Create(name, 44100, 1, 44100, false);
        }

        private static AudioSource AddOneShotAudio(GameObject owner, AudioClip clip)
        {
            AudioSource source = owner.AddComponent<AudioSource>();
            source.clip = clip;
            source.playOnAwake = false;
            source.loop = false;
            source.spatialBlend = 0f;
            return source;
        }

        private static void AssertCueHasNoAuthoredAudio(CombatVfxCueProfile profile, CombatVfxCueId cueId)
        {
            Assert.IsTrue(profile.TryGetCue(cueId, out CombatVfxCue cue), $"{cueId} should be authored.");
            Assert.IsNotNull(cue.Prefab, $"{cueId} should reference a cue prefab.");
            AssertNoAuthoredAudio(cue.Prefab, cueId.ToString());
        }

        private static void AssertCueHasReviewedGunshotAudioBank(
            CombatVfxCueProfile profile,
            CombatVfxCueId cueId,
            string[] expectedClipPaths)
        {
            Assert.IsTrue(profile.TryGetCue(cueId, out CombatVfxCue cue), $"{cueId} should be authored.");
            Assert.IsNotNull(cue.Prefab, $"{cueId} should reference a cue prefab.");
            Assert.That(cue.LifetimeSeconds, Is.InRange(0.34f, 0.55f), $"{cueId} lifetime should stay tight for the snappy reviewed gunshot bank.");
            CombatVfxCueAudioRandomizer[] randomizers = cue.Prefab.GetComponentsInChildren<CombatVfxCueAudioRandomizer>(true);
            Assert.AreEqual(1, randomizers.Length, $"{cueId} should carry exactly one reviewed audio randomizer.");
            CombatVfxCueAudioRandomizer randomizer = randomizers[0];
            Assert.AreEqual(expectedClipPaths.Length, randomizer.ClipCount, $"{cueId} should carry the reviewed gunshot bank.");

            for (int i = 0; i < expectedClipPaths.Length; i++)
            {
                AudioClip clip = randomizer.GetClip(i);
                string clipPath = AssetDatabase.GetAssetPath(clip).Replace('\\', '/');
                Assert.AreEqual(expectedClipPaths[i], clipPath, $"{cueId} should use reviewed game-owned gunshot clip {i + 1}.");
                Assert.That(clipPath, Does.StartWith("Assets/_Game/"), $"{cueId} audio clip should be promoted under _Game.");
                Assert.That(clipPath, Does.Not.Contain("/_Imported/"), $"{cueId} audio clip must not reference the raw pack.");
            }

            AudioSource audioSource = randomizer.Source;
            Assert.IsNotNull(audioSource, $"{cueId} randomizer should reference its local AudioSource.");
            Assert.IsNull(audioSource.clip, $"{cueId}.{audioSource.name} should let the randomizer choose the clip on play.");
            Assert.IsFalse(audioSource.playOnAwake, $"{cueId}.{audioSource.name} should not auto-play audio.");
            Assert.IsFalse(audioSource.loop, $"{cueId}.{audioSource.name} should not loop audio.");
            Assert.GreaterOrEqual(audioSource.volume, 0.55f, $"{cueId}.{audioSource.name} should keep the reviewed gunshot audible.");
            Assert.AreEqual(1f, audioSource.pitch, 0.01f, $"{cueId}.{audioSource.name} should not start pitch-shifted.");
            Assert.LessOrEqual(audioSource.spatialBlend, 0.05f, $"{cueId}.{audioSource.name} should play as a clear local player shot.");
            Assert.That(randomizer.MinimumPitch, Is.InRange(1f, 1.04f));
            Assert.That(randomizer.MaximumPitch, Is.InRange(1.06f, 1.1f));
            Assert.That(randomizer.MinimumVolumeMultiplier, Is.InRange(0.9f, 1f));
            Assert.That(randomizer.MaximumVolumeMultiplier, Is.InRange(1f, 1.08f));
        }

        private static void AssertProjectilePrefabHasNoAuthoredAudio(string prefabPath)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            Assert.IsNotNull(prefab, $"Missing projectile prefab at {prefabPath}.");
            AssertNoAuthoredAudio(prefab, prefab.name);
        }

        private static void AssertNoAuthoredAudio(GameObject root, string label)
        {
            AudioSource[] audioSources = root.GetComponentsInChildren<AudioSource>(true);
            for (int i = 0; i < audioSources.Length; i++)
            {
                AudioSource audioSource = audioSources[i];
                Assert.IsNull(audioSource.clip, $"{label}.{audioSource.name} should not carry temporary authored SFX.");
                Assert.IsFalse(audioSource.playOnAwake, $"{label}.{audioSource.name} should not auto-play audio.");
                Assert.IsFalse(audioSource.loop, $"{label}.{audioSource.name} should not loop audio.");
            }
        }

        private static void SetObjectReference(Object target, string propertyName, Object value)
        {
            SerializedObject serializedObject = new SerializedObject(target);
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            Assert.IsNotNull(property, $"{target.name} should expose serialized property {propertyName}.");
            property.objectReferenceValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
