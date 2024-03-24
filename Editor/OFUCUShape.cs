using CWAEmu.OFUCU.Flash.Tags;
using UnityEngine;

namespace CWAEmu.OFUCU {
    public class OFUCUShape : AbstractOFUCUObject {
        private void Awake() {
            // get svg component
        }

        public override void setAddColor(Color color) {
            Debug.LogWarning("AdditiveColor is not setup for shapes");
            // TODO: 
        }

        public override void setBlendMode(EnumFlashBlendMode blendMode) {
            Debug.LogWarning("BlendMode is not setup for shapes");
            // TODO: 
        }

        public override void setMultColor(Color color) {
            Debug.LogWarning("Multiply is not setup for shapes");
            // TODO: 
        }
    }
}
