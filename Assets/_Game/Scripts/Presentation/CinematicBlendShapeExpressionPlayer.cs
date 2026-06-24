using System;
using System.Collections.Generic;
using UnityEngine;

namespace DimensionBrawl.Presentation
{
    [DisallowMultipleComponent]
    public sealed class CinematicBlendShapeExpressionPlayer : MonoBehaviour
    {
        [Serializable]
        public struct ShapeWeight
        {
            [SerializeField] private string shapeName;
            [SerializeField, Range(0f, 100f)] private float weight;

            public ShapeWeight(string shapeName, float weight)
            {
                this.shapeName = shapeName;
                this.weight = Mathf.Clamp(weight, 0f, 100f);
            }

            public string ShapeName => shapeName;
            public float Weight => Mathf.Clamp(weight, 0f, 100f);
        }

        [Serializable]
        public struct ExpressionPreset
        {
            [SerializeField] private string expressionName;
            [SerializeField] private ShapeWeight[] shapes;

            public ExpressionPreset(string expressionName, ShapeWeight[] shapes)
            {
                this.expressionName = expressionName;
                this.shapes = shapes ?? Array.Empty<ShapeWeight>();
            }

            public string ExpressionName => expressionName;
            public ShapeWeight[] Shapes => shapes ?? Array.Empty<ShapeWeight>();
        }

        private struct RuntimeShapeTarget
        {
            public SkinnedMeshRenderer Renderer;
            public int ShapeIndex;
            public float TargetWeight;
        }

        [SerializeField] private SkinnedMeshRenderer[] renderers = Array.Empty<SkinnedMeshRenderer>();
        [SerializeField] private ExpressionPreset[] presets = Array.Empty<ExpressionPreset>();
        [SerializeField] private bool resetPresetShapesBeforePlay = true;
        [SerializeField, Min(0f)] private float blendSpeed = 14f;

        private readonly Dictionary<string, RuntimeShapeTarget> activeTargetsByKey =
            new Dictionary<string, RuntimeShapeTarget>();

        private string lastExpressionName = string.Empty;
        private int playCount;

        public string LastExpressionName => lastExpressionName;
        public int PlayCount => playCount;

        private void Awake()
        {
            if (renderers == null || renderers.Length == 0)
            {
                renderers = GetComponentsInChildren<SkinnedMeshRenderer>(includeInactive: true);
            }
        }

        private void Update()
        {
            if (activeTargetsByKey.Count == 0)
            {
                return;
            }

            float step = blendSpeed <= 0f ? 1f : 1f - Mathf.Exp(-blendSpeed * Time.deltaTime);
            foreach (RuntimeShapeTarget target in activeTargetsByKey.Values)
            {
                if (target.Renderer == null || target.Renderer.sharedMesh == null)
                {
                    continue;
                }

                float current = target.Renderer.GetBlendShapeWeight(target.ShapeIndex);
                float next = Mathf.Lerp(current, target.TargetWeight, step);
                target.Renderer.SetBlendShapeWeight(target.ShapeIndex, next);
            }
        }

        public void Configure(ExpressionPreset[] newPresets)
        {
            presets = newPresets ?? Array.Empty<ExpressionPreset>();
            renderers = GetComponentsInChildren<SkinnedMeshRenderer>(includeInactive: true);
        }

        public bool PlayExpression(string expressionName)
        {
            if (string.IsNullOrWhiteSpace(expressionName))
            {
                return false;
            }

            for (int i = 0; i < presets.Length; i++)
            {
                ExpressionPreset preset = presets[i];
                if (!string.Equals(preset.ExpressionName, expressionName, StringComparison.Ordinal))
                {
                    continue;
                }

                QueuePreset(preset);
                lastExpressionName = expressionName;
                playCount++;
                return true;
            }

            return false;
        }

        public bool ApplyExpressionImmediate(string expressionName)
        {
            if (!PlayExpression(expressionName))
            {
                return false;
            }

            foreach (RuntimeShapeTarget target in activeTargetsByKey.Values)
            {
                if (target.Renderer == null || target.Renderer.sharedMesh == null)
                {
                    continue;
                }

                target.Renderer.SetBlendShapeWeight(target.ShapeIndex, target.TargetWeight);
            }

            return true;
        }

        private void QueuePreset(ExpressionPreset preset)
        {
            if (resetPresetShapesBeforePlay)
            {
                for (int i = 0; i < presets.Length; i++)
                {
                    ShapeWeight[] shapes = presets[i].Shapes;
                    for (int j = 0; j < shapes.Length; j++)
                    {
                        QueueShapeTarget(shapes[j].ShapeName, 0f);
                    }
                }
            }

            ShapeWeight[] presetShapes = preset.Shapes;
            for (int i = 0; i < presetShapes.Length; i++)
            {
                QueueShapeTarget(presetShapes[i].ShapeName, presetShapes[i].Weight);
            }
        }

        private void QueueShapeTarget(string shapeName, float weight)
        {
            if (string.IsNullOrWhiteSpace(shapeName))
            {
                return;
            }

            for (int i = 0; i < renderers.Length; i++)
            {
                SkinnedMeshRenderer renderer = renderers[i];
                if (renderer == null || renderer.sharedMesh == null)
                {
                    continue;
                }

                int shapeIndex = renderer.sharedMesh.GetBlendShapeIndex(shapeName);
                if (shapeIndex < 0)
                {
                    continue;
                }

                string key = $"{renderer.GetInstanceID()}:{shapeIndex}";
                activeTargetsByKey[key] = new RuntimeShapeTarget
                {
                    Renderer = renderer,
                    ShapeIndex = shapeIndex,
                    TargetWeight = Mathf.Clamp(weight, 0f, 100f)
                };
                return;
            }
        }
    }
}
