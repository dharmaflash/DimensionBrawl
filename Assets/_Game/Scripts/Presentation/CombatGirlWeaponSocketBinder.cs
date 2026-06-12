using System;
using UnityEngine;

namespace DimensionBrawl.Presentation
{
    [DisallowMultipleComponent]
    public sealed class CombatGirlWeaponSocketBinder : MonoBehaviour
    {
        [Serializable]
        private sealed class SocketBinding
        {
            [SerializeField] private string label;
            [SerializeField] private Transform driver;
            [SerializeField] private Transform target;
            [SerializeField] private Vector3 localPositionOffset;
            [SerializeField] private Quaternion localRotationOffset = Quaternion.identity;

            public bool IsValid => driver != null && target != null;

            public void Configure(string bindingLabel, Transform driverTransform, Transform targetTransform)
            {
                label = bindingLabel;
                driver = driverTransform;
                target = targetTransform;
                CaptureCurrentOffset();
            }

            public void CaptureCurrentOffset()
            {
                if (!IsValid)
                {
                    return;
                }

                localPositionOffset = driver.InverseTransformPoint(target.position);
                localRotationOffset = Quaternion.Inverse(driver.rotation) * target.rotation;
            }

            public void Apply()
            {
                if (!IsValid)
                {
                    return;
                }

                target.SetPositionAndRotation(
                    driver.TransformPoint(localPositionOffset),
                    driver.rotation * localRotationOffset);
            }

            public bool IsAligned(float positionTolerance, float angleToleranceDegrees)
            {
                if (!IsValid)
                {
                    return false;
                }

                Vector3 expectedPosition = driver.TransformPoint(localPositionOffset);
                Quaternion expectedRotation = driver.rotation * localRotationOffset;
                return Vector3.Distance(target.position, expectedPosition) <= positionTolerance
                    && Quaternion.Angle(target.rotation, expectedRotation) <= angleToleranceDegrees;
            }
        }

        [SerializeField] private SocketBinding[] bindings = Array.Empty<SocketBinding>();

        public int BindingCount => bindings?.Length ?? 0;
        public bool AllBindingsValid
        {
            get
            {
                if (bindings == null || bindings.Length == 0)
                {
                    return false;
                }

                for (int i = 0; i < bindings.Length; i++)
                {
                    if (bindings[i] == null || !bindings[i].IsValid)
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        public void ConfigureWeaponSockets(
            Transform leftHand,
            Transform leftWeaponSocket,
            Transform rightHand,
            Transform rightWeaponSocket)
        {
            bindings = new[]
            {
                new SocketBinding(),
                new SocketBinding()
            };

            bindings[0].Configure("Left weapon socket", leftHand, leftWeaponSocket);
            bindings[1].Configure("Right weapon socket", rightHand, rightWeaponSocket);
        }

        public void CaptureCurrentOffsets()
        {
            if (bindings == null)
            {
                return;
            }

            for (int i = 0; i < bindings.Length; i++)
            {
                bindings[i]?.CaptureCurrentOffset();
            }
        }

        public void ApplyBindings()
        {
            if (bindings == null)
            {
                return;
            }

            for (int i = 0; i < bindings.Length; i++)
            {
                bindings[i]?.Apply();
            }
        }

        public bool AreBindingsAligned(float positionTolerance, float angleToleranceDegrees)
        {
            if (!AllBindingsValid)
            {
                return false;
            }

            for (int i = 0; i < bindings.Length; i++)
            {
                if (!bindings[i].IsAligned(positionTolerance, angleToleranceDegrees))
                {
                    return false;
                }
            }

            return true;
        }

        private void LateUpdate()
        {
            ApplyBindings();
        }
    }
}
