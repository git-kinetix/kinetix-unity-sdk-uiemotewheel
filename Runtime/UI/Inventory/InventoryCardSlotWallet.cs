// // ----------------------------------------------------------------------------
// // <copyright file="InventoryCardSlotWallet.cs" company="Kinetix SAS">
// // Kinetix Unity SDK - Copyright (C) 2022 Kinetix SAS
// // </copyright>
// // ----------------------------------------------------------------------------

using System;
using Kinetix.UI.Common;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Kinetix.UI.EmoteWheel
{
    [RequireComponent(typeof(Image))]
    [Serializable]
    public class InventoryCardSlotWallet : DraggableItem, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private TextMeshProUGUI label;
        [SerializeField] protected AnimationIcon   animationIcon;
        [SerializeField] private GameObject      outline;
        [SerializeField] private GameObject      favoriteElement;
        [SerializeField] private GameObject      goNotification;
        [SerializeField] public string UUID;

        public bool hasData;
        public  Action<AnimationIds, Vector2> OnActionStartDrag;

        private RectTransform                   rectTransform;
        private CanvasGroup                     canvasGroup;
        
        protected AnimationIds Ids   { get; private set; }
        
        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
        }

        public void SetAnimationData(AnimationIds _Ids)
        {            
            if(_Ids.Equals(Ids))
                return;
            
            UUID = _Ids.UUID;
            Ids = _Ids;
            hasData = true;
            
            animationIcon.SetAwait();
            animationIcon.Set(_Ids);

            KinetixCore.Metadata.GetAnimationMetadataByAnimationIds(_Ids, (animationMetadata) =>
            {
                label.text = animationMetadata.Name;
                if(goNotification)
                {
                    if( System.DateTime.Compare(animationMetadata.CreatedAt, System.DateTime.Now.AddDays(-KinetixConstantsEmoteWheel.c_NotificationDuration)) > 0 )
                    {
                        if(!SaveSystem.DidEmoteChecked(_Ids.UUID))
                            EnableNotificationIcon();
                    }
                }
            });

            gameObject.SetActive(true);
        }

        public void RemoveAnimationData(bool KeepAnimationIconLoaded = false)
        {
            if(!KeepAnimationIconLoaded)
                if(Ids != null && !string.IsNullOrEmpty(Ids.UUID))
                    animationIcon.Unload();
                       
            Ids = null;
            hasData = false;
            goNotification.SetActive(false);
            gameObject.SetActive(false);
        }

        protected override void OnBeginDrag()
        {
            OnActionStartDrag?.Invoke(Ids, rectTransform.sizeDelta);
        }

        protected override void OnDrag()
        {

        }

        protected override void OnEndDrag()
        {

        }

        public void EnableNotificationIcon()
        {
            if(goNotification)
                goNotification.gameObject.SetActive(true);
        }

        public void DisableNotificationIcon()
        {            
            if(goNotification)
                goNotification.gameObject.SetActive(false);
        }

        public void EnableFavorite()
        {
            favoriteElement.gameObject.SetActive(true);
        }

        public void DisableFavorite()
        {
            favoriteElement.gameObject.SetActive(false);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            outline.gameObject.SetActive(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            outline.gameObject.SetActive(false);
        }
    }
}
