using UnityEditor;
using UnityEngine.UIElements;

namespace CWAEmu.OFUCU {
    [CustomEditor(typeof(OFUCUSprite))]
    public class OFUCUSpriteInspector : Editor {
        public VisualTreeAsset Inspector;

        private OFUCUSprite sprite;

        private void OnEnable() {
            sprite = (OFUCUSprite) target;
        }

        public override VisualElement CreateInspectorGUI() {
            VisualElement root = new();

            Inspector.CloneTree(root);

            var placeBtnEle = root.Q("place");
            if (placeBtnEle is Button placeBtn) {
                placeBtn.clicked += () => {
                    sprite.place();
                };
            }

            var animBtnEle = root.Q("anim");
            if (animBtnEle is Button animBtn) {
                animBtn.clicked += () => {
                    sprite.animate();
                };
            }

            return root;
        }
    }
}
