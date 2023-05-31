// // ----------------------------------------------------------------------------
// // <copyright file="InventoryView.cs" company="Kinetix SAS">
// // Kinetix Unity SDK - Copyright (C) 2022 Kinetix SAS
// // </copyright>
// // ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;
using Debug=UnityEngine.Debug;

namespace Kinetix.UI.EmoteWheel
{
	[Serializable]
	public class InventoryBank
	{
		[SerializeField] private Transform parentInventoryWalletGrid;
		[SerializeField] private ScrollRect InventoryWalletScrollRect;
		[SerializeField] private GameObject inventoryCardSlotWalletPrefab;
		[SerializeField] private Transform  parentInventoryDraggable;
		[SerializeField] private GameObject inventoryDraggableCardPrefab;

		public Action<string>			OnCheckEmoteAction;
		public Action<AnimationIds>		OnEndDragCardAction;
		
		// CACHE
		private Dictionary<string, InventoryCardSlotWallet> cardsSlotWalletByAnimationUUID;
		[SerializeField] private List<InventoryCardSlotWallet> listCardsSlotWalletByIndex;
		private Dictionary<int, AnimationIds> cardsIdsByIndex;
		private List<InventoryCardSlotWallet> pooledCardWallet;
		
		private int currentBankIndexSelected;
		private int currentFirstCardVisible;
		private int previousFirstCardVisible;
		private int currentTotalCard;
		private int amountCardVisible = 30;
		private int sizePool = 50;
		private int cardByRow = 6;
		private int CellSizeX = 92;
		private int CellSizeY = 135;
		private int SpacingX = 15;
		private int SpacingY = 25;
		private int Padding = 15;

		public void Init()
		{
			currentBankIndexSelected  = 0;
			currentFirstCardVisible = -1;
			previousFirstCardVisible = -1;

			cardsSlotWalletByAnimationUUID	??= new Dictionary<string, InventoryCardSlotWallet>();
			cardsIdsByIndex					??= new Dictionary<int, AnimationIds>();
			listCardsSlotWalletByIndex      ??= new List<InventoryCardSlotWallet>();

			InventoryWalletScrollRect.onValueChanged.AddListener(OnScrollRectValueChanged);

			InventoryWalletScrollRect.elasticity = 0.05f;
#if UNITY_EDITOR
			InventoryWalletScrollRect.scrollSensitivity = 6;
#elif UNITY_WEBGL
			InventoryWalletScrollRect.elasticity = 0.02f;
			InventoryWalletScrollRect.scrollSensitivity = 6;
#else
			InventoryWalletScrollRect.scrollSensitivity = 4;
#endif
			Reload();
			CreatePoolCard();
		}

		public void Reload()
		{
			RefreshInventoryBankAnimations();
		}

		public void OnDestroy()
		{
			if (InventoryWalletScrollRect != null)
				InventoryWalletScrollRect.onValueChanged.RemoveListener(OnScrollRectValueChanged);

			for(int i = 0; i < sizePool; i++)
			{
				if(pooledCardWallet!=null)
					if( pooledCardWallet[i] != null)
						pooledCardWallet[i].OnActionStartDrag -= OnStartDragInventoryCardWalletSlot;
			}
		}
		
		void CreatePoolCard()
		{
			pooledCardWallet = new List<InventoryCardSlotWallet>();
			InventoryCardSlotWallet tmp;
			for(int i = 0; i < sizePool; i++)
			{
				tmp = GameObject.Instantiate(inventoryCardSlotWalletPrefab, parentInventoryWalletGrid).GetComponent<InventoryCardSlotWallet>();
				tmp.OnActionStartDrag += OnStartDragInventoryCardWalletSlot;
				tmp.gameObject.SetActive(false);
				pooledCardWallet.Add(tmp);
			}
		}

		private void OnCheckEmote(string UUID)
		{			
			if( cardsSlotWalletByAnimationUUID.ContainsKey(UUID) )
				cardsSlotWalletByAnimationUUID[UUID].DisableNotificationIcon();
			
			OnCheckEmoteAction?.Invoke(UUID);
		}

		private InventoryCardSlotWallet GetPooledCard()
		{
			for(int i = 0; i < sizePool; i++)
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
			for(int i = 0; i < sizePool; i++)
			{
				pooledCardWallet[i].RemoveAnimationData();
			}            
		}

