using CWAEmu.OFUCU.Flash;
using CWAEmu.OFUCU.Flash.Records;
using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using Rect = UnityEngine.Rect;

namespace CWAEmu.OFUCU {
    /// <summary>
    /// Editor Window that will parse all SWF files in a supplied directory, parse them, and run some code on them.
    /// 
    /// Currently configured to generate statistics about matrix usage.
    /// </summary>
    public class BulkParseWindow : EditorWindow {
        private string path;

        [MenuItem("OFUCU/Bulk Parse (DEBUG)")]
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
            int count = 0;
            int fileCount = 0;
            int total = 0;
            var files = Directory.EnumerateFiles(path, "*.swf", SearchOption.TopDirectoryOnly);
            foreach (var file in files) {
                Debug.Log($"Parsing SWF file at {file}");

                try {
                    SWFFile swfFile = SWFFile.readFull(file);
                    if (swfFile == null) {
                        Debug.LogError($"File at {file} failed to parse");
                        continue;
                    }

                    fileCount++;

                    Debug.Log($"{swfFile.Name} has {swfFile.CharacterTags.Count} parsed characters");

                    foreach (var text in swfFile.EditTexts) {
                        if (text.Value.HasLayout) {
                            Debug.Log($"{swfFile.Name} has layout of {text.Value.LeftMargin} {text.Value.RightMargin} {text.Value.Indent} {text.Value.Leading}");
                            count++;
                        }
                        total++;
                    }

                } catch (Exception e) {
                    Debug.LogError($"File at {file} failed to parse with exception: ");
                    Debug.LogException(e);
                }

            }

            Debug.Log($"There are {count} with layout over {fileCount} files with {total} text entries");
        }
    }
}
