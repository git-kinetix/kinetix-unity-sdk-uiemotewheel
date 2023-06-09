using System.Collections.Generic;

namespace Kinetix.UI.EmoteWheel
{
    public static class KinetixIconLoadingManager
    {
        private static Dictionary<AnimationIds, List<AnimationIcon>> locksByIds;

        public static void RegisterLoadedIconForIds(AnimationIds _Ids, AnimationIcon _Icon)
        {
            locksByIds ??= new Dictionary<AnimationIds, List<AnimationIcon>>();

            if (!locksByIds.ContainsKey(_Ids))
                locksByIds[_Ids] = new List<AnimationIcon>();

            if (!locksByIds[_Ids].Contains(_Icon))
            {
                locksByIds[_Ids].Add(_Icon);
            }

            
        }

        public static void UnregisterLoadedIdsForIcon(AnimationIds _Ids, AnimationIcon _Icon)
        {
            locksByIds ??= new Dictionary<AnimationIds, List<AnimationIcon>>();

            if (_Ids == null)
                return;

            if (locksByIds.ContainsKey(_Ids) == false)
                return;

            locksByIds[_Ids].Remove(_Icon);
        }

        public static void Refresh()
        {
            locksByIds ??= new Dictionary<AnimationIds, List<AnimationIcon>>();

            foreach (KeyValuePair<AnimationIds, List<AnimationIcon>> iconsByAnimIds in locksByIds) 
            {
                if (locksByIds[iconsByAnimIds.Key].Exists(icon => icon.ids.Equals(iconsByAnimIds.Key)) == false)
                {
                    KinetixCore.Metadata.UnloadIconByAnimationId(iconsByAnimIds.Key);
                    locksByIds[iconsByAnimIds.Key].Clear();
                }
            }
        }
    }
}
