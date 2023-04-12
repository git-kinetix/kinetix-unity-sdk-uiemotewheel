using System.Collections;
using UnityEngine;

namespace Kinetix.UI.EmoteWheel
{
    public class EffectAlphaCanvasGroup : Effect
    {
        [SerializeField] private CanvasGroup    canvasGroup;
        [SerializeField] private float          startAlpha;
        [SerializeField] private float          endAlpha;
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
                canvasGroup.alpha =  Mathf.Lerp(startAlpha, endAlpha, animationCurve.Evaluate(time / duration));
                time              += Time.deltaTime;
                yield return null;
            }

            canvasGroup.alpha = endAlpha;
        }
    }
}
