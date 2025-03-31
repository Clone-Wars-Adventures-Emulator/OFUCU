using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace CWAEmu.OFUCU {
    [CustomEditor(typeof(OFUCUButton2))]
    public class OFUCUButton2Inspector : Editor {
        public VisualTreeAsset Inspector;

        private OFUCUButton2 btn;

        private void OnEnable() {
            btn = (OFUCUButton2) target;
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
                    btn.saveAsPrefab();
                };
            }

            return root;
        }
    }
}
