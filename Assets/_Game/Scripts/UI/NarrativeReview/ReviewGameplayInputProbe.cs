using UnityEngine;

namespace DimensionBrawl.UI.NarrativeReview
{
    /// <summary>
    /// Review-only stand-in for a gameplay input owner. It carries no product input authority.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ReviewGameplayInputProbe : MonoBehaviour
    {
        public bool AcceptsReviewGameplayInput => isActiveAndEnabled;
    }
}
