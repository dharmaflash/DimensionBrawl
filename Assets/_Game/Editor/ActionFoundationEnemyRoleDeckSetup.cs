using System;
using System.Collections.Generic;
using DimensionBrawl.AI;
using UnityEditor;
using UnityEngine;

namespace DimensionBrawl.Editor
{
    public static class ActionFoundationEnemyRoleDeckSetup
    {
        public const string RoleRoot = ActionFoundationProfileSetup.ProfileRoot + "/EnemyRoles";

        public const string EntryProbeDeckPath = RoleRoot + "/DB_RoleDeck_EntryProbe.asset";
        public const string BreakGateBruiserDeckPath = RoleRoot + "/DB_RoleDeck_BreakGateBruiser.asset";
        public const string BacklineShooterDeckPath = RoleRoot + "/DB_RoleDeck_BacklineShooter.asset";
        public const string FanSuppressorDeckPath = RoleRoot + "/DB_RoleDeck_FanSuppressor.asset";
        public const string LineCasterDeckPath = RoleRoot + "/DB_RoleDeck_LineCaster.asset";
        public const string SkirmisherDeckPath = RoleRoot + "/DB_RoleDeck_Skirmisher.asset";
        public const string BreakHandoffDeckPath = RoleRoot + "/DB_RoleDeck_BreakHandoff.asset";
        public const string FinalStandDeckPath = RoleRoot + "/DB_RoleDeck_FinalStand.asset";

        public const string EntryProbeRolePath = RoleRoot + "/DB_Role_EntryProbe.asset";
        public const string CloseGuardRolePath = RoleRoot + "/DB_Role_CloseGuard.asset";
        public const string LungeChaserRolePath = RoleRoot + "/DB_Role_LungeChaser.asset";
        public const string LineCasterRolePath = RoleRoot + "/DB_Role_LineCaster.asset";
        public const string FanSuppressorRolePath = RoleRoot + "/DB_Role_FanSuppressor.asset";
        public const string BacklineShooterRolePath = RoleRoot + "/DB_Role_BacklineShooter.asset";
        public const string SkirmisherRolePath = RoleRoot + "/DB_Role_Skirmisher.asset";
        public const string ShieldBreakerEliteRolePath = RoleRoot + "/DB_Role_ShieldBreakerElite.asset";
        public const string AuraCaptainEliteRolePath = RoleRoot + "/DB_Role_AuraCaptainElite.asset";
        public const string SummonCallerEliteRolePath = RoleRoot + "/DB_Role_SummonCallerElite.asset";
        public const string PhaseDuelistEliteRolePath = RoleRoot + "/DB_Role_PhaseDuelistElite.asset";
        public const string FinalStandCommanderEliteRolePath = RoleRoot + "/DB_Role_FinalStandCommanderElite.asset";

        private static readonly string[] RoleProfilePaths =
        {
            EntryProbeRolePath,
            CloseGuardRolePath,
            LungeChaserRolePath,
            LineCasterRolePath,
            FanSuppressorRolePath,
            BacklineShooterRolePath,
            SkirmisherRolePath,
            ShieldBreakerEliteRolePath,
            AuraCaptainEliteRolePath,
            SummonCallerEliteRolePath,
            PhaseDuelistEliteRolePath,
            FinalStandCommanderEliteRolePath
        };

        [MenuItem("DimensionBrawl/Reapply Action Foundation Enemy Role Decks")]
        public static void ReapplyEnemyRoleDecksMenu()
        {
            EnsureEnemyRoleAssets();
            Debug.Log("Reapplied ActionFoundation enemy role deck assets.");
        }

        [MenuItem("DimensionBrawl/Validate Action Foundation Enemy Role Decks")]
        public static void ValidateEnemyRoleDecksMenu()
        {
            ValidateEnemyRoleAssets();
            Debug.Log("Action foundation enemy role deck validation passed.");
        }

