using System;
using UnityEngine;

namespace DimensionBrawl.LevelDesign
{
    [CreateAssetMenu(
        menuName = "DimensionBrawl/Profiles/Stage Progression Graph",
        fileName = "DB_StageProgressionGraph")]
    public sealed class StageProgressionGraph : ScriptableObject
    {
        [SerializeField, Min(1)] private int schemaVersion = 1;
        [SerializeField] private string progressionGraphId;
        [SerializeField, Min(1)] private int revision = 1;
        [SerializeField] private StageProgressionCyclePolicy cyclePolicy =
            StageProgressionCyclePolicy.DisallowCyclesWithinEachRelation;
        [SerializeField] private StageProgressionNode[] nodes =
            Array.Empty<StageProgressionNode>();
        [SerializeField] private string canonicalDigest;

        public int SchemaVersion => schemaVersion;
        public string ProgressionGraphId => progressionGraphId;
        public int Revision => revision;
        public StageProgressionCyclePolicy CyclePolicy => cyclePolicy;
        public int NodeCount => nodes != null ? nodes.Length : 0;
        public string CanonicalDigest => canonicalDigest;

        public StageProgressionNode GetNode(int index)
        {
            if (nodes == null || index < 0 || index >= nodes.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return nodes[index];
        }

        public bool TryCreateSnapshot(
            out StageProgressionGraphSnapshot snapshot,
            out string error)
        {
            return StageProgressionGraphSnapshot.TryCreate(this, out snapshot, out error);
        }

        public bool TryComputeCanonicalDigest(
            out string canonicalDigest,
            out string error)
        {
            return StageProgressionGraphSnapshot.TryComputeCanonicalDigest(
                this,
                out canonicalDigest,
                out error);
        }
    }
}
