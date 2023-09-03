using UnityEditor;
using UnityEngine.UIElements;

namespace CWAEmu.OFUCU.Inspectors {
    [CustomEditor(typeof(DictonaryEntry))]
    public class DictonaryEntryInspector : Editor {
        public VisualTreeAsset ImageInspector;
        public VisualTreeAsset ShapeInspector;
        public VisualTreeAsset SpriteInspector;

        private DictonaryEntry entry;

        private void OnEnable() {
            entry = (DictonaryEntry)target;
        }

        public override VisualElement CreateInspectorGUI() {
            VisualElement root = new();

            switch (entry.CharacterType) {
                case DictonaryEntry.EnumDictonaryCharacterType.Image:
                    ImageInspector.CloneTree(root);

                    var saveBtnEle = root.Q("save");
                    if (saveBtnEle is Button saveBtn) {
                        saveBtn.clicked += () => {
                            AssetPathModal.ShowModal(entry.containingFile.File.Name, entry.saveImageToAsset);
                        };
                    }
                    break;
                case DictonaryEntry.EnumDictonaryCharacterType.Shape:
                    ShapeInspector.CloneTree(root);

                    var fillBtnEle = root.Q("fill");
                    if (fillBtnEle is Button fillBtn) {
                        fillBtn.clicked += () => {
                            entry.fillShape();
                        };
                    }

                    var flattenBtnEle = root.Q("flat");
                    if (flattenBtnEle is Button flattenBtn) {
                        flattenBtn.clicked += () => {
                            entry.flattenShape();
                        };
                    }
                    break;
                case DictonaryEntry.EnumDictonaryCharacterType.Sprite:
                    SpriteInspector.CloneTree(root);

                    var placeBtnEle = root.Q("place");
                    if (placeBtnEle is Button placeBtn) {
                        placeBtn.clicked += () => {
                            entry.placeFrames();
                        };
                    }

                    var animBtnEle = root.Q("anim");
                    if (animBtnEle is Button animBtn) {
                        animBtn.clicked += () => {
                            entry.animateFrames();
                        };
                    }
                    break;
            }

            return root;
        }
    }
}
