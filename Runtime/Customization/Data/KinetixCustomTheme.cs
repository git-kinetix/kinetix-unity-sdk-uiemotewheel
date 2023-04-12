// // ----------------------------------------------------------------------------
// // <copyright file="KinetixCustomTheme.cs" company="Kinetix SAS">
// // Kinetix Unity SDK - Copyright (C) 2022 Kinetix SAS
// // </copyright>
// // ----------------------------------------------------------------------------

using Kinetix.UI.Common;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace Kinetix.UI.EmoteWheel
{
    public class KinetixCustomTheme : ScriptableObject
    {
        [SerializeField]
        public List<CustomSprite> customSprites;
        [SerializeField]
        public List<CustomFont> customFonts;
        [SerializeField]
        public List<CustomColor> customColors;
    }
}
