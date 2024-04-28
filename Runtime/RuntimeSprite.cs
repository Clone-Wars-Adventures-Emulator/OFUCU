using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CWAEmu.OFUCU.Runtime {
    public class RuntimeSprite : RuntimeObject {
        private RuntimeObject[] children = new RuntimeObject[0];
        private bool initGuard = false;

        private void Awake() {
            initReferences();
        }

        public override void initReferences() {
            if (initGuard) {
                return;
            }
            initGuard = true;
            loadChildren();
        }

        public override void applyMult(Color col) {
            foreach (var obj in children) {
                obj.setParentMultColor(col);
            }
        }

        public override void applyAdd(Color col) {
            foreach (var obj in children) {
                obj.setParentAddColor(col);
            }
        }

        public void loadChildren() {
            var objects = gameObject.GetComponentsInChildren<RuntimeObject>(true);
            HashSet<RuntimeObject> objs = new();
            foreach (var obj in objects) {
                if (obj != this) {
                    objs.Add(obj);
                    obj.initReferences();
                }
            }
            children = objs.ToArray();
        }
    }
}
