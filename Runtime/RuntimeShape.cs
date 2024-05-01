using Unity.VectorGraphics;
using UnityEngine;

namespace CWAEmu.OFUCU.Runtime {
    public class RuntimeShape : RuntimeObject {
        private SVGImage img;
        private Material mat;

        private void Awake() {
            initReferences();
        }

        public override void initReferences() {
            img = GetComponent<SVGImage>();
            mat = img.materialForRendering;
        }

        public override void applyMult(Color col) {
            mat.SetColor("_Color", col);
        }

        public override void applyAdd(Color col) {
            mat.SetColor("_AdditiveColor", col);
        }
    }
}
