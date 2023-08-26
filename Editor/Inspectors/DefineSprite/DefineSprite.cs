using CWAEmu.OFUCU.Flash.Tags;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace CWAEmu.OFUCU.Inspectors {
    [CustomPropertyDrawer(typeof(DefineSprite), true)]
    public class DefineSpriteDrawer : PropertyDrawer {
        public override VisualElement CreatePropertyGUI(SerializedProperty property) {
            VisualElement root = new();

            // TODO: add label to this
            Foldout fold = new();
            fold.Add(new PropertyField(property.FindPropertyRelative("NumFrames")));

            root.Add(fold);

            return root;
        }
    }
}
