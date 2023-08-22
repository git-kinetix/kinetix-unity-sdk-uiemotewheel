using UnityEngine;
using Kinetix;
using Kinetix.UI;
using Kinetix.UI.EmoteWheel;

namespace Kinetix.Sample
{
    public class LegacyAnimationClip : MonoBehaviour
    {
        [SerializeField] private string gameAPIKey;
        [SerializeField] private GameObject character;
        [SerializeField] private Avatar     avatar;
        [SerializeField] private Transform  rootTransform;

        private Animation animationComponent;

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

            // EVENTS UI
            KinetixUI.OnPlayedAnimationWithEmoteSelector += OnLocalPlayedAnimation;

            KinetixCore.Animation.RegisterLocalPlayerCustom(avatar, rootTransform, EExportType.AnimationClipLegacy);

            KinetixCore.Account.ConnectAccount("sdk-sample-user-id", OnAccountConnected);
        }

        private void OnAccountConnected()
        {
            KinetixCore.Account.AssociateEmotesToUser("f4daf21a-38b8-4f84-a27c-5eb37cd5726e");
            KinetixCore.Account.AssociateEmotesToUser("62cb8c38-ff49-448d-94c1-d820df90ccdb");
            KinetixCore.Account.AssociateEmotesToUser("a5d188f1-4d0f-4dfc-bb44-eaea54d0780f");
        }

        private void OnLocalPlayedAnimation(AnimationIds _AnimationIds)
        {
            KinetixCore.Animation.GetRetargetedAnimationClipLegacyForLocalPlayer(_AnimationIds, (animationClip) =>
            {
                if (animationComponent == null)
                {
                    animationComponent                   = character.gameObject.AddComponent<Animation>();
                    animationComponent.playAutomatically = false;
                }

                animationComponent.AddClip(animationClip, _AnimationIds.UUID);
                animationComponent.Play(_AnimationIds.UUID);
            });
        }
    }
}
