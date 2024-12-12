using UnityEngine;

namespace CWAEmu.OFUCU.Runtime {
    public class AnchoredAnimatedRuntimeObject : AnimatedRuntimeObject {
        public RectTransform canvas;
        public RectTransform anchorReference;
        public Vector3 position;
        public Vector3 scale = new(1, 1, 1);

        protected override void LateUpdate() {
            handleColor();

            var anchorScale = anchorReference.lossyScale;
            var parentScale = transform.parent.lossyScale;
            float x = (anchorScale.x / parentScale.x) * scale.x;
            float y = (anchorScale.y / parentScale.y) * scale.y;
            float z = (anchorScale.z / parentScale.z) * scale.z;
            transform.localScale = new Vector3(x, y, z);

            Quaternion targetRot = anchorReference.rotation * Quaternion.Euler(0f, 0f, zRot);

            var anchorPos = anchorReference.position;
            x = anchorPos.x + (position.x * anchorScale.x);
            y = anchorPos.y + (position.y * anchorScale.y);
            z = anchorPos.z + (position.z * anchorScale.z);
            transform.SetPositionAndRotation(new Vector3(x, y, z), targetRot);
        }
    }
}
