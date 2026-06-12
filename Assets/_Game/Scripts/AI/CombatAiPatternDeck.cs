using System;
using UnityEngine;

namespace DimensionBrawl.AI
{
    [CreateAssetMenu(menuName = "DimensionBrawl/Profiles/Combat AI Pattern Deck")]
    public sealed class CombatAiPatternDeck : ScriptableObject
    {
        [SerializeField] private string deckId = "SciFiSoldier.BasicTraining";
        [SerializeField] private CombatAiPatternDeckEntry[] entries = Array.Empty<CombatAiPatternDeckEntry>();

        public string DeckId => deckId;
        public int EntryCount => entries != null ? entries.Length : 0;

        public CombatAiPatternDeckEntry GetEntry(int index)
        {
            if (entries == null || index < 0 || index >= entries.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return entries[index];
        }

        public bool TrySelectPattern(
            float targetDistance,
            CombatAiPatternProfile currentProfile,
            float timeSeconds,
            float[] lastUseTimes,
            out CombatAiPatternProfile selectedProfile,
            out int selectedIndex)
        {
            selectedProfile = null;
            selectedIndex = -1;

            if (entries == null || entries.Length == 0)
            {
                return false;
            }

            float bestPriority = float.NegativeInfinity;
            for (int i = 0; i < entries.Length; i++)
            {
                CombatAiPatternDeckEntry entry = entries[i];
                if (!entry.CanSelect(targetDistance, timeSeconds, ResolveLastUseTime(lastUseTimes, i)))
                {
                    continue;
                }

                float priority = entry.Priority;
                if (entry.Profile == currentProfile)
                {
                    priority += 0.01f;
                }

                if (priority <= bestPriority)
                {
                    continue;
                }

                bestPriority = priority;
                selectedProfile = entry.Profile;
                selectedIndex = i;
            }

            return selectedProfile != null;
        }

        private static float ResolveLastUseTime(float[] lastUseTimes, int index)
        {
            if (lastUseTimes == null || index < 0 || index >= lastUseTimes.Length)
            {
                return -1f;
            }

            return lastUseTimes[index];
        }
    }

    [Serializable]
    public struct CombatAiPatternDeckEntry
    {
        [SerializeField] private CombatAiPatternProfile profile;
        [SerializeField, Min(0f)] private float minimumDistance;
        [SerializeField, Min(0f)] private float maximumDistance;
        [SerializeField, Min(0f)] private float cooldownSeconds;
        [SerializeField] private float priority;

        public CombatAiPatternProfile Profile => profile;
        public float MinimumDistance => minimumDistance;
        public float MaximumDistance => maximumDistance;
        public float CooldownSeconds => cooldownSeconds;
        public float Priority => priority;

        public CombatAiPatternDeckEntry(
            CombatAiPatternProfile profile,
            float minimumDistance,
            float maximumDistance,
            float cooldownSeconds,
            float priority)
        {
            this.profile = profile;
            this.minimumDistance = minimumDistance;
            this.maximumDistance = maximumDistance;
            this.cooldownSeconds = cooldownSeconds;
            this.priority = priority;
        }

        public bool CanSelect(float targetDistance, float timeSeconds, float lastUseTime)
        {
            if (profile == null)
            {
                return false;
            }

            if (!IsDistanceInRange(targetDistance))
            {
                return false;
            }

            return cooldownSeconds <= 0f || lastUseTime < 0f || timeSeconds - lastUseTime >= cooldownSeconds;
        }

        public bool IsDistanceInRange(float targetDistance)
        {
            if (targetDistance < minimumDistance)
            {
                return false;
            }

            return maximumDistance <= 0f || targetDistance <= maximumDistance;
        }
    }
}
