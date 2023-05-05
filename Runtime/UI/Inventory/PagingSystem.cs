// // ----------------------------------------------------------------------------
// // <copyright file="InventoryFavoritePaging.cs" company="Kinetix SAS">
// // Kinetix Unity SDK - Copyright (C) 2022 Kinetix SAS
// // </copyright>
// // ----------------------------------------------------------------------------

using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Kinetix.UI.Common.Translation;
using UnityEngine.EventSystems;

namespace Kinetix.UI.EmoteWheel
{
    public class PagingSystem : MonoBehaviour
    {
        [SerializeField] private string          suffix;
        [SerializeField] private Button          buttonLeft;
        [SerializeField] private Button          buttonRight;
        [SerializeField] private KinetixUITranslator labelPage;

        public UnityAction OnSwitchPreviousPage;
        public UnityAction OnSwitchNextPage;

        public void Init()
        {
            buttonLeft.onClick.AddListener(OnSwitchPreviousPage);
            buttonRight.onClick.AddListener(OnSwitchNextPage);
        }

        public void UpdatePageLabel(int _Page, int _TotalPage)
        {
            labelPage.text = suffix + (_Page + 1) + "/" + _TotalPage;
        }

        public void OnDestroy()
        {
            buttonLeft.onClick.RemoveAllListeners();
            buttonRight.onClick.RemoveAllListeners();
        }

        public void ClickLeft()
        {
            ExecuteEvents.Execute(buttonLeft.gameObject, new BaseEventData(EventSystem.current), ExecuteEvents.submitHandler);
        }

        public void ClickRight()
        {
            ExecuteEvents.Execute(buttonRight.gameObject, new BaseEventData(EventSystem.current), ExecuteEvents.submitHandler);
        }
    }
}
