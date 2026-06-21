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
        private const string BossBarrageProjectilePrefabPath =
            "Assets/_Game/Prefabs/Combat/PF_BossBarrageProjectile_NeedleLock.prefab";
        private const string Skill1ProjectilePrefabPath =
            "Assets/_Game/Prefabs/Combat/PF_PlayerSkill1Projectile_LaneBolt.prefab";
        private const string SummonSlot1ProjectilePrefabPath =
            "Assets/_Game/Prefabs/Combat/PF_SummonSlot1Projectile_AssistBolt.prefab";

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

        [Test]
        public void PlayerRangedCombatCuesUsePromotedShotAudio()
        {
            CombatVfxCueProfile profile = AssetDatabase.LoadAssetAtPath<CombatVfxCueProfile>(CombatVfxCueProfilePath);
            Assert.IsNotNull(profile, $"Missing combat VFX cue profile at {CombatVfxCueProfilePath}.");

            AssertPromotedCueAudio(profile, CombatVfxCueId.PlayerRangedMuzzleFlash, "SFX_Vefects_Shots_Weapon_Rifle");
            AssertPromotedCueAudio(profile, CombatVfxCueId.PlayerRangedProjectileImpact, "SFX_Vefects_Shots_Squib_Metal");
        }

        [Test]
        public void CombatStateCuesUsePromotedMagicMissilesAudio()
        {
            CombatVfxCueProfile profile = AssetDatabase.LoadAssetAtPath<CombatVfxCueProfile>(CombatVfxCueProfilePath);
            Assert.IsNotNull(profile, $"Missing combat VFX cue profile at {CombatVfxCueProfilePath}.");

            const string magicMissilesAudioPath = "Assets/_Game/Art/VFX/MagicMissiles/Audio/";
            AssertPromotedCueAudio(profile, CombatVfxCueId.EnemyHit, "light_hit", magicMissilesAudioPath);
            AssertPromotedCueAudio(profile, CombatVfxCueId.EnemyDeath, "death_hit", magicMissilesAudioPath);
            AssertPromotedCueAudio(profile, CombatVfxCueId.EliteShieldSignal, "holy_hit", magicMissilesAudioPath);
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
        public void PromotedProjectilePrefabsUseOneShotAudioResources()
        {
            AssertPromotedProjectileAudio(BossBarrageProjectilePrefabPath, "fire_shoot");
            AssertPromotedProjectileAudio(Skill1ProjectilePrefabPath, "arcane_shoot");
            AssertPromotedProjectileAudio(SummonSlot1ProjectilePrefabPath, "light_shoot");
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

        private static void AssertPromotedCueAudio(
            CombatVfxCueProfile profile,
            CombatVfxCueId cueId,
            string expectedClipName,
            string expectedPathPrefix = "Assets/_Game/Art/VFX/CombatCues/Audio/")
        {
            Assert.IsTrue(profile.TryGetCue(cueId, out CombatVfxCue cue), $"{cueId} should be authored.");
            Assert.IsNotNull(cue.Prefab, $"{cueId} should reference a cue prefab.");
            AudioSource[] audioSources = cue.Prefab.GetComponentsInChildren<AudioSource>(true);
            Assert.IsNotEmpty(audioSources, $"{cueId} should include a promoted one-shot AudioSource.");

            bool foundExpectedClip = false;
            for (int i = 0; i < audioSources.Length; i++)
            {
                AudioSource audioSource = audioSources[i];
                Assert.IsNotNull(audioSource.clip, $"{cueId} AudioSource {i} should reference a clip.");
                Assert.IsFalse(audioSource.playOnAwake, $"{cueId} AudioSource {i} should only play when CombatVfxCuePlayer fires the cue.");
                Assert.IsFalse(audioSource.loop, $"{cueId} AudioSource {i} should be a one-shot cue.");

                string clipPath = AssetDatabase.GetAssetPath(audioSource.clip).Replace('\\', '/');
                foundExpectedClip |= clipPath.StartsWith(expectedPathPrefix, System.StringComparison.Ordinal)
                    && clipPath.Contains(expectedClipName);
            }

            Assert.IsTrue(
                foundExpectedClip,
                $"{cueId} should include {expectedClipName} under {expectedPathPrefix}.");
        }

        private static void AssertPromotedProjectileAudio(string prefabPath, string expectedClipName)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            Assert.IsNotNull(prefab, $"Missing projectile prefab at {prefabPath}.");
            AudioSource audioSource = prefab.GetComponentInChildren<AudioSource>(true);
            Assert.IsNotNull(audioSource, $"{prefab.name} should include a promoted projectile AudioSource.");
            Assert.IsNotNull(audioSource.clip, $"{prefab.name} AudioSource should reference a clip.");
            Assert.IsFalse(audioSource.playOnAwake, $"{prefab.name} AudioSource should play only when the projectile is configured.");
            Assert.IsFalse(audioSource.loop, $"{prefab.name} AudioSource should be a one-shot projectile cue.");

            string clipPath = AssetDatabase.GetAssetPath(audioSource.clip).Replace('\\', '/');
            Assert.IsTrue(
                clipPath.StartsWith("Assets/_Game/Art/VFX/", System.StringComparison.Ordinal),
                $"{prefab.name} audio should be promoted under _Game/Art/VFX, found {clipPath}.");
            Assert.IsTrue(
                clipPath.Contains(expectedClipName),
                $"{prefab.name} should use the intended promoted projectile audio clip, found {clipPath}.");
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
