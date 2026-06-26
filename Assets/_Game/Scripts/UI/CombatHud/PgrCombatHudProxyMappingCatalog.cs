using System;
using UnityEngine;

namespace DimensionBrawl.UI
{
    [CreateAssetMenu(menuName = "DimensionBrawl/UI/PGR Combat HUD Proxy Mapping Catalog")]
    public sealed class PgrCombatHudProxyMappingCatalog : ScriptableObject
    {
        [SerializeField] private PgrCombatHudProxyMapping[] mappings = Array.Empty<PgrCombatHudProxyMapping>();

        private static PgrCombatHudProxyMapping[] defaultP0Mappings;

        public PgrCombatHudProxyMapping[] Mappings => mappings ?? Array.Empty<PgrCombatHudProxyMapping>();

        public static PgrCombatHudProxyMapping[] DefaultP0Mappings
        {
            get
            {
                defaultP0Mappings ??= BuildDefaultP0Mappings();
                return defaultP0Mappings;
            }
        }

        public void Configure(PgrCombatHudProxyMapping[] newMappings)
        {
            mappings = newMappings ?? Array.Empty<PgrCombatHudProxyMapping>();
        }

        public bool TryFindByMappingId(string mappingId, out PgrCombatHudProxyMapping mapping)
        {
            return TryFindByMappingId(Mappings, mappingId, out mapping);
        }

        public bool TryResolve(string pgrMaskTarget, string pgrClickKey, out PgrCombatHudProxyMapping mapping)
        {
            return TryResolve(Mappings, pgrMaskTarget, pgrClickKey, out mapping);
        }

        public static PgrCombatHudProxyMappingCatalog CreateRuntimeDefaultP0()
        {
            PgrCombatHudProxyMappingCatalog catalog = CreateInstance<PgrCombatHudProxyMappingCatalog>();
            catalog.Configure(DefaultP0Mappings);
            return catalog;
        }

        public static bool TryFindDefaultP0ByMappingId(string mappingId, out PgrCombatHudProxyMapping mapping)
        {
            return TryFindByMappingId(DefaultP0Mappings, mappingId, out mapping);
        }

        public static bool TryResolveDefaultP0(string pgrMaskTarget, string pgrClickKey, out PgrCombatHudProxyMapping mapping)
        {
            return TryResolve(DefaultP0Mappings, pgrMaskTarget, pgrClickKey, out mapping);
        }

        private static bool TryFindByMappingId(
            PgrCombatHudProxyMapping[] rows,
            string mappingId,
            out PgrCombatHudProxyMapping mapping)
        {
            for (int i = 0; i < rows.Length; i++)
            {
                if (string.Equals(rows[i].MappingId, mappingId, StringComparison.Ordinal))
                {
                    mapping = rows[i];
                    return true;
                }
            }

            mapping = default;
            return false;
        }

        private static bool TryResolve(
            PgrCombatHudProxyMapping[] rows,
            string pgrMaskTarget,
            string pgrClickKey,
            out PgrCombatHudProxyMapping mapping)
        {
            for (int i = 0; i < rows.Length; i++)
            {
                if (rows[i].MatchesSource(pgrMaskTarget, pgrClickKey))
                {
                    mapping = rows[i];
                    return true;
                }
            }

            mapping = default;
            return false;
        }

