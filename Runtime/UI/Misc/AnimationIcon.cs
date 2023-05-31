using UnityEngine;
using UnityEngine.UI;
using System.Threading;
using Kinetix.Utils;

namespace Kinetix.UI.EmoteWheel
{
    public class AnimationIcon : MonoBehaviour
    {
        [SerializeField] private Image      img;
        [SerializeField] private GameObject spinner;
        [SerializeField] private Sprite     emptyIcon;

        private bool fetchedIcon;
        private TokenCancel cancellationTokenSource;

        private void Awake()
        {
            fetchedIcon = false;
        }
        
        public void SetAwait()
        {
            if (img != null)
                img.gameObject.SetActive(false);
            if (spinner != null)
                spinner.gameObject.SetActive(true);
        }

        public void Set(AnimationIds _Ids)
        {
            if (cancellationTokenSource != null)
            {
                cancellationTokenSource.Cancel();
                cancellationTokenSource = null;
            }

            fetchedIcon = false;
            cancellationTokenSource = new TokenCancel();

            KinetixCore.Metadata.LoadIconByAnimationId(_Ids, (sprite) =>
            {
                if (cancellationTokenSource == null)
                    return;

                cancellationTokenSource = null;
                
                if (sprite != null)
                {
                    if (img != null)
                    {
                        img.transform.localScale = Vector3.one * 2.0f;
                        SetSprite(sprite);
                    }
                }
                else
                {
                    Debug.Log("AnimationIcon Ids " +_Ids.UUID );
                    if (img != null)
                    {
                        Debug.Log("AnimationIcon Ids 2 " +_Ids.UUID );
                        img.transform.localScale = Vector3.one;
                        SetSprite(emptyIcon);
                    }
                }
                
                fetchedIcon = true;
                Activate();
            }, cancellationTokenSource);
        }

        public void Activate()
        {   
            if (fetchedIcon)
            {
                if (spinner != null)
                    spinner.SetActive(false);
                if (img != null)
                    img.gameObject.SetActive(true);
            }
            else
            {
                if (spinner != null)
                    spinner.SetActive(true);
                if (img != null)
                    img.gameObject.SetActive(false);
            }
        }

        public void Deactivate()
        {
            if (cancellationTokenSource != null)
            {
                cancellationTokenSource.Cancel();
                cancellationTokenSource = null;
            }


            if (spinner != null)
            {
                spinner.gameObject.SetActive(false);
            }


            if (img != null)
            {
                img.gameObject.SetActive(false);
            }
        }

        public void Unload(AnimationIds _Ids)
        {
            KinetixCore.Metadata.UnloadIconByAnimationId(_Ids);
        }        
        
        private void SetSprite(Sprite _Sprite)
        {
            if(img == null)
                return;            
        
            img.sprite = _Sprite;                
        }
    }
}
