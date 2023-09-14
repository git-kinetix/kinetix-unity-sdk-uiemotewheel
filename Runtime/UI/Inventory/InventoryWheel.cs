// // ----------------------------------------------------------------------------
// // <copyright file="InventoryView.cs" company="Kinetix SAS">
// // Kinetix Unity SDK - Copyright (C) 2022 Kinetix SAS
// // </copyright>
// // ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;

namespace Kinetix.UI.EmoteWheel
{
	[Serializable]
	public class InventoryWheel
	{
		[SerializeField] private PagingSystem favoritePagingSystem;
		[SerializeField] private GameObject inventoryDraggableFavoritePrefab;
		[SerializeField] private GameObject inventoryRemoveBtn;
		[SerializeField] private List<InventoryCardSlotFavorite> inventoryCardSlotFavorites;
		
		// CACHE
		private Dictionary<int, AnimationIds>  favoritesIdByIndex;
		private Dictionary<int, InventoryCardSlotFavorite>  cardsSlotFavoriteByIndex;
		
		private int currentWheelSlotIndex;
		private int currentPageIndexFavorites;

		private IEnumerator DelayExitFavorite;

		public Action<int, AnimationIds> OnAddFavoriteAction;
		public Action<int, AnimationIds> OnRemoveFavoriteAction;
		public Action                    OnRefillFavoritesAction;


		public void Init()
		{
			currentWheelSlotIndex     = -1;
			currentPageIndexFavorites = 0;
			cardsSlotFavoriteByIndex ??= new Dictionary<int, InventoryCardSlotFavorite>();

			favoritePagingSystem.OnSwitchNextPage     += OnChangedFavoriteNextPage;
			favoritePagingSystem.OnSwitchPreviousPage += OnChangedFavoritePreviousPage;
			favoritePagingSystem.Init();

			InitInventoryFavoriteCardSlots();
			Reload();
			HideRemoveButton();
		}

		private void Reload()
		{
			RefreshPages();
		}
		
		public void RefreshPages()
		{
			favoritePagingSystem.UpdatePageLabel(currentPageIndexFavorites, KinetixConstantsEmoteWheel.c_CountFavoritePages);
		}

		public void OnDestroy()
		{
			if (favoritePagingSystem != null)
			{
				favoritePagingSystem.OnSwitchNextPage     -= OnChangedFavoriteNextPage;
				favoritePagingSystem.OnSwitchPreviousPage -= OnChangedFavoritePreviousPage;
			}
		}


		private bool IsAnimationIsInTheFavorite(int indexCard, string UUID)
		{
			foreach (KeyValuePair<int, AnimationIds> favoriteKVP in favoritesIdByIndex)
			{
				if (favoriteKVP.Value.UUID == UUID)
					return true;
			}
			return false;
		}

		//*****************************************************************
		//********* Favorites
		//*****************************************************************

