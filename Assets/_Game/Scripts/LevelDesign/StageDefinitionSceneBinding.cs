using System;
using UnityEngine;

namespace DimensionBrawl.LevelDesign
{
    public sealed class StageDefinitionSceneBinding : MonoBehaviour
    {
        [SerializeField] private StageDefinitionProfile stageDefinition;
        [SerializeField] private Transform mapRoot;
        [SerializeField] private StageAnchorPoint[] anchorPoints = Array.Empty<StageAnchorPoint>();
        [SerializeField] private StageCutscenePort[] cutscenePorts = Array.Empty<StageCutscenePort>();

        public StageDefinitionProfile StageDefinition => stageDefinition;
        public Transform MapRoot => mapRoot;
        public int AnchorPointCount => anchorPoints != null ? anchorPoints.Length : 0;
        public int CutscenePortCount => cutscenePorts != null ? cutscenePorts.Length : 0;

        public void Configure(
            StageDefinitionProfile profile,
            Transform contentRoot,
            StageAnchorPoint[] points)
        {
            Configure(profile, contentRoot, points, Array.Empty<StageCutscenePort>());
        }

        public void Configure(
            StageDefinitionProfile profile,
            Transform contentRoot,
            StageAnchorPoint[] points,
            StageCutscenePort[] ports)
        {
            if (profile == null)
            {
                throw new ArgumentNullException(nameof(profile));
            }

            stageDefinition = profile;
            mapRoot = contentRoot;
            anchorPoints = points ?? Array.Empty<StageAnchorPoint>();
            cutscenePorts = ports ?? Array.Empty<StageCutscenePort>();
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

        public StageCutscenePort GetCutscenePort(int index)
        {
            if (cutscenePorts == null || index < 0 || index >= cutscenePorts.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return cutscenePorts[index];
        }

        public bool TryGetCutscenePort(string handoffId, out StageCutscenePort cutscenePort)
        {
            if (cutscenePorts != null && !string.IsNullOrWhiteSpace(handoffId))
            {
                for (int i = 0; i < cutscenePorts.Length; i++)
                {
                    if (cutscenePorts[i] != null
                        && string.Equals(cutscenePorts[i].HandoffId, handoffId, StringComparison.Ordinal))
                    {
                        cutscenePort = cutscenePorts[i];
                        return true;
                    }
                }
            }

            cutscenePort = null;
            return false;
        }
    }
}
