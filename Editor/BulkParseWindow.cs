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
            Matrix.All.Clear();

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

            // TODO: remove debug
            int t = 0;
            int r = 0;
            int s = 0;
            int tr = 0;
            int ts = 0;
            int sr = 0;
            int tsr = 0;
            int nonUnifScale = 0;
            int anyWithScale = 0;
            foreach (Matrix matrix in Matrix.All) {
                bool bt = matrix.hasT();
                bool br = matrix.hasR();
                bool bs = matrix.hasS();

                if (bs) {
                    anyWithScale++;
                    if (Mathf.Abs(matrix.ScaleX - matrix.ScaleY) > 0.0001) {
                        nonUnifScale++;
                    }
                }

                if (bt && !br && !bs) {
                    t++;
                }
                if (!bt && br && !bs) {
                    r++;
                }
                if (!bt && !br && bs) {
                    s++;
                }

                if (bt && br && !bs) {
                    tr++;
                }
                if (bt && !br && bs) {
                    ts++;
                }
                if (!bt && br && bs) {
                    sr++;
                }

                if (bt && br && bs) {
                    tsr++;
                }
            }

            Debug.Log($" total: {Matrix.All.Count}");
            Debug.Log($"   all: {tsr}");
            Debug.Log($" t & r: {tr}");
            Debug.Log($" t & s: {ts}");
            Debug.Log($" s & r: {sr}");
            Debug.Log($"only t: {t}");
            Debug.Log($"only s: {s}");
            Debug.Log($"only r: {r}");
            Debug.Log($"Scale things: {nonUnifScale}/{anyWithScale}");

            Debug.Log($"There are {count} with layout over {fileCount} files with {total} text entries");
        }
    }
}