		public void FillFavorites(Dictionary<int, AnimationIds> _FavoritesAnimationIdByIndex)
		{
			if (_FavoritesAnimationIdByIndex == null)
				return;

			EmptyFavoritesDataOnSlot();

			favoritesIdByIndex = _FavoritesAnimationIdByIndex;
			foreach (KeyValuePair<int, AnimationIds> favoriteKVP in _FavoritesAnimationIdByIndex)
			{
				bool isOnPage = favoriteKVP.Key >= GetRealPageIndexFavorites(0) &&
								favoriteKVP.Key < GetRealPageIndexFavorites(KinetixConstantsEmoteWheel.c_CountSlotOnWheel);
				
				KinetixCore.Metadata.IsAnimationOwnedByUser(favoriteKVP.Value.UUID, owned =>
				{
					if (owned)
					{
						SetAndShowFavoriteDataOnSlot(favoriteKVP.Key, favoriteKVP.Value, isOnPage);
					}
					else
					{
						HideFavoriteDataOnSlot(favoriteKVP.Key, favoriteKVP.Value, isOnPage);
					}
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
   
		private void OnEnterCardSlotFavorite (int _Index)
		{
			currentWheelSlotIndex = _Index;
			if(DelayExitFavorite!= null)
				CoroutineUtils.Instance.StopCoroutine(DelayExitFavorite);
		}

		private void OnExitCardSlotFavorite ()
		{
			DelayExitFavorite = DelayOneFrame();
			CoroutineUtils.Instance.StartCoroutine(DelayExitFavorite);
		}
		
		private IEnumerator DelayOneFrame ()
		{
			yield return new WaitForEndOfFrame();
			currentWheelSlotIndex = -1;
		}

		public void OnAddFavoriteCard(AnimationIds _IDs)
		{
			if (currentWheelSlotIndex == -1)
				return;

			if (cardsSlotFavoriteByIndex[currentWheelSlotIndex].HasData)
				RemoveFavorite(currentWheelSlotIndex);

			SetAndShowFavoriteDataOnSlot(currentWheelSlotIndex, _IDs);
			OnAddFavoriteAction?.Invoke(GetRealPageIndexFavorites(currentWheelSlotIndex), _IDs);
		}

		public void SetAndShowFavoriteDataOnSlot(int _Index, AnimationIds _Ids, bool isOnPage = true)
		{	
			int clampedIndex = (int)Mathf.Repeat(_Index, KinetixConstantsEmoteWheel.c_CountSlotOnWheel);

			if (isOnPage)
			{
				cardsSlotFavoriteByIndex[clampedIndex].SetAnimationData(_Ids);
				cardsSlotFavoriteByIndex[clampedIndex].ShowAnimationData();
			}
		}

		public void HideFavoriteDataOnSlot(int _Index, AnimationIds _Ids, bool isOnPage = true)
		{			
			int clampedIndex = (int)Mathf.Repeat(_Index, KinetixConstantsEmoteWheel.c_CountSlotOnWheel);

			if (isOnPage)
			{
				cardsSlotFavoriteByIndex[clampedIndex].Remove();
			}
		}


		private void EmptyFavoritesDataOnSlot()
		{
			foreach (KeyValuePair<int, InventoryCardSlotFavorite> kvp in cardsSlotFavoriteByIndex)
			{
				kvp.Value.Empty();
			}
		}

		public void RemoveFavorite(int _Index)
		{
			if(_Index>= 0 && _Index<KinetixConstantsEmoteWheel.c_CountSlotOnWheel)
			{
				if(cardsSlotFavoriteByIndex[_Index].HasData)
				{
					cardsSlotFavoriteByIndex[_Index].Remove();
					OnRemoveFavoriteAction?.Invoke(GetRealPageIndexFavorites(_Index), cardsSlotFavoriteByIndex[_Index].Ids);
					HideRemoveButton();
				}
			}
		}
				
		public void OnChangedFavoriteNextPage()
		{
			currentPageIndexFavorites++;
			currentPageIndexFavorites = (int)Mathf.Repeat(currentPageIndexFavorites, KinetixConstantsEmoteWheel.c_CountFavoritePages);
			favoritePagingSystem.UpdatePageLabel(currentPageIndexFavorites, KinetixConstantsEmoteWheel.c_CountFavoritePages);
			OnRefillFavoritesAction?.Invoke();
		}

		public void OnChangedFavoritePreviousPage()
		{
			currentPageIndexFavorites--;
			currentPageIndexFavorites = (int)Mathf.Repeat(currentPageIndexFavorites, KinetixConstantsEmoteWheel.c_CountFavoritePages);
			favoritePagingSystem.UpdatePageLabel(currentPageIndexFavorites, KinetixConstantsEmoteWheel.c_CountFavoritePages);
			OnRefillFavoritesAction?.Invoke();
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

		public void ShowFavoriteOutline(int indexToShow = -1)
		{
			for (int i = 0; i < KinetixConstantsEmoteWheel.c_CountSlotOnWheel; i++)
			{
				if(i != indexToShow)
					cardsSlotFavoriteByIndex[i].HideOutline();
				else 
					cardsSlotFavoriteByIndex[i].ShowOutline();
			}
		}

		public void ShowFavoriteTrash(int indexToShow = -1)
		{
			for (int i = 0; i < KinetixConstantsEmoteWheel.c_CountSlotOnWheel; i++)
			{
				if(i != indexToShow)
					cardsSlotFavoriteByIndex[i].HideTrash();
				else 
					cardsSlotFavoriteByIndex[i].ShowTrash();
			}
		}

		//******************************************************************
		// GamePad Manager
		//******************************************************************

		int indexSlotWhereToAdd;		
		int indexSlotWhereToDelete;		

		public void ShowTrashOnIcon(Vector2 _direction)
		{
			indexSlotWhereToDelete = KinetixUtils.GetIndexFromAWheel(_direction, KinetixConstantsEmoteWheel.c_CountSlotOnWheel, 3);
			ShowFavoriteTrash(indexSlotWhereToDelete);					
		}

		public void MoveCardOnWheel(InventoryCardDraggable inventoryCardSelectedToAdd, Vector2 _direction )
		{
			if(inventoryCardSelectedToAdd != null)
			{
				indexSlotWhereToAdd = KinetixUtils.GetIndexFromAWheel(_direction, KinetixConstantsEmoteWheel.c_CountSlotOnWheel, 3);
				(inventoryCardSelectedToAdd.transform as RectTransform).position = (cardsSlotFavoriteByIndex[indexSlotWhereToAdd].transform as RectTransform).position;
				ShowFavoriteOutline(indexSlotWhereToAdd);
			}
		}

		public void SelectRemoveFavorite()
		{
			RemoveFavorite(indexSlotWhereToDelete);
			ShowFavoriteTrash(-1);
		}

		public void SetFavoriteCard(InventoryCardDraggable inventoryCardSelectedToAdd)
		{
			currentWheelSlotIndex = indexSlotWhereToAdd;
			OnAddFavoriteCard(new AnimationIds(inventoryCardSelectedToAdd.UUID));
			ShowFavoriteOutline(-1);
		}
	}
}
