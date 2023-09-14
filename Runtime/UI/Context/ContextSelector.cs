// // ----------------------------------------------------------------------------
// // <copyright file="InventoryView.cs" company="Kinetix SAS">
// // Kinetix Unity SDK - Copyright (C) 2022 Kinetix SAS
// // </copyright>
// // ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Kinetix.UI.EmoteWheel
{
	[Serializable]
	public class ContextSelector
	{
		[SerializeField] private Transform parentContextSelectorGrid;
		[SerializeField] private ScrollRect contextSelectorScrollRect;
		[SerializeField] private GameObject contextCardSlotPrefab;
		
		// CACHE
		private Dictionary<string, ContextualEmote>  ContextEmotesByEventName;
		private Dictionary<int, string>  eventNameByIndex;
		[SerializeField] private List<ContextCard> listCardsSlotContext;
		
		private IEnumerator DelayExitEvent;

		private int currentContextSlotIndex;

		public Action<string, string> OnAddContext;
		public Action<string> OnRemoveContext;

		public bool bTriggerAvailable = true;

		private int CellSizeX = 340;
		private int CellSizeY = 210;
		private int SpacingY = 0;
		private int Padding = 0;

		public void Init()
		{
			currentContextSlotIndex	= -1;
			listCardsSlotContext 	??= new List<ContextCard>();
			eventNameByIndex 		??= new Dictionary<int, string>();

			contextSelectorScrollRect.elasticity = 0.05f;
#if UNITY_EDITOR
			contextSelectorScrollRect.scrollSensitivity = 6;
#elif UNITY_WEBGL
			contextSelectorScrollRect.elasticity = 0.02f;
			contextSelectorScrollRect.scrollSensitivity = 6;
#else
			contextSelectorScrollRect.scrollSensitivity = 4;
#endif
		}

		public void OnDestroy()
		{
			foreach (ContextCard contextCard in listCardsSlotContext)
			{
				contextCard.OnEnter  -= EnterCardSlot;
				contextCard.OnExit   -= ExitCardSlot;
				contextCard.OnRemove -= RemoveContext;
			}
		}

		public void FillContext (Dictionary<string, ContextualEmote> _ContextEmotes)
		{
			if (_ContextEmotes == null)
				return;

			EmptyContextCards();
			
			ContextEmotesByEventName = _ContextEmotes;
			int index = 0;
			foreach (KeyValuePair<string, ContextualEmote> contextEmote in ContextEmotesByEventName)
			{
				listCardsSlotContext.Add (GetContextCard (index, contextEmote.Value));				

				AnimationIds _ids = new AnimationIds(contextEmote.Value.EmoteID);
				KinetixCore.Metadata.IsAnimationOwnedByUser(_ids.UUID, owned =>
				{
					ContextCard ctxCard = listCardsSlotContext[index];
					if (owned)
					{
						ctxCard.SetAnimationData(_ids);
					}
				});
				index += 1;
			}

			CoroutineUtils.Instance.StartCoroutine(SetDataAfterDelay());

			//refresh size of the background of the grid, to make the scrollbar effective
			RectTransform rt = (parentContextSelectorGrid.transform as RectTransform);
			rt.sizeDelta = new Vector2 (rt.sizeDelta.x, (ContextEmotesByEventName.Count) * (CellSizeY+SpacingY) + Padding/2);
		}

		private Dictionary<int, AnimationIds>  favoritesIdByIndex;
		public void RefreshFavorites(Dictionary<int, AnimationIds> _FavoritesAnimationIdByIndex)
		{
			if (_FavoritesAnimationIdByIndex == null)
				return;

			favoritesIdByIndex = _FavoritesAnimationIdByIndex;
		}

		private ContextCard GetContextCard (int index, ContextualEmote contextEmote)
		{
			ContextCard contextCard = GameObject.Instantiate(contextCardSlotPrefab, parentContextSelectorGrid).GetComponent<ContextCard>();
			contextCard.Init(index);
			contextCard.OnEnter  += EnterCardSlot;
			contextCard.OnExit   += ExitCardSlot;
			contextCard.OnRemove += RemoveContext;

			int posY = -index * (CellSizeY+SpacingY) - Padding;
			int posX = Padding;
			(contextCard.transform  as RectTransform).sizeDelta = new Vector2(CellSizeX, CellSizeY);
			(contextCard.transform  as RectTransform).localPosition = new Vector3(posX, posY, 0f);

			eventNameByIndex[index] = contextEmote.ContextName;

			contextCard.SetEventContextData(contextEmote);
		
			return contextCard;
		}

		private void EmptyContextCards()
		{
			foreach (ContextCard contextCard in listCardsSlotContext)
			{
				contextCard.OnEnter  -= EnterCardSlot;
				contextCard.OnExit   -= ExitCardSlot;
				contextCard.OnRemove -= RemoveContext;
				contextCard.RemoveAnimationData(true);
				GameObject.Destroy(contextCard.gameObject);
			}
			listCardsSlotContext.Clear();
		}

		private bool IsAnimationIsInTheFavorite(string UUID)
		{
			if(favoritesIdByIndex == null)
				return false;

			foreach (KeyValuePair<int, AnimationIds> favoriteKVP in favoritesIdByIndex)
			{
				if (favoriteKVP.Value.UUID == UUID)
					return true;
			}
			return false;
		}

		private IEnumerator SetDataAfterDelay ()
		{
			yield return new WaitForSeconds(1);
			foreach (ContextCard contextCard in listCardsSlotContext)
			{
				contextCard.SetIcon();
			}
		}

		private void EnterCardSlot (int _Index)
		{
			currentContextSlotIndex = _Index;
			if(DelayExitEvent!= null)
				CoroutineUtils.Instance.StopCoroutine(DelayExitEvent);
		}

		private void ExitCardSlot ()
		{
			DelayExitEvent = DelayOneFrame();
			CoroutineUtils.Instance.StartCoroutine(DelayExitEvent);
		}
		
		private IEnumerator DelayOneFrame ()
		{
			yield return new WaitForEndOfFrame();
			currentContextSlotIndex = -1;
		}

		public void OnAddAnimationToContext(AnimationIds Ids)
		{
			if (currentContextSlotIndex == -1)
				return;

			if (listCardsSlotContext[currentContextSlotIndex].hasData)
				RemoveContext(currentContextSlotIndex);

			SetAndShowContextDataOnSlot(currentContextSlotIndex, Ids);
			OnAddContext?.Invoke(eventNameByIndex[currentContextSlotIndex], Ids.UUID);
		}

		private void SetAndShowContextDataOnSlot(int _Index, AnimationIds _Ids)
		{
			listCardsSlotContext[_Index].SetAnimationData(_Ids);
		}

		public void RemoveContext(int _Index)
		{
			if(listCardsSlotContext[_Index].hasData)
			{
				listCardsSlotContext[_Index].RemoveAnimationData(true);
				string eventName = eventNameByIndex[_Index];
				OnRemoveContext?.Invoke(eventName);
			}
		}

		public void ShowFavoriteOutline(int indexToShow = -1)
		{
			for (int i = 0; i < listCardsSlotContext.Count; i++)
			{
				if(i != indexToShow)
					listCardsSlotContext[i].HideOutline();
				else 
					listCardsSlotContext[i].ShowOutline();
			}
		}

		public void ShowContextTrash(int indexToShow = -1)
		{
			for (int i = 0; i < listCardsSlotContext.Count; i++)
			{
				if(i != indexToShow)
					listCardsSlotContext[i].HideTrash();
				else 
					listCardsSlotContext[i].ShowTrash();
			}
		}

		private Coroutine scrollToPosition;
		private Coroutine moveCardToPosition;
		private void UpdateScrollPosition(int indexToGo)
		{			
			float positionYContent = indexToGo * (CellSizeY + SpacingY);
			float maxHeight = contextSelectorScrollRect.content.sizeDelta.y - (contextSelectorScrollRect.transform as RectTransform).sizeDelta.y;

			//if height of the grid is inferior to the scrollRect, so no need to scroll
			if (maxHeight < 0f) maxHeight = 0f;

			positionYContent = Mathf.Clamp(positionYContent, 0f, maxHeight);

			Vector3 tempPosition = (contextSelectorScrollRect.content.transform  as RectTransform).localPosition;

			if (inventoryCardSelectedToAdd != null)
			{
				Vector3 moveCardToPositionInitial = listCardsSlotContext[indexToGo].GetPositionEmoteCard() + new Vector3(-100f, 100f, 0f);
				(inventoryCardSelectedToAdd.transform as RectTransform).position = moveCardToPositionInitial;
			}

			//get the final position of the card
			(contextSelectorScrollRect.content.transform  as RectTransform).localPosition = new Vector3(0f, positionYContent, 0f);
			
			Vector3 desiredCardPosition = listCardsSlotContext[indexToGo].GetPositionEmoteCard() + new Vector3(-100f, 100f, 0f);

			moveCardToPosition = CoroutineUtils.Instance.StartCoroutine(TweenCardToPosition(desiredCardPosition));
			
			(contextSelectorScrollRect.content.transform as RectTransform).localPosition = tempPosition;

			scrollToPosition = CoroutineUtils.Instance.StartCoroutine(TweenToScrollPosition(positionYContent));
		}
	
		private float duration = 0.3f;
		private IEnumerator TweenToScrollPosition(float _EndScrollPosition)
		{
			float time = 0.0f;
			while (time < duration)
			{
				float currentPosition = (contextSelectorScrollRect.content.transform as RectTransform).localPosition.y;
				(contextSelectorScrollRect.content.transform  as RectTransform).localPosition = new Vector3(0f, Mathf.Lerp(currentPosition, _EndScrollPosition, time / duration), 0f);
				time += Time.deltaTime;
				yield return null;
			}
			(contextSelectorScrollRect.content.transform  as RectTransform).localPosition = new Vector3(0f, _EndScrollPosition, 0f);
		}

		private IEnumerator TweenCardToPosition(Vector3 _EndCardPosition)
		{
			float time = 0.0f;
			while (time < duration)
			{
				if (inventoryCardSelectedToAdd==null) yield break;;
				float currentCardPosition = (inventoryCardSelectedToAdd.transform as RectTransform).position.y;
				(inventoryCardSelectedToAdd.transform as RectTransform).position = new Vector3(_EndCardPosition.x, Mathf.Lerp(currentCardPosition, _EndCardPosition.y, time / duration), 0f);
				time += Time.deltaTime;
				yield return null;
			}
			
			(inventoryCardSelectedToAdd.transform as RectTransform).position = new Vector3(_EndCardPosition.x, _EndCardPosition.y, 0f);
		}

		//******************************************************************
		// GamePad Manager
		//******************************************************************

		int indexSlotWhereToAdd = 0;
		InventoryCardDraggable inventoryCardSelectedToAdd;
		public void MoveCardOnContext(InventoryCardDraggable _inventoryCardSelectedToAdd, Vector2 _direction, int newContextSlotIndex =-1)
		{
			if(_inventoryCardSelectedToAdd == null)
				return;

			if (bTriggerAvailable || _direction == Vector2.zero)
			{
				bTriggerAvailable = false;

				if(newContextSlotIndex == -1) newContextSlotIndex = indexSlotWhereToAdd;

				indexSlotWhereToAdd = KinetixUtils.GetIndexFromAList(_direction, newContextSlotIndex, listCardsSlotContext.Count);
				inventoryCardSelectedToAdd = _inventoryCardSelectedToAdd;

				UpdateScrollPosition(indexSlotWhereToAdd);
				ShowFavoriteOutline(indexSlotWhereToAdd);
			}
		}

		public void SetContextCard(InventoryCardDraggable inventoryCardSelectedToAdd)
		{
			currentContextSlotIndex = indexSlotWhereToAdd;
			OnAddAnimationToContext(new AnimationIds(inventoryCardSelectedToAdd.UUID));
			ShowFavoriteOutline(-1);
		}

		public void ShowTrashOnIcon(Vector2 _direction)
		{
			if (bTriggerAvailable)
			{
				bTriggerAvailable = false;
				if (currentContextSlotIndex == -1) currentContextSlotIndex = 0;
				int indexToShow = KinetixUtils.GetIndexFromAList(_direction, currentContextSlotIndex, listCardsSlotContext.Count);
				currentContextSlotIndex = indexToShow;
				UpdateScrollPosition(currentContextSlotIndex);
				ShowContextTrash(indexToShow);
				ShowFavoriteOutline(indexToShow);				
			}
		}

		public void SelectRemoveContext()
		{
			RemoveContext(currentContextSlotIndex);
			ShowContextTrash(-1);
			ShowFavoriteOutline(-1);
		}
	}
}
