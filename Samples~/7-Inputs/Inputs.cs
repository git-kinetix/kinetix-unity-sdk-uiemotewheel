// // ----------------------------------------------------------------------------
// // <copyright file="CustomMain.cs" company="Kinetix SAS">
// // Kinetix Unity SDK - Copyright (C) 2022 Kinetix SAS
// // </copyright>
// // ----------------------------------------------------------------------------

using UnityEngine;
using Kinetix;
using Kinetix.UI;
using Kinetix.UI.EmoteWheel;

namespace Kinetix.Sample
{
    public class Inputs : MonoBehaviour
    {
        [SerializeField] private string            gameAPIKey;
        [SerializeField] private Animator          localPlayerAnimator;
        [SerializeField] private KinetixInputMapSO kinetixCustomInputActionMap;

        private void Awake()
        {
            KinetixCore.OnInitialized += OnKinetixInitialized;
            KinetixCore.Initialize(new KinetixCoreConfiguration()
            {
                GameAPIKey                            = gameAPIKey,
                PlayAutomaticallyAnimationOnAnimators = true,
                ShowLogs                              = false,
                EnableAnalytics                       = false
            });
        }


        private void OnDestroy()
        {
            KinetixCore.OnInitialized -= OnKinetixInitialized;
        }

        private void OnKinetixInitialized()
        {
            KinetixUIEmoteWheel.Initialize(new KinetixUIEmoteWheelConfiguration()
            {
                kinetixInputActionMap = kinetixCustomInputActionMap,
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
            KinetixCore.Account.AssociateEmotesToUser("d228a057-6409-4560-afd0-19c804b30b84");
            KinetixCore.Account.AssociateEmotesToUser("bd6749e5-ac29-46e4-aae2-bb1496d04cbb");
            KinetixCore.Account.AssociateEmotesToUser("7a6d 483e-ebdc-4efd-badb-12a2e210e618");
        }
    }
}
