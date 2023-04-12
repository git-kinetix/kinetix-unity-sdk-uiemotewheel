// // ----------------------------------------------------------------------------
// // <copyright file="DraggableItem.cs" company="Kinetix SAS">
// // Kinetix Unity SDK - Copyright (C) 2022 Kinetix SAS
// // </copyright>
// // ----------------------------------------------------------------------------

using UnityEngine;
using UnityEngine.EventSystems;


namespace Kinetix.UI.EmoteWheel
{
    public abstract class DraggableItem : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        protected abstract void OnBeginDrag();
        protected abstract void OnDrag();
        protected abstract void OnEndDrag();

        bool IsDragging;

        public virtual void OnPointerDown(PointerEventData eventData)
        {
            IsDragging = true;
            OnBeginDrag();
        }
        
        public virtual void OnPointerUp(PointerEventData eventData)
        {
            IsDragging = false;
            OnEndDrag();
        }

        void Update()
        {
            if(IsDragging)
                OnDrag();
        }
    }
}
