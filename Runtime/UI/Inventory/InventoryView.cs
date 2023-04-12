// // ----------------------------------------------------------------------------
// // <copyright file="InventoryView.cs" company="Kinetix SAS">
// // Kinetix Unity SDK - Copyright (C) 2022 Kinetix SAS
// // </copyright>
// // ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Kinetix.UI.Common;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Diagnostics;
using UnityEngine.EventSystems;
using Debug=UnityEngine.Debug;
using UnityEngine.InputSystem;

namespace Kinetix.UI.EmoteWheel
{
    public class InventoryView : CategoryView
    {
        [Header("InventoryBank")]
        [SerializeField] private Transform parentInventoryWalletGrid;
        [SerializeField] private ScrollRect InventoryWalletScrollRect;
        [SerializeField] private GameObject inventoryCardSlotWalletPrefab;

        [Header("Draggable")]
        [SerializeField] private Transform  parentInventoryDraggable;
        [SerializeField] private GameObject inventoryDraggableCardPrefab;
        [SerializeField] private GameObject inventoryDraggableFavoritePrefab;

        [Header("InventoryWheel")]
        [SerializeField] private Transform  parentInventoryWheel;
        [SerializeField] private List<InventoryCardSlotFavorite> inventoryCardSlotFavorites;
        [SerializeField] private PagingSystem favoritePagingSystem;

        [Header("Favorites Remove Zone")]
        [SerializeField] private GameObject inventoryRemoveBtn;
        
        // CACHE
        private Dictionary<string, InventoryCardSlotWallet> cardsSlotWalletByAnimationUUID;
        [SerializeField] private List<InventoryCardSlotWallet> listCardsSlotWalletByIndex;
        private Dictionary<int, AnimationIds>               cardsIdsByIndex;
        private Dictionary<int, InventoryCardSlotFavorite>  cardsSlotFavoriteByIndex;
        private Dictionary<int, AnimationIds>  favoritesIdByIndex;
        
        private int currentWheelSlotIndex;
        private int currentPageIndexFavorites;
        private int currentBankIndexSelected;
        private int currentFavoriteSlotForAdd;
        private int currentFirstCardVisible;
        private int previousFirstCardVisible;
        private int currentTotalCard;
        private int amountCardVisible = 30;
        private int bufferPool = 50;
        private int cardByRow = 6;
        private int CellSizeX = 92;
        private int CellSizeY = 135;
        private int SpacingX = 15;
        private int SpacingY = 25;
        private int Padding = 15;

        private IEnumerator DelayExitFavorite;

        public Action<int, AnimationIds> OnAddFavorite;
        public Action<int>               OnRemoveFavorite;
        public Action                    OnRefillFavorites;
        public Action                    OnRefillBank;


        private enum State { NORMAL, ADDED, DELETE };
        private State currentState = State.NORMAL;

        public void Init()
        {
            currentWheelSlotIndex     = -1;
            currentPageIndexFavorites = 0;
            currentBankIndexSelected  = 0;
            currentFirstCardVisible = -1;
            previousFirstCardVisible = -1;

            cardsSlotWalletByAnimationUUID  ??= new Dictionary<string, InventoryCardSlotWallet>();
            cardsIdsByIndex                  ??= new Dictionary<int, AnimationIds>();
            cardsSlotFavoriteByIndex        ??= new Dictionary<int, InventoryCardSlotFavorite>();
            listCardsSlotWalletByIndex      ??= new List<InventoryCardSlotWallet>();

            KinetixInputManager.OnHitNextPage +=  OnChangedFavoriteNextPage;
            KinetixInputManager.OnHitPrevPage += OnChangedFavoritePreviousPage;

            KinetixInputManager.OnNavigate += OnNavigateBank;
            KinetixInputManager.OnCancelNavigate += OnCancelNavigateBank;

            KinetixInputManager.OnSelect += OnSelect;
            KinetixInputManager.OnCancel += OnCancel;
            KinetixInputManager.OnDeleteMode += OnDeleteMode;

            favoritePagingSystem.OnSwitchNextPage     += OnChangedFavoriteNextPage;
            favoritePagingSystem.OnSwitchPreviousPage += OnChangedFavoritePreviousPage;
            favoritePagingSystem.Init();

            InventoryWalletScrollRect.onValueChanged.AddListener(OnScrollRectValueChanged);

            InventoryWalletScrollRect.elasticity = 0.05f;
#if UNITY_EDITOR
            InventoryWalletScrollRect.scrollSensitivity = 6;
#elif UNITY_WEBGL
            InventoryWalletScrollRect.elasticity = 0.02f;
            InventoryWalletScrollRect.scrollSensitivity = 2;
#else
            InventoryWalletScrollRect.scrollSensitivity = 4;
#endif

            CreatePoolCard();
            InitInventoryFavoriteCardSlots();
            
            Reload();
            HideRemoveButton();
        }

