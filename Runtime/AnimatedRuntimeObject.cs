using UnityEngine;

namespace CWAEmu.OFUCU.Runtime {
    [RequireComponent(typeof(RuntimeObject))]
    public class AnimatedRuntimeObject : MonoBehaviour {
        private RuntimeObject obj;

        public bool hasAdd;
        public bool hasMult;
        public Color addColor;
        public Color multColor;
        public float lastZRot;
        public float zRot;

        private void Start() {
            obj = GetComponent<RuntimeObject>();
        }

        private void LateUpdate() {
            if (hasAdd) {
                obj.setAddColor(addColor);
            }

            if (hasMult) {
                obj.setMultColor(multColor);
            }

            // TODO: position update or something for here? gotta figure out how to animate masked objects

            // TODO: code checking if the jump in Z was crazy (if it was we need to manually handle in here,
            // though maybe i make it happen in that other code, TBD, see the old tool for how to do this)

            transform.rotation = Quaternion.Euler(0, 0, zRot);
        }
    }
}
