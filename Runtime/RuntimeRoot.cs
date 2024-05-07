using UnityEngine;

namespace CWAEmu.OFUCU.Runtime {
    [RequireComponent(typeof(RuntimeObject))]
    public class RuntimeRoot : MonoBehaviour {
        public RectTransform canvasScalar;

        public void Start() {
            var ras = GetComponentsInChildren<RuntimeAnchor>(true);
            Debug.Log($"starting runtime root on {name}, there are {ras.Length} children");
            foreach (var ra in ras) {
                Debug.Log($"Saving pos on obj {ra.name} with {(transform as RectTransform)?.name} as root and {canvasScalar.name} as rsh");
                ra.savePos(transform as RectTransform, canvasScalar);
            }
        }
    }
}
