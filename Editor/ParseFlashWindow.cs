using CWAEmu.OFUCU.Flash;
using CWAEmu.OFUCU.MagicButton;
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

            GUILayout.BeginHorizontal();
            GUILayout.Space(5);
            var magic = GUILayout.Button("Magic Button");
            GUILayout.Space(5);
            GUILayout.EndHorizontal();

            GUILayout.EndArea();

            if (browseSwf) {
                So.FindProperty("swfPath").stringValue = EditorUtility.OpenFilePanel("Select SWF File", "", "swf");
            } else if (browseRoot) {
                var dir = EditorUtility.OpenFolderPanel("Select Asset Root", "Assets", "");
                if (dir != null) {
                    dir = $"Assets/{Path.GetRelativePath(Application.dataPath, dir).Replace('\\', '/')}";
                    So.FindProperty("unityRoot").stringValue = dir;
                }
            } else if (readSwf) {
                try {
                    attemptSWFRead();
                } catch (Exception e) {
                    Debug.LogError($"Failed to parse swf {swfPath}");
                    Debug.LogException(e);
                }
            } else if (magic) {
                try {
                    magicButton();
                } catch (Exception e) {
                    Debug.LogError($"Failed to run magic button for {swfPath}");
                    Debug.LogException(e);
                }
            }

            if (So.hasModifiedProperties) {
                So.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        private bool commonRead(out SWFFile file, out Func<OFUCUSWF> createSwf, bool placeDict = true) {
            // compile proection
            createSwf = null;

            var localFile = SWFFile.readFull(swfPath, false);
            file = localFile;

            if (file == null) {
                Debug.LogError($"The supplied SWF file {swfPath} does not exist or an error occured.");
                return false;
            }

            Dictionary<int, Font> fontMap = new();
            foreach (var mapping in fonts) {
                if (fontMap.TryGetValue(mapping.fontId, out var f)) {
                    Debug.LogError($"Duplicate font mapping for {mapping.fontId}, found {f.name}, not using {mapping.font.name}");
                    continue;
                }

                fontMap.Add(mapping.fontId, mapping.font);
            }

            if (!OFUCUSWF.verifySwfPlaceable(file, unityRoot, out var tempIds)) {
                return false;
            }

            // return a function that will place the SWF when the consumer is ready
            createSwf = () => OFUCUSWF.placeNewSWFFile(localFile, unityRoot, placeDict, fontMap, tempIds);

            return true;
        }

        private void attemptSWFRead() {
            // discard the output from the common read, we dont need it for the original functionality
            commonRead(out _, out var create, placeDict: placeDict);
            create?.Invoke();
        }

        private void magicButton() {
            // we care about the results from this, if it succeeded tho
            var succ = commonRead(out var file, out var create);
            if (!succ) {
                return;
            }

            // and now the magic starts to occur
            var analysis = SwfAnalysis.of(file, unityRoot);

            GetWindow<MagicButtonWindow>().setCurrent(analysis, create);
        }
    }
}