        public static void EnsureEnemyRoleAssets()
        {
            ActionFoundationEnemyPatternExpansionSetup.EnsureExtendedPatternAssets();
            EnsureFolder(RoleRoot);

            PatternRefs patterns = LoadPatternRefs();
            EliteRefs elites = LoadEliteRefs();

            RoleDeckRefs decks = EnsureRoleDeckAssets(patterns);
            EnsureRoleProfileAssets(patterns, elites, decks);

            AssetDatabase.SaveAssets();
        }

        public static void ValidateEnemyRoleAssets()
        {
            var coveredSegments = new HashSet<CombatEnemyRunSegment>();
            int eliteCount = 0;
            int generalCount = 0;

            for (int i = 0; i < RoleProfilePaths.Length; i++)
            {
                CombatEnemyRoleProfile role = AssetDatabase.LoadAssetAtPath<CombatEnemyRoleProfile>(RoleProfilePaths[i]);
                if (role == null)
                {
                    throw new InvalidOperationException($"Missing enemy role profile at {RoleProfilePaths[i]}.");
                }

                if (string.IsNullOrWhiteSpace(role.RoleId))
                {
                    throw new InvalidOperationException($"{RoleProfilePaths[i]} has no role id.");
                }

                if (role.StartingPattern == null)
                {
                    throw new InvalidOperationException($"{role.RoleId} has no starting pattern.");
                }

                if (role.PatternDeck == null || role.PatternDeck.EntryCount == 0)
                {
                    throw new InvalidOperationException($"{role.RoleId} has no usable pattern deck.");
                }

                ValidateDeckEntries(role);
                coveredSegments.Add(role.PreferredSegment);

                if (role.EliteRole)
                {
                    eliteCount++;
                    if (role.EliteProfileCount == 0)
                    {
                        throw new InvalidOperationException($"{role.RoleId} is an elite role but has no elite profiles.");
                    }
                }
                else
                {
                    generalCount++;
                    if (role.EliteProfileCount != 0)
                    {
                        throw new InvalidOperationException($"{role.RoleId} is a general role but has elite profiles assigned.");
                    }
                }
            }

            foreach (CombatEnemyRunSegment segment in Enum.GetValues(typeof(CombatEnemyRunSegment)))
            {
                if (!coveredSegments.Contains(segment))
                {
                    throw new InvalidOperationException($"Enemy role catalog does not cover linear run segment {segment}.");
                }
            }

            if (generalCount < 7 || eliteCount < 5)
            {
                throw new InvalidOperationException($"Expected at least 7 general and 5 elite role profiles, found {generalCount} general and {eliteCount} elite.");
            }
        }

