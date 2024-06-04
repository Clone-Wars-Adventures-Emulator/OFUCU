using UnityEngine;

namespace CWAEmu.OFUCU.Runtime {
    public class RuntimeObject : MonoBehaviour {
        [SerializeField]
        private Color parentMult = new(1, 1, 1, 1);
        [SerializeField]
        private Color parentAdd = new(0, 0, 0, 0);
        [SerializeField]
        private Color selfMult = new(1, 1, 1, 1);
        [SerializeField]
        private Color selfAdd = new(0, 0, 0, 0);

        public Color getSelfMult() {
            return selfMult;
        }

        public Color getSelfAdd() {
            return selfAdd;
        }

        public void setMultColor(Color color) {
            selfMult = color;
            var r = Mathf.Clamp(parentMult.r * selfMult.r, 0, 1);
            var g = Mathf.Clamp(parentMult.g * selfMult.g, 0, 1);
            var b = Mathf.Clamp(parentMult.b * selfMult.b, 0, 1);
            var a = Mathf.Clamp(parentMult.a * selfMult.a, 0, 1);
            var res = new Color(r, g, b, a);
            applyMult(res);
        }

        public void setAddColor(Color color) {
            selfAdd = color;
            var r = Mathf.Clamp(parentAdd.r + selfAdd.r, 0, 1);
            var g = Mathf.Clamp(parentAdd.g + selfAdd.g, 0, 1);
            var b = Mathf.Clamp(parentAdd.b + selfAdd.b, 0, 1);
            var a = Mathf.Clamp(parentAdd.a + selfAdd.a, 0, 1);
            var res = new Color(r, g, b, a);
            applyAdd(res);
        }

        public void setParentMultColor(Color color) {
            parentMult = color;
            var r = Mathf.Clamp(parentMult.r * selfMult.r, 0, 1);
            var g = Mathf.Clamp(parentMult.g * selfMult.g, 0, 1);
            var b = Mathf.Clamp(parentMult.b * selfMult.b, 0, 1);
            var a = Mathf.Clamp(parentMult.a * selfMult.a, 0, 1);
            var res = new Color(r, g, b, a);
            applyMult(res);
        }

        public void setParentAddColor(Color color) {
            parentAdd = color;
            var r = Mathf.Clamp(parentAdd.r + selfAdd.r, 0, 1);
            var g = Mathf.Clamp(parentAdd.g + selfAdd.g, 0, 1);
            var b = Mathf.Clamp(parentAdd.b + selfAdd.b, 0, 1);
            var a = Mathf.Clamp(parentAdd.a + selfAdd.a, 0, 1);
            var res = new Color(r, g, b, a);
            applyAdd(res);
        }

        public virtual void applyMult(Color col) {

        }

        public virtual void applyAdd(Color col) {

        }

        // hack to get around the issues of awake/start not being called if the object is disabled
        public virtual void initReferences() {

        }
    }
}
