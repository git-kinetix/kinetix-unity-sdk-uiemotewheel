// // ----------------------------------------------------------------------------
// // <copyright file="InventoryCardDraggable.cs" company="Kinetix SAS">
// // Kinetix Unity SDK - Copyright (C) 2022 Kinetix SAS
// // </copyright>
// // ----------------------------------------------------------------------------

using System;
using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

namespace Kinetix.UI.EmoteWheel
{
    public class InventoryCardFavoriteDraggable : InventoryCardSlotFavorite
    {
        public Action<AnimationIds> OnEndDragCard;

        // CACHE
        private RectTransform rectTr;
        private Transform     tr;

        private void Update()
        {
            if (Mouse.current.leftButton.isPressed)
                UpdatePosition();
            else if (Mouse.current.leftButton.wasReleasedThisFrame)
                EndDrag();
        }

        private void UpdatePosition()
        {
            tr ??= transform;
            Vector3 pointToGo = new Vector3(Mouse.current.position.ReadValue().x, Mouse.current.position.ReadValue().y, tr.parent.position.z);
        }
        
        private void EndDrag()
        {           
            OnEndDragCard?.Invoke(Ids);
        }

        public void AnimationBack()
        {
            StartCoroutine(BackToStartingPoint());
        }

        private IEnumerator BackToStartingPoint()
        {
            yield return new WaitForEndOfFrame();
        }

        public void DestroyThis()
        {
            Destroy(gameObject);
        }
    }
}