        private static RoleDeckRefs EnsureRoleDeckAssets(PatternRefs patterns)
        {
            RoleDeckRefs decks = new RoleDeckRefs
            {
                EntryProbe = LoadOrCreate<CombatAiPatternDeck>(EntryProbeDeckPath),
                BreakGateBruiser = LoadOrCreate<CombatAiPatternDeck>(BreakGateBruiserDeckPath),
                BacklineShooter = LoadOrCreate<CombatAiPatternDeck>(BacklineShooterDeckPath),
                FanSuppressor = LoadOrCreate<CombatAiPatternDeck>(FanSuppressorDeckPath),
                LineCaster = LoadOrCreate<CombatAiPatternDeck>(LineCasterDeckPath),
                Skirmisher = LoadOrCreate<CombatAiPatternDeck>(SkirmisherDeckPath),
                BreakHandoff = LoadOrCreate<CombatAiPatternDeck>(BreakHandoffDeckPath),
                FinalStand = LoadOrCreate<CombatAiPatternDeck>(FinalStandDeckPath)
            };

            ConfigureCombatAiPatternDeck(
                decks.EntryProbe,
                "SciFiSoldier.Role.EntryProbe",
                new[]
                {
                    CreateCombatAiPatternDeckEntry(patterns.ClosePunish, 0f, 1.85f, 0.70f, 4.2f),
                    CreateCombatAiPatternDeckEntry(patterns.LungeStrike, 1.25f, 3.25f, 0.95f, 3.2f),
                    CreateCombatAiPatternDeckEntry(patterns.LinePressure, 3.0f, 6.6f, 1.45f, 1.4f)
                });

            ConfigureCombatAiPatternDeck(
                decks.BreakGateBruiser,
                "SciFiSoldier.Role.BreakGateBruiser",
                new[]
                {
                    CreateCombatAiPatternDeckEntry(patterns.HeavyWindup, 0.75f, 2.9f, 1.20f, 4.4f),
                    CreateCombatAiPatternDeckEntry(patterns.ClosePunish, 0f, 1.9f, 0.75f, 3.8f),
                    CreateCombatAiPatternDeckEntry(patterns.LungeStrike, 1.4f, 3.4f, 1.05f, 3.0f),
                    CreateCombatAiPatternDeckEntry(patterns.LinePressure, 3.2f, 7.0f, 1.30f, 1.8f)
                });

            ConfigureCombatAiPatternDeck(
                decks.BacklineShooter,
                "SciFiSoldier.Role.BacklineShooter",
                new[]
                {
                    CreateCombatAiPatternDeckEntry(patterns.RetreatShot, 0f, 5.8f, 0.95f, 4.5f),
                    CreateCombatAiPatternDeckEntry(patterns.LinePressure, 3.0f, 7.6f, 1.10f, 3.4f),
                    CreateCombatAiPatternDeckEntry(patterns.RetreatBlink, 0f, 3.4f, 1.80f, 3.0f),
                    CreateCombatAiPatternDeckEntry(patterns.FanPressure, 2.2f, 5.6f, 1.35f, 2.3f)
                });

            ConfigureCombatAiPatternDeck(
                decks.FanSuppressor,
                "SciFiSoldier.Role.FanSuppressor",
                new[]
                {
                    CreateCombatAiPatternDeckEntry(patterns.FanPressure, 1.8f, 5.4f, 0.90f, 4.2f),
                    CreateCombatAiPatternDeckEntry(patterns.LinePressure, 3.0f, 7.2f, 1.10f, 3.2f),
                    CreateCombatAiPatternDeckEntry(patterns.LungeStrike, 1.2f, 3.2f, 1.00f, 2.4f),
                    CreateCombatAiPatternDeckEntry(patterns.ClosePunish, 0f, 1.85f, 0.70f, 2.0f)
                });

            ConfigureCombatAiPatternDeck(
                decks.LineCaster,
                "SciFiSoldier.Role.LineCaster",
                new[]
                {
                    CreateCombatAiPatternDeckEntry(patterns.LinePressure, 2.8f, 7.8f, 0.95f, 4.5f),
                    CreateCombatAiPatternDeckEntry(patterns.RetreatShot, 1.8f, 5.8f, 1.15f, 3.1f),
                    CreateCombatAiPatternDeckEntry(patterns.FanPressure, 2.0f, 5.4f, 1.35f, 2.3f),
                    CreateCombatAiPatternDeckEntry(patterns.ClosePunish, 0f, 1.75f, 0.85f, 1.7f)
                });

            ConfigureCombatAiPatternDeck(
                decks.Skirmisher,
                "SciFiSoldier.Role.Skirmisher",
                new[]
                {
                    CreateCombatAiPatternDeckEntry(patterns.RetreatBlink, 0f, 3.5f, 1.25f, 4.0f),
                    CreateCombatAiPatternDeckEntry(patterns.LungeStrike, 1.1f, 3.5f, 0.80f, 3.5f),
                    CreateCombatAiPatternDeckEntry(patterns.RetreatShot, 1.7f, 5.5f, 1.00f, 2.8f),
                    CreateCombatAiPatternDeckEntry(patterns.FanPressure, 2.2f, 5.2f, 1.20f, 2.0f)
                });

            ConfigureCombatAiPatternDeck(
                decks.BreakHandoff,
                "SciFiSoldier.Role.BreakHandoff",
                new[]
                {
                    CreateCombatAiPatternDeckEntry(patterns.GuardBreak, 0f, 2.7f, 1.05f, 4.8f),
                    CreateCombatAiPatternDeckEntry(patterns.HeavyWindup, 0.9f, 3.1f, 1.10f, 4.0f),
                    CreateCombatAiPatternDeckEntry(patterns.RetreatBlink, 0f, 3.7f, 1.45f, 3.1f),
                    CreateCombatAiPatternDeckEntry(patterns.LinePressure, 3.0f, 7.5f, 1.05f, 2.7f)
                });

            ConfigureCombatAiPatternDeck(
                decks.FinalStand,
                "SciFiSoldier.Role.FinalStand",
                new[]
                {
                    CreateCombatAiPatternDeckEntry(patterns.GuardBreak, 0f, 2.7f, 1.05f, 4.7f),
                    CreateCombatAiPatternDeckEntry(patterns.HeavyWindup, 0.9f, 3.1f, 1.05f, 4.1f),
                    CreateCombatAiPatternDeckEntry(patterns.LinePressure, 2.8f, 7.8f, 0.95f, 3.4f),
                    CreateCombatAiPatternDeckEntry(patterns.FanPressure, 1.8f, 5.6f, 0.95f, 3.0f),
                    CreateCombatAiPatternDeckEntry(patterns.RetreatBlink, 0f, 3.6f, 1.35f, 2.8f),
                    CreateCombatAiPatternDeckEntry(patterns.ClosePunish, 0f, 1.8f, 0.75f, 2.2f)
                });

            return decks;
        }

