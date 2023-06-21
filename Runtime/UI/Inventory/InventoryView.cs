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
		[SerializeField] private InventoryBank bank = new InventoryBank();
		[SerializeField] private InventoryWheel wheel = new InventoryWheel();
		
		public Action<int, AnimationIds> OnAddFavorite;
		public Action<int>               OnRemoveFavorite;
		public Action                    OnRefillFavorites;
		public Action                    OnRefillBank;
		public Action<string>            OnCheckEmoteAction;

		private enum State { NORMAL, ADDED, DELETE };
		private State currentState = State.NORMAL;
		private int indexSlotWhereToAdd;
		private int indexSlotWhereToDelete;
		private InventoryCardDraggable inventoryCardSelectedToAdd;


		public void Init()
		{
			bank.Init();
			wheel.Init();
			
			bank.OnEndDragCardAction 			+= OnEndDragCardInventory;
			bank.OnCheckEmoteAction 			+= OnCheckEmote;

			wheel.OnAddFavoriteAction 			+= OnAddFavoriteFunc;
			wheel.OnRemoveFavoriteAction 		+= OnRemoveFavoriteFunc;
			wheel.OnRefillFavoritesAction 		+= OnRefillFavoritesFunc;

			KinetixInputManager.OnHitNextPage 	+=  OnChangedFavoriteNextPage;
			KinetixInputManager.OnHitPrevPage 	+= OnChangedFavoritePreviousPage;

			KinetixInputManager.OnNavigate 		+= OnNavigateBank;
			KinetixInputManager.OnCancelNavigate += OnCancelNavigateBank;

			KinetixInputManager.OnSelect 		+= OnSelect;
			KinetixInputManager.OnCancel 		+= OnCancel;
			KinetixInputManager.OnDeleteMode 	+= OnDeleteMode;
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			wheel.OnDestroy();
			bank.OnDestroy();

			KinetixInputManager.OnHitNextPage -=  OnChangedFavoriteNextPage;
			KinetixInputManager.OnHitPrevPage -= OnChangedFavoritePreviousPage;
			KinetixInputManager.OnSelect -= OnSelect;

			KinetixInputManager.OnNavigate -= OnNavigateBank;
			KinetixInputManager.OnCancelNavigate -= OnCancelNavigateBank;

			KinetixInputManager.OnSelect -= OnSelect;
			KinetixInputManager.OnCancel -= OnCancel;
			KinetixInputManager.OnDeleteMode -= OnDeleteMode;
		}

		

		//*****************************************************************
		//********* BANK
		//*****************************************************************
		public void RefreshInventoryBankAnimations()
		{
			bank.Reload();
		}


		//*****************************************************************
		//********* Wheel Favorites
		//*****************************************************************
		public void FillFavorites(Dictionary<int, AnimationIds> _FavoritesAnimationIdByIndex)
		{
			wheel.FillFavorites(_FavoritesAnimationIdByIndex);
			bank.RefreshFavorites(_FavoritesAnimationIdByIndex);
		}

		private void OnChangedFavoriteNextPage()
		{
			wheel.OnChangedFavoriteNextPage();
		}

		private void OnChangedFavoritePreviousPage()
		{
			wheel.OnChangedFavoritePreviousPage();
		}

		private void OnEndDragCardInventory(AnimationIds ids)
		{
			wheel.OnAddFavoriteCard(ids);
		}

		private void OnCheckEmote(string UUID)
		{
			OnCheckEmoteAction?.Invoke(UUID);
		}

		private void OnAddFavoriteFunc(int index, AnimationIds ids)
		{
			bank.EnableFavoriteFlag(ids);
			OnAddFavorite?.Invoke(index, ids);
		}

		private void OnRemoveFavoriteFunc(int index, AnimationIds ids)
		{
			bank.RemoveFavoriteFlag(ids);
			OnRemoveFavorite?.Invoke(index);
		}
		
		private void OnRefillFavoritesFunc()
		{
			OnRefillFavorites?.Invoke();
		}

		//*****************************************************************
		//********* State Input Manager
		//*****************************************************************		
		private void OnNavigateBank (Vector2 _direction)
		{
			if(Visible)
			{
				switch( currentState )
				{                
					case State.NORMAL:
						bank.NavigateNormal(_direction);
						break;

					case State.ADDED:						
						wheel.MoveCardOnWheel(inventoryCardSelectedToAdd, _direction);
						break;

					case State.DELETE:
						wheel.ShowTrashOnIcon(_direction);			
						break;				
				}
			}
		}

		private void OnCancelNavigateBank()
		{
			if(Visible)
			 	bank.bTriggerAvailable = true;
		}
 
		private void OnSelect()
		{
			if(Visible)
			{
				switch( currentState )
				{                
					case State.NORMAL:
						// create card and put it on top of the inventory wheel favorites
						inventoryCardSelectedToAdd = bank.CreateAndGetCardToAdd();
						wheel.MoveCardOnWheel(inventoryCardSelectedToAdd, Vector2.up);			
						currentState = State.ADDED;
						break;
					case State.ADDED:
						wheel.SetFavoriteCard(inventoryCardSelectedToAdd);
						Destroy(inventoryCardSelectedToAdd.gameObject);					
						currentState = State.NORMAL;
						break;
					case State.DELETE:
						wheel.SelectRemoveFavorite();						
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
						wheel.ShowFavoriteOutline(-1);
						Destroy(inventoryCardSelectedToAdd.gameObject);
						currentState = State.NORMAL;
						break;
					case State.DELETE:
						wheel.ShowFavoriteTrash(-1);
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
						wheel.ShowTrashOnIcon(Vector2.up);
						currentState = State.DELETE;
						break;
					case State.ADDED:
						wheel.ShowFavoriteOutline(-1);
						wheel.ShowTrashOnIcon(Vector2.up);
						Destroy(inventoryCardSelectedToAdd.gameObject);
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


		

