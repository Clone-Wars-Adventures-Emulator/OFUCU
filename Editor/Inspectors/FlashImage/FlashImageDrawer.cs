using CWAEmu.OFUCU.Flash.Records;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace CWAEmu.OFUCU.Inspectors {
    [CustomPropertyDrawer(typeof(FlashImage), true)]
    public class FlashImageDrawer : PropertyDrawer {
        public override VisualElement CreatePropertyGUI(SerializedProperty property) {
            VisualElement root = new();

            // TODO: add label to this
            Foldout fold = new();
            fold.Add(new PropertyField(property.FindPropertyRelative("width")));
            fold.Add(new PropertyField(property.FindPropertyRelative("height")));

            root.Add(fold);

            return root;
        }
    }
}