        private static void EnsureRoleProfileAssets(PatternRefs patterns, EliteRefs elites, RoleDeckRefs decks)
        {
            ConfigureRoleProfile(LoadOrCreate<CombatEnemyRoleProfile>(EntryProbeRolePath), "SciFiSoldier.EntryProbe", "Entry Probe", false, CombatEnemyRunSegment.EntryRead, 1, 2, "Teach camera, movement, and the first readable melee/line tell.", "Move, read windup, then dodge.", patterns.ClosePunish, decks.EntryProbe, Array.Empty<CombatAiElitePatternProfile>());
            ConfigureRoleProfile(LoadOrCreate<CombatEnemyRoleProfile>(CloseGuardRolePath), "SciFiSoldier.CloseGuard", "Close Guard", false, CombatEnemyRunSegment.BreakGate, 1, 3, "Hold the near pocket so face-hugging has a readable cost.", "Dodge out, punish recovery, or later call Break.", patterns.ClosePunish, decks.BreakGateBruiser, Array.Empty<CombatAiElitePatternProfile>());
            ConfigureRoleProfile(LoadOrCreate<CombatEnemyRoleProfile>(LungeChaserRolePath), "SciFiSoldier.LungeChaser", "Lunge Chaser", false, CombatEnemyRunSegment.PressureRescue, 1, 2, "Punish passive retreat without forcing a new enemy controller.", "Side dodge the committed lunge, then counter.", patterns.LungeStrike, decks.Skirmisher, Array.Empty<CombatAiElitePatternProfile>());
            ConfigureRoleProfile(LoadOrCreate<CombatEnemyRoleProfile>(LineCasterRolePath), "SciFiSoldier.LineCaster", "Line Caster", false, CombatEnemyRunSegment.Backline, 1, 2, "Create a clean lane threat that teaches side reposition.", "Side dodge or use a future Tank screen.", patterns.LinePressure, decks.LineCaster, Array.Empty<CombatAiElitePatternProfile>());
            ConfigureRoleProfile(LoadOrCreate<CombatEnemyRoleProfile>(FanSuppressorRolePath), "SciFiSoldier.FanSuppressor", "Fan Suppressor", false, CombatEnemyRunSegment.PressureRescue, 1, 2, "Fill mid-range space with a readable cone instead of invisible pressure.", "Reposition forward/back and punish recovery.", patterns.FanPressure, decks.FanSuppressor, Array.Empty<CombatAiElitePatternProfile>());
            ConfigureRoleProfile(LoadOrCreate<CombatEnemyRoleProfile>(BacklineShooterRolePath), "SciFiSoldier.BacklineShooter", "Backline Shooter", false, CombatEnemyRunSegment.Backline, 1, 2, "Force target priority and chase discipline in the backline pocket.", "Close carefully, dodge line fire, or later answer with Arrow.", patterns.RetreatShot, decks.BacklineShooter, Array.Empty<CombatAiElitePatternProfile>());
            ConfigureRoleProfile(LoadOrCreate<CombatEnemyRoleProfile>(SkirmisherRolePath), "SciFiSoldier.Skirmisher", "Skirmisher", false, CombatEnemyRunSegment.PressureRescue, 1, 2, "Keep pressure mobile with retreat-blink and lunge reads.", "Track the blink, avoid overchase, punish committed attack.", patterns.RetreatBlink, decks.Skirmisher, Array.Empty<CombatAiElitePatternProfile>());
            ConfigureRoleProfile(LoadOrCreate<CombatEnemyRoleProfile>(ShieldBreakerEliteRolePath), "SciFiSoldier.Elite.ShieldBreaker", "Shield Breaker Elite", true, CombatEnemyRunSegment.BreakGate, 1, 1, "Make Break feel necessary through guard meter and heavy punish windows.", "Break shield/armor, then attack during recovery.", patterns.GuardBreak, decks.BreakGateBruiser, new[] { elites.ShieldCycle, elites.ArmorBreak });
            ConfigureRoleProfile(LoadOrCreate<CombatEnemyRoleProfile>(AuraCaptainEliteRolePath), "SciFiSoldier.Elite.AuraCaptain", "Aura Captain Elite", true, CombatEnemyRunSegment.Backline, 1, 1, "Create a priority support target without scene-wide ally search.", "Focus or future Arrow answer before clearing protected adds.", patterns.FanPressure, decks.FanSuppressor, new[] { elites.AuraBuffer, elites.ShieldCycle });
            ConfigureRoleProfile(LoadOrCreate<CombatEnemyRoleProfile>(SummonCallerEliteRolePath), "SciFiSoldier.Elite.SummonCaller", "Summon Caller Elite", true, CombatEnemyRunSegment.PressureRescue, 1, 1, "Reserve a summon-package read using signal objects, not runtime spawning.", "Interrupt, clear support, or future Tank/Arrow answer.", patterns.RetreatShot, decks.BacklineShooter, new[] { elites.SummonPackage, elites.AuraBuffer });
            ConfigureRoleProfile(LoadOrCreate<CombatEnemyRoleProfile>(PhaseDuelistEliteRolePath), "SciFiSoldier.Elite.PhaseDuelist", "Phase Duelist Elite", true, CombatEnemyRunSegment.BossBreakHandoff, 1, 1, "Teach a phase/deck swap handoff before a real boss exists.", "Reset position after phase tell and answer the new deck.", patterns.GuardBreak, decks.BreakHandoff, new[] { elites.PhaseSwap, elites.ArmorBreak });
            ConfigureRoleProfile(LoadOrCreate<CombatEnemyRoleProfile>(FinalStandCommanderEliteRolePath), "SciFiSoldier.Elite.FinalStandCommander", "Final Stand Commander Elite", true, CombatEnemyRunSegment.FinalStand, 1, 1, "Combine the most readable heavy, line, fan, and phase pressure for final-stand tuning.", "Use any correct future role, dodge burst patterns, and punish relief.", patterns.GuardBreak, decks.FinalStand, new[] { elites.ShieldCycle, elites.ArmorBreak, elites.PhaseSwap });
        }

