using DimensionBrawl.Presentation.Narrative;
using UnityEngine;

namespace DimensionBrawl.UI.NarrativeReview
{
    /// <summary>
    /// Records a review tutorial-start signal. It does not start or complete a product tutorial.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ReviewTutorialStartProbe : MonoBehaviour
    {
        public int DispatchCount { get; private set; }
        public long LastGeneration { get; private set; }
        public StoryTutorialReviewReceipt LastReceipt { get; private set; }

        public bool TryRecord(StoryTutorialReviewReceipt receipt)
        {
            if (!receipt.CanDispatchReviewTutorialStart
                || receipt.Generation <= LastGeneration)
            {
                return false;
            }

            LastGeneration = receipt.Generation;
            LastReceipt = receipt;
            DispatchCount++;
            return true;
        }

        public bool WasDispatchedFor(long generation)
        {
            return generation > 0 && generation == LastGeneration;
        }
    }
}
