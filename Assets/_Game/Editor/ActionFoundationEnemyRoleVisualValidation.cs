using System;
using System.Collections.Generic;
using System.Linq;
using DimensionBrawl.Enemies;
using DimensionBrawl.Presentation;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace DimensionBrawl.Editor
{
    internal static partial class ActionFoundationEnemyRoleVisualSetup
    {
        private static AnimatorState AddState(AnimatorStateMachine stateMachine, EnemyRoleVisualSpec spec, string key, Vector3 position)
        {
            RoleAnimationClipSpec clip = RequireClip(spec, key);
            AnimatorState state = stateMachine.AddState(key, position);
            state.motion = LoadClip(ClipTargetPath(spec, clip));
            state.speed = clip.Speed;
            state.writeDefaultValues = true;
            return state;
        }

        private static RoleAnimationClipSpec RequireClip(EnemyRoleVisualSpec spec, string key)
        {
            for (int i = 0; i < spec.Clips.Length; i++)
            {
                if (spec.Clips[i].Key == key)
                {
                    return spec.Clips[i];
                }
            }

            throw new InvalidOperationException($"{spec.RoleTag} is missing animation clip mapping for {key}.");
        }

        private static string ClipTargetPath(EnemyRoleVisualSpec spec, RoleAnimationClipSpec clip)
        {
            return $"{spec.AnimationRoot}/{SanitizeAssetName(clip.TargetClipName)}.fbx";
        }

        private static AnimationClip LoadClip(string path)
        {
            AnimationClip clip = AssetDatabase.LoadAllAssetsAtPath(path)
                .OfType<AnimationClip>()
                .FirstOrDefault(clipAsset => !clipAsset.name.StartsWith("__preview__", StringComparison.Ordinal));
            if (clip == null)
            {
                throw new InvalidOperationException($"Missing animation clip at {path}.");
            }

            return clip;
        }

        private static void RemoveRoleVisualChildren(Transform parent)
        {
            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                Transform child = parent.GetChild(i);
                if (child.name.StartsWith(RoleVisualPrefix, StringComparison.Ordinal)
                    || VisualChildNamesToRemove.Any(name => child.name == name || child.name.StartsWith(name + "_", StringComparison.Ordinal)))
                {
                    UnityEngine.Object.DestroyImmediate(child.gameObject);
                }
            }
        }

        private static Renderer[] CollectPresentableRenderers(GameObject root)
        {
            return root.GetComponentsInChildren<Renderer>(true)
                .Where(renderer => renderer.enabled && IsActiveInHierarchy(renderer.transform, root.transform))
                .ToArray();
        }

        private static bool IsActiveInHierarchy(Transform candidate, Transform root)
        {
            for (Transform current = candidate; current != null; current = current.parent)
            {
                if (!current.gameObject.activeSelf)
                {
                    return false;
                }

                if (current == root)
                {
                    return true;
                }
            }

            return false;
        }

        private static void ValidateRendererAssets(GameObject visual, string roleId)
        {
            Renderer[] renderers = visual.GetComponentsInChildren<Renderer>(true);
            bool hasGameOwnedMaterial = false;
            for (int i = 0; i < renderers.Length; i++)
            {
                Material[] materials = renderers[i].sharedMaterials;
                for (int j = 0; j < materials.Length; j++)
                {
                    Material material = materials[j];
                    if (material == null)
                    {
                        continue;
                    }

                    string materialPath = AssetDatabase.GetAssetPath(material).Replace('\\', '/');
                    if (materialPath.Contains("/_Imported/", StringComparison.Ordinal)
                        || !materialPath.StartsWith("Assets/_Game/", StringComparison.Ordinal)
                        || materialPath.Contains("/Art/Materials/Enemies/ActionFoundationRoleVariants/", StringComparison.Ordinal))
                    {
                        throw new InvalidOperationException($"{roleId} renderer {renderers[i].name} uses invalid material {materialPath}.");
                    }

                    hasGameOwnedMaterial = true;
                    string[] textureNames = material.GetTexturePropertyNames();
                    for (int textureIndex = 0; textureIndex < textureNames.Length; textureIndex++)
                    {
                        Texture texture = material.GetTexture(textureNames[textureIndex]);
                        if (texture == null)
                        {
                            continue;
                        }

                        string texturePath = AssetDatabase.GetAssetPath(texture).Replace('\\', '/');
                        if (texturePath.Contains("/_Imported/", StringComparison.Ordinal) || !texturePath.StartsWith("Assets/_Game/", StringComparison.Ordinal))
                        {
                            throw new InvalidOperationException($"{roleId} material {material.name} uses invalid texture {texturePath}.");
                        }
                    }
                }
            }

            if (!hasGameOwnedMaterial)
            {
                throw new InvalidOperationException($"{roleId} should use at least one promoted game-owned material.");
            }
        }

        private static void ValidateWeapons(Transform visual, EnemyRoleVisualSpec spec)
        {
            for (int i = 0; i < spec.Weapons.Length; i++)
            {
                Transform weapon = FindDescendant(visual, "RoleWeapon_" + spec.Weapons[i].Name);
                if (weapon == null)
                {
                    throw new InvalidOperationException($"{spec.RoleId} should include weapon {spec.Weapons[i].Name}.");
                }

                ValidatePrefabSourcePath(weapon.gameObject, spec.Weapons[i].TargetModelPath);
                ValidateRendererAssets(weapon.gameObject, spec.RoleId + "/" + spec.Weapons[i].Name);
            }
        }

        private static void ValidateCombatReferences(GameObject root, Transform visual, Animator animator, string roleId)
        {
            BasicSoldierEnemy soldier = RequireComponent<BasicSoldierEnemy>(root, roleId);
            EnemyAttackTelegraphPresenter telegraphPresenter = RequireComponent<EnemyAttackTelegraphPresenter>(root, roleId);
            CombatHitFeedback hitFeedback = RequireComponent<CombatHitFeedback>(root, roleId);
            ValidateObjectReference(soldier, "animator", animator);
            ValidateObjectReference(telegraphPresenter, "poseRoot", visual);

            SerializedProperty flashRenderers = RequireProperty(new SerializedObject(hitFeedback), "flashRenderers");
            if (flashRenderers.arraySize == 0)
            {
                throw new InvalidOperationException($"{roleId} hit feedback should reference promoted visual renderers.");
            }
        }

        private static void ValidatePrefabSourcePath(GameObject instance, string expectedPath)
        {
            GameObject source = PrefabUtility.GetCorrespondingObjectFromSource(instance);
            string sourcePath = AssetDatabase.GetAssetPath(source).Replace('\\', '/');
            if (!string.Equals(sourcePath, expectedPath, StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"{instance.name} should come from {expectedPath}, found {sourcePath}.");
            }
        }

        private static Transform FindDescendant(Transform root, string childName)
        {
            if (root.name == childName)
            {
                return root;
            }

            for (int i = 0; i < root.childCount; i++)
            {
                Transform result = FindDescendant(root.GetChild(i), childName);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        private static Avatar LoadAvatar(string modelPath)
        {
            Avatar avatar = AssetDatabase.LoadAllAssetsAtPath(modelPath).OfType<Avatar>().FirstOrDefault();
            if (avatar == null)
            {
                throw new InvalidOperationException($"Missing humanoid avatar at {modelPath}.");
            }

            return avatar;
        }

        private static void ClearParameters(AnimatorController controller)
        {
            for (int i = controller.parameters.Length - 1; i >= 0; i--)
            {
                controller.RemoveParameter(controller.parameters[i]);
            }
        }

        private static void ClearStateMachine(AnimatorStateMachine stateMachine)
        {
            for (int i = stateMachine.anyStateTransitions.Length - 1; i >= 0; i--)
            {
                stateMachine.RemoveAnyStateTransition(stateMachine.anyStateTransitions[i]);
            }

            for (int i = stateMachine.states.Length - 1; i >= 0; i--)
            {
                stateMachine.RemoveState(stateMachine.states[i].state);
            }
        }

        private static void AddMoveTransition(AnimatorState from, AnimatorState to, AnimatorConditionMode mode, float threshold)
        {
            AnimatorStateTransition transition = from.AddTransition(to);
            transition.hasExitTime = false;
            transition.duration = 0.12f;
            transition.AddCondition(mode, threshold, "MoveSpeed");
        }

        private static void AddAnyTriggerTransition(AnimatorStateMachine stateMachine, AnimatorState destination, string trigger, float duration)
        {
            AnimatorStateTransition transition = stateMachine.AddAnyStateTransition(destination);
            transition.hasExitTime = false;
            transition.canTransitionToSelf = false;
            transition.duration = duration;
            transition.AddCondition(AnimatorConditionMode.If, 0f, trigger);
        }

        private static void AddExitTransition(AnimatorState from, AnimatorState to, float exitTime, float duration)
        {
            AnimatorStateTransition transition = from.AddTransition(to);
            transition.hasExitTime = true;
            transition.exitTime = exitTime;
            transition.duration = duration;
        }

        private static T RequireComponent<T>(GameObject root, string label) where T : Component
        {
            T component = root.GetComponent<T>();
            if (component == null)
            {
                throw new InvalidOperationException($"{label} is missing required component {typeof(T).Name}.");
            }

            return component;
        }

        private static void SetObjectReference(UnityEngine.Object target, string propertyName, UnityEngine.Object value)
        {
            var serializedObject = new SerializedObject(target);
            RequireProperty(serializedObject, propertyName).objectReferenceValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }

        private static void SetObjectReferenceArray(UnityEngine.Object target, string propertyName, UnityEngine.Object[] values)
        {
            var serializedObject = new SerializedObject(target);
            SerializedProperty array = RequireProperty(serializedObject, propertyName);
            array.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++)
            {
                array.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            }

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }

        private static void SetBool(UnityEngine.Object target, string propertyName, bool value)
        {
            var serializedObject = new SerializedObject(target);
            RequireProperty(serializedObject, propertyName).boolValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }

        private static void ValidateObjectReference(UnityEngine.Object target, string propertyName, UnityEngine.Object expected)
        {
            UnityEngine.Object actual = RequireProperty(new SerializedObject(target), propertyName).objectReferenceValue;
            if (actual != expected)
            {
                string expectedName = expected != null ? expected.name : "null";
                string actualName = actual != null ? actual.name : "null";
                throw new InvalidOperationException($"{target.name}.{propertyName} expected {expectedName}, found {actualName}.");
            }
        }

        private static SerializedProperty RequireProperty(SerializedObject serializedObject, string propertyName)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null)
            {
                throw new InvalidOperationException($"{serializedObject.targetObject.name} is missing serialized property {propertyName}.");
            }

            return property;
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

        private static string PathParent(string path)
        {
            int separatorIndex = path.LastIndexOf('/');
            return path.Substring(0, separatorIndex);
        }

        private static string SanitizeAssetName(string value)
        {
            char[] chars = value.ToCharArray();
            for (int i = 0; i < chars.Length; i++)
            {
                if (!char.IsLetterOrDigit(chars[i]) && chars[i] != '_' && chars[i] != '-')
                {
                    chars[i] = '_';
                }
            }

            return new string(chars);
        }

        private enum TelegraphStyle
        {
            SmallRead,
            Guard,
            Lunge,
            Line,
            Fan,
            EliteGuard,
            EliteAura,
            EliteLine,
            FinalStand
        }

        private enum TextureUsage
        {
            Color,
            Linear,
            Normal
        }
    }
}
