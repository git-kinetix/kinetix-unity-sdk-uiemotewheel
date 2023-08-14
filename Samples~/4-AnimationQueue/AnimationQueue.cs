using UnityEngine;
using System.Linq;
using Kinetix;
using Kinetix.UI;
using Kinetix.UI.EmoteWheel;

namespace Kinetix.Sample
{
    public class AnimationQueue : MonoBehaviour
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
                ShowLogs                              = true
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

            // EVENTS
            KinetixCore.Animation.OnPlayedAnimationQueueLocalPlayer     += OnPlayedAnimationQueueLocal;
            KinetixCore.Animation.OnAnimationStartOnLocalPlayerAnimator += OnAnimationStartOnLocalPlayerAnimator;
            KinetixCore.Animation.OnAnimationEndOnLocalPlayerAnimator   += OnAnimationEndOnLocalPlayerAnimator;

            KinetixCore.Animation.RegisterLocalPlayerAnimator(localPlayerAnimator);

            KinetixCore.Account.ConnectAccount("sdk-sample-user-id", OnAccountConnected);
        }

        private void OnAccountConnected()
        {
            KinetixCore.Account.AssociateEmotesToUser("f4daf21a-38b8-4f84-a27c-5eb37cd5726e");
            KinetixCore.Account.AssociateEmotesToUser("62cb8c38-ff49-448d-94c1-d820df90ccdb");
            KinetixCore.Account.AssociateEmotesToUser("a5d188f1-4d0f-4dfc-bb44-eaea54d0780f");

            KinetixCore.Metadata.GetUserAnimationMetadatas(animations =>
            {
                AnimationIds[] animationIDs = animations.Select(metadata => metadata.Ids).Take(2).ToArray();
                KinetixCore.Animation.LoadLocalPlayerAnimations(animationIDs, "AnimationQueueSampleImplementation", () => { KinetixCore.Animation.PlayAnimationQueueOnLocalPlayer(animationIDs, true); });
            });
        }


        private void OnPlayedAnimationQueueLocal(AnimationIds[] _AnimationIdsArray)
        {
            string animationStr = "";
            _AnimationIdsArray.ToList().ForEach(animationIds => animationStr += animationIds.UUID + "\n");
            Debug.Log("[LOCAL] Played Animation queue : \n" + animationStr);
        }

        private void OnAnimationStartOnLocalPlayerAnimator(AnimationIds _AnimationIds)
        {
            Debug.Log("[LOCAL] Animation Started : " + _AnimationIds.UUID);
        }

        private void OnAnimationEndOnLocalPlayerAnimator(AnimationIds _AnimationIds)
        {
            Debug.Log("[LOCAL] Animation Ended : " + _AnimationIds.UUID);
        }
    }
}
