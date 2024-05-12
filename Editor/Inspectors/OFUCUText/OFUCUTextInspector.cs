using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace CWAEmu.OFUCU {
    [CustomEditor(typeof(OFUCUText))]
    public class OFUCUTextInspector : Editor {
        public VisualTreeAsset Inspector;

        private OFUCUText text;

        private void OnEnable() {
            text = (OFUCUText) target;
        }

        public override VisualElement CreateInspectorGUI() {
            if (Application.isPlaying) {
                return new();
            }

            VisualElement root = new();

            Inspector.CloneTree(root);

            var placeBtnEle = root.Q("save");
            if (placeBtnEle is Button placeBtn) {
                placeBtn.clicked += () => {
                    text.saveAsPrefab();
                };
            }

            return root;
        }
    }
}