        private static PgrCombatHudProxyMapping[] BuildDefaultP0Mappings()
        {
            return new[]
            {
                new PgrCombatHudProxyMapping(
                    "basic_attack_primary",
                    "basic_attack_button",
                    "combat-basic-attack-tap",
                    "AttackButton",
                    "15",
                    "Hud.BasicAttackButton",
                    ProxyCombatHudInputEvent.BasicAttack(),
                    ProxyCombatHudCompletionKind.BasicAttackAccepted,
                    -1,
                    "mask_and_spotlight_single_button",
                    "fallback_to_bottom_right_attack_cluster",
                    "P0",
                    54,
                    "102 | 117 | 12140116 | 300629537 | 300631241 | 300631244",
                    "Hit the enemy repeatedly with Basic Attacks to receive a Signal Orb.",
                    "GuideFightStep.json :: id=102 | id=117 | id=12140116",
                    "Primary normal attack tutorial. This is the cleanest click-key mapping."),
                new PgrCombatHudProxyMapping(
                    "signal_orb_first_slot",
                    "signal_orb_strip",
                    "combat-signal-orb-ping",
                    "PanelBallBox/FirstBall",
                    "1",
                    "Hud.SignalOrbSlots[0]",
                    ProxyCombatHudInputEvent.SignalOrb(0),
                    ProxyCombatHudCompletionKind.SignalOrbPinged,
                    0,
                    "mask_single_orb_slot",
                    "fallback_to_orb_strip_group",
                    "P0",
                    69,
                    "103 | 106 | 109 | 300 | 301014 | 301033",
                    "Tap to ping us with a Signal Orb to use a Construct-specific Skill.",
                    "GuideFightStep.json :: id=103 | id=106 | id=109",
                    "Single-orb teaching step."),
                new PgrCombatHudProxyMapping(
                    "signal_orb_three_ping",
                    "signal_orb_strip",
                    "combat-signal-orb-ping",
                    "PanelBallBox/TopThreeBalls",
                    "1|2|3",
                    "Hud.SignalOrbGroup.TopThree",
                    ProxyCombatHudInputEvent.SignalOrbSequence(0, 1, 2),
                    ProxyCombatHudCompletionKind.ThreePingAccepted,
                    -1,
                    "mask_grouped_orb_slots",
                    "fallback_to_orb_strip_group",
                    "P0",
                    124,
                    "104 | 108 | 301028 | 301030 | 301045 | 301048",
                    "Tap 3 adjacent Orbs of the same color to 3-Ping for a powerful blow.",
                    "GuideFightStep.json :: id=104 | id=108 | id=301028",
                    "Group target; do not treat it as one button."),
                new PgrCombatHudProxyMapping(
                    "dodge_matrix_primary",
                    "dodge_matrix_button",
                    "combat-dodge-matrix",
                    "DodgeButton",
                    "16",
                    "Hud.DodgeButton",
                    ProxyCombatHudInputEvent.Dodge(),
                    ProxyCombatHudCompletionKind.DodgeOrMatrixAccepted,
                    -1,
                    "mask_and_spotlight_single_button",
                    "fallback_to_bottom_right_dodge_cluster",
                    "P0",
                    57,
                    "105 | 301002 | 301005 | 301007 | 301009 | 301013",
                    "If you Dodge before being hit, you will enter the Matrix state.",
                    "GuideFightStep.json :: id=105 | id=301002 | id=301005",
                    "Primary dodge/matrix tutorial mapping."),
                new PgrCombatHudProxyMapping(
                    "signature_skill_primary",
                    "signature_skill_button",
                    "combat-signature-skill",
                    "ExSkillButton",
                    "17",
                    "Hud.SignatureSkillButton",
                    ProxyCombatHudInputEvent.SignatureSkill(),
                    ProxyCombatHudCompletionKind.SignatureSkillCast,
                    -1,
                    "mask_and_spotlight_single_button",
                    "fallback_to_skill_cluster",
                    "P0",
                    63,
                    "110 | 10172331 | 15030956 | 300631246 | 301008112 | 301008125",
                    "Hit enemies to generate Energy. Use Energy for Signature Moves.",
                    "GuideFightStep.json :: id=110 | id=10172331 | id=15030956",
                    "Primary signature/EX skill tutorial mapping."),
                new PgrCombatHudProxyMapping(
                    "character_switch_slot_1",
                    "character_switch_qte_portrait",
                    "combat-character-switch-qte",
                    "PanelPortrait/NpcPortrait1",
                    "20",
                    "Hud.PartyPortraitSlots[1]",
                    ProxyCombatHudInputEvent.SwitchOrQte(1),
                    ProxyCombatHudCompletionKind.CharacterSwitchOrQteAccepted,
                    1,
                    "mask_party_portrait",
                    "fallback_to_party_portrait_group",
                    "P0",
                    92,
                    "301019 | 301023 | 301075 | 301108 | 10171917 | 10172123",
                    "When a Construct overheats, you can switch to another one.",
                    "GuideFightStep.json :: id=301019 | id=301023 | id=301075",
                    "Main teammate switch/QTE portrait mapping."),
                new PgrCombatHudProxyMapping(
                    "character_switch_slot_2",
                    "character_switch_qte_portrait",
                    "combat-character-switch-qte",
                    "PanelPortrait/NpcPortrait2",
                    "21",
                    "Hud.PartyPortraitSlots[2]",
                    ProxyCombatHudInputEvent.SwitchOrQte(2),
                    ProxyCombatHudCompletionKind.CharacterSwitchOrQteAccepted,
                    2,
                    "mask_party_portrait",
                    "fallback_to_party_portrait_group",
                    "P0",
                    19,
                    "301109 | 301100049 | 301100060 | 301100084 | 301100102 | 301102937",
                    "Switch to another party member.",
                    "GuideFightStep.json :: id=301109 | id=301100049 | id=301100060",
                    "Second teammate switch/QTE portrait mapping."),
                new PgrCombatHudProxyMapping(
                    "boss_poise_endure_bar",
                    "boss_state_bar_explainer",
                    "combat-boss-hp-poise-rage",
                    "HpTopBossTemplate/HpTopNormalTemplateList/Endure",
                    "(none)",
                    "Hud.BossPoiseBar",
                    ProxyCombatHudInputEvent.None,
                    ProxyCombatHudCompletionKind.DurationOrReadAck,
                    -1,
                    "spotlight_boss_state_bar_keep_input_policy_explicit",
                    "fallback_to_boss_frame_text_callout",
                    "P0",
                    4,
                    "118 | 124",
                    "Enemies gain Rage in battle and enter a stronger state when full.",
                    "GuideFightStep.json :: id=118 | id=124",
                    "Explanation overlay; most rows are not click-gated."),
                new PgrCombatHudProxyMapping(
                    "partner_skill_button",
                    "field_object_partner_interaction",
                    "combat-enemy-object-highlight",
                    "PartnerSkillButton",
                    "23",
                    "Hud.PartnerSkillButton",
                    ProxyCombatHudInputEvent.PartnerSkill(),
                    ProxyCombatHudCompletionKind.PartnerSkillAccepted,
                    -1,
                    "mask_and_spotlight_single_button",
                    "fallback_to_partner_skill_text_callout",
                    "P0",
                    166,
                    "301010083 | 301010088 | 301010089 | 301010102 | 301010103 | 301010107",
                    "Casting a partner skill can block or pressure the enemy.",
                    "GuideFightStep.json :: id=301010083 | id=301010088 | id=301010089",
                    "Partner skill / support control rows have strong click evidence.")
            };
        }
    }
}
