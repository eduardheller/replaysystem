using IREX.ReplaySystem.Monobehaviours;
using UnityEngine;

namespace IREX.ReplaySystem
{
    public static class ReplayMath
    {

        private static float _tolerance = ReplayManager.Instance.floatTolerance;
    
        public static bool IsApproximatelyEqual(float a, float b) 
        {
            return Mathf.Abs(a - b) < _tolerance;
        }
    
        public static bool IsApproximatelyEqual(Vector3 a, Vector3 b) 
        {
            var dx = a.x - b.x;
            var eq = Mathf.Abs(dx) < _tolerance;
            if (eq)
            {
                var dy = a.y - b.y;
                eq = Mathf.Abs(dy) < _tolerance;
                if (eq)
                {
                    var dz = a.z - b.z;
                    eq = Mathf.Abs(dz) < _tolerance;
                    if (eq)
                        return true;
                }
            }
            
            return false;
        }

        public static bool IsApproximatelyEqual(Quaternion a, Quaternion b) 
        {
            var dx = a.x - b.x;
            var eq = Mathf.Abs(dx) < _tolerance;
            if (eq)
            {
                var dy = a.y - b.y;
                eq = Mathf.Abs(dy) < _tolerance;
                if (eq)
                {
                    var dz = a.z - b.z;
                    eq = Mathf.Abs(dz) < _tolerance;
                    if (eq)
                    {
                        var dw = a.w - b.w;
                        eq = Mathf.Abs(dw) < _tolerance;
                        if (eq)
                            return true;

                    }
                }
            }
            return false;
        }
        
    }
}