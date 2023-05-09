// // ----------------------------------------------------------------------------
// // <copyright file="EKinetixUICategory.cs" company="Kinetix SAS">
// // Kinetix Unity SDK - Copyright (C) 2022 Kinetix SAS
// // </copyright>
// // ----------------------------------------------------------------------------

using UnityEngine;

namespace Kinetix.UI.EmoteWheel
{
    [System.Serializable]
    public class CustomSprite
    {
        [SerializeField]
        public ECustomTagSprite Tag;
        [SerializeField]
        public bool EnableCustomSprite;
        [SerializeField]
        public Sprite sprite;
        [SerializeField]
        public bool EnableCustomColor;
        [SerializeField]
        public Color color;
    }
}
