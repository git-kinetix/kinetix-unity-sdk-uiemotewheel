using UnityEngine;
using UnityEngine.UI;
using System.Threading;
using Kinetix.Utils;
using Kinetix.UI.Common;

namespace Kinetix.UI.EmoteWheel
{
    public class AnimationIcon : MonoBehaviour
    {
        [SerializeField] private Image      img;
        [SerializeField] private GameObject spinner;
        [SerializeField] private Sprite     emptyIcon;
        [SerializeField] public AnimationIds ids;

        private bool fetchedIcon;
        private CancellationTokenSource cancellationTokenSource;

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
            cancellationTokenSource = new CancellationTokenSource();

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
                    if (img != null)
                    {
                        img.transform.localScale = Vector3.one;
                        SetSprite(emptyIcon);
                    }
                }
                
                fetchedIcon = true;

                if (_Ids != null && !_Ids.Equals(ids))
                {
                    KinetixIconLoadingManager.RegisterLoadedIconForIds(_Ids, this);
                    ids = _Ids;
                }
                
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

        public void Unload()
        {
            KinetixIconLoadingManager.UnregisterLoadedIdsForIcon(ids, this);
            ids = null;
        }

        private void SetSprite(Sprite _Sprite)
        {
            if(img == null)
                return;
        
            img.sprite = _Sprite;                
        }
    }
}