        private void Reload()
        {
            RefreshInventoryBankAnimations();
            RefreshPages();
        }
        
        private void RefreshPages()
        {
            favoritePagingSystem.UpdatePageLabel(currentPageIndexFavorites, KinetixConstantsEmoteWheel.c_CountFavoritePages);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (favoritePagingSystem != null)
            {
                favoritePagingSystem.OnSwitchNextPage     -= OnChangedFavoriteNextPage;
                favoritePagingSystem.OnSwitchPreviousPage -= OnChangedFavoritePreviousPage;

                KinetixInputManager.OnHitNextPage -=  OnChangedFavoriteNextPage;
                KinetixInputManager.OnHitPrevPage -= OnChangedFavoritePreviousPage;

                KinetixInputManager.OnSelect -= OnSelect;
            }

            if (InventoryWalletScrollRect != null)
                InventoryWalletScrollRect.onValueChanged.RemoveListener(OnScrollRectValueChanged);	
        }


        private List<InventoryCardSlotWallet> pooledCardWallet;
        void CreatePoolCard()
        {
            pooledCardWallet = new List<InventoryCardSlotWallet>();
            InventoryCardSlotWallet tmp;
            for(int i = 0; i < bufferPool; i++)
            {
                tmp = Instantiate(inventoryCardSlotWalletPrefab, parentInventoryWalletGrid).GetComponent<InventoryCardSlotWallet>();
                tmp.OnActionStartDrag += OnStartDragInventoryCardWalletSlot;
                tmp.gameObject.SetActive(false);
                pooledCardWallet.Add(tmp);
            }
        }


        private InventoryCardSlotWallet GetPooledCard()
        {
            for(int i = 0; i < bufferPool; i++)
            {
                if(!pooledCardWallet[i].hasData)
                {
                    return pooledCardWallet[i];
                }
            }
            return null;
        }


        private void EmptyPool()
        {
            for(int i = 0; i < bufferPool; i++)
            {
                pooledCardWallet[i].RemoveAnimationData();
            }            
        }


        public void RefreshInventoryBankAnimations ()
        {
            KinetixCore.Metadata.GetUserAnimationMetadatas(animationMetadatas =>
            {
                //Refresh only if there are new cards to show
                if( currentTotalCard >= animationMetadatas.Length )
                    return;

                currentTotalCard = animationMetadatas.Length;

                //refresh size of the background of the grid, to make the scrollbar effective
                RectTransform rt = (parentInventoryWalletGrid.transform as RectTransform);
                rt.sizeDelta = new Vector2 (rt.sizeDelta.x, ((int)((currentTotalCard-1)/cardByRow)+1) * (CellSizeY+SpacingY) + Padding/2);

                cardsIdsByIndex.Clear();
                for (int i = 0; i < currentTotalCard; i++)
                {
                    cardsIdsByIndex[i] = animationMetadatas[i].Ids;
                    if( i >= listCardsSlotWalletByIndex.Count )
                    {
                        listCardsSlotWalletByIndex.Add(null);
                    }
                }

                UpdateCardsPool((1-InventoryWalletScrollRect.verticalScrollbar.value), true);
            });
        }


        private float prevScrollRectY = -1;
        private void OnScrollRectValueChanged (Vector2 valueScrollRect)
        {
            if(valueScrollRect.y != prevScrollRectY)
            {
                prevScrollRectY = valueScrollRect.y;
                UpdateCardsPool(1-valueScrollRect.y);
            }            
        }

