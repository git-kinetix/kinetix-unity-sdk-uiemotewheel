using System.Collections;
using UnityEngine;

namespace Kinetix.UI.EmoteWheel
{
    public class EffectScale : Effect
    {
        [SerializeField] private Transform      tr;
        [SerializeField] private Vector3        baseScale;
        [SerializeField] private Vector3        endScale;
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
                tr.localScale =  Vector3.Lerp(baseScale, endScale, animationCurve.Evaluate(time / duration));
                time          += Time.deltaTime;
                yield return null;
            }

            tr.localScale = endScale;
        }
    }
}

