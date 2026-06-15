using UnityEngine;

namespace DimensionBrawl.Presentation
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public sealed class ShaderGlobalWindDriver : MonoBehaviour
    {
        [SerializeField] private Vector3 windDirection = new Vector3(0.1f, 0f, 0.1f);
        [SerializeField, Min(0f)] private float windStrength = 12.1f;
        [SerializeField, Min(0.001f)] private float windScale = 2f;
        [SerializeField, Min(0f)] private float windSpeed = 5.5f;
        [SerializeField, Min(0f)] private float windJitter = 2.8f;

        private static readonly int WindDirectionId = Shader.PropertyToID("ToonScapesGlobalWindDirection");
        private static readonly int WindStrengthId = Shader.PropertyToID("ToonScapesGlobalWindStrength");
        private static readonly int WindScaleId = Shader.PropertyToID("ToonScapesGlobalWindScale");
        private static readonly int WindSpeedId = Shader.PropertyToID("ToonScapesGlobalWindSpeed");
        private static readonly int WindJitterId = Shader.PropertyToID("ToonScapesGlobalWindJitter");

        public void Configure(Vector3 direction, float strength, float scale, float speed, float jitter)
        {
            windDirection = direction;
            windStrength = Mathf.Max(0f, strength);
            windScale = Mathf.Max(0.001f, scale);
            windSpeed = Mathf.Max(0f, speed);
            windJitter = Mathf.Max(0f, jitter);
            Apply();
        }

        private void OnEnable()
        {
            Apply();
        }

        private void Update()
        {
            Apply();
        }

        private void OnValidate()
        {
            windStrength = Mathf.Max(0f, windStrength);
            windScale = Mathf.Max(0.001f, windScale);
            windSpeed = Mathf.Max(0f, windSpeed);
            windJitter = Mathf.Max(0f, windJitter);
            Apply();
        }

        private void Apply()
        {
            Shader.SetGlobalVector(WindDirectionId, windDirection);
            Shader.SetGlobalFloat(WindStrengthId, windStrength);
            Shader.SetGlobalFloat(WindScaleId, windScale);
            Shader.SetGlobalFloat(WindSpeedId, windSpeed);
            Shader.SetGlobalFloat(WindJitterId, windJitter);
        }
    }
}
