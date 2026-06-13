using System;
using UnityEngine;

namespace DimensionBrawl.UI
{
    public enum LobbyGuideCondition
    {
        Default = 0,
        ReturnFromCombat = 10,
        NewRewardDisplay = 20,
        SummonReadyDisplay = 30,
        RetryPrompt = 40
    }

    [CreateAssetMenu(menuName = "DimensionBrawl/UI/Lobby Guide Feedback Catalog")]
    public sealed class LobbyGuideFeedbackCatalog : ScriptableObject
    {
        [Serializable]
        public struct FeedbackEntry
        {
            [SerializeField] private LobbyGuideCondition condition;
            [SerializeField] private string lineKey;
            [SerializeField] private string voiceKey;
            [SerializeField] private string motionKey;
            [SerializeField, Min(0f)] private float durationSeconds;
            [SerializeField, Min(0)] private int weight;
            [SerializeField, Min(0f)] private float cooldownSeconds;

            public LobbyGuideCondition Condition => condition;
            public string LineKey => lineKey;
            public string VoiceKey => voiceKey;
            public string MotionKey => motionKey;
            public float DurationSeconds => durationSeconds;
            public int Weight => weight;
            public float CooldownSeconds => cooldownSeconds;
        }

        [SerializeField] private FeedbackEntry[] entries = Array.Empty<FeedbackEntry>();

        public bool TryGetFirst(LobbyGuideCondition condition, out FeedbackEntry entry)
        {
            for (int i = 0; i < entries.Length; i++)
            {
                if (entries[i].Condition == condition)
                {
                    entry = entries[i];
                    return true;
                }
            }

            entry = default;
            return false;
        }
    }
}
