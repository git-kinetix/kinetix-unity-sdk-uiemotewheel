// // ----------------------------------------------------------------------------
// // <copyright file="EmoteSelectorSlot.cs" company="Kinetix SAS">
// // Kinetix Unity SDK - Copyright (C) 2022 Kinetix SAS
// // </copyright>
// // ----------------------------------------------------------------------------

using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Kinetix.UI.EmoteWheel
{
    public class EmoteSelectorSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public int indexSlot;
        public Action<AnimationIds, int> OnSelectAnimation;
        public Action<AnimationIds, int> OnHoverAnimation;
        
        [SerializeField] private Image              mainImage;
        [SerializeField] private AnimationIcon      animationIcon;
        [SerializeField] private GameObject         selectedElements;
        [SerializeField] private ScaleEffect        scaleEffect;
        [SerializeField] private AlphaEffectImage   alphaEffect;

        private bool         isAvailable;
        private Button       button;
        private AnimationIds ids;

        private bool isMouseOn;

        private void Awake()
        {
            mainImage.alphaHitTestMinimumThreshold = 0.5f;
            isAvailable                            = false;
            button                                 = GetComponent<Button>();
            button.onClick.AddListener(Select);
        }

        private void OnDestroy()
        {
            button.onClick.RemoveAllListeners();
        }

        public void SetAnimationData(AnimationIds _Ids)
        {
            ids = _Ids;

            animationIcon.SetAwait();
            
            KinetixCore.Metadata.GetAnimationMetadataByAnimationIds(_Ids, (animationMetadata) =>
            {
                animationIcon.Set(_Ids);
                
                if (KinetixCore.Animation.IsAnimationAvailableOnLocalPlayer(_Ids))
                {
                    SetAvailable(_Ids);
                }
                else
                {
                    KinetixCore.Animation.GetNotifiedOnAnimationReadyOnLocalPlayer(_Ids, () =>
                    {
                        SetAvailable(_Ids);
                    });
                }
            });
        }

        public void SetAvailable(AnimationIds _Ids)
        {
            if (ids == null) 
                return;

            if( !_Ids.Equals(ids))
                return;

            isAvailable = true;
            animationIcon.Activate();

            if (isMouseOn)
                Hover();
        }

        public void SetAwait()
        {
            isAvailable = false;
            animationIcon.SetAwait();
        }
        
        public void SetUnavailable()
        {
            ids = null;
            isAvailable = false;
            animationIcon.Deactivate();
        }

        private void Select()
        {
            if (isAvailable)
                OnSelectAnimation?.Invoke(ids, indexSlot);
        }

        private void Hover()
        {
            if (isAvailable)
            {
                OnHoverAnimation.Invoke(ids, indexSlot);
                scaleEffect.ScaleUp();
                alphaEffect.Appear();
            }
        }

        private void Unhover()
        {
            scaleEffect.ScaleDown();
            alphaEffect.Disappear();
        }
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            isMouseOn = true;
            Hover();
        }

        public void OnPointerExit(PointerEventData  eventData)
        {
            isMouseOn = false;
            Unhover();
        }
    }
}
