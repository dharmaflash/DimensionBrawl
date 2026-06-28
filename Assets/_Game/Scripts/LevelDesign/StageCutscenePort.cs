using System;
using UnityEngine;

namespace DimensionBrawl.LevelDesign
{
    public enum StageCutscenePortKind
    {
        Intro = 0,
        BossEntrance = 1,
        GameplayHandoff = 2
    }

    public sealed class StageCutscenePort : MonoBehaviour
    {
        [SerializeField] private string portId;
        [SerializeField] private StageCutscenePortKind portKind;
        [SerializeField] private string handoffId;
        [SerializeField] private string anchorId;
        [SerializeField] private string runtimeStateId;
        [SerializeField] private Transform payloadRoot;
        [TextArea, SerializeField] private string purpose;

        public string PortId => portId;
        public StageCutscenePortKind PortKind => portKind;
        public string HandoffId => handoffId;
        public string AnchorId => anchorId;
        public string RuntimeStateId => runtimeStateId;
        public Transform PayloadRoot => payloadRoot;
        public string Purpose => purpose;
        public bool HasPayloadRoot => payloadRoot != null;

        public void Configure(
            string newPortId,
            StageCutscenePortKind newPortKind,
            string newHandoffId,
            string newAnchorId,
            string newRuntimeStateId,
            Transform newPayloadRoot,
            string newPurpose)
        {
            if (string.IsNullOrWhiteSpace(newPortId))
            {
                throw new ArgumentException("Cutscene port id is required.", nameof(newPortId));
            }

            if (string.IsNullOrWhiteSpace(newHandoffId))
            {
                throw new ArgumentException("Cutscene handoff id is required.", nameof(newHandoffId));
            }

            if (string.IsNullOrWhiteSpace(newAnchorId))
            {
                throw new ArgumentException("Stage anchor id is required.", nameof(newAnchorId));
            }

            if (string.IsNullOrWhiteSpace(newRuntimeStateId))
            {
                throw new ArgumentException("Runtime state id is required.", nameof(newRuntimeStateId));
            }

            portId = newPortId;
            portKind = newPortKind;
            handoffId = newHandoffId;
            anchorId = newAnchorId;
            runtimeStateId = newRuntimeStateId;
            payloadRoot = newPayloadRoot;
            purpose = newPurpose ?? string.Empty;
        }
    }
}
