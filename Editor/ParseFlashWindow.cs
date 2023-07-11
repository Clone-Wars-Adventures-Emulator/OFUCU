using UnityEditor;
using UnityEngine;

using CWAEmu.FlashConverter.Flash;

namespace CWAEmu.FlashConverter {
    public class ParseFlashWindow : EditorWindow {
        private string swfPath;
        private bool parseImages;

        [MenuItem("Flash Tools/Parse Flash")]
        public static void showWindow() {
            GetWindow<ParseFlashWindow>("SWF Parser");
        }

        private void OnGUI() {
            GUILayout.BeginArea(new Rect(0, 0, Screen.width / EditorGUIUtility.pixelsPerPoint, Screen.height / EditorGUIUtility.pixelsPerPoint));

            GUILayout.Space(20);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            GUILayout.Label("SWF File Path: ");

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            swfPath = EditorGUILayout.TextField(swfPath);

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            parseImages = GUILayout.Toggle(parseImages, "Parse Images");

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Read SWF")) {
                attemptSWFRead();
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.EndArea();
        }

        private void attemptSWFRead() {
            SWFFile file = SWFFile.readFull(swfPath, parseImages);

            if (file == null) {
                Debug.LogError("That file does not exist bozo head. Skill issue, git gud, be better.");
                return;
            }

            new FlashToUnityOneShot(file);
        }
    }
}
