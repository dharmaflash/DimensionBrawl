using System.Collections;
using UnityEngine;

namespace DimensionBrawl.Presentation
{
    [DisallowMultipleComponent]
    public sealed class CinematicSequenceAutoPlay : MonoBehaviour
    {
        [SerializeField] private CinematicSequenceRunner runner;
        [SerializeField] private bool playOnStart = true;
        [SerializeField, Min(0f)] private float startDelaySeconds = 0.1f;

        private IEnumerator Start()
        {
            if (!playOnStart)
            {
                yield break;
            }

            if (startDelaySeconds > 0f)
            {
                yield return new WaitForSeconds(startDelaySeconds);
            }

            if (runner == null)
            {
                runner = GetComponent<CinematicSequenceRunner>();
            }

            runner?.TryPlay();
        }
    }
}
