using UnityEngine;

namespace CWAEmu.OFUCU.Runtime {
    public class RuntimeButton2 : RuntimeObject {
        // TODO: all needed props
        private bool initGuard = false;

        public override void initReferences() {
            if (initGuard) {
                return;
            }
            initGuard = true;
        }

        public override void applyMult(Color col) {
            // TODO: this is kina a sprite
        }

        public override void applyAdd(Color col) {
            // TODO: this is kina a sprite
        }
    }
}