        private static void ValidateDeckEntries(CombatEnemyRoleProfile role)
        {
            for (int i = 0; i < role.PatternDeck.EntryCount; i++)
            {
                CombatAiPatternDeckEntry entry = role.PatternDeck.GetEntry(i);
                if (entry.Profile == null)
                {
                    throw new InvalidOperationException($"{role.RoleId} deck entry {i} has no pattern profile.");
                }

                if (entry.MaximumDistance > 0f && entry.MaximumDistance < entry.MinimumDistance)
                {
                    throw new InvalidOperationException($"{role.RoleId} deck entry {i} has an invalid distance band.");
                }
            }
        }

        private static PatternRefs LoadPatternRefs()
        {
            return new PatternRefs
            {
                ClosePunish = LoadPattern(ActionFoundationProfileSetup.EnemyPatternProfilePath),
                LungeStrike = LoadPattern(ActionFoundationProfileSetup.EnemyLungePatternProfilePath),
                HeavyWindup = LoadPattern(ActionFoundationProfileSetup.EnemyHeavyWindupPatternProfilePath),
                LinePressure = LoadPattern(ActionFoundationProfileSetup.EnemyLinePressurePatternProfilePath),
                FanPressure = LoadPattern(ActionFoundationProfileSetup.EnemyFanPressurePatternProfilePath),
                RetreatShot = LoadPattern(ActionFoundationEnemyPatternExpansionSetup.RetreatShotPatternPath),
                RetreatBlink = LoadPattern(ActionFoundationEnemyPatternExpansionSetup.RetreatBlinkPatternPath),
                GuardBreak = LoadPattern(ActionFoundationEnemyPatternExpansionSetup.GuardBreakPatternPath)
            };
        }

