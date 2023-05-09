// // ----------------------------------------------------------------------------
// // <copyright file="KinetixUISquareMenu.cs" company="Kinetix SAS">
// // Kinetix Unity SDK - Copyright (C) 2022 Kinetix SAS
// // </copyright>
// // ----------------------------------------------------------------------------

using Kinetix.UI.EmoteWheel;
using UnityEngine;

namespace Kinetix.UI
{
    public static class KinetixUIEmoteWheel
    {
        public static KinetixEmoteWheelManager kinetixEmoteWheel;

        public static void Initialize( KinetixUIEmoteWheelConfiguration kinetixCustomUIEmoteWheelConfiguration = null)
        {
            kinetixEmoteWheel = Object.FindObjectOfType<KinetixEmoteWheelManager>();
            if (kinetixEmoteWheel == null)
                KinetixEmoteWheelManager.Instantiate( kinetixCustomUIEmoteWheelConfiguration );
        }

        public static void UpdateTheme( ECustomTheme customTheme )
        {
            KinetixUIEmoteWheelBehavior.UpdateTheme(customTheme);
        }

        public static void UpdateThemeOverride( KinetixCustomTheme kinetixCustomTheme)
        {
            KinetixUIEmoteWheelBehavior.UpdateThemeOverride( kinetixCustomTheme );
        }
    }
}
