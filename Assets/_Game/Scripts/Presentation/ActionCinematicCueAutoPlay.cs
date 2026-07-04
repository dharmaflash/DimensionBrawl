using UnityEngine;

namespace DimensionBrawl.Presentation
{
    [DisallowMultipleComponent]
    public sealed class ActionCinematicCueAutoPlay : MonoBehaviour
    {
        [SerializeField] private ActionCinematicCueDirector cueDirector;
        [SerializeField] private bool playOnStart = true;
        [SerializeField] private ActionCinematicCueProfile.CueKind cueKind =
            ActionCinematicCueProfile.CueKind.BossIntro;
        [SerializeField, Min(1)] private int tier = 1;
        [SerializeField] private bool usePlanarDirectionOverride = true;
        [SerializeField] private Vector3 planarDirectionOverride = Vector3.back;

        public bool HasPlayed { get; private set; }

        private void Awake()
        {
            if (cueDirector == null)
            {
                cueDirector = GetComponent<ActionCinematicCueDirector>();
            }
        }

        private void Start()
        {
            if (playOnStart)
            {
                TryPlay();
            }
        }

        public bool TryPlay()
        {
            if (cueDirector == null || HasPlayed)
            {
                return false;
            }

            Vector3 direction = ResolvePlanarDirection();
            if (!cueDirector.TryPlay(cueKind, tier, direction))
            {
                return false;
            }

            HasPlayed = true;
            return true;
        }

        private Vector3 ResolvePlanarDirection()
        {
            Vector3 direction = usePlanarDirectionOverride
                ? planarDirectionOverride
                : transform.forward;
            direction = Vector3.ProjectOnPlane(direction, Vector3.up);
            return direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector3.forward;
        }
    }
}