        //percent is from 0f to 1f, Of is top of the scroll
        private void UpdateCardsPool (float percentSlider, bool forceUpdate = false)
        {
            currentFirstCardVisible = GetIndexFirstCardVisible(percentSlider);

            if(currentFirstCardVisible == previousFirstCardVisible && !forceUpdate)
                return;
            
            previousFirstCardVisible = currentFirstCardVisible;
            cardsSlotWalletByAnimationUUID.Clear();

            //empty the cardSlot first
            for (int i = 0; i < currentTotalCard; i++)
            {
                if (i < currentFirstCardVisible || i >= (currentFirstCardVisible+amountCardVisible) )
                {
                    if( listCardsSlotWalletByIndex[i] != null )
                    {
                        //only remove animation icon who are not on the favorite
                        listCardsSlotWalletByIndex[i].RemoveAnimationData ( IsAnimationIsInTheFavorite (i));
                        listCardsSlotWalletByIndex[i] = null;
                    }
                }
            }

            //fill the cardSlot needed
            for (int i = 0; i < currentTotalCard; i++)
            {
                if (i >= currentFirstCardVisible && i < (currentFirstCardVisible+amountCardVisible) )
                {                    
                    if( listCardsSlotWalletByIndex[i] == null )
                        SetCardPool(i);

                    cardsSlotWalletByAnimationUUID.Add(cardsIdsByIndex[i].UUID, listCardsSlotWalletByIndex[i]);    

                    if( currentBankIndexSelected == i )
                        listCardsSlotWalletByIndex[i].OnPointerEnter(null);
                }
            }

            //set the favorite on wallet card
            if (cardsSlotFavoriteByIndex.Count == KinetixConstantsEmoteWheel.c_CountSlotOnWheel) 
            {
                for (int i = 0; i < KinetixConstantsEmoteWheel.c_CountSlotOnWheel; i++)
                {
                    if (cardsSlotFavoriteByIndex[i].HasData && cardsSlotWalletByAnimationUUID.ContainsKey(cardsSlotFavoriteByIndex[i].Ids.UUID) )
                    {
                        cardsSlotWalletByAnimationUUID[cardsSlotFavoriteByIndex[i].Ids.UUID].EnableFavorite();
                    }
                }
            }
        }

        private bool IsAnimationIsInTheFavorite(int indexCard)
        {
            foreach (KeyValuePair<int, AnimationIds> favoriteKVP in favoritesIdByIndex)
            {
                if (favoriteKVP.Value.UUID == cardsIdsByIndex[indexCard].UUID)
                    return true;
            }
            return false;
        }

        //set the pool card, at the good position and with the good metadata
        private void SetCardPool (int index)
        {
            listCardsSlotWalletByIndex[index] = GetPooledCard();
            listCardsSlotWalletByIndex[index].SetAnimationData(cardsIdsByIndex[index]);
            listCardsSlotWalletByIndex[index].DisableFavorite();

            int posX = (int)(index%cardByRow) * (CellSizeX+SpacingX) + CellSizeX/2 + Padding;
            int posY = -(int)(index/cardByRow) * (CellSizeY+SpacingY) - CellSizeY/2 - Padding;

            (listCardsSlotWalletByIndex[index].transform  as RectTransform).sizeDelta = new Vector2(CellSizeX, CellSizeY);
            (listCardsSlotWalletByIndex[index].transform  as RectTransform).localPosition = new Vector3(posX, posY, 0f);

            //set the selected red outline visible
            if(index == currentBankIndexSelected)
                ExecuteEvents.Execute( listCardsSlotWalletByIndex[index].gameObject, new PointerEventData(EventSystem.current), ExecuteEvents.pointerEnterHandler);
            else
                ExecuteEvents.Execute( listCardsSlotWalletByIndex[index].gameObject, new PointerEventData(EventSystem.current), ExecuteEvents.pointerExitHandler);
        }

