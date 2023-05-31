// // ----------------------------------------------------------------------------
// // <copyright file="InventoryCardSlotWallet.cs" company="Kinetix SAS">
// // Kinetix Unity SDK - Copyright (C) 2022 Kinetix SAS
// // </copyright>
// // ----------------------------------------------------------------------------

using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Kinetix.UI.EmoteWheel
{
    [RequireComponent(typeof(Image))]
    [Serializable]
    public class ContextCard : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private TextMeshProUGUI labelAnimation;
        [SerializeField] private TextMeshProUGUI labelEventName;
        [SerializeField] private TextMeshProUGUI labelDescription;
        [SerializeField] private AnimationIcon   animationIcon;
        [SerializeField] private GameObject      EmoteCard;
        [SerializeField] private GameObject      PlaceHere;
        [SerializeField] private GameObject      outline;
        [SerializeField] private Button          btnRemove;
        [SerializeField] public string UUID;

        public bool hasData;
        public  Action<AnimationIds, Vector2>   OnActionStartDrag;
        public Action<int>                      OnEnter;
        public Action                           OnExit;
        public Action<int>                      OnRemove;

        private int _index;
        private RectTransform                   rectTransform;
        private CanvasGroup                     canvasGroup;
        
        protected AnimationIds Ids   { get; private set; }
        
        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            btnRemove.onClick.AddListener(OnHitRemove);
            HidePlaceHere(false);
        }

        private void OnHitRemove()
        {
            OnRemove?.Invoke(_index);
        }

        public void Init(int index)
        {
            _index = index;            
        }

        public void SetAnimationData(AnimationIds _Ids)
        {
            // if(_Ids.Equals(Ids))
            //    return;
            
            UUID = _Ids.UUID;
            Ids = _Ids;
            hasData = true;
            
            animationIcon.SetAwait();
            animationIcon.Set(_Ids);

            KinetixCore.Metadata.GetAnimationMetadataByAnimationIds(_Ids, (animationMetadata) =>
            {
                labelAnimation.text = animationMetadata.Name;
            });
            HidePlaceHere(true);
        }

        public void SetIcon()
        {
            if(UUID != string.Empty)
                SetAnimationData(Ids);
        }

        private void HidePlaceHere(bool bflag)
        {
            if(bflag)
            {
                PlaceHere.SetActive(false);
                EmoteCard.SetActive(true);
            } 
            else 
            {
                PlaceHere.SetActive(true);
                EmoteCard.SetActive(false);
            }                
        }

        public void SetEventContextData(ContextualEmote contextEmote)
        {
            labelEventName.text = contextEmote.ContextName;
            labelDescription.text = contextEmote.ContextDescription;
        }
        
        public void RemoveAnimationData(bool KeepAnimationIconLoaded = false)
        {
            if(!KeepAnimationIconLoaded)
                if(Ids != null && !string.IsNullOrEmpty(Ids.UUID))
                    animationIcon.Unload(Ids);
                       
            Ids = null;
            hasData = false;
            HidePlaceHere(false);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            OnEnter?.Invoke(_index);
            outline.gameObject.SetActive(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            OnExit?.Invoke();
            outline.gameObject.SetActive(false);
        }

        public void ShowOutline() 
        {
            outline.gameObject.SetActive(true);
        }

        public void HideOutline()
        {
            outline.gameObject.SetActive(false);
        }

        public void ShowTrash() 
        {
            ColorBlock cb = btnRemove.colors;
            cb.normalColor = Color.white;
            btnRemove.colors = cb;
        }

        public void HideTrash()
        {
            ColorBlock cb = btnRemove.colors;
            cb.normalColor = new Color(255,255,255,0);
            btnRemove.colors = cb;
        }

        public Vector3 GetPositionEmoteCard()
        {
            if (EmoteCard != null)
            {
                return EmoteCard.transform.position;
            }
            return Vector3.zero;
        }
    }
}
