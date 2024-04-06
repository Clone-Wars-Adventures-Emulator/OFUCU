using UnityEngine;

namespace CWAEmu.OFUCU {
    [RequireComponent(typeof(AbstractOFUCUObject))]
    public class AnimatedOFUCUObject : MonoBehaviour {
        private AbstractOFUCUObject obj;

        public bool hasAdd;
        public bool hasMult;
        public Color addColor;
        public Color multColor;
        public float lastZRot;
        public float zRot;

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

            // TODO: code checking if the jump in Z was crazy (if it was we need to manually handle in here,
            // though maybe i make it happen in that other code, TBD, see the old tool for how to do this)

            transform.rotation = Quaternion.Euler(0, 0, zRot);
        }
    }
}
