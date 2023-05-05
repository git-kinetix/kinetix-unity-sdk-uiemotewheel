// // ----------------------------------------------------------------------------
// // <copyright file="EmoteSelector.cs" company="Kinetix SAS">
// // Kinetix Unity SDK - Copyright (C) 2022 Kinetix SAS
// // </copyright>
// // ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kinetix.UI.EmoteWheel
{
    public class EmoteSelector : MonoBehaviour
    {
        [Header("References")] [SerializeField]
        public EmoteSelectorView View;

        public Action<AnimationIds> OnSelectAnimation;
        public Action               OnRefillWheel;

        public void Init()
        {
            View.OnSelectAnimation += SelectAnimation;
            View.OnRefillWheel     += RefillWheel;
            View.Init();
        }

        public void Load(Dictionary<int, AnimationIds> _FavoritesIdByIndex)
        {
            View.FillSlots(_FavoritesIdByIndex);
        }

        private void OnDestroy()
        {
            if (View == null) 
                return;
            
            View.OnSelectAnimation -= OnSelectAnimation;
            View.OnRefillWheel     += RefillWheel;
        }

        private void RefillWheel()
        {
            OnRefillWheel?.Invoke();
        }
        
        private void SelectAnimation(AnimationIds _IDs)
        {
            OnSelectAnimation?.Invoke(_IDs);
        }
    }
}
