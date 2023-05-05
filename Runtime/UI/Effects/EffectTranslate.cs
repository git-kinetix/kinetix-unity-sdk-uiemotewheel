using System.Collections;
using UnityEngine;

namespace Kinetix.UI.EmoteWheel
{
    public class EffectTranslate : Effect
    {
        [SerializeField] private RectTransform  rectTransform;
        [SerializeField] private Vector3        basePosition;
        [SerializeField] private Vector3        endPosition;
        [SerializeField] private float          duration;
        [SerializeField] private AnimationCurve animationCurve;

        // CACHE
        private Coroutine coroutine;
        
        public override void Play()
        {
            if (coroutine != null)
                StopCoroutine(coroutine);
            coroutine = StartCoroutine(PlayEffect());
        }
        
        public override void Stop()
        {
            if (coroutine != null)
                StopCoroutine(coroutine);
        }

        private IEnumerator PlayEffect()
        {
            float time = 0.0f;
            while (time < duration)
            {
                rectTransform.anchoredPosition =  Vector3.Lerp(basePosition, endPosition, animationCurve.Evaluate(time / duration));
                time               += Time.deltaTime;
                yield return null;
            }

            rectTransform.anchoredPosition = endPosition;
        }
    }

}
