using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Kinetix.UI.EmoteWheel
{
    public class AlphaEffectImage : MonoBehaviour
    {        
        [SerializeField] private float duration;
        [SerializeField] private float minAlpha;
        [SerializeField] private float maxAlpha;

        private Image image;

        // CACHE
        private Coroutine alphaCoroutine;

        private void Awake()
        {
            image = GetComponent<Image>();
        }
    
        public void Appear()
        {
            // gameObject.SetActive(true);
            if (alphaCoroutine != null)
                StopCoroutine(alphaCoroutine);
            alphaCoroutine = StartCoroutine(Play(maxAlpha));
        }

        public void Disappear()
        {
            if (alphaCoroutine != null)
                StopCoroutine(alphaCoroutine);
            alphaCoroutine = StartCoroutine(Play(minAlpha));
        }
    
        private IEnumerator Play(float _EndAlpha)
        {
            float time = 0.0f;

            while (time < duration)
            {
                float currentAlpha = image.color.a;
                image.color = new Color(image.color.r, image.color.g, image.color.b, Mathf.Lerp(currentAlpha, _EndAlpha, time / duration));
                time += Time.deltaTime;
                yield return null;
            }

            image.color = new Color(image.color.r, image.color.g, image.color.b, _EndAlpha);
            // if(_EndAlpha == 0f)
            //     gameObject.SetActive(false);
        }
    }
}
