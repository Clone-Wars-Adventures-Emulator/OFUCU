using CWAEmu.OFUCU.Flash;
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace CWAEmu.OFUCU {
    public class BulkParseWindow : EditorWindow {
        private string path;

        [MenuItem("Flash Tools/Bulk Parse (DEBUG)")]
        public static void showWindow() {
            GetWindow<BulkParseWindow>("SWF Bulk Parser");
        }

        private void OnGUI() {
            GUILayout.BeginArea(new Rect(0, 0, Screen.width / EditorGUIUtility.pixelsPerPoint, Screen.height / EditorGUIUtility.pixelsPerPoint));

            GUILayout.Space(20);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            GUILayout.Label("Directory to bulk parse: ");

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            path = EditorGUILayout.TextField(path);

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Parse")) {
                bulkParse();
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.EndArea();
        }

        private void bulkParse() {
            var files = Directory.EnumerateFiles(path, "*.swf", SearchOption.TopDirectoryOnly);
            foreach (var file in files) {
                Debug.Log($"Parsing SWF file at {file}");

                try {
                    SWFFile swfFile = SWFFile.readFull(file);
                    if (swfFile == null) {
                        Debug.LogError($"File at {file} failed to parse");
                        continue;
                    }

                    Debug.Log($"{swfFile.Name} has {swfFile.CharacterTags.Count} parsed characters");

                } catch (Exception e) {
                    Debug.LogException(e);
                }

            }
        }
    }
}
