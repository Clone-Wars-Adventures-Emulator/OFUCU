using CWAEmu.OFUCU.Flash;
using CWAEmu.OFUCU.Flash.Tags;
using System;
using System.Collections.Generic;
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
            int fileCount = 0;

            int bad = 0;

            var files = Directory.EnumerateFiles(path, "*.swf", SearchOption.TopDirectoryOnly);
            foreach (var file in files) {
                Debug.Log($"Parsing SWF file at {file}");

                try {
                    SWFFile swfFile = SWFFile.readFull(file);
                    if (swfFile == null) {
                        // Temporarily ingore files that the parser doesnt parse (usually version related)
                        // Debug.LogError($"File at {file} failed to parse");
                        continue;
                    }

                    fileCount++;
                    bad += checkFrameOrdering(swfFile.Frames, swfFile.Name);
                    foreach (var sprite in swfFile.Sprites.Values) {
                        bad += checkFrameOrdering(sprite.Frames, $"{swfFile.Name}.Sprite.{sprite.CharacterId}");
                    }
                } catch (Exception e) {
                    Debug.LogError($"File at {file} failed to parse with exception: ");
                    Debug.LogException(e);
                }

            }

            Debug.Log($"There are {bad} instances of RemoveObject after PlaceObject over {fileCount} files");
        }

        private int checkFrameOrdering(List<Frame> frames, string name) {
            if (frames.Count == 0) {
                return 0;
            }

            Debug.Log($"Checking {frames.Count} frames of {name}");
            int bad = 0;
            foreach (var frame in frames) {
                bool seenPlace = false;
                foreach (var tag in frame.Tags) {
                    if (tag is PlaceObject or PlaceObject2) {
                        seenPlace = true;
                    }

                    if (tag is RemoveObject or RemoveObject2 && seenPlace) {
                        Debug.LogError($"Frame {frame.FrameIndex} of {name} has a remove after a place");
                        bad++;
                    }
                }
            }
            return bad;
        }
    }
}
