using System;
using UnityEngine;

namespace DimensionBrawl.LevelDesign
{
    [CreateAssetMenu(menuName = "DimensionBrawl/Profiles/Linear Stage Segment Profile", fileName = "DB_LinearStageSegment")]
    public sealed class LinearStageSegmentProfile : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string segmentId = "S1.EntryRead";
        [SerializeField] private string displayName = "Entry Read";
        [SerializeField] private LinearStageSegmentKind segmentKind = LinearStageSegmentKind.EntryRead;

        [Header("Pacing")]
        [SerializeField, Min(0f)] private float recommendedDurationSeconds = 25f;
        [SerializeField, Range(0f, 1f)] private float targetIntensity = 0.25f;
        [SerializeField] private bool requiresReliefAfter;

        [Header("Design Intent")]
        [SerializeField] private string combatLesson = "Teach one readable threat before mixing roles.";
        [SerializeField] private string cameraRead = "Keep the player and active threat readable.";
        [SerializeField] private string excludedScope = "No runtime spawning contract yet.";

        [Header("Encounter Pockets")]
        [SerializeField] private LinearStagePocket[] pockets = Array.Empty<LinearStagePocket>();

        public string SegmentId => segmentId;
        public string DisplayName => displayName;
        public LinearStageSegmentKind SegmentKind => segmentKind;
        public float RecommendedDurationSeconds => recommendedDurationSeconds;
        public float TargetIntensity => targetIntensity;
        public bool RequiresReliefAfter => requiresReliefAfter;
        public string CombatLesson => combatLesson;
        public string CameraRead => cameraRead;
        public string ExcludedScope => excludedScope;
        public int PocketCount => pockets != null ? pockets.Length : 0;

        public LinearStagePocket GetPocket(int index)
        {
            if (pockets == null || index < 0 || index >= pockets.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return pockets[index];
        }
    }
}
