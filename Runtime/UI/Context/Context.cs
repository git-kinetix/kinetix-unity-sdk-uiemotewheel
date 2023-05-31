// // ----------------------------------------------------------------------------
// // <copyright file="Inventory.cs" company="Kinetix SAS">
// // Kinetix Unity SDK - Copyright (C) 2022 Kinetix SAS
// // </copyright>
// // ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Kinetix.UI.Common;
using UnityEngine;

namespace Kinetix.UI.EmoteWheel
{
    public class Context : MonoBehaviour
    {
        [Header("References")]
        public ContextView View;

        public Action<string, string>  OnAddContext;
        public Action<string>                   OnRemoveContext;
        public Action                           OnRefillContext;
        public Action                           OnRefillBank;
        public Action<string>					OnCheckEmote;

        public void Init()
        {
            View.OnAddContext       += OnAddContextEvent;
            View.OnRemoveContext    += OnRemoveContextEvent;
            View.OnRefillContext    += RefillContext;
            View.OnRefillBank       += RefillBank;
            View.OnCheckEmoteAction += CheckEmote;
            View.Init();
        }

        private void OnDestroy()
        {
            if (View == null)
                return;

            View.OnAddContext       -= OnAddContextEvent;
            View.OnRemoveContext    -= OnRemoveContextEvent;
            View.OnRefillContext    -= RefillContext;
            View.OnRefillBank       -= RefillBank;
            View.OnCheckEmoteAction -= CheckEmote;
        }

        public void FillContext(Dictionary<string, ContextualEmote> _ContextualEmoteByEvent)
        {
            View.FillContext(_ContextualEmoteByEvent);
        }

        public void FillFavorites(Dictionary<int, AnimationIds> _FavoritesAnimationIdByIndex)
        {
            View.FillFavorites(_FavoritesAnimationIdByIndex);
        }

		public void RefreshInventoryBankAnimations()
        {
            View.RefreshInventoryBankAnimations();
        }

        private void OnAddContextEvent(string eventName, string UUID)
        {
            OnAddContext?.Invoke(eventName, UUID);
        }

        private void OnRemoveContextEvent(string eventName)
        {
            OnRemoveContext?.Invoke(eventName);
        }

        private void RefillContext()
        {
            OnRefillContext?.Invoke();
        }

        private void RefillBank()
        {
            OnRefillBank?.Invoke();
        }

        private void CheckEmote(string UUID)
        {
            OnCheckEmote?.Invoke(UUID);
        }
    }
}
