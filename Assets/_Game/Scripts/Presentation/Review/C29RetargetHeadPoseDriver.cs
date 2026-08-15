using UnityEngine;

namespace DimensionBrawl.Presentation
{
    /// <summary>
    /// Review-only replacement for the source C29 actor's head lift. The source
    /// actor clip is deliberately absent; this additive pose is authored for Inori.
    /// It ships no active behaviour because only the editor-only review scene binds it.
    /// </summary>
    [ExecuteAlways]
    public sealed class C29RetargetHeadPoseDriver : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private SkinnedMeshRenderer faceRenderer;
        [SerializeField] private Mesh eyesClosedMesh;
        [SerializeField] private Mesh eyesOpenMesh;
        [SerializeField, Min(0f)] private float eyeOpenSeconds = 1.95f;
        [SerializeField, Min(0f)] private float revealStartSeconds = 1.55f;
        [SerializeField, Min(0.01f)] private float revealDurationSeconds = 0.78f;
        [SerializeField] private float neckPitchDegrees = -9f;
        [SerializeField] private float headPitchDegrees = -28f;

        private float playbackStartedAt;

        public void Configure(
            Animator targetAnimator,
            SkinnedMeshRenderer targetFaceRenderer,
            Mesh closedMesh,
            Mesh openMesh,
            float openSeconds,
            float startSeconds,
            float durationSeconds,
            float neckPitch,
            float headPitch)
        {
            animator = targetAnimator;
            faceRenderer = targetFaceRenderer;
            eyesClosedMesh = closedMesh;
            eyesOpenMesh = openMesh;
            eyeOpenSeconds = Mathf.Max(0f, openSeconds);
            revealStartSeconds = Mathf.Max(0f, startSeconds);
            revealDurationSeconds = Mathf.Max(0.01f, durationSeconds);
            neckPitchDegrees = neckPitch;
            headPitchDegrees = headPitch;
        }

        public void ApplySample(float elapsedSeconds)
        {
            if (animator == null)
            {
                return;
            }

            float linear = Mathf.Clamp01(
                (Mathf.Max(0f, elapsedSeconds) - revealStartSeconds) / revealDurationSeconds);
            float weight = Mathf.SmoothStep(0f, 1f, linear);
            ApplyPitch(animator.GetBoneTransform(HumanBodyBones.Neck), neckPitchDegrees * weight);
            ApplyPitch(animator.GetBoneTransform(HumanBodyBones.Head), headPitchDegrees * weight);
            ApplyEyePose(elapsedSeconds < eyeOpenSeconds);
        }

        private void OnEnable()
        {
            playbackStartedAt = Time.unscaledTime;
        }

        private void LateUpdate()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            ApplySample(Time.unscaledTime - playbackStartedAt);
        }

        private static void ApplyPitch(Transform bone, float degrees)
        {
            if (bone != null && Mathf.Abs(degrees) > 0.001f)
            {
                bone.localRotation *= Quaternion.Euler(degrees, 0f, 0f);
            }
        }

        private void ApplyEyePose(bool closed)
        {
            if (faceRenderer == null)
            {
                return;
            }

            Mesh target = closed ? eyesClosedMesh : eyesOpenMesh;
            if (target != null && faceRenderer.sharedMesh != target)
            {
                faceRenderer.sharedMesh = target;
            }

            if (target != null)
            {
                // Editor Camera.Render can otherwise reuse the skinning cache from
                // the previously sampled mesh even though sharedMesh changed.
                faceRenderer.enabled = false;
                faceRenderer.enabled = true;
                faceRenderer.forceMatrixRecalculationPerRender = true;
            }
        }
    }
}
