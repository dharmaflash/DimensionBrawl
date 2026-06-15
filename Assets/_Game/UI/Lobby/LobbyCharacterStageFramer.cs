using UnityEngine;

namespace DimensionBrawl.UI
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public sealed class LobbyCharacterStageFramer : MonoBehaviour
    {
        [SerializeField] private Camera stageCamera;
        [SerializeField] private Transform targetRoot;
        [SerializeField] private bool useSkinnedRenderersOnlyForFraming = true;
        [SerializeField, Range(10f, 60f)] private float fieldOfView = 34f;
        [SerializeField, Range(0.45f, 1.5f)] private float boundsPadding = 0.72f;
        [SerializeField, Range(0.35f, 1.1f)] private float viewportHeightFill = 0.82f;
        [SerializeField, Range(0f, 1f)] private float horizontalFitWeight = 0.25f;
        [SerializeField, Range(0.5f, 1.5f)] private float distanceScale = 1f;
        [SerializeField, Min(0.1f)] private float minimumDistance = 0.1f;
        [SerializeField, Range(0f, 0.2f)] private float feetViewportY = 0.06f;
        [SerializeField, Range(0, 4)] private int runtimeRefitFrameCount = 2;
        [SerializeField] private Vector3 focusOffset = new Vector3(0f, 0.05f, 0f);
        [SerializeField] private Vector2 compositionOffset = Vector2.zero;
        [SerializeField] private Vector3 cameraEulerAngles = Vector3.zero;

        private int pendingRefitFrames;

        private void Reset()
        {
            stageCamera = GetComponentInChildren<Camera>(true);
        }

        private void OnEnable()
        {
            QueueFrame();
        }

        private void OnValidate()
        {
            if (!isActiveAndEnabled)
            {
                return;
            }

            QueueFrame();
        }

        private void LateUpdate()
        {
            if (pendingRefitFrames <= 0)
            {
                return;
            }

            FrameNow();
            pendingRefitFrames--;
        }

        [ContextMenu("Frame Lobby Character")]
        public void FrameNow()
        {
            Camera cameraToFrame = ResolveCamera();
            if (cameraToFrame == null || !TryGetRenderBounds(out Bounds renderBounds))
            {
                return;
            }

            cameraToFrame.orthographic = false;
            cameraToFrame.fieldOfView = fieldOfView;
            cameraToFrame.transform.localRotation = Quaternion.Euler(cameraEulerAngles);
            SyncCameraAspect(cameraToFrame);

            float distance = CalculateDistance(cameraToFrame, renderBounds);
            Vector3 focusPoint = ResolveFocusPoint(cameraToFrame, renderBounds);
            cameraToFrame.transform.position = focusPoint - cameraToFrame.transform.forward * distance;
            AnchorFeet(cameraToFrame, renderBounds);
            cameraToFrame.farClipPlane = Mathf.Max(cameraToFrame.farClipPlane, distance + renderBounds.extents.magnitude * 3f);
        }

        private void QueueFrame()
        {
            FrameNow();
            pendingRefitFrames = Application.isPlaying ? runtimeRefitFrameCount : 1;
        }

        private Camera ResolveCamera()
        {
            if (stageCamera != null)
            {
                return stageCamera;
            }

            stageCamera = GetComponentInChildren<Camera>(true);
            return stageCamera;
        }

        private static void SyncCameraAspect(Camera cameraToFrame)
        {
            RenderTexture targetTexture = cameraToFrame.targetTexture;
            if (targetTexture == null || targetTexture.height <= 0)
            {
                return;
            }

            cameraToFrame.aspect = targetTexture.width / (float)targetTexture.height;
        }

        private bool TryGetRenderBounds(out Bounds renderBounds)
        {
            Transform root = targetRoot != null ? targetRoot : transform;
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            if (useSkinnedRenderersOnlyForFraming && TryCalculateRenderBounds(renderers, true, out renderBounds))
            {
                return true;
            }

            return TryCalculateRenderBounds(renderers, false, out renderBounds);
        }

        private static bool TryCalculateRenderBounds(Renderer[] renderers, bool skinnedOnly, out Bounds renderBounds)
        {
            renderBounds = default;
            bool hasBounds = false;

            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer == null || !renderer.enabled || !renderer.gameObject.activeInHierarchy)
                {
                    continue;
                }

                if (skinnedOnly && renderer is not SkinnedMeshRenderer)
                {
                    continue;
                }

                if (!hasBounds)
                {
                    renderBounds = renderer.bounds;
                    hasBounds = true;
                    continue;
                }

                renderBounds.Encapsulate(renderer.bounds);
            }

            return hasBounds;
        }

        private float CalculateDistance(Camera cameraToFrame, Bounds renderBounds)
        {
            float verticalFov = Mathf.Max(1f, cameraToFrame.fieldOfView) * Mathf.Deg2Rad;
            float horizontalFov = 2f * Mathf.Atan(Mathf.Tan(verticalFov * 0.5f) * Mathf.Max(0.01f, cameraToFrame.aspect));
            float heightFill = Mathf.Max(0.01f, viewportHeightFill);
            float verticalDistance = renderBounds.extents.y * boundsPadding / Mathf.Tan(verticalFov * 0.5f) / heightFill;
            float horizontalDistance = renderBounds.extents.x * boundsPadding / Mathf.Tan(horizontalFov * 0.5f);
            float weightedDistance = Mathf.Lerp(verticalDistance, Mathf.Max(verticalDistance, horizontalDistance), horizontalFitWeight);

            float scaledDistance = weightedDistance * distanceScale;
            return Mathf.Max(minimumDistance, scaledDistance);
        }

        private Vector3 ResolveFocusPoint(Camera cameraToFrame, Bounds renderBounds)
        {
            Vector3 focusPoint = renderBounds.center + ResolveStageOffset(focusOffset);
            Vector3 horizontalOffset = cameraToFrame.transform.right * (compositionOffset.x * renderBounds.size.x);
            Vector3 verticalOffset = cameraToFrame.transform.up * (compositionOffset.y * renderBounds.size.y);
            return focusPoint - horizontalOffset - verticalOffset;
        }

        private void AnchorFeet(Camera cameraToFrame, Bounds renderBounds)
        {
            if (feetViewportY <= 0f)
            {
                return;
            }

            Vector3 feetPoint = ResolveFeetAnchorPoint(renderBounds);
            Vector3 viewportPoint = cameraToFrame.WorldToViewportPoint(feetPoint);
            if (viewportPoint.z <= 0.01f)
            {
                return;
            }

            float verticalFov = Mathf.Max(1f, cameraToFrame.fieldOfView) * Mathf.Deg2Rad;
            float worldHeightAtFeet = 2f * viewportPoint.z * Mathf.Tan(verticalFov * 0.5f);
            float offset = (viewportPoint.y - feetViewportY) * worldHeightAtFeet;
            cameraToFrame.transform.position += cameraToFrame.transform.up * offset;
        }

        private Vector3 ResolveFeetAnchorPoint(Bounds renderBounds)
        {
            Animator animator = ResolveAnimator();
            if (animator == null || !animator.isHuman)
            {
                return new Vector3(renderBounds.center.x, renderBounds.min.y, renderBounds.center.z);
            }

            Vector3 sum = Vector3.zero;
            int count = 0;
            float lowestY = float.PositiveInfinity;

            AccumulateBone(animator, HumanBodyBones.LeftFoot, ref sum, ref count, ref lowestY);
            AccumulateBone(animator, HumanBodyBones.RightFoot, ref sum, ref count, ref lowestY);
            AccumulateBone(animator, HumanBodyBones.LeftToes, ref sum, ref count, ref lowestY);
            AccumulateBone(animator, HumanBodyBones.RightToes, ref sum, ref count, ref lowestY);

            if (count == 0)
            {
                return new Vector3(renderBounds.center.x, renderBounds.min.y, renderBounds.center.z);
            }

            Vector3 average = sum / count;
            return new Vector3(average.x, lowestY, average.z);
        }

        private Animator ResolveAnimator()
        {
            Transform root = targetRoot != null ? targetRoot : transform;
            return root != null ? root.GetComponentInChildren<Animator>(true) : null;
        }

        private static void AccumulateBone(
            Animator animator,
            HumanBodyBones bone,
            ref Vector3 sum,
            ref int count,
            ref float lowestY)
        {
            Transform boneTransform = animator.GetBoneTransform(bone);
            if (boneTransform == null)
            {
                return;
            }

            Vector3 position = boneTransform.position;
            sum += position;
            count++;
            lowestY = Mathf.Min(lowestY, position.y);
        }

        private Vector3 ResolveStageOffset(Vector3 localOffset)
        {
            Transform parent = transform;
            return parent != null ? parent.TransformVector(localOffset) : localOffset;
        }
    }
}
