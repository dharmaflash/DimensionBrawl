using UnityEngine;

namespace DimensionBrawl.Presentation
{
    public sealed partial class ActionCinematicCueDirector
    {
        private void RequestShot(ActionCinematicCueProfile.CameraShot shot, int tier, Vector3 planarDirection)
        {
            Transform space = cueSpace != null ? cueSpace : transform;
            Vector3 offset = space.TransformDirection(shot.localOffset);
            Vector3 direction = Vector3.ProjectOnPlane(planarDirection, Vector3.up);
            if (direction.sqrMagnitude > 0.0001f)
            {
                offset += direction.normalized * shot.planarDirectionOffset;
            }

            float tierWeight = Mathf.Clamp01((Mathf.Max(1, tier) - 1) / 2f);
            float scale = Mathf.Lerp(1f, Mathf.Max(0f, shot.tierScale), tierWeight);
            cameraController.RequestCue(
                offset * scale,
                shot.durationSeconds,
                shot.fieldOfViewDelta * scale,
                shot.cameraDistanceDelta * scale,
                shot.focusHeightDelta * scale);
        }

        private Vector3 ResolveDefaultDirection()
        {
            Transform space = cueSpace != null ? cueSpace : transform;
            Vector3 forward = Vector3.ProjectOnPlane(space.forward, Vector3.up);
            return forward.sqrMagnitude > 0.0001f ? forward.normalized : Vector3.forward;
        }

        private static float EstimateSequenceSeconds(ActionCinematicCueProfile.CueSequence sequence)
        {
            if (sequence.shots == null || sequence.shots.Length == 0)
            {
                return 0f;
            }

            float total = 0f;
            for (int i = 0; i < sequence.shots.Length; i++)
            {
                ActionCinematicCueProfile.CameraShot shot = sequence.shots[i];
                total += Mathf.Max(0.01f, shot.durationSeconds + Mathf.Max(0f, shot.pauseAfterSeconds));
            }

            return total;
        }
    }
}
