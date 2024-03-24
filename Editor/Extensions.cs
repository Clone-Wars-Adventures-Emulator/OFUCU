using System.Runtime.CompilerServices;
using UnityEngine;

namespace CWAEmu.OFUCU {
    public static class Extensions {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 ToVector3(this Vector2 vec, float z = 0) {
            return new Vector3(vec.x, vec.y, z);
        }

        public static int FullChildCount(this Transform t) {
            int count = t.childCount;
            int total = count;
            for (int i = 0; i < count; i++) {
                total += t.GetChild(i).FullChildCount();
            }

            return total;
        }
    }
}
