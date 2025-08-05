using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace CWAEmu.OFUCU {
    [CustomEditor(typeof(OFUCUButton2))]
    [CanEditMultipleObjects]
    public class OFUCUButton2Inspector : Editor {
        public VisualTreeAsset Inspector;

        public override VisualElement CreateInspectorGUI() {
            if (Application.isPlaying) {
                return new();
            }

            VisualElement root = new();

            Inspector.CloneTree(root);

            var placeBtnEle = root.Q("save");
            if (placeBtnEle is Button placeBtn) {
                placeBtn.clicked += () => {
                    foreach (var btnTarget in targets) {
                        try {
                            ((OFUCUButton2) btnTarget).saveAsPrefab();
                        } catch (Exception e) {
                            Debug.LogError($"Failed to save {btnTarget.name} as a prefab");
                            Debug.LogException(e);
                        }
                    }
                };
            }

            return root;
        }
    }
}
