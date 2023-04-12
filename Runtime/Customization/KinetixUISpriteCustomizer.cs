// // ----------------------------------------------------------------------------
// // <copyright file="KinetixUITranslator.cs" company="Kinetix SAS">
// // Kinetix Unity SDK - Copyright (C) 2022 Kinetix SAS
// // </copyright>
// // ----------------------------------------------------------------------------

using UnityEngine;
using UnityEngine.UI;

namespace Kinetix.UI.EmoteWheel
{
    public class KinetixUISpriteCustomizer : MonoBehaviour, ISpriteCustomizable
    {
        public ECustomTagSprite eCustomTagSprite;
        
        private Image img;

        private void Awake()
        {
            img = GetComponent<Image>();
        }

        private void Start()
        {
            KinetixCustomizeManager.RegisterSprite( eCustomTagSprite, this as ISpriteCustomizable);
        }

        private void OnDestroy()
        {
            KinetixCustomizeManager.UnregisterSprite( eCustomTagSprite, this as ISpriteCustomizable);
        }

        public void SetSprite()
        {
            if (img == null)
                return;

            if(KinetixCustomizeManager.HasEnableCustomSprite(eCustomTagSprite))
                img.sprite = KinetixCustomizeManager.GetCustomSprite(eCustomTagSprite);
        }

        public void SetColor()
        {
            if (img == null)
                return;
            
            if(KinetixCustomizeManager.HasEnableCustomColorSprite(eCustomTagSprite))
                img.color = KinetixCustomizeManager.GetCustomColorSprite(eCustomTagSprite);
        }
    }
}
