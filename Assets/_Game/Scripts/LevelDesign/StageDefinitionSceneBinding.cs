using System;
using UnityEngine;

namespace DimensionBrawl.LevelDesign
{
    public sealed class StageDefinitionSceneBinding : MonoBehaviour
    {
        [SerializeField] private StageDefinitionProfile stageDefinition;
        [SerializeField] private Transform mapRoot;
        [SerializeField] private StageAnchorPoint[] anchorPoints = Array.Empty<StageAnchorPoint>();

        public StageDefinitionProfile StageDefinition => stageDefinition;
        public Transform MapRoot => mapRoot;
        public int AnchorPointCount => anchorPoints != null ? anchorPoints.Length : 0;

        public void Configure(
            StageDefinitionProfile profile,
            Transform contentRoot,
            StageAnchorPoint[] points)
        {
            if (profile == null)
            {
                throw new ArgumentNullException(nameof(profile));
            }

            stageDefinition = profile;
            mapRoot = contentRoot;
            anchorPoints = points ?? Array.Empty<StageAnchorPoint>();
        }

        public StageAnchorPoint GetAnchorPoint(int index)
        {
            if (anchorPoints == null || index < 0 || index >= anchorPoints.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return anchorPoints[index];
        }

        public bool TryGetAnchorPoint(string anchorId, out StageAnchorPoint anchorPoint)
        {
            if (anchorPoints != null && !string.IsNullOrWhiteSpace(anchorId))
            {
                for (int i = 0; i < anchorPoints.Length; i++)
                {
                    if (anchorPoints[i] != null && string.Equals(anchorPoints[i].AnchorId, anchorId, StringComparison.Ordinal))
                    {
                        anchorPoint = anchorPoints[i];
                        return true;
                    }
                }
            }

            anchorPoint = null;
            return false;
        }
    }
}
