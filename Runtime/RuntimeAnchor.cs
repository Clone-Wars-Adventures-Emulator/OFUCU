using UnityEngine;

namespace CWAEmu.OFUCU.Runtime {
    // TODO: how does this handle getting animated? do we even animate props of things under a mask? (yes we do, but only position?)
    public class RuntimeAnchor : MonoBehaviour {
        public Vector3 anchorPosition;
        public Quaternion anchorRotation;

        private void LateUpdate() {
            transform.SetPositionAndRotation(anchorPosition, anchorRotation);
        }
    }
}
