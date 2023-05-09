// // ----------------------------------------------------------------------------
// // <copyright file="InventoryView.cs" company="Kinetix SAS">
// // Kinetix Unity SDK - Copyright (C) 2022 Kinetix SAS
// // </copyright>
// // ----------------------------------------------------------------------------

using System;
using Kinetix.UI.Common;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Kinetix.Internal;
using TMPro;


namespace Kinetix.UI.EmoteWheel
{
    public class CreateView : CategoryView
    {
        [Header("QR CODE SETTINGS")]

        [SerializeField] private Image qrCodeImage;
        [SerializeField] private Button btnCopy;
        [SerializeField] private Button btnURL;

        [SerializeField] private Color highlightColor = Color.black;
        [SerializeField] private Color dominantColor = Color.white;

        private string ugcURL = string.Empty;
        private Texture2D texture;

        private Coroutine timeOutCoroutine;


        public void Init()
        {
#if UNITY_WEBGL
            btnCopy.gameObject.SetActive(false);
            btnURL.gameObject.GetComponent<RectTransform>().offsetMax = new Vector2(-20, btnURL.gameObject.GetComponent<RectTransform>().offsetMax.y);
#else
            btnCopy.onClick.AddListener(OnCopyLinkToClipboard);
#endif            
            btnURL.onClick.AddListener(OnClickURL);

            KinetixCore.UGC.OnUGCTokenExpired += () => {
                KinetixCore.UGC.GetUgcUrl(OnUgcUrlFetched);
            };            
        }

        protected override void OnDestroy()
        {
            btnCopy.onClick.AddListener(OnCopyLinkToClipboard);
            btnURL.onClick.AddListener(OnClickURL);

            KinetixCore.UGC.OnUGCTokenExpired -= () => {
                KinetixCore.UGC.GetUgcUrl(OnUgcUrlFetched);
            };
            base.OnDestroy();
        }

        public void CreateQRCode()
        {
            // Check if UGC are available
            if (!KinetixCore.UGC.IsUGCAvailable())
                return;

            KinetixCore.UGC.GetUgcUrl(OnUgcUrlFetched);
        }

        private void OnUgcUrlFetched(string _UgcUrl)
        {
            ugcURL = _UgcUrl;

            // Destroy texture and sprite if they exists
            if(texture != null)
                Texture2D.Destroy(texture);
                
            if(qrCodeImage.sprite != null)
                Sprite.Destroy(qrCodeImage.sprite);

            texture = KinetixQRCodeHelper.Instance.GetQRCodeForUrl(ugcURL, dominantColor, highlightColor);

            // Display a QRCOde
            qrCodeImage.gameObject.SetActive(true);
            qrCodeImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);

            // Then start polling to know if new emote is create 
            KinetixCore.UGC.StartPollingForUGC();

            // And start polling to know if the token is still available
            KinetixCore.UGC.StartPollingForNewUGCToken();

            //launch TimeOut, that refresh the QRCode if it has been updated
            if(timeOutCoroutine != null)                
                CoroutineUtils.Instance.StopCoroutine(timeOutCoroutine);

            timeOutCoroutine = CoroutineUtils.Instance.StartCoroutine( TimeOutUgcUrl( KinetixConstants.c_TimeOutCreateQRCode) );
        }

        private IEnumerator TimeOutUgcUrl(float seconds)
        {
            yield return new WaitForSecondsRealtime(seconds);
            
            timeOutCoroutine = null;
            if (Visible)
            {
                CreateQRCode();
            }
        }

        private void OnCopyLinkToClipboard()
        {
            if (ugcURL != "")
            {
#if !UNITY_WEBGL
                GUIUtility.systemCopyBuffer = ugcURL;
#endif
            }
        }

        private void OnClickURL()
        {
            if (ugcURL != "")
            {
                Application.OpenURL(ugcURL);
            }
        }
    }
}
