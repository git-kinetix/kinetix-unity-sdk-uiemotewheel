// // ----------------------------------------------------------------------------
// // <copyright file="KinetixSquareMenuManager.cs" company="Kinetix SAS">
// // Kinetix Unity SDK - Copyright (C) 2022 Kinetix SAS
// // </copyright>
// // ----------------------------------------------------------------------------

using System.Collections.Generic;
using UnityEngine;
using Kinetix.UI.Common;
using System.Linq;

namespace Kinetix.UI.EmoteWheel
{
    public class KinetixEmoteWheelManager : AKinetixCommonUIManager
    {
        // REFERENCES
        [SerializeField] private EmoteSelector  emoteSelector;
        [SerializeField] private Inventory      inventory;
        [SerializeField] private Create         create;
        [SerializeField] private CanvasGroup    headerMenuCanvasGroup;
        [SerializeField] private TabManager     tabManager;
        
        // CACHE
        private         List<KinetixView>       kinetixViews;
        
        protected override void Setup()
        {
            inventory.OnAddFavorite         += OnAddFavoriteAnimation;
            inventory.OnRemoveFavorite      += OnRemoveFavoriteAnimation;
            inventory.OnCheckEmote          += OnCheckEmote;

            emoteSelector.OnRefillWheel     += OnRefillWheel;
            emoteSelector.OnSelectAnimation += OnSelectAnimation;
            
            //KinetixUIBehaviour.OnShow    += OnShowView;
            KinetixUIBehaviour.OnHideAll += OnHideAll;
            KinetixUI.OnShowView         += OnShowView;
            KinetixUI.OnHideView         += OnHideView;

            KinetixUIEmoteWheelBehavior.OnUpdateTheme += UpdateTheme;
            KinetixUIEmoteWheelBehavior.OnUpdateThemeOverride += UpdateThemeOverride;

            KinetixInputManager.OnHitNextTab += OnNextTab;
            KinetixInputManager.OnHitPrevTab += OnPrevTab;

            kinetixViews = new List<KinetixView>();

            HideHeaderMenu();

            tabManager.Init(kinetixCommonUIConfiguration as KinetixUIEmoteWheelConfiguration);
            InitCustomConfig(kinetixCommonUIConfiguration as KinetixUIEmoteWheelConfiguration);
            InitInventory();
            InitEmoteSelector();
            InitCreate();
            tabManager.SetCurrentTab(EKinetixUICategory.EMOTE_SELECTOR);
            
            base.Setup();
        }

        protected override void OnConnectedAccount()
        {
        }

        private void OnShowView(EKinetixUICategory _KinetixCategory)
        {
            ShowHeaderMenu();

            if(_KinetixCategory == EKinetixUICategory.CREATE)
            {
                create.CreateQRCode();
            }
        }
        
        private void OnHideView(EKinetixUICategory _KinetixCategory)
        {
            HideHeaderMenu();
        }
        
        private void OnHideAll()
        {
            HideHeaderMenu();
        }

        private void ShowHeaderMenu()
        {
            headerMenuCanvasGroup.alpha          = 1.0f;
            headerMenuCanvasGroup.interactable   = true;
            headerMenuCanvasGroup.blocksRaycasts = true;
        }

        private void HideHeaderMenu()
        {
            headerMenuCanvasGroup.alpha          = 0.0f;
            headerMenuCanvasGroup.interactable   = false;
            headerMenuCanvasGroup.blocksRaycasts = false;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (inventory != null)
            {
                inventory.OnAddFavorite     -= OnAddFavoriteAnimation;
                inventory.OnRemoveFavorite  -= OnRemoveFavoriteAnimation;
                inventory.OnRefillFavorites -= OnRefillFavorites;
                inventory.OnRefillBank      -= OnRefillBank;
                inventory.OnCheckEmote      -= OnCheckEmote;
            }

            if (emoteSelector != null)
            {
                emoteSelector.OnSelectAnimation -= OnSelectAnimation;
            }

            KinetixInputManager.OnHitNextTab -= OnNextTab;
            KinetixInputManager.OnHitPrevTab -= OnPrevTab;
        }

        public void UpdateTheme (ECustomTheme customTheme)
        {
            (kinetixCommonUIConfiguration as KinetixUIEmoteWheelConfiguration).customTheme = customTheme;
            (kinetixCommonUIConfiguration as KinetixUIEmoteWheelConfiguration).customThemeOverride = null;
            InitCustomConfig(kinetixCommonUIConfiguration as KinetixUIEmoteWheelConfiguration);
        }

        public void UpdateThemeOverride(KinetixCustomTheme kinetixCustomTheme)
        {
            (kinetixCommonUIConfiguration as KinetixUIEmoteWheelConfiguration).customThemeOverride = kinetixCustomTheme;
            InitCustomConfig(kinetixCommonUIConfiguration as KinetixUIEmoteWheelConfiguration);
        }

