// // ----------------------------------------------------------------------------
// // <copyright file="InventoryView.cs" company="Kinetix SAS">
// // Kinetix Unity SDK - Copyright (C) 2022 Kinetix SAS
// // </copyright>
// // ----------------------------------------------------------------------------

using Kinetix.UI.Common;
using UnityEngine;
using UnityEngine.UI;
using Kinetix.Internal;

namespace Kinetix.UI.EmoteWheel
{
    public class CreateView : CategoryView
    {
        [Header("QR CODE SETTINGS")] [SerializeField]
        private Image qrCodeImage;

        [SerializeField] private Button btnCopy;
        [SerializeField] private Button btnURL;

        [SerializeField] private Color highlightColor = Color.black;
        [SerializeField] private Color dominantColor  = Color.white;

        private string    ugcURL = string.Empty;
        private Texture2D texture;

        private bool isFetching;

        public void Init()
        {
#if UNITY_WEBGL
            btnCopy.gameObject.SetActive(false);
            btnURL.gameObject.GetComponent<RectTransform>().offsetMax =
 new Vector2(-20, btnURL.gameObject.GetComponent<RectTransform>().offsetMax.y);
#else
            btnCopy.onClick.AddListener(OnCopyLinkToClipboard);
#endif
            btnURL.onClick.AddListener(OnClickURL);

            InitCreateSystem();
        }

        private void InitCreateSystem()
        {
            KinetixCore.UGC.OnUGCTokenExpired         += OnTokenExpired;
            KinetixCore.Account.OnDisconnectedAccount += DisposeFetchUgcUrl;
        }

        protected override void OnDestroy()
        {
            if (btnCopy != null)
                btnCopy.onClick.AddListener(OnCopyLinkToClipboard);
            if (btnURL != null)
                btnURL.onClick.AddListener(OnClickURL);

            KinetixCore.UGC.OnUGCTokenExpired         -= OnTokenExpired;
            KinetixCore.Account.OnDisconnectedAccount -= DisposeFetchUgcUrl;
            base.OnDestroy();
        }

        private void FetchUgcUrl()
        {
            if (isFetching)
                return;

            if (!Visible)
                return;

            KinetixCore.UGC.GetUgcUrl(OnUgcUrlFetched);
            isFetching = true;
        }

        private void OnTokenExpired()
        {
            isFetching = false;
            FetchUgcUrl();
        }

        private void DisposeFetchUgcUrl()
        {
            isFetching = false;

            if (texture != null)
                Destroy(texture);

            if (qrCodeImage.sprite != null)
                Destroy(qrCodeImage.sprite);

            qrCodeImage.gameObject.SetActive(false);
        }

        public void CreateQRCode()
        {
            // Check if UGC are available
            if (!KinetixCore.UGC.IsUGCAvailable())
                return;

            FetchUgcUrl();
        }

        private void OnUgcUrlFetched(string _UgcUrl)
        {
            ugcURL = _UgcUrl;

            // Destroy texture and sprite if they exists
            if (texture != null)
                Destroy(texture);

            if (qrCodeImage.sprite != null)
                Destroy(qrCodeImage.sprite);

            if (_UgcUrl == null)
                return;

            texture = KinetixQRCodeHelper.Instance.GetQRCodeForUrl(ugcURL, dominantColor, highlightColor);

            // Display a QRCOde
            qrCodeImage.gameObject.SetActive(true);
            qrCodeImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);

            // And start polling to know if the token is still available
            KinetixCore.UGC.StartPollingForNewUGCToken();
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
