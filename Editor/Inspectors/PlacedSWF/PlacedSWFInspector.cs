using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace CWAEmu.OFUCU {
    [CustomEditor(typeof(PlacedSWFFile))]
    public class PlacedSWFInspector : Editor {
        public VisualTreeAsset Inspector;

        private PlacedSWFFile entry;

        private void OnEnable() {
            entry = (PlacedSWFFile)target;
        }

        public override VisualElement CreateInspectorGUI() {
            VisualElement root = new();

            Inspector.CloneTree(root);

            var btnEle = root.Q("save");
            if (btnEle is Button btn) {
                btn.clicked += () => {
                    AssetPathModal.ShowModal(entry.File.Name, saveAllImaages);
                };
            }

            return root;
        }

        private void saveAllImaages(string path) {
            entry.runOnAllOfType(entry => entry.saveImageToAsset(path), DictonaryEntry.EnumDictonaryCharacterType.Image);
        }
    }
}
