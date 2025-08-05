using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace CWAEmu.OFUCU {
    [CustomEditor(typeof(OFUCUSprite))]
    [CanEditMultipleObjects]
    public class OFUCUSpriteInspector : Editor {
        public VisualTreeAsset Inspector;

        public override VisualElement CreateInspectorGUI() {
            if (Application.isPlaying) {
                return new();
            }

            VisualElement root = new();

            Inspector.CloneTree(root);

            var placeBtnEle = root.Q("place");
            if (placeBtnEle is Button placeBtn) {
                placeBtn.clicked += () => {
                    foreach (var spriteTarget in targets) {
                        try {
                            ((OFUCUSprite) spriteTarget).place();
                        } catch (Exception e) {
                            Debug.LogError($"Failed to place {spriteTarget.name}");
                            Debug.LogException(e);
                        }
                    }
                };
            }

            var placeDepsBtnEle = root.Q("placedeps");
            if (placeDepsBtnEle is Button placeDepsBtn) {
                placeDepsBtn.clicked += () => {
                    foreach (var spriteTarget in targets) {
                        try {
                            ((OFUCUSprite) spriteTarget).place(forceDeps: true);
                        } catch (Exception e) {
                            Debug.LogError($"Failed to forcibly place {spriteTarget.name}");
                            Debug.LogException(e);
                        }
                    }
                };
            }

            var placeLightBtnEle = root.Q("placelight");
            if (placeLightBtnEle is Button placeLightBtn) {
                placeLightBtn.clicked += () => {
                    foreach (var spriteTarget in targets) {
                        try {
                            ((OFUCUSprite) spriteTarget).place(ignoreMissing: true);
                        } catch (Exception e) {
                            Debug.LogError($"Failed to place {spriteTarget.name} while ignoring missing");
                            Debug.LogException(e);
                        }
                    }
                };
            }

            var animBtnEle = root.Q("anim");
            if (animBtnEle is Button animBtn) {
                animBtn.clicked += () => {
                    foreach (var spriteTarget in targets) {
                        try {
                            ((OFUCUSprite) spriteTarget).animate();
                        } catch (Exception e) {
                            Debug.LogError($"Failed to animate {spriteTarget.name}");
                            Debug.LogException(e);
                        }
                    }
                };
            }

            var matsBtnEle = root.Q("mats");
            if (matsBtnEle is Button matsBtn) {
                matsBtn.clicked += () => {
                    foreach (var spriteTarget in targets) {
                        try {
                            ((OFUCUSprite) spriteTarget).uniquifyMaterials();
                        } catch (Exception e) {
                            Debug.LogError($"Failed to uniquify {spriteTarget.name}");
                            Debug.LogException(e);
                        }
                    }
                };
            }

            var saveBtnEle = root.Q("save");
            if (saveBtnEle is Button saveBtn) {
                saveBtn.clicked += () => {
                    foreach (var spriteTarget in targets) {
                        try {
                            ((OFUCUSprite) spriteTarget).saveAsPrefab();
                        } catch (Exception e) {
                            Debug.LogError($"Failed to save {spriteTarget.name} as a prefab");
                            Debug.LogException(e);
                        }
                    }
                };
            }

            return root;
        }
    }
}
