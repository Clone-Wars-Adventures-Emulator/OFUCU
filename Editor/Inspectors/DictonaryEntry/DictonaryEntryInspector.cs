using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
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
                    break;
                case DictonaryEntry.EnumDictonaryCharacterType.Shape:
                    ShapeInspector.CloneTree(root);
                    break;
                case DictonaryEntry.EnumDictonaryCharacterType.Sprite:
                    SpriteInspector.CloneTree(root);
                    break;
            }

            return root;
        }
    }
}
