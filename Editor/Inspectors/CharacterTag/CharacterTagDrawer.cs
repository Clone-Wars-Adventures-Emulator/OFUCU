using CWAEmu.OFUCU.Flash.Tags;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace CWAEmu.OFUCU.Inspectors {
    [CustomPropertyDrawer(typeof(CharacterTag), true)]
    public class CharacterTagDrawer : PropertyDrawer {
        public override VisualElement CreatePropertyGUI(SerializedProperty property) {
            VisualElement root = new();

            // TODO: add label to this
            Foldout fold = new();
            var header = property.FindPropertyRelative("header");
            fold.Add(new PropertyField(header.FindPropertyRelative("tagType"), "Tag Type"));
            fold.Add(new PropertyField(header.FindPropertyRelative("tagLength"), "Tag Length (bytes)"));
            fold.Add(new PropertyField(property.FindPropertyRelative("charId"), "Char Id"));
            
            root.Add(fold);

            return root;
        }
    }
}
