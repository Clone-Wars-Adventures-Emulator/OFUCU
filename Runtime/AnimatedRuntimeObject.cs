using UnityEngine;

namespace CWAEmu.OFUCU.Runtime {
    [RequireComponent(typeof(RuntimeObject))]
    public class AnimatedRuntimeObject : MonoBehaviour {
        private RuntimeObject obj;

        public bool hasAdd;
        public bool hasMult;
        public Color addColor;
        public Color multColor;
        public float zRot;

        private void Start() {
            obj = GetComponent<RuntimeObject>();
        }

#if UNITY_EDITOR
        [ExecuteInEditMode]
        public void Update() {
            // late update isnt called in the editor, so we do this crap to get animations to show up during development
            LateUpdate();
        }
#endif

        protected void handleColor() {
            if (hasAdd) {
                obj.setAddColor(addColor);
            }

            if (hasMult) {
                obj.setMultColor(multColor);
            }
        }

        protected virtual void LateUpdate() {
            handleColor();

            // originally, the script accounted for the jumps in the rotation, but then that got moved to the AnimationData AnimatedThing.
            // This was left for back compat and also because originally I didnt know how to animate the rotation with EULER angles (via ac.SetCurve)
            transform.localRotation = Quaternion.Euler(0, 0, zRot);
        }
    }
}
