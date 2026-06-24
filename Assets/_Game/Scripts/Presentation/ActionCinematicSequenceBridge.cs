using UnityEngine;

namespace DimensionBrawl.Presentation
{
    [DisallowMultipleComponent]
    public sealed class ActionCinematicSequenceBridge : MonoBehaviour
    {
        [Header("Playback")]
        [SerializeField] private CinematicSequenceRunner runner;
        [SerializeField] private bool blockLegacyCameraShotsWhenPlayed = true;
        [SerializeField] private bool blockLegacySignalsWhenPlayed = true;
        [SerializeField, Min(0f)] private float minimumLockSeconds = 0.12f;

        [Header("Action Cue Mapping")]
        [SerializeField] private CinematicSequenceProfile skillCutInProfile;
        [SerializeField] private CinematicSequenceProfile summonEntryProfile;
        [SerializeField] private CinematicSequenceProfile ultimateCutInProfile;
        [SerializeField] private CinematicSequenceProfile bossPressureBreakProfile;
        [SerializeField] private CinematicSequenceProfile summonFollowupHitProfile;
        [SerializeField] private CinematicSequenceProfile summonEmpowerProfile;
        [SerializeField] private CinematicSequenceProfile summonRecallProfile;
        [SerializeField] private CinematicSequenceProfile pocketClearProfile;
        [SerializeField] private CinematicSequenceProfile pocketFailProfile;

        private int totalPlayCount;
        private ActionCinematicCueProfile.CueKind lastPlayedKind;
        private int lastPlayedTier;
        private CinematicSequenceProfile lastPlayedProfile;

        public CinematicSequenceRunner Runner => runner;
        public bool BlockLegacyCameraShotsWhenPlayed => blockLegacyCameraShotsWhenPlayed;
        public bool BlockLegacySignalsWhenPlayed => blockLegacySignalsWhenPlayed;
        public int TotalPlayCount => totalPlayCount;
        public ActionCinematicCueProfile.CueKind LastPlayedKind => lastPlayedKind;
        public int LastPlayedTier => lastPlayedTier;
        public CinematicSequenceProfile LastPlayedProfile => lastPlayedProfile;

        private void Awake()
        {
            if (runner == null)
            {
                runner = GetComponent<CinematicSequenceRunner>();
            }
        }

        public bool TryPlay(
            ActionCinematicCueProfile.CueKind kind,
            int tier,
            Vector3 planarDirection,
            out float lockSeconds)
        {
            lockSeconds = 0f;
            if (runner == null)
            {
                runner = GetComponent<CinematicSequenceRunner>();
            }

            CinematicSequenceProfile profile = ResolveProfile(kind);
            if (runner == null || profile == null)
            {
                return false;
            }

            if (!runner.TryPlayProfile(profile, planarDirection))
            {
                return false;
            }

            totalPlayCount++;
            lastPlayedKind = kind;
            lastPlayedTier = Mathf.Max(1, tier);
            lastPlayedProfile = profile;
            lockSeconds = Mathf.Max(minimumLockSeconds, profile.EstimatedDurationSeconds);
            return true;
        }

        private CinematicSequenceProfile ResolveProfile(ActionCinematicCueProfile.CueKind kind)
        {
            return kind switch
            {
                ActionCinematicCueProfile.CueKind.SkillCutIn => skillCutInProfile,
                ActionCinematicCueProfile.CueKind.SummonEntry => summonEntryProfile,
                ActionCinematicCueProfile.CueKind.UltimateCutIn => ultimateCutInProfile,
                ActionCinematicCueProfile.CueKind.BossPressureBreak => bossPressureBreakProfile,
                ActionCinematicCueProfile.CueKind.SummonFollowupHit => summonFollowupHitProfile,
                ActionCinematicCueProfile.CueKind.SummonEmpower => summonEmpowerProfile,
                ActionCinematicCueProfile.CueKind.SummonRecall => summonRecallProfile,
                ActionCinematicCueProfile.CueKind.PocketClear => pocketClearProfile,
                ActionCinematicCueProfile.CueKind.PocketFail => pocketFailProfile,
                _ => null
            };
        }
    }
}