        private int GetIndexFirstCardVisible (float percentSlider)
        {
            percentSlider = Mathf.Clamp(percentSlider, 0f, 1f);
            int offset = 10;
            if( currentTotalCard > amountCardVisible )
            {
                int indexCard = (int)KinetixUtils.Remap(percentSlider, 0f, 1f, -offset, currentTotalCard-amountCardVisible+offset);
                if(indexCard<0) indexCard = 0;
                if(indexCard>(currentTotalCard-amountCardVisible) ) indexCard = currentTotalCard-amountCardVisible;
                return indexCard;
            }            
            return 0;
        }
        

        Coroutine scrollToPosition;
        private void UpdateScrollPosition()
        {
            int lineWhereIndexIsSelected = currentBankIndexSelected/cardByRow;
            float positionYContent = lineWhereIndexIsSelected * (CellSizeY + SpacingY);
            float maxHeight = InventoryWalletScrollRect.content.sizeDelta.y - (InventoryWalletScrollRect.transform as RectTransform).sizeDelta.y;

            //if height of the grid is inferior to the scrollRect, so no need to scroll
            if(maxHeight < 0f) return;

            positionYContent = Mathf.Clamp(positionYContent, 0f, maxHeight);
            scrollToPosition = StartCoroutine(TweenToScrollPosition(positionYContent));
        }
    
        float duration = 0.4f;
        private IEnumerator TweenToScrollPosition(float _EndScrollPosition)
        {
            float time = 0.0f;
            while (time < duration)
            {
                float currentPosition = (InventoryWalletScrollRect.content.transform as RectTransform).localPosition.y;
                //InventoryWalletScrollRect.verticalScrollbar.value = Mathf.Lerp(currentPosition, _EndScrollPosition, time / duration);
                (InventoryWalletScrollRect.content.transform  as RectTransform).localPosition = new Vector3(0f, Mathf.Lerp(currentPosition, _EndScrollPosition, time / duration), 0f);
                
                time += Time.deltaTime;
                yield return null;
            }

            (InventoryWalletScrollRect.content.transform  as RectTransform).localPosition = new Vector3(0f, _EndScrollPosition, 0f);
        }


        private void OnCancelNavigateBank()
        {
            if(Visible)
                bTriggerAvailable = true;
        }

        private void UnselectAllBankCard()
        {
            if(Visible)
                for (int i = 0; i < currentTotalCard; i++)
                {
                    if( listCardsSlotWalletByIndex[i] != null )
                        listCardsSlotWalletByIndex[i].OnPointerExit(null);
                }
        }

        //*****************************************************************
        //********* Favorites
        //*****************************************************************

        public void FillFavorites(Dictionary<int, AnimationIds> _FavoritesAnimationIdByIndex)
        {
            if (_FavoritesAnimationIdByIndex == null)
                return;
            
            EmptyFavoritesDataOnSlot();

            for (int i = 0; i < currentTotalCard; i++)
            {
                if( listCardsSlotWalletByIndex[i]!=null )
                    listCardsSlotWalletByIndex[i].DisableFavorite();
            }

            favoritesIdByIndex = _FavoritesAnimationIdByIndex;
            foreach (KeyValuePair<int, AnimationIds> favoriteKVP in _FavoritesAnimationIdByIndex)
            {
                bool isOnPage = favoriteKVP.Key >= GetRealPageIndexFavorites(0) &&
                                favoriteKVP.Key < GetRealPageIndexFavorites(KinetixConstantsEmoteWheel.c_CountSlotOnWheel);
                
                KinetixCore.Metadata.IsAnimationOwnedByUser(favoriteKVP.Value, owned =>
                {
                    if (!owned)
                        return;
                    SetAndShowFavoriteDataOnSlot(favoriteKVP.Key, favoriteKVP.Value, isOnPage);
                });
            }
        }

        private void InitInventoryFavoriteCardSlots ()
        {
            for (int i = 0; i < KinetixConstantsEmoteWheel.c_CountSlotOnWheel; i++)
            {
                InventoryCardSlotFavorite inventoryCardSlotFavorite = inventoryCardSlotFavorites[i];
                cardsSlotFavoriteByIndex.Add(i, inventoryCardSlotFavorite);
                inventoryCardSlotFavorite.Init(i);
                inventoryCardSlotFavorite.SetAwait();

                inventoryCardSlotFavorite.OnEnter  += OnEnterCardSlotFavorite;
                inventoryCardSlotFavorite.OnExit   += OnExitCardSlotFavorite;
                inventoryCardSlotFavorite.OnRemove += RemoveFavorite;
                inventoryCardSlotFavorite.OnDown += ShowRemoveButton;
                inventoryCardSlotFavorite.OnUp += HideRemoveButton;
            }
        }

