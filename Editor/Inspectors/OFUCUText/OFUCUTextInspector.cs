using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace CWAEmu.OFUCU {
    [CustomEditor(typeof(OFUCUText))]
    [CanEditMultipleObjects]
    public class OFUCUTextInspector : Editor {
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
                    foreach (var textTarget in targets) {
                        try {
                            ((OFUCUText) textTarget).saveAsPrefab();
                        } catch (Exception e) {
                            Debug.LogError($"Failed to save {textTarget.name} as a prefab");
                            Debug.LogException(e);
                        }
                    }
                };
            }

            return root;
        }
    }
}
