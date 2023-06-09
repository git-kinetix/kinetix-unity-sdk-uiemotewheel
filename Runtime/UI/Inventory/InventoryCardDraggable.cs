// // ----------------------------------------------------------------------------
// // <copyright file="InventoryCardDraggable.cs" company="Kinetix SAS">
// // Kinetix Unity SDK - Copyright (C) 2022 Kinetix SAS
// // </copyright>
// // ----------------------------------------------------------------------------

using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Kinetix.UI.EmoteWheel
{
    public class InventoryCardDraggable : InventoryCardSlotWallet
    {
        public Action<AnimationIds> OnEndDragCard;
        public bool _movable = true;

        // CACHE
        private RectTransform rectTr;
        private Transform     tr;

        private void Update()
        {
            if(_movable)
                if (KinetixInputManager.IsPressed() )
                    UpdatePosition();
                else if (KinetixInputManager.WasReleasedThisFrame())
                    EndDrag();
        }

        private void UpdatePosition()
        {
            tr ??= transform;            
            transform.position = new Vector3( KinetixInputManager.PositionTouchOrMouse().x, KinetixInputManager.PositionTouchOrMouse().y, tr.parent.position.z);
        }

        private void EndDrag()
        {
            OnEndDragCard?.Invoke(Ids);
            animationIcon.Unload();
            Destroy(gameObject);
        }
    }
}