        private Vector2 tempPosMouse;
        private void OnStartDragInventoryCardWalletSlot (AnimationIds _Ids, Vector2 _SizeDeltaCard)
        {
            tempPosMouse = KinetixInputManager.PositionTouchOrMouse();
            StartCoroutine(OnStartDragDelayed(_Ids,_SizeDeltaCard));
        }
        
        private IEnumerator OnStartDragDelayed(AnimationIds _Ids, Vector2 _SizeDeltaCard)
        {
            yield return new WaitForSeconds(0.05f);

            float offsetX = Math.Abs(tempPosMouse.x-KinetixInputManager.PositionTouchOrMouse().x);
            float offsetY = Math.Abs(tempPosMouse.y-KinetixInputManager.PositionTouchOrMouse().y);

            if(offsetX == 0f && offsetY == 0f)
            {
                StartCoroutine(OnStartDragDelayed(_Ids,_SizeDeltaCard));
            }
            else if( (offsetX*1.5f) > offsetY)
            {
                InventoryCardDraggable inventoryCardDraggable = Instantiate(inventoryDraggableCardPrefab, parentInventoryDraggable)
                .GetComponent<InventoryCardDraggable>();
                inventoryCardDraggable.GetComponent<RectTransform>().sizeDelta = new Vector2( CellSizeX, CellSizeY);
                inventoryCardDraggable.SetAnimationData(_Ids);
                inventoryCardDraggable.OnEndDragCard += OnEndDragDraggableInventoryWalletCard;
                InventoryWalletScrollRect.enabled = false;
            }
        }
   
        private void OnEnterCardSlotFavorite (int _Index)
        {
            currentWheelSlotIndex = _Index;
            if(DelayExitFavorite!= null)
                StopCoroutine(DelayExitFavorite);
        }

        private void OnExitCardSlotFavorite ()
        {
            DelayExitFavorite = DelayOneFrame();
            StartCoroutine(DelayExitFavorite);
        }
        
        private IEnumerator DelayOneFrame ()
        {
            yield return new WaitForEndOfFrame();
            currentWheelSlotIndex = -1;
        }

        private void OnEndDragDraggableInventoryWalletCard (AnimationIds ids)
        {
            InventoryWalletScrollRect.enabled = true;
            if (currentWheelSlotIndex == -1)
                return;
            OnAddFavoriteCard(currentWheelSlotIndex, ids);
        }

        private void OnAddFavoriteCard(int _Index, AnimationIds _IDs)
        {
            if (cardsSlotFavoriteByIndex[_Index].HasData)
                RemoveFavorite(_Index);

            SetAndShowFavoriteDataOnSlot(_Index, _IDs);
            OnAddFavorite?.Invoke(GetRealPageIndexFavorites(_Index), _IDs);
        }

        private void SetAndShowFavoriteDataOnSlot(int _Index, AnimationIds _Ids, bool isOnPage = true)
        {
            int clampedIndex = (int)Mathf.Repeat(_Index, KinetixConstantsEmoteWheel.c_CountSlotOnWheel);
            
            if (isOnPage)
            {
                cardsSlotFavoriteByIndex[clampedIndex].SetAnimationData(_Ids);
                cardsSlotFavoriteByIndex[clampedIndex].ShowAnimationData();
                
                if (cardsSlotWalletByAnimationUUID.ContainsKey(_Ids.UUID))
                {
                    cardsSlotWalletByAnimationUUID[_Ids.UUID].EnableFavorite();
                }
            }
        }

        private void EmptyFavoritesDataOnSlot()
        {
            foreach (KeyValuePair<int, InventoryCardSlotFavorite> kvp in cardsSlotFavoriteByIndex)
            {
                kvp.Value.Empty();
            }
        }

