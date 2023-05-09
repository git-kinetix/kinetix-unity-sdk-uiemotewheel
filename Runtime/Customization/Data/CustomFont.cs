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
    public class CustomFont
    {
        [SerializeField]
        public ECustomTagFontWeight Tag;
        [SerializeField]
        public bool EnableCustomFont;
        [SerializeField]
        public TMP_FontAsset font;
    }
}
