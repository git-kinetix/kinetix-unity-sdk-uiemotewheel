
using UnityEngine;

namespace Kinetix.UI.EmoteWheel
{
    public static class KinetixUtils 
    { 
        public static float Remap (this float from, float fromMin, float fromMax, float toMin,  float toMax)
        {
            var fromAbs  =  from - fromMin;
            var fromMaxAbs = fromMax - fromMin;      
        
            var normal = fromAbs / fromMaxAbs;
    
            var toMaxAbs = toMax - toMin;
            var toAbs = toMaxAbs * normal;
    
            var to = toAbs + toMin;
        
            return to;
        }

        public static int GetIndexFromAWheel( Vector2 direction, int totalIndex, int offset = 3)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            if(angle < 0 ) angle = 360 + angle;
            angle = 360 - angle;
            float anglePerElement = 360.0f / totalIndex;
            int index = (int)(angle / anglePerElement) + offset;
            if( index > totalIndex-1) index = index - totalIndex;

            return Mathf.Clamp(index, 0, totalIndex-1);
        }

        public static int GetIndexFromAList( Vector2 direction, int currentIndex, int totalIndex)
        {
            float _thresholdValue = 0.001f;
            if( direction.y > _thresholdValue )
                currentIndex--;
            else if( direction.y < -_thresholdValue )
                currentIndex++;
                
            if( currentIndex > totalIndex-1) currentIndex = 0;
            if( currentIndex < 0) currentIndex = totalIndex-1;

            return currentIndex;
        }
    }
}