        private void RemoveFavorite(int _Index)
        {
            if(_Index>= 0 && _Index<KinetixConstantsEmoteWheel.c_CountSlotOnWheel)
            {
                if(cardsSlotFavoriteByIndex[_Index].HasData)
                {
                    if (cardsSlotWalletByAnimationUUID.ContainsKey(cardsSlotFavoriteByIndex[_Index].Ids.UUID))
                        cardsSlotWalletByAnimationUUID[cardsSlotFavoriteByIndex[_Index].Ids.UUID].DisableFavorite();
                    cardsSlotFavoriteByIndex[_Index].Remove();
                    OnRemoveFavorite?.Invoke(GetRealPageIndexFavorites(_Index));
                    HideRemoveButton();
                }
            }
        }
                
        private void OnChangedFavoriteNextPage()
        {
            currentPageIndexFavorites++;
            currentPageIndexFavorites = (int)Mathf.Repeat(currentPageIndexFavorites, KinetixConstantsEmoteWheel.c_CountFavoritePages);
            favoritePagingSystem.UpdatePageLabel(currentPageIndexFavorites, KinetixConstantsEmoteWheel.c_CountFavoritePages);
            OnRefillFavorites?.Invoke();
        }

        private void OnChangedFavoritePreviousPage()
        {
            currentPageIndexFavorites--;
            currentPageIndexFavorites = (int)Mathf.Repeat(currentPageIndexFavorites, KinetixConstantsEmoteWheel.c_CountFavoritePages);
            favoritePagingSystem.UpdatePageLabel(currentPageIndexFavorites, KinetixConstantsEmoteWheel.c_CountFavoritePages);
            OnRefillFavorites?.Invoke();
        }

        private int GetRealPageIndexFavorites(int _Index)
        {
            return _Index + KinetixConstantsEmoteWheel.c_CountSlotOnWheel * currentPageIndexFavorites;
        }

        private void ShowRemoveButton()
        {
            inventoryRemoveBtn.gameObject.SetActive(true);
            inventoryRemoveBtn.GetComponent<ScaleEffect>().ScaleUp();
        }

        private void HideRemoveButton()
        {
            inventoryRemoveBtn.gameObject.SetActive(true);
            inventoryRemoveBtn.GetComponent<ScaleEffect>().ScaleDown();
        }

        private void ShowFavoriteOutline(int indexToShow = -1)
        {
            for (int i = 0; i < KinetixConstantsEmoteWheel.c_CountSlotOnWheel; i++)
            {
                if(i != indexToShow)
                    cardsSlotFavoriteByIndex[i].HideOutline();
                else 
                    cardsSlotFavoriteByIndex[i].ShowOutline();
            }
        }

        private void ShowFavoriteTrash(int indexToShow = -1)
        {
            for (int i = 0; i < KinetixConstantsEmoteWheel.c_CountSlotOnWheel; i++)
            {
                if(i != indexToShow)
                    cardsSlotFavoriteByIndex[i].HideTrash();
                else 
                    cardsSlotFavoriteByIndex[i].ShowTrash();
            }
        }

        //*****************************************************************
        //********* State Manager
        //*****************************************************************

