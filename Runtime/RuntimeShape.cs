using Unity.VectorGraphics;
using UnityEngine;

namespace CWAEmu.OFUCU.Runtime {
    public class RuntimeShape : RuntimeObject {
        private SVGImage img;
        private Material mat;

        private void Awake() {
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
