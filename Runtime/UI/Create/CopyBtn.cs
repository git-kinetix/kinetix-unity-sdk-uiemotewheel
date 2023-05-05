// // ----------------------------------------------------------------------------
// // <copyright file="EmoteSelectorSlot.cs" company="Kinetix SAS">
// // Kinetix Unity SDK - Copyright (C) 2022 Kinetix SAS
// // </copyright>
// // ----------------------------------------------------------------------------

using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Kinetix.UI.EmoteWheel
{
    public class CopyBtn : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {        
        [SerializeField] private ScaleEffect			scaleEffect;

        public void OnPointerEnter(PointerEventData eventData)
        {
            scaleEffect.ScaleUp();            
        }

        public void OnPointerExit(PointerEventData  eventData)
        {
            scaleEffect.ScaleDown();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            scaleEffect.ScaleDown();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            scaleEffect.ScaleUp();
        }
    }
}
