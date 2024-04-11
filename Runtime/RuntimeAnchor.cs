using UnityEngine;

namespace CWAEmu.OFUCU.Runtime {
    // TODO: how does this handle getting animated? do we even animate props of things under a mask?
    public class RuntimeAnchor : MonoBehaviour {
        private Vector3 anchorPosition;
        private Quaternion anchorRotation;

        private void Start() {
            anchorPosition = transform.position;
            anchorRotation = transform.rotation;
        }

        private void LateUpdate() {
            transform.SetPositionAndRotation(anchorPosition, anchorRotation);
        }
    }
}
