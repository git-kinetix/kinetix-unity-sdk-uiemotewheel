// // ----------------------------------------------------------------------------
// // <copyright file="KinetixCustomTheme.cs" company="Kinetix SAS">
// // Kinetix Unity SDK - Copyright (C) 2022 Kinetix SAS
// // </copyright>
// // ----------------------------------------------------------------------------

using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Kinetix.UI.EmoteWheel
{
    public class KinetixCustomizeManager
    {

        //cache asset
        public static Dictionary<ECustomTagSprite, CustomSprite> dCustomSprites;
        public static Dictionary<ECustomTagColor, CustomColor> dCustomColors;
        public static Dictionary<ECustomTagFontWeight, CustomFont> dCustomFonts;

        //ref iCustomizer
        public static Dictionary<ECustomTagSprite, List<ISpriteCustomizable>> refCustomSprite;
        public static Dictionary<ECustomTagFontWeight, List<IFontCustomizable>> refCustomFont;
        public static Dictionary<ECustomTagColor, List<IFontCustomizable>> refCustomColor;

        private static void Awake()
        {
            refCustomSprite = new Dictionary<ECustomTagSprite, List<ISpriteCustomizable>>();
            refCustomFont = new Dictionary<ECustomTagFontWeight, List<IFontCustomizable>>();
            refCustomColor = new Dictionary<ECustomTagColor, List<IFontCustomizable>>();
        }

        public static void Initialize(KinetixCustomTheme _kinetixCustomTheme )
        {
            OnThemeUpdate(_kinetixCustomTheme);
        }

        /// <summary>
        /// change the new theme  at runtime
        /// </summary>
        /// <param name="_kinetixCustomTheme">the new theme to update</param>
        private static void OnThemeUpdate(KinetixCustomTheme _kinetixCustomTheme)
        {
            dCustomSprites = new Dictionary<ECustomTagSprite, CustomSprite>();
            _kinetixCustomTheme.customSprites.ForEach(delegate(CustomSprite cSprite)
            {
                dCustomSprites.Add(cSprite.Tag, cSprite);
            });

            dCustomColors = new Dictionary<ECustomTagColor, CustomColor>();
            _kinetixCustomTheme.customColors.ForEach(delegate(CustomColor cColor)
            {
                dCustomColors.Add(cColor.Tag, cColor);
            });

            dCustomFonts = new Dictionary<ECustomTagFontWeight, CustomFont>();
            _kinetixCustomTheme.customFonts.ForEach(delegate(CustomFont cFont)
            {
                dCustomFonts.Add(cFont.Tag, cFont);
            });

            if( refCustomSprite != null)
            {
                foreach (ECustomTagSprite eTag in refCustomSprite.Keys)
                {     
                    foreach (ISpriteCustomizable iCustomizable in refCustomSprite[eTag])
                    {
                        iCustomizable.SetSprite();
                        iCustomizable.SetColor();
                    }
                }
            }

            if( refCustomFont != null)
            {
                foreach (ECustomTagFontWeight eTag in refCustomFont.Keys)
                {                
                    foreach (IFontCustomizable iCustomizable in refCustomFont[eTag])
                    {
                        iCustomizable.SetFont();
                    }
                }
            }

            if( refCustomColor != null)
            {
                foreach (ECustomTagColor eTag in refCustomColor.Keys)
                {                
                    foreach (IFontCustomizable iCustomizable in refCustomColor[eTag])
                    {
                        iCustomizable.SetColor();
                    }
                }
            }            
        }

        //*********************************************************************************************************
        public static void RegisterSprite(ECustomTagSprite eTag, ISpriteCustomizable iCustomizable)
        {
            if (refCustomSprite == null)
                refCustomSprite = new Dictionary<ECustomTagSprite, List<ISpriteCustomizable>>();
            
            if (!refCustomSprite.ContainsKey(eTag))
                refCustomSprite.Add(eTag, new List<ISpriteCustomizable>());
            
            refCustomSprite[eTag].Add(iCustomizable);
            iCustomizable.SetSprite();
            iCustomizable.SetColor();
        }

        public static void UnregisterSprite(ECustomTagSprite eTag, ISpriteCustomizable iCustomizable)
        {
            if (refCustomSprite != null && refCustomSprite.ContainsKey(eTag) && refCustomSprite[eTag] != null)
            {
                if (refCustomSprite != null && refCustomSprite[eTag].Contains(iCustomizable) )
                    refCustomSprite[eTag].Remove(iCustomizable);
            }
        }

        //*********************************************************************************************************
        public static void RegisterFont(ECustomTagFontWeight eTag, IFontCustomizable iCustomizable)
        {
            if( refCustomFont == null)
                refCustomFont = new Dictionary<ECustomTagFontWeight, List<IFontCustomizable>>();

            if( !refCustomFont.ContainsKey(eTag))
                refCustomFont.Add(eTag, new List<IFontCustomizable>());
            
            refCustomFont[eTag].Add(iCustomizable);
            iCustomizable.SetFont();
        }

        public static void UnregisterFont(ECustomTagFontWeight eTag, IFontCustomizable iCustomizable)
        {
            
            if (refCustomFont!= null && refCustomFont[eTag] != null)
                if( refCustomFont[eTag].Contains(iCustomizable) )
                    refCustomFont[eTag].Remove(iCustomizable);
        }

        //*********************************************************************************************************
        public static void RegisterColor(ECustomTagColor eTag, IFontCustomizable iCustomizable)
        {
            if( refCustomColor == null)
                refCustomColor = new Dictionary<ECustomTagColor, List<IFontCustomizable>>();

            if( !refCustomColor.ContainsKey(eTag))
                refCustomColor.Add(eTag, new List<IFontCustomizable>());
            
            refCustomColor[eTag].Add(iCustomizable);
            iCustomizable.SetColor();
        }

        public static void UnregisterColor(ECustomTagColor eTag, IFontCustomizable iCustomizable)
        {
            if (refCustomColor != null && refCustomColor.ContainsKey(eTag) && refCustomColor[eTag] != null)
                if( refCustomColor[eTag].Contains(iCustomizable) )
                    refCustomColor[eTag].Remove(iCustomizable);
        }

        //*******************************************************************************
        public static bool HasEnableCustomSprite(ECustomTagSprite eTag)
        {
            if (dCustomSprites != null && dCustomSprites.ContainsKey(eTag) && dCustomSprites[eTag] != null)
                return dCustomSprites[eTag].EnableCustomSprite;

            return false;
        }

        public static Sprite GetCustomSprite(ECustomTagSprite eTag)
        {
            return dCustomSprites[eTag].sprite;
        }

        //*******************************************************************************
        public static bool HasEnableCustomColorSprite(ECustomTagSprite eTag)
        {
            if (dCustomSprites != null && dCustomSprites.ContainsKey(eTag) && dCustomSprites[eTag] != null)
                return dCustomSprites[eTag].EnableCustomColor;

            return false;
        }

        public static Color GetCustomColorSprite(ECustomTagSprite eTag)
        {
            return dCustomSprites[eTag].color;
        }

        //*******************************************************************************
        public static bool HasEnableCustomFont(ECustomTagFontWeight eTag)
        {
            if (dCustomFonts != null && dCustomFonts.ContainsKey(eTag) && dCustomFonts[eTag] != null)
                return dCustomFonts[eTag].EnableCustomFont;
                
            return false;            
        }

        public static TMP_FontAsset GetCustomFont(ECustomTagFontWeight eTag)
        {
            return dCustomFonts[eTag].font;
        }

        //*******************************************************************************
        public static bool HasEnableCustomFontColor(ECustomTagColor eTag)
        {
            if (dCustomColors != null && dCustomColors.ContainsKey(eTag) && dCustomColors[eTag] != null)
                return dCustomColors[eTag].EnableCustomColor;
                
            return false;            
        }

        public static Color GetCustomFontColor(ECustomTagColor eTag)
        {
            return dCustomColors[eTag].color;
        }



    }
}
