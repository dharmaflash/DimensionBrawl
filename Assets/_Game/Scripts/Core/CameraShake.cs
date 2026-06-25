using System.Collections;
using UnityEngine;

namespace IsekaiBrawl.Gameplay
{
    public class CameraShake : MonoBehaviour
    {
        public static CameraShake Instance { get; private set; }

        public Vector3 CurrentOffset { get; private set; }

        private Coroutine shakeRoutine;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        public void PlayShake(float duration, float magnitude)
        {
            if (shakeRoutine != null)
            {
                StopCoroutine(shakeRoutine);
            }

            shakeRoutine = StartCoroutine(Shake(duration, magnitude));
        }

        public IEnumerator Shake(float duration, float magnitude)
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                Vector3 offset = Random.insideUnitSphere * magnitude;
                offset.z = 0f;
                CurrentOffset = offset;
                yield return null;
            }

            CurrentOffset = Vector3.zero;
            shakeRoutine = null;
        }
    }
}
