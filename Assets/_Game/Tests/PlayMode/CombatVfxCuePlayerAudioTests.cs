using System.Collections;
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

        private static void AssertPromotedCueAudio(
            CombatVfxCueProfile profile,
            CombatVfxCueId cueId,
            string expectedClipName)
        {
            Assert.IsTrue(profile.TryGetCue(cueId, out CombatVfxCue cue), $"{cueId} should be authored.");
            Assert.IsNotNull(cue.Prefab, $"{cueId} should reference a cue prefab.");
            AudioSource audioSource = cue.Prefab.GetComponentInChildren<AudioSource>(true);
            Assert.IsNotNull(audioSource, $"{cueId} should include a promoted one-shot AudioSource.");
            Assert.IsNotNull(audioSource.clip, $"{cueId} AudioSource should reference a clip.");
            Assert.IsFalse(audioSource.playOnAwake, $"{cueId} AudioSource should only play when CombatVfxCuePlayer fires the cue.");
            Assert.IsFalse(audioSource.loop, $"{cueId} AudioSource should be a one-shot cue.");

            string clipPath = AssetDatabase.GetAssetPath(audioSource.clip).Replace('\\', '/');
            Assert.IsTrue(
                clipPath.StartsWith("Assets/_Game/Art/VFX/CombatCues/Audio/", System.StringComparison.Ordinal),
                $"{cueId} audio should be promoted into _Game, found {clipPath}.");
            Assert.IsTrue(
                clipPath.Contains(expectedClipName),
                $"{cueId} should use the intended shot audio clip, found {clipPath}.");
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
