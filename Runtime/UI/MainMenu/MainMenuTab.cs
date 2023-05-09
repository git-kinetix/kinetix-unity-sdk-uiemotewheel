using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Kinetix.UI.EmoteWheel
{
    public class MainMenuTab : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [SerializeField] private GameObject         selectedElement;
        [SerializeField] private Image              imgNotification;
        [SerializeField] public EKinetixUICategory  kinetixCategory;
        [SerializeField] private ScaleEffect        scaleEffect;

        [SerializeField] private Vector3 baseScale;
        [SerializeField] private Vector3 selectedScale;

        private Image imgCadreUnselected;        
        private bool isSelected;
        
        private void Awake()
        {
            imgCadreUnselected = GetComponent<Image>();
            baseScale = transform.localScale;
            
            KinetixUI.OnShowView += OnShowView;
            KinetixUI.OnHideView += OnHideView;

            KinetixUI.OnUpdateNotificationNewEmote += UpdateNotificationNewEmote;
        }

        private void OnDestroy()
        {
            KinetixUI.OnShowView -= OnShowView;
            KinetixUI.OnHideView -= OnHideView;

            KinetixUI.OnUpdateNotificationNewEmote -= UpdateNotificationNewEmote;
        }

        private void OnShowView(EKinetixUICategory _Category)
        {
            if (kinetixCategory == _Category)
            {
                isSelected           = true;
                transform.localScale = selectedScale;
                selectedElement.gameObject.SetActive(true);
                SwitchAlphaImg( imgCadreUnselected, 0f );
            }
        }

        private void OnHideView(EKinetixUICategory _Category)
        {
            if (kinetixCategory == _Category)
            {
                isSelected = false;
                selectedElement.gameObject.SetActive(false);
                transform.localScale = baseScale;
                SwitchAlphaImg( imgCadreUnselected, 1f );
            }
        }

        private void UpdateNotificationNewEmote(bool bShow)
        {
            if(kinetixCategory == EKinetixUICategory.INVENTORY)
            {
                imgNotification.gameObject.SetActive(bShow);
            }
        }

        private void SwitchAlphaImg(Image img, float alpha)
        {
            Color tempColor = img.color;
            tempColor.a = alpha;
            img.color = tempColor;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (isSelected)
                return;
            
            scaleEffect.ScaleUp();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (isSelected)
                return;
            
            scaleEffect.ScaleDown();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (isSelected)
                return;

            KinetixUI.HideAll();
            KinetixUI.Show(kinetixCategory);

            if(kinetixCategory == EKinetixUICategory.EMOTE_SELECTOR)
                KinetixAnalytics.SendEvent("Click_Wheel_Button", "", KinetixAnalytics.Page.EmoteWheel, KinetixAnalytics.Event_type.Click);
            else if(kinetixCategory == EKinetixUICategory.INVENTORY)
                KinetixAnalytics.SendEvent("Click_Bag_Button", "", KinetixAnalytics.Page.Inventory, KinetixAnalytics.Event_type.Click);
            else if(kinetixCategory == EKinetixUICategory.CREATE)
                KinetixAnalytics.SendEvent("Click_Create_Button", "", KinetixAnalytics.Page.Create, KinetixAnalytics.Event_type.Click);
        }
    }
}

