using UnityEngine;

namespace CWAEmu.OFUCU.Runtime {
    [RequireComponent(typeof(RuntimeObject))]
    public class RuntimeRoot : MonoBehaviour {
        public RectTransform canvasScalar;

        public void Start() {
            // Suppress this warning because we still want to suppor this feature set, but we others should not build off of it
#pragma warning disable CS0618 // Type or member is obsolete
            var ras = GetComponentsInChildren<RuntimeAnchor>(true);
            foreach (var ra in ras) {
                ra.savePos(transform as RectTransform, canvasScalar);
            }
#pragma warning restore CS0618 // Type or member is obsolete

            var aaros = GetComponentsInChildren<AnchoredAnimatedRuntimeObject>(true);
            foreach (var aaro in aaros) {
                aaro.canvas = canvasScalar;
            }
        }
    }
}
