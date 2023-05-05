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
    public class IconDevice : MonoBehaviour
    {
        [SerializeField] private GameObject iconClassic;
        [SerializeField] private GameObject iconXBOX;
        [SerializeField] private GameObject iconPS;
        [SerializeField] private GameObject labelToHide;

        private void Awake()
        {
            KinetixInputManager.OnChangeDevice += ChangeIconDevice;
        }

        private void OnDestroy()
        {
            KinetixInputManager.OnChangeDevice -= ChangeIconDevice;           
        }

        private void ChangeIconDevice(string device)
        {
            Show(iconClassic, false);
            Show(iconXBOX, false);
            Show(iconPS, false);
            Show(labelToHide, false);

            if( device.Contains(InputMappingConstants.XBOX) )
            {
                Show(iconXBOX, true);
                Show(labelToHide, true);
            }
            //else if( device.Contains(InputMappingConstants.KEYBOARD) )
            else if( device.Contains("InputMappingConstants.PLAYSTATION") )
            {
                Show(iconPS, true);
                Show(labelToHide, true);
            }
            else if( device.Contains(InputMappingConstants.GAMEPAD) )
            {
                Show(iconPS, true);
                Show(labelToHide, true);
            } 
            else 
            {
                Show(iconClassic, true);
            }
        }


        private void Show(GameObject go, bool bShow)
        {
            if(go != null) go.SetActive(bShow);
        }


    }
}
