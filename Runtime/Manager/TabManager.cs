// // ----------------------------------------------------------------------------
// // <copyright file="TabManager.cs" company="Kinetix SAS">
// // Kinetix Unity SDK - Copyright (C) 2022 Kinetix SAS
// // </copyright>
// // ----------------------------------------------------------------------------

using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Kinetix.UI.Common;

namespace Kinetix.UI.EmoteWheel
{
	public class TabManager : MonoBehaviour
    {
        [SerializeField] private List<MainMenuTab> listMenuTab;

		private int currentTab;
        private List<EKinetixUICategory> listTab;
        private KinetixUIEmoteWheelConfiguration configuration; 

		public void Init(KinetixUIEmoteWheelConfiguration _Configuration)
        {
            configuration = _Configuration;
            listTab = new List<EKinetixUICategory>();
            
            HideAllTab();
            
            KinetixUIBehaviour.OnDisplayTabs += DisplayTabs;
            KinetixUIBehaviour.OnDisplayEnabledTabs += DisplayEnableTabs;
        }

		private void HideAllTab()
		{
			foreach(MainMenuTab mainMenuTab in listMenuTab)
			{
				mainMenuTab.gameObject.SetActive(false);
			}
		}

        public void DisplayTabs(EKinetixUICategory[] _DisplayCategories)
        {
            HideAllTab();

            if (_DisplayCategories == null)
                return;
            
            List<EKinetixUICategory> categories = _DisplayCategories.ToList();
            categories.RemoveAll(category => !configuration.enabledCategories.Contains(category));
            
            if (categories.Contains(EKinetixUICategory.CREATE) && !KinetixCore.UGC.IsUGCAvailable())
                categories.Remove(EKinetixUICategory.CREATE);
            
            categories.ForEach(ShowMenuTab);
        }

        public void DisplayEnableTabs()
        {
            listTab.ForEach(tab =>
            {
                if (tab == EKinetixUICategory.CREATE)
                {
                    if (KinetixCore.UGC.IsUGCAvailable())
                        ShowMenuTab(tab);
                    else
                        HideMenuTab(tab);
                }
                else
                {
                    ShowMenuTab(tab);
                }
            });
        }

		public void AddTab(EKinetixUICategory ECatTab)
		{
            if (!listTab.Contains(ECatTab))
            {
                bool addCategory = false;
                
                if (configuration == null)
                    addCategory = true;
                if (configuration != null && configuration.enabledCategories == null)
                    addCategory = true;
                if (configuration != null && configuration.enabledCategories != null && configuration.enabledCategories.Contains(ECatTab))
                    addCategory = true;

                if (addCategory)
                {
                    listTab.Add(ECatTab);
                    ShowMenuTab(ECatTab);
                }
            }
        }

		private void ShowMenuTab(EKinetixUICategory enumCatTab)
		{
			foreach (MainMenuTab mainMenuTab in listMenuTab)
			{
                if (mainMenuTab.kinetixCategory == enumCatTab)
                {
                    mainMenuTab.gameObject.SetActive(true);
                }
			}
		} 
        
        private void HideMenuTab(EKinetixUICategory enumCatTab)
        {
            foreach (MainMenuTab mainMenuTab in listMenuTab)
            {
                if (mainMenuTab.kinetixCategory == enumCatTab)
                {
                    mainMenuTab.gameObject.SetActive(false);
                }
            }
        } 

        public EKinetixUICategory GetNextTab()
		{
			currentTab++;
			if (currentTab >= listTab.Count)
				currentTab = 0;

			return listTab[currentTab];
		}

		public EKinetixUICategory GetPreviousTab()
		{
			currentTab--;
			if (currentTab < 0)
				currentTab = listTab.Count-1;

			return listTab[currentTab];
		}

		public void SetCurrentTab(EKinetixUICategory tab)
		{
			currentTab = listTab.IndexOf(tab);
		}
	}
}
