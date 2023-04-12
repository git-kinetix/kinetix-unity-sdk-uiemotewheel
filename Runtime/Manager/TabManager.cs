// // ----------------------------------------------------------------------------
// // <copyright file="TabManager.cs" company="Kinetix SAS">
// // Kinetix Unity SDK - Copyright (C) 2022 Kinetix SAS
// // </copyright>
// // ----------------------------------------------------------------------------

using UnityEngine;
using System.Collections.Generic;

namespace Kinetix.UI.EmoteWheel
{
    public class TabManager: MonoBehaviour
    {
        private int currentTab;

        private List<EKinetixUICategory> listTab = new List<EKinetixUICategory>();


        public void AddTab(EKinetixUICategory tab)
        {
            if(!listTab.Contains(tab))
                listTab.Add(tab);
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
