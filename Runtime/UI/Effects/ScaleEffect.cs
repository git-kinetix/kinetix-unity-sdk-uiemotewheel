using System.Collections;
using UnityEngine;

namespace Kinetix.UI.EmoteWheel
{
    public class ScaleEffect : MonoBehaviour
    {
        [SerializeField] private float   duration;
        [SerializeField] private Vector3 minScale;
        [SerializeField] private Vector3 maxScale;

        // CACHE
        private Coroutine scaleCoroutine;
    
        public void ScaleUp()
        {
            if (scaleCoroutine != null)
                StopCoroutine(scaleCoroutine);
            scaleCoroutine = StartCoroutine(Scale(maxScale));
        }

        public void ScaleDown()
        {
            if (scaleCoroutine != null)
                StopCoroutine(scaleCoroutine);
            scaleCoroutine = StartCoroutine(Scale(minScale));
        }
    
        private IEnumerator Scale(Vector3 _EndScale)
        {
            float time = 0.0f;

            while (time < duration)
            {
                Vector3 currentScale = transform.localScale;

                transform.localScale =  Vector3.Lerp(currentScale, _EndScale, time / duration);
                time                 += Time.deltaTime;
                yield return null;
            }

            transform.localScale = _EndScale;
        }
    }
}
