using System;
using UnityEngine;

namespace CWAEmu.OFUCU.Runtime {
    [Obsolete("Only Used in OFUCU < 1.3, kept for backwards compatability")]
    public class RuntimeAnchor : MonoBehaviour {
        public RectTransform canvas;
        public RectTransform anchorReference;
        public Vector3 anchorPositionOffset;
        public Vector3 anchorScale;
        public Quaternion anchorRotationOffset;

        private void LateUpdate() {
            var canvasScale = canvas.localScale;
            var tmp = anchorPositionOffset;
            tmp = new Vector3(tmp.x * canvasScale.x, tmp.y * canvasScale.y, tmp.z * canvasScale.z);
            var pos = anchorReference.position + tmp;
            var rot = anchorReference.rotation * anchorRotationOffset;
            transform.SetPositionAndRotation(pos, rot);

            var parLoss = transform.parent.lossyScale;
            float x = (anchorScale.x * canvasScale.x) / parLoss.x;
            float y = (anchorScale.y * canvasScale.y) / parLoss.y;
            float z = (anchorScale.z * canvasScale.z) / parLoss.z;
            transform.localScale = new Vector3(x, y, z);
        }

        public void savePos(RectTransform anchor, RectTransform canvas) {
            var canvasScale = canvas.localScale;
            this.canvas = canvas;

            anchorReference = anchor;
            anchorPositionOffset = (transform.position - anchor.position);
            anchorPositionOffset = new Vector3(anchorPositionOffset.x / canvasScale.x, anchorPositionOffset.y / canvasScale.y, anchorPositionOffset.z / canvasScale.z);
            anchorRotationOffset = anchor.rotation * Quaternion.Inverse(transform.rotation);

            var lossyScale = transform.lossyScale;
            float x = lossyScale.x / canvasScale.x;
            float y = lossyScale.y / canvasScale.y;
            float z = lossyScale.z / canvasScale.z;
            anchorScale = new Vector3(x, y, z);
        }
    }
}