		private void RefreshInventoryBankAnimations ()
		{
			KinetixCore.Metadata.GetUserAnimationMetadatas(animationMetadatas =>
			{
				currentTotalCard = animationMetadatas.Length;

				//sort the array to have the first emotes created on top 
				animationMetadatas = animationMetadatas.OrderByDescending( x => x.CreatedAt ).ToArray();

				//refresh size of the background of the grid, to make the scrollbar effective
				RectTransform rt = (parentInventoryWalletGrid.transform as RectTransform);
				rt.sizeDelta = new Vector2 (rt.sizeDelta.x, ((int)((currentTotalCard-1)/cardByRow)+1) * (CellSizeY+SpacingY) + Padding/2);

				//empty list card wallet
				foreach( InventoryCardSlotWallet icw in listCardsSlotWalletByIndex)
				{
					if(icw.hasData)
						icw.RemoveAnimationData(true);
				}
				listCardsSlotWalletByIndex.Clear();
				cardsIdsByIndex.Clear();
				for (int i = 0; i < currentTotalCard; i++)
				{
					cardsIdsByIndex[i] = animationMetadatas[i].Ids;
					listCardsSlotWalletByIndex.Add(null);
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

					if( i == currentBankIndexSelected )
						listCardsSlotWalletByIndex[i].OnPointerEnter(null);
				}
			}			
		}

		private Dictionary<int, AnimationIds> favoritesIdByIndex;
		public void RefreshFavorites( Dictionary<int, AnimationIds> _FavoritesAnimationIdByIndex)
		{
			favoritesIdByIndex = _FavoritesAnimationIdByIndex;
			foreach (KeyValuePair<int, AnimationIds> favoriteKVP in _FavoritesAnimationIdByIndex)
			{
				KinetixCore.Metadata.IsAnimationOwnedByUser(favoriteKVP.Value, owned =>
				{
					if (!owned)
						return;
					
					if(cardsSlotWalletByAnimationUUID.ContainsKey(favoriteKVP.Value.UUID))
						cardsSlotWalletByAnimationUUID[favoriteKVP.Value.UUID].EnableFavorite();
				});
			}
		}
		
		private bool IsAnimationIsInTheFavorite(int indexCard)
		{
			if(favoritesIdByIndex == null)
				return false;

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

		private Coroutine scrollToPosition;
		private void UpdateScrollPosition()
		{
			int lineWhereIndexIsSelected = currentBankIndexSelected/cardByRow;
			float positionYContent = lineWhereIndexIsSelected * (CellSizeY + SpacingY);
			float maxHeight = InventoryWalletScrollRect.content.sizeDelta.y - (InventoryWalletScrollRect.transform as RectTransform).sizeDelta.y;

			//if height of the grid is inferior to the scrollRect, so no need to scroll
			if(maxHeight < 0f) return;

			positionYContent = Mathf.Clamp(positionYContent, 0f, maxHeight);
			scrollToPosition = CoroutineUtils.Instance.StartCoroutine(TweenToScrollPosition(positionYContent));
		}
	
		private float duration = 0.4f;
		private IEnumerator TweenToScrollPosition(float _EndScrollPosition)
		{
			float time = 0.0f;
			while (time < duration)
			{
				float currentPosition = (InventoryWalletScrollRect.content.transform as RectTransform).localPosition.y;
				(InventoryWalletScrollRect.content.transform  as RectTransform).localPosition = new Vector3(0f, Mathf.Lerp(currentPosition, _EndScrollPosition, time / duration), 0f);
				time += Time.deltaTime;
				yield return null;
			}

			(InventoryWalletScrollRect.content.transform  as RectTransform).localPosition = new Vector3(0f, _EndScrollPosition, 0f);
		}

		private void UnselectAllBankCard()
		{
			for (int i = 0; i < currentTotalCard; i++)
			{
				if( listCardsSlotWalletByIndex[i] != null )
					listCardsSlotWalletByIndex[i].OnPointerExit(null);
			}
		}
		
		private Vector2 tempPosMouse;
		private void OnStartDragInventoryCardWalletSlot (AnimationIds _Ids, Vector2 _SizeDeltaCard)
		{
			OnCheckEmote(_Ids.UUID);
			tempPosMouse = KinetixInputManager.PositionTouchOrMouse();
			CoroutineUtils.Instance.StartCoroutine(OnStartDragDelayed(_Ids,_SizeDeltaCard));
		}
		
		private IEnumerator OnStartDragDelayed(AnimationIds _Ids, Vector2 _SizeDeltaCard)
		{
			yield return new WaitForSeconds(0.05f);

			float offsetX = Math.Abs(tempPosMouse.x-KinetixInputManager.PositionTouchOrMouse().x);
			float offsetY = Math.Abs(tempPosMouse.y-KinetixInputManager.PositionTouchOrMouse().y);

			if(offsetX == 0f && offsetY == 0f)
			{
				CoroutineUtils.Instance.StartCoroutine(OnStartDragDelayed(_Ids,_SizeDeltaCard));
			}
			else if( (offsetX*2f) > offsetY)
			{
				InventoryCardDraggable inventoryCardDraggable = GameObject.Instantiate(inventoryDraggableCardPrefab, new Vector3(-5000f,0f,0f), new Quaternion(), parentInventoryDraggable)
				.GetComponent<InventoryCardDraggable>();
				inventoryCardDraggable.GetComponent<RectTransform>().sizeDelta = new Vector2( CellSizeX, CellSizeY);
				inventoryCardDraggable.SetAnimationData(_Ids);
				inventoryCardDraggable.OnEndDragCard += OnEndDragCardInventory;
				InventoryWalletScrollRect.enabled = false;
			}
		}
   

		private void OnEndDragCardInventory (AnimationIds ids)
		{
			InventoryWalletScrollRect.enabled = true;
			OnEndDragCardAction?.Invoke(ids);
		}


		public void EnableFavoriteFlag(AnimationIds _Ids)
		{
			if (cardsSlotWalletByAnimationUUID.ContainsKey(_Ids.UUID))
				cardsSlotWalletByAnimationUUID[_Ids.UUID].EnableFavorite();
		}

		public void RemoveFavoriteFlag(AnimationIds _Ids)
		{
			if (cardsSlotWalletByAnimationUUID.ContainsKey(_Ids.UUID))
				cardsSlotWalletByAnimationUUID[_Ids.UUID].DisableFavorite();
		}


		//******************************************************************
		// GamePad Manager
		//******************************************************************

		public bool bTriggerAvailable = true;
		float _thresholdValue = 0.99f;
		public void NavigateNormal (Vector2 _direction)
		{
			int tempCurrentIndexBank = currentBankIndexSelected;
			if(bTriggerAvailable)
			{
				if(_direction.y > _thresholdValue)
					tempCurrentIndexBank -= cardByRow;
				else if(_direction.y < -_thresholdValue)
					tempCurrentIndexBank += cardByRow;

				if(_direction.x > _thresholdValue)
					tempCurrentIndexBank += 1;
				else if(_direction.x < -_thresholdValue)
					tempCurrentIndexBank -= 1;

				tempCurrentIndexBank = Mathf.Clamp(tempCurrentIndexBank, 0, currentTotalCard-1); 
				if( currentBankIndexSelected != tempCurrentIndexBank )
				{
					bTriggerAvailable = false;
					UnselectAllBankCard();
					UpdateScrollPosition();

					if (tempCurrentIndexBank >= currentFirstCardVisible && tempCurrentIndexBank< (currentFirstCardVisible+sizePool))           
						if (listCardsSlotWalletByIndex[tempCurrentIndexBank] != null) 
							listCardsSlotWalletByIndex[tempCurrentIndexBank].OnPointerEnter(null);

					currentBankIndexSelected = tempCurrentIndexBank;
				}
			}						
		}

		public InventoryCardDraggable CreateAndGetCardOnWheel()
		{
			OnCheckEmote(cardsIdsByIndex[currentBankIndexSelected].UUID);
			InventoryCardDraggable inventoryCardSelectedToAdd = GameObject.Instantiate(inventoryDraggableCardPrefab, parentInventoryDraggable)
			.GetComponent<InventoryCardDraggable>();
			inventoryCardSelectedToAdd._movable = false;
			inventoryCardSelectedToAdd.GetComponent<RectTransform>().sizeDelta = new Vector2( CellSizeX, CellSizeY);
			inventoryCardSelectedToAdd.SetAnimationData(cardsIdsByIndex[currentBankIndexSelected]);
			return inventoryCardSelectedToAdd;
		}
	}
}
