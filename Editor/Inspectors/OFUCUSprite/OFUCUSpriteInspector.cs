using UnityEditor;
using UnityEngine;
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
            if (Application.isPlaying) {
                return new();
            }

            VisualElement root = new();

            Inspector.CloneTree(root);

            var placeBtnEle = root.Q("place");
            if (placeBtnEle is Button placeBtn) {
                placeBtn.clicked += () => {
                    sprite.place();
                };
            }

            var placeDepsBtnEle = root.Q("placedeps");
            if (placeDepsBtnEle is Button placeDepsBtn) {
                placeDepsBtn.clicked += () => {
                    sprite.place(true);
                };
            }

            var animBtnEle = root.Q("anim");
            if (animBtnEle is Button animBtn) {
                animBtn.clicked += () => {
                    sprite.animate();
                };
            }

            var matsBtnEle = root.Q("mats");
            if (matsBtnEle is Button matsBtn) {
                matsBtn.clicked += () => {
                    sprite.uniquifyMaterials();
                };
            }

            var saveBtnEle = root.Q("save");
            if (saveBtnEle is Button saveBtn) {
                saveBtn.clicked += () => {
                    sprite.saveAsPrefab();
                };
            }

            return root;
        }
    }
}
