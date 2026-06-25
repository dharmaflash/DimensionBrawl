using UnityEngine;
using System.Collections.Generic;

namespace IsekaiBrawl.Gameplay
{
    public enum SummonType
    {
        Melee,
        Ranged,
        Tank,
        Support
    }

    [CreateAssetMenu(fileName = "SummonData", menuName = "\uCC28\uC6D0\uB300\uB09C\uD22C/SummonData")]
    public class SummonData : ScriptableObject
    {
        public string summonName;
        public string shortLabel;
        [TextArea] public string roleDescription;
        public Sprite cardSprite;
        public GameObject prefab;
        public float energyCost;
        public float maxHP;
        public float attackDamage;
        public float attackRange;
        public float attackCooldown;
        public float moveSpeed;
        public float structureDamageMultiplier = 1f;
        public float baseDamageMultiplier = 1f;
        public float splashRadius;
        public float healAmount;
        public float healRadius;
        public float healCooldown;
        public SummonType summonType;
    }

    public static class PrototypeDeckFactory
    {
        public static List<SummonData> BuildPrototypeDeck(IReadOnlyList<SummonData> sourceDeck)
        {
            List<SummonData> output = new();
            if (sourceDeck == null || sourceDeck.Count == 0)
            {
                return output;
            }

            SummonData meleeBase = FindFirstByType(sourceDeck, SummonType.Melee) ?? sourceDeck[0];
            SummonData tankBase = FindFirstByType(sourceDeck, SummonType.Tank) ?? meleeBase;
            SummonData rangedBase = FindFirstByType(sourceDeck, SummonType.Ranged) ?? sourceDeck[0];

            output.Add(CreateVariant(meleeBase, "Rush Blade", "Rush", "Cheap opener. Races into empty space and forces the first answer.", SummonType.Melee, 18f, 68f, 15f, 1.28f, 0.62f, 3.95f, 1.08f, 0.9f, 0f, 0f, 0f, 0f));
            output.Add(CreateVariant(meleeBase, "Breaker Knight", "Break", "Mid-cost diver that punishes greedy heroes and cracks open stalled lanes.", SummonType.Melee, 24f, 124f, 23f, 1.45f, 0.86f, 3.05f, 2.65f, 2.15f, 0f, 0f, 0f, 0f));
            output.Add(CreateVariant(tankBase, "Bulwark Golem", "Tank", "Frontline anchor. Soaks pressure, holds space, and protects the backline.", SummonType.Tank, 60f, 430f, 16f, 1.3f, 1.18f, 1.02f, 1f, 0.82f, 0f, 0f, 0f, 0f));
            output.Add(CreateVariant(rangedBase, "Night Archer", "Arrow", "Lane control archer. Stays behind the line and erases fragile rushers.", SummonType.Ranged, 38f, 78f, 22f, 8.6f, 0.96f, 2.05f, 0.7f, 0.65f, 0f, 0f, 0f, 0f));
            output.Add(CreateVariant(rangedBase, "Meteor Witch", "Meteor", "Slow splash caster. Punishes clustered pushes once a frontline is holding.", SummonType.Ranged, 50f, 92f, 24f, 6.6f, 1.76f, 1.68f, 0.82f, 0.72f, 2.8f, 0f, 0f, 0f));
            output.Add(CreateVariant(tankBase, "Halo Pixie", "Heal", "Backline support. Trails behind a push and rewards protecting one lane.", SummonType.Support, 22f, 118f, 6f, 5.2f, 1.8f, 2.05f, 0.45f, 0.35f, 0f, 24f, 5.6f, 1.4f));

            return output;
        }

        private static SummonData FindFirstByType(IReadOnlyList<SummonData> sourceDeck, SummonType summonType)
        {
            for (int index = 0; index < sourceDeck.Count; index++)
            {
                SummonData summonData = sourceDeck[index];
                if (summonData != null && summonData.summonType == summonType)
                {
                    return summonData;
                }
            }

            return null;
        }

        private static SummonData CreateVariant(
            SummonData template,
            string summonName,
            string shortLabel,
            string roleDescription,
            SummonType summonType,
            float energyCost,
            float maxHp,
            float attackDamage,
            float attackRange,
            float attackCooldown,
            float moveSpeed,
            float structureDamageMultiplier,
            float baseDamageMultiplier,
            float splashRadius,
            float healAmount,
            float healRadius,
            float healCooldown)
        {
            SummonData clone = ScriptableObject.Instantiate(template);
            clone.hideFlags = HideFlags.DontSave;
            clone.name = summonName;
            clone.summonName = summonName;
            clone.shortLabel = shortLabel;
            clone.roleDescription = roleDescription;
            clone.summonType = summonType;
            clone.energyCost = energyCost;
            clone.maxHP = maxHp;
            clone.attackDamage = attackDamage;
            clone.attackRange = attackRange;
            clone.attackCooldown = attackCooldown;
            clone.moveSpeed = moveSpeed;
            clone.structureDamageMultiplier = structureDamageMultiplier;
            clone.baseDamageMultiplier = baseDamageMultiplier;
            clone.splashRadius = splashRadius;
            clone.healAmount = healAmount;
            clone.healRadius = healRadius;
            clone.healCooldown = healCooldown;
            return clone;
        }
    }
}
