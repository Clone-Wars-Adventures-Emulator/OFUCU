using UnityEngine;

namespace CWAEmu.OFUCU.Runtime {
    public class RuntimeText : RuntimeObject {
        // TODO: all needed props
        private bool initGuard = false;

        public override void initReferences() {
            if (initGuard) {
                return;
            }
            initGuard = true;
        }

        public override void applyMult(Color col) {

        }

        public override void applyAdd(Color col) {

        }
    }
}
