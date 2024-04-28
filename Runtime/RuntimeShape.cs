using Unity.VectorGraphics;
using UnityEngine;

namespace CWAEmu.OFUCU.Runtime {
    public class RuntimeShape : RuntimeObject {
        private SVGImage img;
        private Material mat;
        private bool initGuard = false;

        private void Awake() {
            initReferences();
        }

        public override void initReferences() {
            if (initGuard) {
                return;
            }
            initGuard = true;
            img = GetComponent<SVGImage>();
            mat = img.material;
        }

        public override void applyMult(Color col) {
            mat.SetColor("_Color", col);
        }

        public override void applyAdd(Color col) {
            mat.SetColor("_AdditiveColor", col);
        }
    }
}
