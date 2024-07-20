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
            var r = parentMult.r * selfMult.r;
            var g = parentMult.g * selfMult.g;
            var b = parentMult.b * selfMult.b;
            var a = parentMult.a * selfMult.a;
            var res = new Color(r, g, b, a);
            applyMult(res);
        }

        public void setAddColor(Color color) {
            selfAdd = color;
            var r = parentAdd.r + selfAdd.r;
            var g = parentAdd.g + selfAdd.g;
            var b = parentAdd.b + selfAdd.b;
            var a = parentAdd.a + selfAdd.a;
            var res = new Color(r, g, b, a);
            applyAdd(res);
        }

        public void setParentMultColor(Color color) {
            parentMult = color;
            var r = parentMult.r * selfMult.r;
            var g = parentMult.g * selfMult.g;
            var b = parentMult.b * selfMult.b;
            var a = parentMult.a * selfMult.a;
            var res = new Color(r, g, b, a);
            applyMult(res);
        }

        public void setParentAddColor(Color color) {
            parentAdd = color;
            var r = parentAdd.r + selfAdd.r;
            var g = parentAdd.g + selfAdd.g;
            var b = parentAdd.b + selfAdd.b;
            var a = parentAdd.a + selfAdd.a;
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
