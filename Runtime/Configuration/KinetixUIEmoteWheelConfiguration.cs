// // ----------------------------------------------------------------------------
// // <copyright file="EKinetixUICategory.cs" company="Kinetix SAS">
// // Kinetix Unity SDK - Copyright (C) 2022 Kinetix SAS
// // </copyright>
// // ----------------------------------------------------------------------------

using Kinetix.UI.Common;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Kinetix.UI.EmoteWheel
{
    public class KinetixUIEmoteWheelConfiguration : KinetixCommonUIConfiguration
    {
        [Tooltip("Kinetix Customization")]
        public KinetixCustomTheme customThemeOverride;

        [Tooltip("Dark / Light")]
        public ECustomTheme customTheme;

        [Tooltip("Enabled Categories")] 
        public EKinetixUICategory[] enabledCategories;

        [Tooltip("Count Of Favorite Pages")]
        public int c_CountFavoritePages = 0;
    }
}
