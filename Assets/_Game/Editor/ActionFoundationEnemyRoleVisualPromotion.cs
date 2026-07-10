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
        private static void EnsureRoleVisualAssets(EnemyRoleVisualSpec spec)
        {
            EnsureFolder(PathParent(spec.TargetModelPath));
            EnsureFolder(spec.MaterialRoot);
            EnsureFolder(spec.TextureRoot);
            EnsureFolder(spec.AnimationRoot);
            EnsureFolder(PromotedWeaponRoot);

            if (AssetDatabase.LoadAssetAtPath<GameObject>(spec.TargetModelPath) == null &&
                !AssetDatabase.CopyAsset(spec.SourceModelPath, spec.TargetModelPath))
            {
                throw new InvalidOperationException($"Failed to promote {spec.RoleTag} model from {spec.SourceModelPath} to {spec.TargetModelPath}.");
            }

            ConfigureModelImporter(spec.TargetModelPath);
            Avatar avatar = LoadAvatar(spec.TargetModelPath);
            for (int i = 0; i < spec.Clips.Length; i++)
            {
                PromoteAndConfigureClip(spec, spec.Clips[i], avatar);
            }

            for (int i = 0; i < spec.Weapons.Length; i++)
            {
                PromoteWeapon(spec.Weapons[i]);
            }

            BuildAnimatorController(spec);
        }

        private static void PromoteAndConfigureClip(EnemyRoleVisualSpec spec, RoleAnimationClipSpec clip, Avatar avatar)
        {
            string targetPath = ClipTargetPath(spec, clip);
            if (AssetDatabase.LoadAssetAtPath<AnimationClip>(targetPath) == null)
            {
                string sourcePath = $"{clip.SourceRoot}/{clip.SourceFileName}";
                if (!AssetDatabase.CopyAsset(sourcePath, targetPath))
                {
                    throw new InvalidOperationException($"Failed to promote {spec.RoleTag} animation from {sourcePath} to {targetPath}.");
                }
            }

            ConfigureAnimationImporter(targetPath, clip.TargetClipName, clip.LoopTime, avatar, clip.HeightFromFeet);
        }

        private static void PromoteWeapon(RoleWeaponSpec weapon)
        {
            EnsureFolder(PathParent(weapon.TargetModelPath));
            EnsureFolder(weapon.TargetMaterialRoot);
            EnsureFolder(weapon.TargetTextureRoot);
            if (AssetDatabase.LoadAssetAtPath<GameObject>(weapon.TargetModelPath) == null &&
                !AssetDatabase.CopyAsset(weapon.SourceModelPath, weapon.TargetModelPath))
            {
                throw new InvalidOperationException($"Failed to promote weapon {weapon.Name} from {weapon.SourceModelPath} to {weapon.TargetModelPath}.");
            }

            ConfigureWeaponModelImporter(weapon.TargetModelPath);
        }

        private static void ConfigureModelImporter(string path)
        {
            ModelImporter importer = AssetImporter.GetAtPath(path) as ModelImporter;
            if (importer == null)
            {
                throw new InvalidOperationException($"Missing model importer at {path}.");
            }

            importer.animationType = ModelImporterAnimationType.Human;
            importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
            importer.importAnimation = false;
            importer.materialImportMode = ModelImporterMaterialImportMode.None;
            importer.SaveAndReimport();
        }

        private static void ConfigureWeaponModelImporter(string path)
        {
            ModelImporter importer = AssetImporter.GetAtPath(path) as ModelImporter;
            if (importer == null)
            {
                throw new InvalidOperationException($"Missing weapon importer at {path}.");
            }

            importer.importAnimation = false;
            importer.materialImportMode = ModelImporterMaterialImportMode.None;
            importer.SaveAndReimport();
        }

        private static void ConfigureAnimationImporter(
            string path,
            string clipName,
            bool loopTime,
            Avatar avatar,
            bool heightFromFeet)
        {
            ModelImporter importer = AssetImporter.GetAtPath(path) as ModelImporter;
            if (importer == null)
            {
                throw new InvalidOperationException($"Missing animation importer at {path}.");
            }

            importer.animationType = ModelImporterAnimationType.Human;
            importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
            importer.sourceAvatar = null;
            importer.importAnimation = true;

            ModelImporterClipAnimation[] clips = importer.defaultClipAnimations;
            if (clips.Length == 0)
            {
                clips = importer.clipAnimations;
            }

            if (clips.Length == 0)
            {
                throw new InvalidOperationException($"{path} has no imported clips.");
            }

            for (int i = 0; i < clips.Length; i++)
            {
                clips[i].name = clipName;
                clips[i].loopTime = loopTime;
                clips[i].keepOriginalOrientation = true;
                clips[i].keepOriginalPositionY = !heightFromFeet;
                clips[i].keepOriginalPositionXZ = true;
                clips[i].heightFromFeet = heightFromFeet;
            }

            importer.clipAnimations = clips;
            importer.SaveAndReimport();
        }

        private static AnimatorController BuildAnimatorController(EnemyRoleVisualSpec spec)
        {
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(spec.ControllerPath);
            if (controller == null)
            {
                controller = AnimatorController.CreateAnimatorControllerAtPath(spec.ControllerPath);
            }

            ClearParameters(controller);
            AddParameterSet(controller);

            AnimatorStateMachine stateMachine = GetOrCreateBaseStateMachine(controller);
            ClearStateMachine(stateMachine);

            Dictionary<string, AnimatorState> states = new Dictionary<string, AnimatorState>();
            states["Idle"] = AddState(stateMachine, spec, "Idle", new Vector3(250f, 80f, 0f));
            states["Run"] = AddState(stateMachine, spec, "Run", new Vector3(250f, 170f, 0f));
            states["Attack"] = AddState(stateMachine, spec, "Attack", new Vector3(520f, 80f, 0f));
            states["AttackCombo2"] = AddState(stateMachine, spec, "AttackCombo2", new Vector3(520f, 170f, 0f));
            states["AttackCombo3"] = AddState(stateMachine, spec, "AttackCombo3", new Vector3(520f, 260f, 0f));
            states["AttackHeavy"] = AddState(stateMachine, spec, "AttackHeavy", new Vector3(520f, 350f, 0f));
            states["AttackLinePressure"] = AddState(stateMachine, spec, "AttackLinePressure", new Vector3(800f, 80f, 0f));
            states["AttackFanPressure"] = AddState(stateMachine, spec, "AttackFanPressure", new Vector3(800f, 170f, 0f));
            states["AttackRetreatShot"] = AddState(stateMachine, spec, "AttackRetreatShot", new Vector3(800f, 260f, 0f));
            states["AttackRetreatBlink"] = AddState(stateMachine, spec, "AttackRetreatBlink", new Vector3(800f, 350f, 0f));
            states["AttackGuardBreak"] = AddState(stateMachine, spec, "AttackGuardBreak", new Vector3(800f, 440f, 0f));
            states["RetreatBackstep"] = AddState(stateMachine, spec, "RetreatBackstep", new Vector3(1080f, 80f, 0f));
            states["RetreatBlink"] = AddState(stateMachine, spec, "RetreatBlink", new Vector3(1080f, 170f, 0f));
            states["EliteShieldCycle"] = AddState(stateMachine, spec, "EliteShieldCycle", new Vector3(1080f, 260f, 0f));
            states["EliteArmorBreak"] = AddState(stateMachine, spec, "EliteArmorBreak", new Vector3(1080f, 350f, 0f));
            states["EliteAuraBuffer"] = AddState(stateMachine, spec, "EliteAuraBuffer", new Vector3(1080f, 440f, 0f));
            states["EliteSummonPackage"] = AddState(stateMachine, spec, "EliteSummonPackage", new Vector3(1360f, 80f, 0f));
            states["ElitePhaseSwap"] = AddState(stateMachine, spec, "ElitePhaseSwap", new Vector3(1360f, 170f, 0f));
            states["Hit"] = AddState(stateMachine, spec, "Hit", new Vector3(1360f, 260f, 0f));
            states["HitHeavy"] = AddState(stateMachine, spec, "HitHeavy", new Vector3(1360f, 350f, 0f));
            states["Death"] = AddState(stateMachine, spec, "Death", new Vector3(1360f, 440f, 0f));
            stateMachine.defaultState = states["Idle"];

            AddMoveTransition(states["Idle"], states["Run"], AnimatorConditionMode.Greater, 0.1f);
            AddMoveTransition(states["Run"], states["Idle"], AnimatorConditionMode.Less, 0.1f);
            AddAnyTriggerTransition(stateMachine, states["Death"], "Death", 0.05f);
            AddAnyTriggerTransition(stateMachine, states["Hit"], "Hit", 0.03f);
            AddAnyTriggerTransition(stateMachine, states["HitHeavy"], "HitHeavy", 0.03f);
            AddTriggerTransitions(stateMachine, states);
            AddExitTransitions(states);

            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssets();
            return controller;
        }

        private static AnimatorStateMachine GetOrCreateBaseStateMachine(AnimatorController controller)
        {
            if (controller.layers.Length == 0)
            {
                controller.AddLayer("Base Layer");
            }

            AnimatorControllerLayer[] layers = controller.layers;
            if (layers[0].stateMachine == null)
            {
                var stateMachine = new AnimatorStateMachine
                {
                    name = layers[0].name
                };
                AssetDatabase.AddObjectToAsset(stateMachine, controller);
                layers[0].stateMachine = stateMachine;
                controller.layers = layers;
                EditorUtility.SetDirty(controller);
            }

            return controller.layers[0].stateMachine;
        }

        private static void AddParameterSet(AnimatorController controller)
        {
            controller.AddParameter("MoveSpeed", AnimatorControllerParameterType.Float);
            string[] triggers =
            {
                "Attack",
                "AttackCombo2",
                "AttackCombo3",
                "AttackHeavy",
                "AttackLinePressure",
                "AttackFanPressure",
                "AttackRetreatShot",
                "AttackRetreatBlink",
                "AttackGuardBreak",
                "RetreatBackstep",
                "RetreatBlink",
                "EliteShieldCycle",
                "EliteArmorBreak",
                "EliteAuraBuffer",
                "EliteSummonPackage",
                "ElitePhaseSwap",
                "Hit",
                "HitHeavy",
                "Death"
            };

            for (int i = 0; i < triggers.Length; i++)
            {
                controller.AddParameter(triggers[i], AnimatorControllerParameterType.Trigger);
            }
        }

        private static void AddTriggerTransitions(AnimatorStateMachine stateMachine, Dictionary<string, AnimatorState> states)
        {
            string[] triggerNames =
            {
                "Attack",
                "AttackCombo2",
                "AttackCombo3",
                "AttackHeavy",
                "AttackLinePressure",
                "AttackFanPressure",
                "AttackRetreatShot",
                "AttackRetreatBlink",
                "AttackGuardBreak",
                "RetreatBackstep",
                "RetreatBlink",
                "EliteShieldCycle",
                "EliteArmorBreak",
                "EliteAuraBuffer",
                "EliteSummonPackage",
                "ElitePhaseSwap"
            };

            for (int i = 0; i < triggerNames.Length; i++)
            {
                AddAnyTriggerTransition(stateMachine, states[triggerNames[i]], triggerNames[i], 0.05f);
            }
        }

        private static void AddExitTransitions(Dictionary<string, AnimatorState> states)
        {
            foreach (KeyValuePair<string, AnimatorState> pair in states)
            {
                if (pair.Key == "Idle" || pair.Key == "Run" || pair.Key == "Death")
                {
                    continue;
                }

                AddExitTransition(pair.Value, states["Idle"], pair.Key.StartsWith("Retreat", StringComparison.Ordinal) ? 0.74f : 0.84f, 0.07f);
            }
        }

        private static GameObject RecreateVisual(Transform parent, EnemyRoleVisualSpec spec)
        {
            GameObject model = AssetDatabase.LoadAssetAtPath<GameObject>(spec.TargetModelPath);
            if (model == null)
            {
                throw new InvalidOperationException($"Missing promoted role model at {spec.TargetModelPath}.");
            }

            GameObject visual = PrefabUtility.InstantiatePrefab(model, parent) as GameObject;
            if (visual == null)
            {
                throw new InvalidOperationException($"Failed to instantiate {spec.RoleTag} visual.");
            }

            visual.name = spec.VisualName;
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localRotation = Quaternion.identity;
            visual.transform.localScale = spec.VisualScale;
            return visual;
        }

        internal static void ReapplyPromotedMaterials(GameObject visual, EnemyRoleVisualSpec spec)
        {
            Dictionary<string, Queue<Renderer>> sourceRenderersByName = LoadSourceRenderersByName(spec.SourcePrefabPath);
            Material[] defaultMaterials = PromoteDefaultMaterials(spec);
            Renderer[] renderers = visual.GetComponentsInChildren<Renderer>(true);
            int promotedSlotCount = 0;

            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                Material[] materials = null;
                if (sourceRenderersByName.TryGetValue(renderer.name, out Queue<Renderer> queue) && queue.Count > 0)
                {
                    materials = PromoteMaterials(queue.Dequeue().sharedMaterials, spec.MaterialRoot, spec.TextureRoot);
                }
                else if (defaultMaterials.Length > 0)
                {
                    int slotCount = Mathf.Max(1, renderer.sharedMaterials.Length);
                    materials = new Material[slotCount];
                    for (int j = 0; j < materials.Length; j++)
                    {
                        materials[j] = defaultMaterials[Mathf.Min(j, defaultMaterials.Length - 1)];
                    }
                }

                if (materials == null || materials.Length == 0)
                {
                    continue;
                }

                renderer.gameObject.SetActive(true);
                renderer.enabled = true;
                renderer.sharedMaterials = materials;
                promotedSlotCount += materials.Length;
                EditorUtility.SetDirty(renderer.gameObject);
                EditorUtility.SetDirty(renderer);
            }

            if (promotedSlotCount == 0)
            {
                throw new InvalidOperationException($"{spec.RoleTag} did not receive promoted game-owned materials.");
            }
        }

        private static Dictionary<string, Queue<Renderer>> LoadSourceRenderersByName(string sourcePrefabPath)
        {
            GameObject sourcePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(sourcePrefabPath);
            if (sourcePrefab == null)
            {
                return new Dictionary<string, Queue<Renderer>>();
            }

            return sourcePrefab
                .GetComponentsInChildren<Renderer>(true)
                .GroupBy(renderer => renderer.name)
                .ToDictionary(group => group.Key, group => new Queue<Renderer>(group));
        }

        private static Material[] PromoteDefaultMaterials(EnemyRoleVisualSpec spec)
        {
            var materials = new List<Material>();
            for (int i = 0; i < spec.DefaultMaterialPaths.Length; i++)
            {
                Material source = AssetDatabase.LoadAssetAtPath<Material>(spec.DefaultMaterialPaths[i]);
                if (source != null)
                {
                    materials.Add(PromoteMaterial(source, spec.MaterialRoot, spec.TextureRoot));
                }
            }

            return materials.ToArray();
        }

        private static void AttachWeapons(GameObject visual, EnemyRoleVisualSpec spec)
        {
            for (int i = 0; i < spec.Weapons.Length; i++)
            {
                RoleWeaponSpec weapon = spec.Weapons[i];
                Transform socket = FindDescendant(visual.transform, weapon.SocketName);
                if (socket == null)
                {
                    throw new InvalidOperationException($"{spec.RoleTag} visual is missing weapon socket {weapon.SocketName}.");
                }

                GameObject weaponAsset = AssetDatabase.LoadAssetAtPath<GameObject>(weapon.TargetModelPath);
                if (weaponAsset == null)
                {
                    throw new InvalidOperationException($"Missing promoted weapon model at {weapon.TargetModelPath}.");
                }

                GameObject weaponInstance = PrefabUtility.InstantiatePrefab(weaponAsset, socket) as GameObject;
                if (weaponInstance == null)
                {
                    throw new InvalidOperationException($"Failed to instantiate weapon {weapon.Name} for {spec.RoleTag}.");
                }

                weaponInstance.name = "RoleWeapon_" + weapon.Name;
                weaponInstance.transform.localPosition = Vector3.zero;
                weaponInstance.transform.localRotation = Quaternion.identity;
                weaponInstance.transform.localScale = Vector3.one;
                AssignWeaponMaterial(weaponInstance, weapon);
            }
        }

        private static void AssignWeaponMaterial(GameObject weaponInstance, RoleWeaponSpec weapon)
        {
            Material source = AssetDatabase.LoadAssetAtPath<Material>(weapon.SourceMaterialPath);
            if (source == null)
            {
                throw new InvalidOperationException($"Missing weapon material at {weapon.SourceMaterialPath}.");
            }

            Material material = PromoteMaterial(source, weapon.TargetMaterialRoot, weapon.TargetTextureRoot);
            Renderer[] renderers = weaponInstance.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                Material[] materials = renderers[i].sharedMaterials;
                if (materials.Length == 0)
                {
                    materials = new Material[1];
                }

                for (int j = 0; j < materials.Length; j++)
                {
                    materials[j] = material;
                }

                renderers[i].sharedMaterials = materials;
                EditorUtility.SetDirty(renderers[i]);
            }
        }

        private static Animator EnsureAnimator(GameObject visual, EnemyRoleVisualSpec spec)
        {
            Animator animator = visual.GetComponent<Animator>();
            if (animator == null)
            {
                animator = visual.AddComponent<Animator>();
            }

            animator.avatar = LoadAvatar(spec.TargetModelPath);
            animator.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<AnimatorController>(spec.ControllerPath);
            animator.applyRootMotion = false;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            EditorUtility.SetDirty(animator);
            return animator;
        }

        private static void ConfigureTelegraph(EnemyAttackTelegraphPresenter presenter, EnemyRoleVisualSpec spec)
        {
            RoleTelegraphSpec telegraph = spec.Telegraph;
            presenter.ConfigureStyle(
                telegraph.WindupStartScale,
                telegraph.WindupEndScale,
                telegraph.ActiveScale,
                new Vector3(0f, 0f, -0.08f),
                new Vector3(0f, 0f, 0.14f),
                telegraph.WindupStartColor,
                telegraph.WindupEndColor,
                telegraph.ActiveColor);
            EditorUtility.SetDirty(presenter);
        }

        private static void ConfigureElitePresentation(GameObject root, Animator animator, Renderer cueRenderer, EnemyRoleVisualSpec spec)
        {
            EnemyElitePatternController eliteController = root.GetComponent<EnemyElitePatternController>();
            if (eliteController == null)
            {
                return;
            }

            SetObjectReference(eliteController, "animator", animator);
            SetObjectReference(eliteController, "cueRenderer", cueRenderer);
            if (spec.CreateSummonIntentAnchor)
            {
                Transform existing = root.transform.Find(SummonSignalName);
                if (existing == null)
                {
                    var signal = new GameObject(SummonSignalName);
                    signal.transform.SetParent(root.transform, worldPositionStays: false);
                    signal.transform.localPosition = new Vector3(0f, 0.08f, -0.6f);
                    signal.transform.localRotation = Quaternion.identity;
                    signal.transform.localScale = Vector3.one;
                    signal.SetActive(false);
                    existing = signal.transform;
                }

                SetObjectReferenceArray(eliteController, "summonSignalObjects", new UnityEngine.Object[] { existing.gameObject });
            }
            else
            {
                Transform existing = root.transform.Find(SummonSignalName);
                if (existing != null)
                {
                    UnityEngine.Object.DestroyImmediate(existing.gameObject);
                }

                SetObjectReferenceArray(eliteController, "summonSignalObjects", Array.Empty<UnityEngine.Object>());
            }
        }

        private static void ValidateElitePresentation(GameObject root, string roleId, EnemyRoleVisualSpec spec)
        {
            EnemyElitePatternController eliteController = root.GetComponent<EnemyElitePatternController>();
            if (eliteController == null)
            {
                return;
            }

            SerializedProperty signals = RequireProperty(new SerializedObject(eliteController), "summonSignalObjects");
            int expectedSignalCount = spec.CreateSummonIntentAnchor ? 1 : 0;
            if (signals.arraySize != expectedSignalCount)
            {
                throw new InvalidOperationException($"{roleId} should bind {expectedSignalCount} summon intent anchors, found {signals.arraySize}.");
            }
        }
    }
}
