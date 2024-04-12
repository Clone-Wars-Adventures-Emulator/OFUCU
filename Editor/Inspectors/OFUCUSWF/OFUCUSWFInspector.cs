using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace CWAEmu.OFUCU {
    [CustomEditor(typeof(OFUCUSWF))]
    public class OFUCUSWFInspector : Editor {
        public VisualTreeAsset Inspector;

        private OFUCUSWF swf;

        private void OnEnable() {
            swf = (OFUCUSWF) target;
        }

        public override VisualElement CreateInspectorGUI() {
            if (Application.isPlaying) {
                return new();
            }

            VisualElement root = new();

            Inspector.CloneTree(root);

            var placeBtnEle = root.Q("place");
            if (placeBtnEle is Button placeBtn) {
                placeBtn.clicked += () => {
                    swf.placeSwf();
                };
            }

            var animBtnEle = root.Q("anim");
            if (animBtnEle is Button animBtn) {
                animBtn.clicked += () => {
                    swf.animSwf();
                };
            }

            return root;
        }
    }
}
