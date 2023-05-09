// // ----------------------------------------------------------------------------
// // <copyright file="InventoryCardSlotFavorite.cs" company="Kinetix SAS">
// // Kinetix Unity SDK - Copyright (C) 2022 Kinetix SAS
// // </copyright>
// // ----------------------------------------------------------------------------

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.InputSystem;

namespace Kinetix.UI.EmoteWheel
{
    public class InventoryCardSlotFavorite : DraggableItem, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Image          mainImage;
        [SerializeField] private Image          outlineHover;
        [SerializeField] private AnimationIcon  animationIcon;
        [SerializeField] private Image          trashIcon;

        public bool         HasData { get; private set; }
        public AnimationIds Ids     { get; private set; }

        // CACHE
        private int index;

        public Action<int> OnEnter;
        public Action      OnExit;
        public Action<int> OnRemove;
        public Action<AnimationIds, int, RectTransform> OnEndDragRemove;
        public Action OnDown;
        public Action OnUp;

        private Vector3 startingPoint;
        private Vector3 endingPoint;

        private void Awake()
        {
            mainImage.alphaHitTestMinimumThreshold = 0.1f;
            HasData                                = false;

            trashIcon.gameObject.GetComponent<RectTransform>().localEulerAngles = new Vector3( 0f, 0f, -transform.parent.GetComponent<RectTransform>().localEulerAngles.z);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            OnEnter?.Invoke(index);
            
            outlineHover.gameObject.SetActive(true);
            if (!HasData)
                return;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            OnExit?.Invoke();

            outlineHover.gameObject.SetActive(false);

            if (!HasData)
                return;
        }

        public void Init(int _Index)
        {
            index = _Index;
            HideAnimationData();
        }

        public void SetAnimationData(AnimationIds _AnimationIds)
        {
            Ids = _AnimationIds;

            animationIcon.SetAwait();
            KinetixCore.Metadata.GetAnimationMetadataByAnimationIds(_AnimationIds, (animationMetadata) =>
            {
                animationIcon.Set(_AnimationIds);
                animationIcon.Activate();
            });
        }

        public void SetAwait()
        {
            animationIcon.SetAwait();
        }
        
        public void Remove()
        {
            Empty();
        }

        public void Empty()
        {
            outlineHover.gameObject.SetActive(false);
            HideAnimationData();
        }

        public void ShowAnimationData()
        {
            HasData = true;
            animationIcon.Activate();
        }

        private void HideAnimationData()
        {
            HasData = false;
            animationIcon.Deactivate();
        }

        public void ShowOutline() 
        {
            outlineHover.gameObject.SetActive(true);
        }

        public void HideOutline()
        {
            outlineHover.gameObject.SetActive(false);
        }

        public void ShowTrash()
        {
            trashIcon.gameObject.SetActive(true);
            outlineHover.gameObject.SetActive(true);
        }

        public void HideTrash()
        {
            trashIcon.gameObject.SetActive(false);
            outlineHover.gameObject.SetActive(false);
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);
            if(HasData)
                OnDown?.Invoke();
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            base.OnPointerUp(eventData);
            if(HasData)
                OnUp?.Invoke();
        }

        protected override void OnBeginDrag()
        {
            if(HasData)
            {
                startingPoint = transform.position;
                endingPoint = transform.parent.GetComponent<RectTransform>().position;
            }
        }

        protected override void OnDrag()
        {
            if(HasData)
            {
                Vector3 pointToGo = new Vector3(KinetixInputManager.PositionTouchOrMouse().x, KinetixInputManager.PositionTouchOrMouse().y, transform.parent.position.z);
                transform.position = UtilsPoint.GetClosestPointOnFiniteLine(pointToGo, startingPoint, endingPoint );              
            }
        }

        protected override void OnEndDrag()
        {
            if(HasData)
            {
                if( (Vector3.Distance(transform.position, endingPoint) / Vector3.Distance(startingPoint, endingPoint)) <0.3f)
                {
                    OnRemove?.Invoke(index);
                    transform.position = startingPoint;
                } 
                else 
                {
                    StartCoroutine(BackToStartingPoint());
                }
            }
        }

        private IEnumerator BackToStartingPoint()
        {            
            while(Vector3.Distance(transform.position, startingPoint)> 1){
                transform.position = Vector3.Lerp(transform.position, startingPoint, 0.2f);
                yield return new WaitForEndOfFrame();
            }
        }
    }
}
