// // ----------------------------------------------------------------------------
// // <copyright file="KinetixUITranslator.cs" company="Kinetix SAS">
// // Kinetix Unity SDK - Copyright (C) 2022 Kinetix SAS
// // </copyright>
// // ----------------------------------------------------------------------------

using UnityEngine;
using TMPro;

namespace Kinetix.UI.EmoteWheel
{
    public class KinetixUIFontCustomizer : MonoBehaviour, IFontCustomizable
    {
        public ECustomTagFontWeight eCustomTagFontWeight;
        public ECustomTagColor eCustomTagFontColor;
        
        private TextMeshProUGUI textMesh;

        private void Awake()
        {
            textMesh = GetComponent<TextMeshProUGUI>();
        }

        private void OnDestroy()
        {
            KinetixCustomizeManager.UnregisterFont( eCustomTagFontWeight, this as IFontCustomizable);
            KinetixCustomizeManager.UnregisterColor( eCustomTagFontColor, this as IFontCustomizable);
        }

        private void Start()
        {
            KinetixCustomizeManager.RegisterFont( eCustomTagFontWeight, this as IFontCustomizable);
            KinetixCustomizeManager.RegisterColor( eCustomTagFontColor, this as IFontCustomizable);
        }

        public void SetColor()
        {
            if (textMesh == null)
                return;

            if(KinetixCustomizeManager.HasEnableCustomFontColor(eCustomTagFontColor))
                textMesh.color = KinetixCustomizeManager.GetCustomFontColor(eCustomTagFontColor);
        }

        public void SetFont()
        {
            if (textMesh == null)
                return;

            if(KinetixCustomizeManager.HasEnableCustomFont(eCustomTagFontWeight))
                textMesh.font = KinetixCustomizeManager.GetCustomFont(eCustomTagFontWeight);
        }
    }
}