        private static EliteRefs LoadEliteRefs()
        {
            return new EliteRefs
            {
                ShieldCycle = LoadElite(ActionFoundationEnemyPatternExpansionSetup.ShieldCycleEliteProfilePath),
                ArmorBreak = LoadElite(ActionFoundationEnemyPatternExpansionSetup.ArmorBreakEliteProfilePath),
                AuraBuffer = LoadElite(ActionFoundationEnemyPatternExpansionSetup.AuraBufferEliteProfilePath),
                SummonPackage = LoadElite(ActionFoundationEnemyPatternExpansionSetup.SummonPackageEliteProfilePath),
                PhaseSwap = LoadElite(ActionFoundationEnemyPatternExpansionSetup.PhaseSwapEliteProfilePath)
            };
        }

        private static void ConfigureCombatAiPatternDeck(
            CombatAiPatternDeck deck,
            string deckId,
            CombatAiPatternDeckEntry[] entries)
        {
            SerializedObject serializedObject = new SerializedObject(deck);
            SetString(serializedObject, "deckId", deckId);
            SerializedProperty entryArray = RequireProperty(serializedObject, "entries");
            entryArray.arraySize = entries.Length;
            for (int i = 0; i < entries.Length; i++)
            {
                SetCombatAiPatternDeckEntry(entryArray.GetArrayElementAtIndex(i), entries[i]);
            }

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(deck);
        }

        private static void ConfigureRoleProfile(
            CombatEnemyRoleProfile role,
            string roleId,
            string displayName,
            bool eliteRole,
            CombatEnemyRunSegment preferredSegment,
            int suggestedMinCount,
            int suggestedMaxCount,
            string pressurePurpose,
            string intendedAnswer,
            CombatAiPatternProfile startingPattern,
            CombatAiPatternDeck patternDeck,
            CombatAiElitePatternProfile[] eliteProfiles)
        {
            SerializedObject serializedObject = new SerializedObject(role);
            SetString(serializedObject, "roleId", roleId);
            SetString(serializedObject, "displayName", displayName);
            SetBool(serializedObject, "eliteRole", eliteRole);
            SetEnum(serializedObject, "preferredSegment", (int)preferredSegment);
            SetInt(serializedObject, "suggestedMinCount", suggestedMinCount);
            SetInt(serializedObject, "suggestedMaxCount", suggestedMaxCount);
            SetString(serializedObject, "pressurePurpose", pressurePurpose);
            SetString(serializedObject, "intendedAnswer", intendedAnswer);
            SetObjectReference(serializedObject, "startingPattern", startingPattern);
            SetObjectReference(serializedObject, "patternDeck", patternDeck);
            SetObjectReferenceArray(serializedObject, "eliteProfiles", eliteProfiles);
            SetBool(serializedObject, "candidateForFutureSummonAiReuse", true);
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(role);
        }

        private static CombatAiPatternDeckEntry CreateCombatAiPatternDeckEntry(
            CombatAiPatternProfile profile,
            float minimumDistance,
            float maximumDistance,
            float cooldownSeconds,
            float priority)
        {
            return new CombatAiPatternDeckEntry(profile, minimumDistance, maximumDistance, cooldownSeconds, priority);
        }

