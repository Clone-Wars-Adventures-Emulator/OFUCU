using Unity.VectorGraphics;
using UnityEngine;

namespace CWAEmu.OFUCU.Runtime {
    public class RuntimeShape : RuntimeObject {
        private SVGImage img;

        private void Awake() {
            initReferences();
        }

        public override void initReferences() {
            img = GetComponent<SVGImage>();
        }

        public override void applyMult(Color col) {
            img.materialForRendering.SetColor("_Color", col);
        }

        public override void applyAdd(Color col) {
            img.materialForRendering.SetColor("_AdditiveColor", col);
        }
    }
}
