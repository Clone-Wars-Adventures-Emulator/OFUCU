using UnityEngine;

namespace CWAEmu.OFUCU {
    [RequireComponent(typeof(AbstractOFUCUObject))]
    public class AnimatedOFUCUObject : MonoBehaviour {
        private AbstractOFUCUObject obj;

        public bool hasAdd;
        public bool hasMult;
        public Color addColor;
        public Color multColor;

        private void Start() {
            obj = GetComponent<AbstractOFUCUObject>();
        }

        private void LateUpdate() {
            if (hasAdd) {
                obj.setAddColor(addColor);
            }

            if (hasMult) {
                obj.setMultColor(multColor);
            }
        }
    }
}