        private void InitCustomConfig(KinetixUIEmoteWheelConfiguration kinetixUIEmoteWheelConfiguration)
        {
            if( kinetixUIEmoteWheelConfiguration == null)
            {
                kinetixUIEmoteWheelConfiguration = new KinetixUIEmoteWheelConfiguration();
                kinetixUIEmoteWheelConfiguration.customThemeOverride = Resources.Load<KinetixCustomTheme>("ScriptableObject/CustomizeUILightMode");
            } 
            else if( kinetixUIEmoteWheelConfiguration.customThemeOverride == null )
            {
                switch(kinetixUIEmoteWheelConfiguration.customTheme)
                {
                    case ECustomTheme.DARK_MODE:
                        kinetixUIEmoteWheelConfiguration.customThemeOverride = Resources.Load<KinetixCustomTheme>("ScriptableObject/CustomizeUIDarkMode");
                        break;
                    
                    case ECustomTheme.LIGHT_MODE:
                        kinetixUIEmoteWheelConfiguration.customThemeOverride = Resources.Load<KinetixCustomTheme>("ScriptableObject/CustomizeUILightMode");
                        break;

                    default : 
                        kinetixUIEmoteWheelConfiguration.customThemeOverride = Resources.Load<KinetixCustomTheme>("ScriptableObject/CustomizeUILightMode");
                        break;
                }
            }

            KinetixCustomizeManager.Initialize(kinetixUIEmoteWheelConfiguration.customThemeOverride);
        }

        public static void Instantiate(KinetixUIEmoteWheelConfiguration kinetixUIEmoteWheelConfiguration)
        {
            KinetixEmoteWheelManager kinetixEmoteWheelManager = Instantiate(Resources.Load<GameObject>("Prefabs/KinetixUI_EmoteWheel")).GetComponent<KinetixEmoteWheelManager>();
            kinetixEmoteWheelManager.Initialize(kinetixUIEmoteWheelConfiguration);           
        }

        #region INVENTORY

        private void InitInventory()
        {
            inventory.Init();
            inventory.OnRefillFavorites += OnRefillFavorites;
            inventory.OnRefillBank      += OnRefillBank;
            kinetixViews.Add(inventory.View);
            tabManager.AddTab(EKinetixUICategory.INVENTORY);
        }

        private void LoadInventory()
        {
            inventory.RefreshInventoryBankAnimations();
            inventory.FillFavorites(FavoritesAnimationIdByIndex);
        }

        #endregion

        #region EMOTE WHEEL

        private void InitEmoteSelector()
        {
            emoteSelector.Init();
            kinetixViews.Add(emoteSelector.View);
            tabManager.AddTab(EKinetixUICategory.EMOTE_SELECTOR);
        }

        private void LoadEmoteSelector()
        {
            emoteSelector.Load(FavoritesAnimationIdByIndex);

        }

        #endregion

        #region CREATE
        
        private void InitCreate()
        {
            create.Init();
            kinetixViews.Add(create.View);
            tabManager.AddTab(EKinetixUICategory.CREATE);
        }

        private void OnCheckEmote(string UUID)
        {
            SaveSystem.SaveEmoteChecked(UUID);
            
            KinetixCore.Metadata.GetUserAnimationMetadatas((userMetadatas) =>
            {
                List<AnimationMetadata> metadatas = userMetadatas.ToList();

                //if has new emotes, show notification indication visual on menu tab Bag
                KinetixUI.OnUpdateNotificationNewEmote?.Invoke( HasNewEmotes(metadatas) );
            });
        }

        #endregion

        private bool HasAtLeastOnViewShow()
        {
            return kinetixViews.Exists(view => view.Visible);
        }
        
        private void OnRefillFavorites()
        {
            LoadInventory();
            LoadEmoteSelector();
        }

        private void OnRefillBank()
        {
            inventory.RefreshInventoryBankAnimations();
            inventory.FillFavorites(FavoritesAnimationIdByIndex);
        }

        private void OnRefillWheel()
        {
            LoadEmoteSelector();
        }

        protected override void OnLoadData()
        {
            LoadEmoteSelector();
            LoadInventory();
        }

        private void OnNextTab()
        {
            if (HasAtLeastOnViewShow())
            {
                KinetixUIBehaviour.HideAll();
                KinetixUIBehaviour.Show(tabManager.GetNextTab());
            }
        }

        private void OnPrevTab()
        {
            if (HasAtLeastOnViewShow())
            {
                KinetixUIBehaviour.HideAll();
                KinetixUIBehaviour.Show(tabManager.GetPreviousTab());
            }
        }
    }
}
