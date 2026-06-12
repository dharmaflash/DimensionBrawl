using DimensionBrawl.Player;
using UnityEngine;

namespace DimensionBrawl.Presentation
{
    public sealed class PlayerDodgeFeedback : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerActionController actionController;
        [SerializeField] private Renderer[] targetRenderers;

        [Header("Feedback")]
        [SerializeField] private Color dodgeColor = new Color(0.75f, 1f, 1f);
        [SerializeField] private bool clearOnDodgeEnd = true;

        private MaterialPropertyBlock propertyBlock;

        private void Awake()
        {
            if (actionController == null)
            {
                actionController = GetComponent<PlayerActionController>();
            }

            propertyBlock = new MaterialPropertyBlock();
        }

        private void OnEnable()
        {
            if (actionController == null)
            {
                return;
            }

            actionController.DodgeStarted += HandleDodgeStarted;
            actionController.DodgeEnded += HandleDodgeEnded;
        }

        private void OnDisable()
        {
            if (actionController != null)
            {
                actionController.DodgeStarted -= HandleDodgeStarted;
                actionController.DodgeEnded -= HandleDodgeEnded;
            }

            ClearColor();
        }

        private void HandleDodgeStarted()
        {
            ApplyColor(dodgeColor);
        }

        private void HandleDodgeEnded()
        {
            if (clearOnDodgeEnd)
            {
                ClearColor();
            }
        }

        private void ApplyColor(Color color)
        {
            if (targetRenderers == null)
            {
                return;
            }

            for (int i = 0; i < targetRenderers.Length; i++)
            {
                Renderer targetRenderer = targetRenderers[i];
                if (targetRenderer == null)
                {
                    continue;
                }

                propertyBlock ??= new MaterialPropertyBlock();
                targetRenderer.GetPropertyBlock(propertyBlock);
                propertyBlock.SetColor("_BaseColor", color);
                propertyBlock.SetColor("_Color", color);
                targetRenderer.SetPropertyBlock(propertyBlock);
            }
        }

        private void ClearColor()
        {
            if (targetRenderers == null)
            {
                return;
            }

            for (int i = 0; i < targetRenderers.Length; i++)
            {
                if (targetRenderers[i] != null)
                {
                    targetRenderers[i].SetPropertyBlock(null);
                }
            }
        }
    }
}
