using CWAEmu.OFUCU.Flash;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CWAEmu.OFUCU {
    /// <summary>
    /// Main plugin Editor Window. Used to parse a supplied SWF file and load it into the scene.
    /// </summary>
    public class ParseFlashWindow : EditorWindow {
        private string swfPath;
        private string unityRoot;

        [SerializeField]
        private List<FontMapping> fonts;
        private SerializedObject so;
        private bool placeDict = true;

        [MenuItem("Flash Tools/Parse Flash")]
        public static void showWindow() {
            GetWindow<ParseFlashWindow>("SWF Parser");
        }

        private void OnEnable() {
            var p = position;
            p.width = 600 / EditorGUIUtility.pixelsPerPoint;
            p.height = 600 / EditorGUIUtility.pixelsPerPoint;
            position = p;
            so = new(this);
        }

        private void OnGUI() {
            GUILayout.BeginArea(new Rect(0, 0, Screen.width / EditorGUIUtility.pixelsPerPoint, Screen.height / EditorGUIUtility.pixelsPerPoint));

            GUILayout.Space(5);

            GUILayout.BeginHorizontal();
            GUILayout.Space(5);
            GUILayout.Label("SWF File Path: ");
            GUILayout.Space(5);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(5);
            swfPath = EditorGUILayout.TextField(swfPath);
            GUILayout.Space(5);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(5);
            GUILayout.Label("Unity Input/Output root: ");
            GUILayout.Space(5);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(5);
            unityRoot = EditorGUILayout.TextField(unityRoot);
            GUILayout.Space(5);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(5);
            EditorGUILayout.PropertyField(so.FindProperty("fonts"), new GUIContent("Font mapping"));
            if (so.hasModifiedProperties) {
                so.ApplyModifiedPropertiesWithoutUndo();
            }
            GUILayout.Space(5);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(5);
            placeDict = GUILayout.Toggle(placeDict, "Place Dictionary");
            GUILayout.Space(5);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(5);
            if (GUILayout.Button("Read SWF")) {
                attemptSWFRead();
            }
            GUILayout.Space(5);
            GUILayout.EndHorizontal();

            GUILayout.EndArea();
        }

        private void attemptSWFRead() {
            // parse the file, this does the actual interaction with the SWF specification
            SWFFile file = SWFFile.readFull(swfPath, false);

            if (file == null) {
                Debug.LogError("The supplied SWF file does not exist or an error occured.");
                return;
            }

            Dictionary<int, Font> fontMap = new();
            foreach (var mapping in fonts) {
                if (fontMap.TryGetValue(mapping.fontId, out var f)) {
                    Debug.LogError($"Duplicate font mapping for {mapping.fontId}, found {f.name}, not using {mapping.font.name}");
                    continue;
                }

                fontMap.Add(mapping.fontId, mapping.font);
            }

            // "Place" the file, this is the start of the conversion steps from SWF to Unity
            OFUCUSWF.placeNewSWFFile(file, unityRoot, placeDict, fontMap);
        }
    }
}
