using UnityEngine;

namespace CWAEmu.OFUCU.Runtime {
    public class RuntimeShape : RuntimeObject {
        public override void applyMult(Color col) {
            Debug.LogWarning("Multiply is not setup for shapes");
        }

        public override void applyAdd(Color col) {
            Debug.LogWarning("AdditiveColor is not setup for shapes");
        }
    }
}
