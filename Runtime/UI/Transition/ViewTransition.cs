using System.Collections.Generic;
using UnityEngine;

namespace Kinetix.UI.EmoteWheel
{
    public class ViewTransition : MonoBehaviour
    {
        [SerializeField] private EKinetixUICategory kinetixCategory;
        [SerializeField] private List<Effect>       effects;
        
        private void Awake()
        {
            KinetixUI.OnShowView += OnShowView;
            KinetixUI.OnHideView += OnHideView;
        }

        private void OnDestroy()
        {
            KinetixUI.OnShowView -= OnShowView;
            KinetixUI.OnHideView -= OnHideView;
        }

        private void OnShowView(EKinetixUICategory kinetixUICategory)
        {
            if (kinetixCategory != kinetixUICategory)
                return;

            effects.ForEach(effect => effect.Play());
        }

        private void OnHideView(EKinetixUICategory kinetixUICategory)
        {
            if (kinetixCategory != kinetixUICategory)
                return;
            
            effects.ForEach(effect => effect.Stop());
        }
    }
}

