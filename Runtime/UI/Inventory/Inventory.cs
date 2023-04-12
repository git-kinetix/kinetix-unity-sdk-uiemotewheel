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
    public class Inventory : MonoBehaviour
    {
        [Header("References")]
        public InventoryView View;

        public Action<int, AnimationIds> OnAddFavorite;
        public Action<int>               OnRemoveFavorite;
        public Action                    OnRefillFavorites;
        public Action                    OnRefillBank;

        public void Init()
        {
            View.OnAddFavorite     += OnAddFavoriteAnimation;
            View.OnRemoveFavorite  += OnRemoveFavoriteAnimation;
            View.OnRefillFavorites += RefillFavorites;
            View.OnRefillBank      += RefillBank;
            View.Init();
        }

        private void OnDestroy()
        {
            if (View == null)
                return;

            View.OnAddFavorite     -= OnAddFavoriteAnimation;
            View.OnRemoveFavorite  -= OnRemoveFavoriteAnimation;
            View.OnRefillFavorites -= RefillFavorites;
            View.OnRefillBank      -= RefillBank;
        }

        public void FillFavorites(Dictionary<int, AnimationIds> _FavoritesAnimationIdByIndex)
        {
            View.FillFavorites(_FavoritesAnimationIdByIndex);
        }

        public void RefreshInventoryBankAnimations()
        {
            View.RefreshInventoryBankAnimations();
        }

        private void OnAddFavoriteAnimation(int _Index, AnimationIds ids)
        {
            OnAddFavorite?.Invoke(_Index, ids);
        }

        private void OnRemoveFavoriteAnimation(int _Index)
        {
            OnRemoveFavorite?.Invoke(_Index);
        }

        private void RefillFavorites()
        {
            OnRefillFavorites?.Invoke();
        }

        private void RefillBank()
        {
            OnRefillBank?.Invoke();
        }
    }
}
