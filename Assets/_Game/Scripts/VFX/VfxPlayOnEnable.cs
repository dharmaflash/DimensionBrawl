using UnityEngine;
using UnityEngine.VFX;

namespace DimensionBrawl.VFX
{
    [DisallowMultipleComponent]
    public sealed class VfxPlayOnEnable : MonoBehaviour
    {
        [SerializeField] private string playEventName = "OnPlay";

        private void OnEnable()
        {
            PlayEffects();
        }

        private void Start()
        {
            PlayEffects();
        }

        private void PlayEffects()
        {
            VisualEffect[] effects = GetComponentsInChildren<VisualEffect>(includeInactive: true);
            for (int i = 0; i < effects.Length; i++)
            {
                effects[i].enabled = true;
                effects[i].Reinit();
                effects[i].SendEvent(playEventName);
                effects[i].Play();
            }
        }
    }
}
