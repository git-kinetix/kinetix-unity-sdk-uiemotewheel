// // ----------------------------------------------------------------------------
// // <copyright file="EmoteSelectorView.cs" company="Kinetix SAS">
// // Kinetix Unity SDK - Copyright (C) 2022 Kinetix SAS
// // </copyright>
// // ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Kinetix.UI.Common;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Kinetix.UI.EmoteWheel
{
    public class EmoteSelectorView : CategoryView
    {
        public Action<AnimationIds> OnSelectAnimation;
        public Action               OnRefillWheel;
    
        [SerializeField] List<AnimationIds> listFavorites = new List<AnimationIds>();
        [SerializeField] private List<EmoteSelectorSlot> emoteSelectorSlots;
        [SerializeField] private PagingSystem            emotePagingSystem;
        [SerializeField] private TextMeshProUGUI         animationLabel;
        
        // CACHE
        private int                             currentPageIndex;
        private Dictionary<int, AnimationIds>   favoritesIdByIndex;
        private int                             currentSlotIndex = -1;
        private int                             c_CountFavoritePagesCurrent;
    
        protected override void Awake()
        {
            base.Awake();
            animationLabel.text = "";
            currentPageIndex    = 0;
            int currentSlot = 0;
            emoteSelectorSlots.ForEach(selector =>
            {
                selector.indexSlot = currentSlot++;
                selector.OnHoverAnimation  += HoverAnimation;
                selector.OnSelectAnimation += SelectAnimation;
            });
        }
        
        public void Init()
        {
            KinetixInputManager.OnHitNextPage += OnChangedEmotesNextPage;
            KinetixInputManager.OnHitPrevPage += OnChangedEmotesPreviousPage;
            KinetixInputManager.OnNavigate += OnNavigateEmote;
            KinetixInputManager.OnCancelNavigate += OnCancelNavigateEmote;
            KinetixInputManager.OnSelect += OnSelectEmote;

            emotePagingSystem.OnSwitchNextPage     += OnChangedEmotesNextPage;
            emotePagingSystem.OnSwitchPreviousPage += OnChangedEmotesPreviousPage;

            emotePagingSystem.Init();
        }
    
        protected override void OnDestroy()
        {
            base.OnDestroy();
            emoteSelectorSlots.ForEach(selector =>
            {
                selector.OnHoverAnimation  -= HoverAnimation;
                selector.OnSelectAnimation -= SelectAnimation;
            });

            if (emotePagingSystem != null)
            {
                emotePagingSystem.OnSwitchNextPage     -= OnChangedEmotesNextPage;
                emotePagingSystem.OnSwitchPreviousPage -= OnChangedEmotesPreviousPage;
            }

            KinetixInputManager.OnHitNextPage -= OnChangedEmotesNextPage;
            KinetixInputManager.OnHitPrevPage -= OnChangedEmotesPreviousPage;

            KinetixInputManager.OnNavigate -= OnNavigateEmote;
            KinetixInputManager.OnSelect -= OnSelectEmote;
        }

        public void Load()
        {
            FillSlots(favoritesIdByIndex);
        }

        public void FillSlots(Dictionary<int, AnimationIds> _FavoritesIdByIndex)
        {
            favoritesIdByIndex = _FavoritesIdByIndex;
            
            emoteSelectorSlots.ForEach(slot  => slot.SetUnavailable());
            
            if (_FavoritesIdByIndex == null)
                return;

            listFavorites.Clear();
            c_CountFavoritePagesCurrent = 0;
            List<AnimationIds> listFavoritesByPage = new List<AnimationIds>();
            bool bFlag = false;
            for (int iPage = 0; iPage<KinetixConstantsEmoteWheel.c_CountFavoritePages; iPage++)
            {
                bFlag = false;
                for(int iSlot=0; iSlot<KinetixConstantsEmoteWheel.c_CountSlotOnWheel; iSlot++)
                {
                    if( favoritesIdByIndex.ContainsKey( GetRealIndexFavorites(iSlot, iPage)))
                    {
                        listFavoritesByPage.Add( favoritesIdByIndex[GetRealIndexFavorites(iSlot, iPage)] );
                        bFlag = true;
                    }
                    else
                    {
                        listFavoritesByPage.Add( null );
                    }
                }
                if(bFlag)
                {
                    listFavorites.AddRange(listFavoritesByPage);
                    c_CountFavoritePagesCurrent++;
                }
                listFavoritesByPage.Clear();
            }

            if( currentPageIndex > c_CountFavoritePagesCurrent )
            {
                currentPageIndex = 0;
                Load();
                return;
            }

            if(listFavorites.Count <= 0)
                return;            

            for (int iSlot=0; iSlot<KinetixConstantsEmoteWheel.c_CountSlotOnWheel; iSlot++)
            {
                if( GetRealPageIndexFavorites(iSlot) >= listFavorites.Count )
                    continue;

                if(listFavorites[GetRealPageIndexFavorites(iSlot)] == null)
                    continue;

                KinetixCore.Metadata.IsAnimationOwnedByUser(listFavorites[GetRealPageIndexFavorites(iSlot)], owned =>
                {
                    EmoteSelectorSlot slot = emoteSelectorSlots[iSlot];
                    
                    if (owned)
                    {
                        slot.SetAnimationData( listFavorites[GetRealPageIndexFavorites(iSlot)] );
                    } 
                    else
                    {
                        slot.SetUnavailable();
                    }
                });
            }

            UnselectAllEmotes();
            RefreshPages();
        }
        
		private void RefreshPages()
		{
			emotePagingSystem.UpdatePageLabel(currentPageIndex, c_CountFavoritePagesCurrent);
		}

        private void OnChangedEmotesNextPage()
        {
            currentPageIndex++;
            currentPageIndex = (int)Mathf.Repeat(currentPageIndex, c_CountFavoritePagesCurrent);
            OnRefillWheel?.Invoke();
        }

        private void OnChangedEmotesPreviousPage()
        {
            currentPageIndex--;
            currentPageIndex = (int)Mathf.Repeat(currentPageIndex, c_CountFavoritePagesCurrent);
            OnRefillWheel?.Invoke();
        }
        
        private int GetRealPageIndexFavorites(int _Index)
        {
            return _Index + KinetixConstantsEmoteWheel.c_CountSlotOnWheel * currentPageIndex;
        }

        private int GetRealIndexFavorites(int _Index, int _Page)
        {
            return _Index + KinetixConstantsEmoteWheel.c_CountSlotOnWheel * _Page;
        }

        private void HoverAnimation(AnimationIds _Ids, int slotIndex)
        {
            UnselectAllEmotes(slotIndex);
            KinetixCore.Metadata.GetAnimationMetadataByAnimationIds(_Ids, (metadata =>
            {
                animationLabel.text = metadata.Name;
            }));
        }
        
        private void SelectAnimation(AnimationIds ids, int slotIndex)
        {
            KinetixAnalytics.SendEvent("Play_Animation", ids.UUID, KinetixAnalytics.Page.EmoteWheel, KinetixAnalytics.Event_type.Click, slotIndex+1);

            OnSelectAnimation?.Invoke(ids);
        }

        private void OnSelectEmote()
        {
            if(currentSlotIndex != -1)
                ExecuteEvents.Execute(emoteSelectorSlots[currentSlotIndex].gameObject, new BaseEventData(EventSystem.current), ExecuteEvents.submitHandler);
        }

        private void OnNavigateEmote(Vector2 direction)
        {
            if(Visible)
            {
                currentSlotIndex = KinetixUtils.GetIndexFromAWheel(direction, KinetixConstantsEmoteWheel.c_CountSlotOnWheel, 3);
                emoteSelectorSlots[currentSlotIndex].OnPointerEnter(null);
            }
        }

        private void UnselectAllEmotes(int slotToNotUnselect = -1)
        {
            emoteSelectorSlots.ForEach(selector =>
            {
                if( slotToNotUnselect != selector.indexSlot )
                    selector.OnPointerExit(null);
                    
            });
        }

        private void OnCancelNavigateEmote()
        {
            currentSlotIndex = -1;
            UnselectAllEmotes();   
        }
    }
}
