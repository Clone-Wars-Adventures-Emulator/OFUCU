using CWAEmu.OFUCU.Flash.Tags;
using CWAEmu.OFUCU.Runtime;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace CWAEmu.OFUCU {
    [RequireComponent(typeof(RectTransform), typeof(RuntimeText))]
    public class OFUCUText : AbstractOFUCUObject {
        // inited
        [SerializeField]
        private OFUCUSWF swf;
        [SerializeField]
        private DefineEditText text;
        [SerializeField]
        private string prefabSaveDir;
        [SerializeField]
        private string matSaveDir;

        // generated
        [SerializeField]
        private string prefabAssetPath;
        public bool HasPrefab => prefabAssetPath != null;

        [SerializeField]
        private GameObject child;
        [SerializeField]
        private RectTransform childT;

        public void init(OFUCUSWF swf, DefineEditText text, string prefabSaveDir, string matSaveDir) {
            this.swf = swf;
            this.text = text;
            this.prefabSaveDir = prefabSaveDir;
            this.matSaveDir = matSaveDir;

            child = new("Text", typeof(RectTransform), typeof(Text));
            childT = child.transform as RectTransform;
            childT.SetParent(transform, false);
            childT.pivot = new Vector2(0, 1);

            childT.anchoredPosition = new Vector2(text.Bounds.X, -text.Bounds.Y);
            childT.sizeDelta = new Vector2(text.Bounds.Width, text.Bounds.Height);

            Text t = child.GetComponent<Text>();
            if (text.HasText) {
                t.text = text.InitialText;
            }

            if (text.WordWrap) {
                t.horizontalOverflow = HorizontalWrapMode.Wrap;
            } else {
                // TODO: This is supposed to be a scroll
                t.horizontalOverflow = HorizontalWrapMode.Overflow;
            }

            if (text.Multiline) {
                t.verticalOverflow = VerticalWrapMode.Overflow;
            } else {
                t.verticalOverflow = VerticalWrapMode.Truncate;
            }

            if (text.Password) {
                Debug.LogWarning($"Unsupported text option Password on {swf.name} {text.CharacterId}");
            }

            if (!text.ReadOnly) {
                // TODO: If this *isnt* readonly, we should do something special? (input field?)
                Debug.LogWarning($"Unsupported text option NonReadOnly on {swf.name} {text.CharacterId}");
            }

            if (text.HasTextColor) {
                t.color = text.TextColor.asUnityColor();
            }

            if (text.HasMaxLength) {
                Debug.LogWarning($"Ignoring MaxLength {text.MaxLength} on {swf.name} {text.CharacterId}");
            }

            if (text.HasFont) {
                if (swf.fontMap.TryGetValue(text.FontId, out var font)) {
                    t.font = font;
                } else {
                    Debug.LogWarning($"No Unity Font mapping for {text.FontId} on {swf.name} {text.CharacterId}");
                }

                t.fontSize = Mathf.RoundToInt(text.FontHeight);
            }

            if (text.HasFontClass) {
                t.fontSize = Mathf.RoundToInt(text.FontHeight);
                // TODO: Handle font specifications (need a way to map to an existing Unity Font)
                Debug.LogWarning($"Unsupported FontClass specification on {swf.name} {text.CharacterId}");
            }

            if (text.AutoSize) {
                Debug.LogWarning($"Unsupported AutoSize specification on {swf.name} {text.CharacterId}");
            }

            if (text.HasLayout) {
                switch (text.Align) {
                    case 0:
                        t.alignment = TextAnchor.UpperLeft;
                        break;
                    case 1:
                        t.alignment = TextAnchor.UpperRight;
                        break;
                    case 2:
                        t.alignment = TextAnchor.UpperCenter;
                        break;
                    case 3:
                        Debug.LogError($"Unsupported Justify Text on {swf.name} {text.CharacterId}");
                        break;
                }

                // TODO: leftmargin, right margin, indent, leading
            }

            if (text.NoSelect) {
                // Ignore, cant select text anyway
            }

            if (text.Border) {
                Debug.LogWarning($"Unsupported Border specification on {swf.name} {text.CharacterId}");
            }

            if (text.WasStatic) {
                // TODO: what does this mean
            }

            if (text.HTML) {
                t.supportRichText = true;
            }

            if (text.UseOutlines) {
                Debug.LogWarning($"Unsupported UseOutlines specification on {swf.name} {text.CharacterId}");
            }
        }

        public override void setBlendMode(EnumFlashBlendMode blendMode, string saveFolder, string path) {
            Debug.LogError("Unimplemented");
        }

        public void saveAsPrefab() {
            if (string.IsNullOrEmpty(prefabAssetPath)) {
                if (!Directory.Exists(prefabSaveDir)) {
                    Directory.CreateDirectory(prefabSaveDir);
                }
                prefabAssetPath = $"{prefabSaveDir}/{name}.prefab";
                PrefabUtility.SaveAsPrefabAssetAndConnect(gameObject, prefabAssetPath, InteractionMode.AutomatedAction);
            } else {
                Debug.LogWarning($"{name} already has a prefab at path {prefabAssetPath}, modify that directly");
                // PrefabUtility.SavePrefabAsset(gameObject);
            }
        }

        public GameObject getCopy() {
            if (prefabAssetPath != null) {
                GameObject pgo = AssetDatabase.LoadAssetAtPath<GameObject>(prefabAssetPath);
                GameObject rgo = (GameObject)PrefabUtility.InstantiatePrefab(pgo);

                return rgo;
            }

            GameObject go = Instantiate(gameObject);

            return go;
        }
    }
}
