// // ----------------------------------------------------------------------------
// // <copyright file="KinetixSquareMenuConfigurationEditor.cs" company="Kinetix SAS">
// // Kinetix Unity SDK - Copyright (C) 2022 Kinetix SAS
// // </copyright>
// // ----------------------------------------------------------------------------

using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace Kinetix.UI.EmoteWheel.Internal
{
    [CustomEditor(typeof(KinetixCustomTheme))]
    public class KinetixCustomThemeEditor : Editor
    {
        // CACHE
        [SerializeField] private Texture2D bannerTexture;

        // GLOBAL
        private SerializedProperty spCustomSprites;
        private SerializedProperty spCustomFonts;
        private SerializedProperty spCustomColors;

        private bool bFallBackLoaded = false;

        KinetixCustomTheme kinetixThemeFallback;
        KinetixCustomTheme kinetixThemeHere;

        private void OnEnable()
        {
            if (!bannerTexture)
                bannerTexture = Resources.Load<Texture2D>("KinetixLogoBanner");

            spCustomSprites = serializedObject.FindProperty("customSprites");
            spCustomFonts = serializedObject.FindProperty("customFonts");
            spCustomColors = serializedObject.FindProperty("customColors");

            kinetixThemeHere = target as KinetixCustomTheme;

            if ( !bFallBackLoaded )
                LoadFallBackTheme();

            //check if all Sprite are created in the Target, if not create new entry into the list at the same place and fill it
            bool bFillWithFallback = false;
            foreach (int i in System.Enum.GetValues(typeof(ECustomTagSprite)))  
            {  
                bFillWithFallback = false;

                if (kinetixThemeHere.customSprites == null)
                    kinetixThemeHere.customSprites = new List<CustomSprite>();

                if (!kinetixThemeHere.customSprites.Exists(customSprite => customSprite.Tag == (ECustomTagSprite)i ) ) 
                {
                    kinetixThemeHere.customSprites.Insert( i, new CustomSprite() );
                    kinetixThemeHere.customSprites[i].Tag = (ECustomTagSprite)i;
                    bFillWithFallback = true;
                }

                if( kinetixThemeHere.customSprites[i].sprite == null && kinetixThemeFallback.customSprites[i].sprite != null)
                    bFillWithFallback = true;
                
                if( bFillWithFallback )
                {
                    kinetixThemeHere.customSprites[i].EnableCustomSprite = kinetixThemeFallback.customSprites[i].EnableCustomSprite;
                    kinetixThemeHere.customSprites[i].sprite = kinetixThemeFallback.customSprites[i].sprite;
                    kinetixThemeHere.customSprites[i].EnableCustomColor = kinetixThemeFallback.customSprites[i].EnableCustomColor;
                    kinetixThemeHere.customSprites[i].color = kinetixThemeFallback.customSprites[i].color;
                }
            }
            
            //same for color, if not entry for the enumTag, create new entry into the list at the same place and fill it
            foreach (int i in System.Enum.GetValues(typeof(ECustomTagColor)))  
            {  
                if (kinetixThemeHere.customColors == null)
                    kinetixThemeHere.customColors = new List<CustomColor>();

                if (! kinetixThemeHere.customColors.Exists(customColor => customColor.Tag == (ECustomTagColor)i ) ) 
                {
                    kinetixThemeHere.customColors.Insert( i, new CustomColor() );
                    kinetixThemeHere.customColors[i].Tag = (ECustomTagColor)i;

                    kinetixThemeHere.customColors[i].color = kinetixThemeFallback.customColors[i].color;
                    kinetixThemeHere.customColors[i].EnableCustomColor = kinetixThemeFallback.customColors[i].EnableCustomColor;
                }
            }

            //same for font
            foreach (int i in System.Enum.GetValues(typeof(ECustomTagFontWeight)))  
            {  
                if (kinetixThemeHere.customFonts == null)
                    kinetixThemeHere.customFonts = new List<CustomFont>();
                
                if (! kinetixThemeHere.customFonts.Exists(customFont => customFont.Tag == (ECustomTagFontWeight)i ) ) 
                {
                    kinetixThemeHere.customFonts.Insert( i, new CustomFont() );
                    kinetixThemeHere.customFonts[i].Tag = (ECustomTagFontWeight)i;

                    kinetixThemeHere.customFonts[i].font = kinetixThemeFallback.customFonts[i].font;
                    kinetixThemeHere.customFonts[i].EnableCustomFont = kinetixThemeFallback.customFonts[i].EnableCustomFont;
                }
            }

            EditorUtility.SetDirty(target);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }


        private void LoadFallBackTheme()
        {
            kinetixThemeFallback = Resources.Load<KinetixCustomTheme>("ScriptableObject/CustomizeUILightMode");
            SerializedObject soKinetixThemeFallBack = new SerializedObject(kinetixThemeFallback);

            bFallBackLoaded = true;
        }


        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawBanner();

            DrawCustomColors();
            DrawCustomFonts();
            DrawCustomSprites();

            DrawSeparator();

            serializedObject.ApplyModifiedProperties();

            AssetDatabase.SaveAssets();
        }


        private void DrawCustomColors()
        {
            DrawSeparator();
            GUILayout.Label(new GUIContent("CUSTOM COLORS"), EditorStyles.boldLabel);

            if(spCustomColors.arraySize <= 0)
                return;

            foreach (int i in System.Enum.GetValues(typeof(ECustomTagColor)))
            {
                DrawSeparator();
                SerializedProperty spCustomColor = spCustomColors.GetArrayElementAtIndex(i);

                EditorGUILayout.LabelField( i + " - " +  (ECustomTagColor)spCustomColor.FindPropertyRelative("Tag").enumValueIndex );

                SerializedProperty EnableCustomColor = spCustomColor.FindPropertyRelative("EnableCustomColor");
                SerializedProperty color = spCustomColor.FindPropertyRelative("color");
                EditorGUILayout.PropertyField(EnableCustomColor, new GUIContent("Enable Custom Color"));

                if (EnableCustomColor.boolValue)
                    EditorGUILayout.PropertyField(color, new GUIContent("Custom Color"));
            }
        }

        private void DrawCustomFonts()
        {
            DrawSeparator();
            GUILayout.Label(new GUIContent("CUSTOM FONTS"), EditorStyles.boldLabel);

            if(spCustomFonts.arraySize <= 0) 
                return;

            foreach (int i in System.Enum.GetValues(typeof(ECustomTagFontWeight)))  
            {
                DrawSeparator();
                
                SerializedProperty spCustomFont = spCustomFonts.GetArrayElementAtIndex(i);

                EditorGUILayout.LabelField( i + " - " +  (ECustomTagFontWeight)spCustomFont.FindPropertyRelative("Tag").enumValueIndex );

                SerializedProperty EnableCustomFont = spCustomFont.FindPropertyRelative("EnableCustomFont");
                SerializedProperty font = spCustomFont.FindPropertyRelative("font");
                EditorGUILayout.PropertyField(EnableCustomFont, new GUIContent("Enable Custom Font"));

                if (EnableCustomFont.boolValue)
                    EditorGUILayout.PropertyField(font, new GUIContent("Custom Font"));                
            }
        }

        private void DrawCustomSprites()
        {
            DrawSeparator();
            GUILayout.Label(new GUIContent("CUSTOM SPRITE"), EditorStyles.boldLabel);

            foreach (int i in System.Enum.GetValues(typeof(ECustomTagSprite)))
            {  
                DrawSeparator();
                
                if( i >= spCustomSprites.arraySize )
                    return;

                SerializedProperty spCustomSprite = spCustomSprites.GetArrayElementAtIndex(i); 

                EditorGUILayout.LabelField( i + " - " +  (ECustomTagSprite)spCustomSprite.FindPropertyRelative("Tag").enumValueIndex );

                SerializedProperty EnableCustomColor = spCustomSprite.FindPropertyRelative("EnableCustomColor");
                SerializedProperty color = spCustomSprite.FindPropertyRelative("color");
                EditorGUILayout.PropertyField(EnableCustomColor, new GUIContent("Enable Custom Color"));

                if (EnableCustomColor.boolValue)
                    EditorGUILayout.PropertyField(color, new GUIContent("Custom Color"));

                if( (ECustomTagSprite)i == ECustomTagSprite.SPRITE_ICON)
                    continue;

                SerializedProperty EnableCustomSprite = spCustomSprite.FindPropertyRelative("EnableCustomSprite");
                SerializedProperty sprite = spCustomSprite.FindPropertyRelative("sprite");
                EditorGUILayout.PropertyField(EnableCustomSprite, new GUIContent("Enable Custom Sprite"));

                if (EnableCustomSprite.boolValue)
                    EditorGUILayout.PropertyField(sprite, new GUIContent("Custom Sprite"));
            }
        }

        private void DrawBanner()
        {
            if (bannerTexture != null)
                EditorGUI.DrawTextureTransparent(
                    new Rect(Screen.width / 4.0f - bannerTexture.width / 2.0f, 10, bannerTexture.width,
                        bannerTexture.height), bannerTexture);

            EditorGUILayout.Space(bannerTexture.height + 5);
        }

        private void DrawSeparator()
        {
            GUILayout.Space(5);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Space(5);
        }
    }
}
