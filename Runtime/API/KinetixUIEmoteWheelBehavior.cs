// // ----------------------------------------------------------------------------
// // <copyright file="KinetixUISquareMenu.cs" company="Kinetix SAS">
// // Kinetix Unity SDK - Copyright (C) 2022 Kinetix SAS
// // </copyright>
// // ----------------------------------------------------------------------------

using System;
using Kinetix.UI.EmoteWheel;

namespace Kinetix.UI
{
    public static class KinetixUIEmoteWheelBehavior
    {
        public static Action<ECustomTheme> OnUpdateTheme;
        public static Action<KinetixCustomTheme> OnUpdateThemeOverride;

        public static void UpdateTheme(ECustomTheme customTheme)
        {
            OnUpdateTheme?.Invoke(customTheme);
        }

        public static void UpdateThemeOverride(KinetixCustomTheme kinetixCustomTheme)
        {
            OnUpdateThemeOverride?.Invoke(kinetixCustomTheme);
        }
    }
}
