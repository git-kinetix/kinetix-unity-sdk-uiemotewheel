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
	public class ContextView : CategoryView
	{
		[SerializeField] private InventoryBank bank = new InventoryBank();
		[SerializeField] private ContextSelector selector = new ContextSelector();
		
		public Action<string, string> 			OnAddContext;
		public Action<string>               	OnRemoveContext;
		public Action                    		OnRefillContext;
		public Action                    		OnRefillBank;
		public Action<string>            		OnCheckEmoteAction;

		private enum State { NORMAL, ADDED, DELETE };
		private State currentState = State.NORMAL;

		public void Init()
		{
			bank.Init();
			selector.Init();
			
			bank.OnEndDragCardAction 				+= OnEndDragCardInventory;

			selector.OnAddContext 					+= OnAddContextFunc;
			selector.OnRemoveContext 				+= OnRemoveContextFunc;

			KinetixInputManager.OnNavigate 			+= OnNavigateBank;
			KinetixInputManager.OnCancelNavigate 	+= OnCancelNavigateBank;

			KinetixInputManager.OnSelect 			+= OnSelect;
			KinetixInputManager.OnCancel 			+= OnCancel;
			KinetixInputManager.OnDeleteMode 		+= OnDeleteMode;
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			selector.OnDestroy();
			bank.OnDestroy();

			KinetixInputManager.OnSelect 			-= OnSelect;
			KinetixInputManager.OnNavigate 			-= OnNavigateBank;
			KinetixInputManager.OnCancelNavigate 	-= OnCancelNavigateBank;
			KinetixInputManager.OnSelect 			-= OnSelect;
			KinetixInputManager.OnCancel 			-= OnCancel;
			KinetixInputManager.OnDeleteMode 		-= OnDeleteMode;
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
		public void FillContext(Dictionary<string, ContextualEmote> _ContextEmotesByEvent)
		{
			selector.FillContext(_ContextEmotesByEvent);
		}

		public void FillFavorites(Dictionary<int, AnimationIds> _FavoritesAnimationIdByIndex)
		{
			selector.RefreshFavorites(_FavoritesAnimationIdByIndex);
			bank.RefreshFavorites(_FavoritesAnimationIdByIndex);
		}

		private void OnEndDragCardInventory(AnimationIds ids)
		{
			selector.OnAddAnimationToContext(ids);
		}

		private void OnAddContextFunc(string eventName, string UUID)
		{
			OnAddContext?.Invoke(eventName, UUID);
		}

		private void OnRemoveContextFunc(string eventName)
		{
			OnRemoveContext?.Invoke(eventName);
		}

		//*****************************************************************
		//********* State Input Manager
		//*****************************************************************
		InventoryCardDraggable inventoryCardSelectedToAdd;		
		
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
						selector.MoveCardOnContext(inventoryCardSelectedToAdd, _direction);
						break;

					case State.DELETE:
						selector.ShowTrashOnIcon(_direction);
						break;				
				}
			}
		}

		private void OnCancelNavigateBank()
		{
			if(Visible){
				bank.bTriggerAvailable = true;
				selector.bTriggerAvailable = true;
			}
		}
 
		private void OnSelect()
		{
			if(Visible)
			{
				switch( currentState )
				{                
					case State.NORMAL:
						// create card and put it on top of the inventory wheel favorites
						inventoryCardSelectedToAdd = bank.CreateAndGetCardOnWheel();
						selector.MoveCardOnContext(inventoryCardSelectedToAdd, Vector2.zero, 0);			
						currentState = State.ADDED;
						break;
					case State.ADDED:
						selector.SetContextCard(inventoryCardSelectedToAdd);
						Destroy(inventoryCardSelectedToAdd.gameObject);
						currentState = State.NORMAL;
						break;
					case State.DELETE:
						selector.SelectRemoveContext();		
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
						selector.ShowFavoriteOutline(-1);
						Destroy(inventoryCardSelectedToAdd.gameObject);
						currentState = State.NORMAL;
						break;
					case State.DELETE:
						selector.ShowFavoriteOutline(-1);
						selector.ShowContextTrash(-1);
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
						selector.ShowTrashOnIcon(Vector2.zero);
						currentState = State.DELETE;
						break;
					case State.ADDED:
						selector.ShowTrashOnIcon(Vector2.zero);
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


		