        bool bTriggerAvailable = true;
        float _thresholdValue = 0.99f;
        private void OnNavigateBank (Vector2 _direction)
        {
            if(Visible)
            {
                switch( currentState )
                {                
                    case State.NORMAL:
                        int tempCurrentIndexBank = currentBankIndexSelected;
                        if(bTriggerAvailable)
                        {                
                            if(_direction.y > _thresholdValue)
                                currentBankIndexSelected -= cardByRow;
                            else if(_direction.y < -_thresholdValue)
                                currentBankIndexSelected += cardByRow;

                            if(_direction.x > _thresholdValue)
                                currentBankIndexSelected += 1;
                            else if(_direction.x < -_thresholdValue)
                                currentBankIndexSelected -= 1;

                            currentBankIndexSelected = Mathf.Clamp(currentBankIndexSelected, 0, currentTotalCard-1); 
                            if( currentBankIndexSelected != tempCurrentIndexBank )
                            {
                                bTriggerAvailable = false;
                                UnselectAllBankCard();
                                UpdateScrollPosition();

                                if (currentBankIndexSelected >= currentFirstCardVisible && currentBankIndexSelected< (currentFirstCardVisible+bufferPool))           
                                    if (listCardsSlotWalletByIndex[currentBankIndexSelected] != null) 
                                        listCardsSlotWalletByIndex[currentBankIndexSelected].OnPointerEnter(null);                            
                            }
                        }
                        break;

                    case State.ADDED:
                        
                        if(inventoryCardSelectedToAdd != null)
                        {
                            indexSlotWhereToAdd = KinetixUtils.GetIndexFromAWheel(_direction, KinetixConstantsEmoteWheel.c_CountSlotOnWheel, 3);
                            (inventoryCardSelectedToAdd.transform as RectTransform).position = (cardsSlotFavoriteByIndex[indexSlotWhereToAdd].transform as RectTransform).position;
                            ShowFavoriteOutline(indexSlotWhereToAdd);
                        }
                        break;

                    case State.DELETE:
                        
                        indexSlotWhereToDelete = KinetixUtils.GetIndexFromAWheel(_direction, KinetixConstantsEmoteWheel.c_CountSlotOnWheel, 3);
                        ShowFavoriteTrash(indexSlotWhereToDelete);
                        
                        break;
                
                }
            }
        }
 
        int indexSlotWhereToAdd;
        int indexSlotWhereToDelete;
        InventoryCardDraggable inventoryCardSelectedToAdd;
        private void OnSelect()
        {
            if(Visible)
            {
                switch( currentState )
                {                
                    case State.NORMAL:
                        // create card and put it on top of the inventory wheel favorites
                        inventoryCardSelectedToAdd = Instantiate(inventoryDraggableCardPrefab, parentInventoryDraggable)
                        .GetComponent<InventoryCardDraggable>();
                        inventoryCardSelectedToAdd._movable = false;
                        inventoryCardSelectedToAdd.GetComponent<RectTransform>().sizeDelta = new Vector2( CellSizeX, CellSizeY);
                        inventoryCardSelectedToAdd.SetAnimationData(cardsIdsByIndex[currentBankIndexSelected]);
                        indexSlotWhereToAdd = 0;                   
                        (inventoryCardSelectedToAdd.transform as RectTransform).position = (cardsSlotFavoriteByIndex[indexSlotWhereToAdd].transform as RectTransform).position;
                        ShowFavoriteOutline(0);
                        currentState = State.ADDED;
                        break;
                    case State.ADDED:
                        OnAddFavoriteCard(indexSlotWhereToAdd, cardsIdsByIndex[currentBankIndexSelected]);
                        ShowFavoriteOutline(-1);
                        Destroy(inventoryCardSelectedToAdd.gameObject);
                        currentState = State.NORMAL;
                        break;
                    case State.DELETE:
                        RemoveFavorite(indexSlotWhereToDelete);
                        ShowFavoriteTrash(-1);
                        currentState = State.NORMAL;
                        break;
                }
            }
        }

        private void OnCancel()
        {
            if(Visible)
            {
                switch( currentState )
                {                
                    case State.NORMAL:
                        //nothing
                        break;
                    case State.ADDED:
                        ShowFavoriteOutline(-1);
                        Destroy(inventoryCardSelectedToAdd.gameObject);
                        currentState = State.NORMAL;
                        break;
                    case State.DELETE:
                        ShowFavoriteTrash(-1);
                        currentState = State.NORMAL;
                        break;
                }
            }             
        }

        private void OnDeleteMode()
        {
            if(Visible)
            {
                switch( currentState )
                {                
                    case State.NORMAL:
                        indexSlotWhereToDelete = 0;
                        cardsSlotFavoriteByIndex[indexSlotWhereToDelete].ShowTrash();
                        currentState = State.DELETE;
                        break;
                    case State.ADDED:
                        ShowFavoriteOutline(-1);
                        Destroy(inventoryCardSelectedToAdd.gameObject);
                        cardsSlotFavoriteByIndex[0].ShowTrash();
                        currentState = State.DELETE;
                        break;
                    case State.DELETE:
                        //nothing
                        break;
                }
            }
        }

    }
}


        

