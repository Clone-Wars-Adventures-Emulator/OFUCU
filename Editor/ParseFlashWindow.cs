using CWAEmu.OFUCU.Flash;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace CWAEmu.OFUCU {
    /// <summary>
    /// Main plugin Editor Window. Used to parse a supplied SWF file and load it into the scene.
    /// </summary>
    public class ParseFlashWindow : EditorWindow {
        [SerializeField]
        private string swfPath;
        [SerializeField]
        private string unityRoot;

        [SerializeField]
        private List<FontMapping> fonts;
        private SerializedObject so;
        private SerializedObject So => so ??= new(this);
        private bool placeDict = true;

        [MenuItem("OFUCU/Parse Flash %#&F")]
        public static void showWindow() {
            GetWindow<ParseFlashWindow>("SWF Parser");
        }

        private void OnEnable() {
            var p = position;
            p.width = 600 / EditorGUIUtility.pixelsPerPoint;
            p.height = 600 / EditorGUIUtility.pixelsPerPoint;
            position = p;
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
            EditorGUILayout.PropertyField(So.FindProperty("swfPath"), new GUIContent(""));
            GUILayout.Space(5);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(5);
            bool browseSwf = GUILayout.Button("Browse for SWF");
            GUILayout.Space(5);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(5);
            GUILayout.Label("Unity Input/Output root: ");
            GUILayout.Space(5);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(5);
            EditorGUILayout.PropertyField(So.FindProperty("unityRoot"), new GUIContent(""));
            GUILayout.Space(5);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(5);
            bool browseRoot = GUILayout.Button("Browse for Unity Root");
            GUILayout.Space(5);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(5);
            EditorGUILayout.PropertyField(So.FindProperty("fonts"), new GUIContent("Font mapping"));
            GUILayout.Space(5);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(5);
            placeDict = GUILayout.Toggle(placeDict, "Place Dictionary");
            GUILayout.Space(5);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(5);
            var readSwf = GUILayout.Button("Read SWF");
            GUILayout.Space(5);
            GUILayout.EndHorizontal();

            GUILayout.EndArea();

            if (browseSwf) {
                So.FindProperty("swfPath").stringValue = EditorUtility.OpenFilePanel("Select SWF File", "", "swf");
            } else if (browseRoot) {
                var dir = EditorUtility.OpenFolderPanel("Select Asset Root", "Assets", "");
                dir = $"Assets/{Path.GetRelativePath(Application.dataPath, dir).Replace('\\', '/')}";
                So.FindProperty("unityRoot").stringValue = dir;
            } else if (readSwf) {
                try {
                    attemptSWFRead();
                } catch (Exception e) {
                    Debug.LogError($"Failed to parse swf {swfPath}");
                    Debug.LogException(e);
                }
            }

            if (so.hasModifiedProperties) {
                so.ApplyModifiedPropertiesWithoutUndo();
            }
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