        private static void SetCombatAiPatternDeckEntry(SerializedProperty entry, CombatAiPatternDeckEntry value)
        {
            SetObjectReference(entry, "profile", value.Profile);
            SetRelativeFloat(entry, "minimumDistance", value.MinimumDistance);
            SetRelativeFloat(entry, "maximumDistance", value.MaximumDistance);
            SetRelativeFloat(entry, "cooldownSeconds", value.CooldownSeconds);
            SetRelativeFloat(entry, "priority", value.Priority);
        }

        private static T LoadOrCreate<T>(string assetPath) where T : ScriptableObject
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
            if (asset != null)
            {
                return asset;
            }

            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, assetPath);
            return asset;
        }

        private static CombatAiPatternProfile LoadPattern(string assetPath)
        {
            CombatAiPatternProfile profile = AssetDatabase.LoadAssetAtPath<CombatAiPatternProfile>(assetPath);
            if (profile == null)
            {
                throw new InvalidOperationException($"Missing pattern profile at {assetPath}.");
            }

            return profile;
        }

        private static CombatAiElitePatternProfile LoadElite(string assetPath)
        {
            CombatAiElitePatternProfile profile = AssetDatabase.LoadAssetAtPath<CombatAiElitePatternProfile>(assetPath);
            if (profile == null)
            {
                throw new InvalidOperationException($"Missing elite pattern profile at {assetPath}.");
            }

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

        private static void SetString(SerializedObject serializedObject, string propertyName, string value)
        {
            RequireProperty(serializedObject, propertyName).stringValue = value;
        }

        private static void SetBool(SerializedObject serializedObject, string propertyName, bool value)
        {
            RequireProperty(serializedObject, propertyName).boolValue = value;
        }

        private static void SetInt(SerializedObject serializedObject, string propertyName, int value)
        {
            RequireProperty(serializedObject, propertyName).intValue = value;
        }

        private static void SetEnum(SerializedObject serializedObject, string propertyName, int value)
        {
            RequireProperty(serializedObject, propertyName).enumValueIndex = value;
        }

        private static void SetObjectReference(SerializedObject serializedObject, string propertyName, UnityEngine.Object value)
        {
            RequireProperty(serializedObject, propertyName).objectReferenceValue = value;
        }

        private static void SetObjectReference(SerializedProperty property, string propertyName, UnityEngine.Object value)
        {
            property.FindPropertyRelative(propertyName).objectReferenceValue = value;
        }

        private static void SetObjectReferenceArray(SerializedObject serializedObject, string propertyName, UnityEngine.Object[] values)
        {
            SerializedProperty array = RequireProperty(serializedObject, propertyName);
            array.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++)
            {
                array.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            }
        }

        private static void SetRelativeFloat(SerializedProperty property, string propertyName, float value)
        {
            property.FindPropertyRelative(propertyName).floatValue = value;
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

        private struct PatternRefs
        {
            public CombatAiPatternProfile ClosePunish;
            public CombatAiPatternProfile LungeStrike;
            public CombatAiPatternProfile HeavyWindup;
            public CombatAiPatternProfile LinePressure;
            public CombatAiPatternProfile FanPressure;
            public CombatAiPatternProfile RetreatShot;
            public CombatAiPatternProfile RetreatBlink;
            public CombatAiPatternProfile GuardBreak;
        }

        private struct EliteRefs
        {
            public CombatAiElitePatternProfile ShieldCycle;
            public CombatAiElitePatternProfile ArmorBreak;
            public CombatAiElitePatternProfile AuraBuffer;
            public CombatAiElitePatternProfile SummonPackage;
            public CombatAiElitePatternProfile PhaseSwap;
        }

        private struct RoleDeckRefs
        {
            public CombatAiPatternDeck EntryProbe;
            public CombatAiPatternDeck BreakGateBruiser;
            public CombatAiPatternDeck BacklineShooter;
            public CombatAiPatternDeck FanSuppressor;
            public CombatAiPatternDeck LineCaster;
            public CombatAiPatternDeck Skirmisher;
            public CombatAiPatternDeck BreakHandoff;
            public CombatAiPatternDeck FinalStand;
        }
    }
}
