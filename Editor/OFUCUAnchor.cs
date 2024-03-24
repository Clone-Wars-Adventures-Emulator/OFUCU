using UnityEngine;

namespace CWAEmu.OFUCU{
    public class OFUCUAnchor : MonoBehaviour {
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
