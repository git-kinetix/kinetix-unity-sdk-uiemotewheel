// // ----------------------------------------------------------------------------
// // <copyright file="InventoryCardSlotFavorite.cs" company="Kinetix SAS">
// // Kinetix Unity SDK - Copyright (C) 2022 Kinetix SAS
// // </copyright>
// // ----------------------------------------------------------------------------

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Kinetix.UI.EmoteWheel
{
    public class InventoryRemoveDragButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        
        public Action OnEnterRemove;
        public Action OnExitRemove;

        [SerializeField] private ScaleEffect   scaleEffect;
                
        public void OnPointerEnter(PointerEventData eventData)
        {
            OnEnterRemove?.Invoke();
            scaleEffect.ScaleUp();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            OnExitRemove?.Invoke();
            scaleEffect.ScaleDown();
        }

        public void OnEnable()
        {            
            scaleEffect.ScaleDown();
        }
    }
}
