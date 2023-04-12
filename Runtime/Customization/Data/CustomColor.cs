// // ----------------------------------------------------------------------------
// // <copyright file="EKinetixUICategory.cs" company="Kinetix SAS">
// // Kinetix Unity SDK - Copyright (C) 2022 Kinetix SAS
// // </copyright>
// // ----------------------------------------------------------------------------

using UnityEngine;
using TMPro;

namespace Kinetix.UI.EmoteWheel
{
    [System.Serializable]
    public class CustomColor
    {
        [SerializeField]
        public ECustomTagColor Tag;
        [SerializeField]
        public bool EnableCustomColor;
        [SerializeField]
        public Color color;
    }
}
