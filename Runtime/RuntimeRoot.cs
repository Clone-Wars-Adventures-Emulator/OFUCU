using UnityEngine;

namespace CWAEmu.OFUCU.Runtime {
    [RequireComponent(typeof(RuntimeObject))]
    public class RuntimeRoot : MonoBehaviour {
        public RectTransform canvasScalar;

        public void Start() {
            var ras = GetComponentsInChildren<RuntimeAnchor>(true);
            foreach (var ra in ras) {
                ra.savePos(transform as RectTransform, canvasScalar);
            }
        }
    }
}
