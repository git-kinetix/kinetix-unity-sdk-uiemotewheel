// // ----------------------------------------------------------------------------
// // <copyright file="KinetixSquareMenuGenerator.cs" company="Kinetix SAS">
// // Kinetix Unity SDK - Copyright (C) 2022 Kinetix SAS
// // </copyright>
// // ----------------------------------------------------------------------------

using System.IO;
using UnityEditor;
using UnityEngine;

namespace Kinetix.UI.EmoteWheel
{
    public static class KinetixCustomThemeGenerator
    {
        [MenuItem("Kinetix/UI/EmoteWheel/Create New Custom Theme")]
        public static void GenerateCustomTheme()
        {
            KinetixCustomTheme customTheme = ScriptableObject.CreateInstance<KinetixCustomTheme>();

            if (!Directory.Exists("Assets/Resources/"))
                Directory.CreateDirectory("Assets/Resources");

            if (!Directory.Exists("Assets/Resources/Kinetix"))
                Directory.CreateDirectory("Assets/Resources/Kinetix");

            if (!Directory.Exists("Assets/Resources/Kinetix/Theme"))
                Directory.CreateDirectory("Assets/Resources/Kinetix/Theme");

            string uniqueFileName = AssetDatabase.GenerateUniqueAssetPath("Assets/Resources/Kinetix/Theme/CustomizeNewTheme.asset");

            AssetDatabase.CreateAsset(customTheme, uniqueFileName);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = customTheme;
        }
    }
}
