using UnityEngine;
using Kinetix;
using Kinetix.UI;
using Kinetix.UI.EmoteWheel;

namespace Kinetix.Sample
{
    public class Events : MonoBehaviour
    {
        [SerializeField] private string virtualWorldKey;
        [SerializeField] private Animator localPlayerAnimator;

        private void Awake()
        {
            KinetixCore.OnInitialized += OnKinetixInitialized;
            KinetixCore.Initialize(new KinetixCoreConfiguration()
            {
                VirtualWorldKey = virtualWorldKey,
                PlayAutomaticallyAnimationOnAnimators = true,
                ShowLogs                              = true,
                EnableAnalytics                       = true
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
                enabledCategories = new []
                {
                    EKinetixUICategory.INVENTORY,
                    EKinetixUICategory.EMOTE_SELECTOR
                }
            });

            //Core Events
            KinetixCore.Animation.OnPlayedAnimationLocalPlayer          += OnPlayedAnimationOnLocalPlayer;
            KinetixCore.Animation.OnAnimationStartOnLocalPlayerAnimator += OnAnimationStartOnLocalPlayerAnimator;
            KinetixCore.Animation.OnAnimationEndOnLocalPlayerAnimator   += OnAnimationEndOnLocalPlayerAnimator;

            //UI Events
            KinetixUI.OnPlayedAnimationWithEmoteSelector += OnPlayedAnimationWithEmoteSelector;

            //local
            KinetixCore.Animation.RegisterLocalPlayerAnimator(localPlayerAnimator);

            KinetixCore.Account.ConnectAccount("sdk-sample-user-id", OnAccountConnected);
        }

        private void OnAccountConnected()
        {
            KinetixCore.Account.AssociateEmotesToUser("f4daf21a-38b8-4f84-a27c-5eb37cd5726e");
            KinetixCore.Account.AssociateEmotesToUser("62cb8c38-ff49-448d-94c1-d820df90ccdb");
            KinetixCore.Account.AssociateEmotesToUser("a5d188f1-4d0f-4dfc-bb44-eaea54d0780f");
        }

        private void OnPlayedAnimationOnLocalPlayer(AnimationIds _AnimationIds)
        {
            Debug.Log("###EVENT### Played Animation : " + _AnimationIds.UUID);
        }

        private void OnAnimationStartOnLocalPlayerAnimator(AnimationIds _AnimationIds)
        {
            Debug.Log("###EVENT### Animation Started : " + _AnimationIds.UUID);
        }

        private void OnAnimationEndOnLocalPlayerAnimator(AnimationIds _AnimationIds)
        {
            Debug.Log("###EVENT### Animation Ended : " + _AnimationIds.UUID);
        }

        private void OnPlayedAnimationWithEmoteSelector(AnimationIds _AnimationIds)
        {
            Debug.Log("###EVENT### Animation Played With Emote Selector : " + _AnimationIds.UUID);
        }
    }
}
