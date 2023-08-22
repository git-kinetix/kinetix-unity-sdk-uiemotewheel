// // ----------------------------------------------------------------------------
// // <copyright file="CustomMain.cs" company="Kinetix SAS">
// // Kinetix Unity SDK - Copyright (C) 2022 Kinetix SAS
// // </copyright>
// // ----------------------------------------------------------------------------

using UnityEngine;
using TMPro;
using System.Collections.Generic;
using Kinetix;
using Kinetix.UI;
using Kinetix.UI.EmoteWheel;

namespace Kinetix.Sample
{
    public class Theme : MonoBehaviour
    {
        [SerializeField] private string gameAPIKey;
        [SerializeField] private Animator           localPlayerAnimator;
        [SerializeField] private KinetixCustomTheme kinetixCustomTheme;
        [SerializeField] private ECustomTheme       defaultConfig = ECustomTheme.DARK_MODE;

        [SerializeField] private TMP_Dropdown dropdown;
        private                  List<string> dropdownTheme;

        private void Awake()
        {
            KinetixCore.OnInitialized += OnKinetixInitialized;
            KinetixCore.Initialize(new KinetixCoreConfiguration()
            {
                GameAPIKey = gameAPIKey,
                PlayAutomaticallyAnimationOnAnimators = true,
                ShowLogs                              = true,
                EnableAnalytics                       = true
            });
        }

        private void Start()
        {
            dropdownTheme = new List<string>();

            dropdownTheme.Add(ECustomTheme.LIGHT_MODE.ToString());
            dropdownTheme.Add(ECustomTheme.DARK_MODE.ToString());
            dropdownTheme.Add("CUSTOM THEME");

            dropdown.AddOptions(dropdownTheme);

            dropdown.value = dropdownTheme.IndexOf(defaultConfig.ToString());
        }

        public void OnThemeDropdownChanged()
        {
            if (dropdown.value < System.Enum.GetValues(typeof(ECustomTheme)).Length)
            {
                KinetixUIEmoteWheel.UpdateTheme((ECustomTheme)dropdown.value);
            }
            else if (dropdown.options[dropdown.value].text == "CUSTOM THEME")
            {
                KinetixUIEmoteWheel.UpdateThemeOverride(kinetixCustomTheme);
            }
        }

        private void OnDestroy()
        {
            KinetixCore.OnInitialized -= OnKinetixInitialized;
        }

        private void OnKinetixInitialized()
        {
            KinetixUIEmoteWheel.Initialize(new KinetixUIEmoteWheelConfiguration()
            {
                customThemeOverride = null,
                customTheme         = defaultConfig,
                baseLanguage        = SystemLanguage.English,
                enabledCategories = new []
                {
                    EKinetixUICategory.INVENTORY,
                    EKinetixUICategory.EMOTE_SELECTOR
                }
            });

            KinetixCore.Animation.RegisterLocalPlayerAnimator(localPlayerAnimator);

            KinetixCore.Account.ConnectAccount("sdk-sample-user-id", OnAccountConnected);
        }

        private void OnAccountConnected()
        {
            KinetixCore.Account.AssociateEmotesToUser("f4daf21a-38b8-4f84-a27c-5eb37cd5726e");
            KinetixCore.Account.AssociateEmotesToUser("62cb8c38-ff49-448d-94c1-d820df90ccdb");
            KinetixCore.Account.AssociateEmotesToUser("a5d188f1-4d0f-4dfc-bb44-eaea54d0780f");
        }
    }
}
