// // ----------------------------------------------------------------------------
// // <copyright file="Inventory.cs" company="Kinetix SAS">
// // Kinetix Unity SDK - Copyright (C) 2022 Kinetix SAS
// // </copyright>
// // ----------------------------------------------------------------------------

using UnityEngine;

namespace Kinetix.UI.EmoteWheel
{
    public class Create : MonoBehaviour
    {
        [Header("References")]
        public CreateView View;

        public void Init()
        {
            View.Init();
        }

        public void CreateQRCode()
        {
            View.CreateQRCode();
        }
    }
}
