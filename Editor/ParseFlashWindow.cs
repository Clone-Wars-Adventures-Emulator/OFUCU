using CWAEmu.OFUCU.Flash;
using UnityEditor;
using UnityEngine;

namespace CWAEmu.OFUCU {
    /// <summary>
    /// Main plugin Editor Window. Used to parse a supplied SWF file and load it into the scene.
    /// </summary>
    public class ParseFlashWindow : EditorWindow {
        private string swfPath;
        private string unityRoot;

        [MenuItem("Flash Tools/Parse Flash")]
        public static void showWindow() {
            GetWindow<ParseFlashWindow>("SWF Parser");
        }

        // TODO: check box to disable placing the dictionary
        // TODO: input to specify font for entire SWF? (this might be nice? idk)
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

            GUILayout.Label("Unity Input/Output root: ");

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            unityRoot = EditorGUILayout.TextField(unityRoot);

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
            // parse the file, this does the actual interaction with the SWF specification
            SWFFile file = SWFFile.readFull(swfPath, false);

            if (file == null) {
                Debug.LogError("The supplied SWF file does not exist or an error occured.");
                return;
            }

            // "Place" the file, this is the start of the conversion steps from SWF to Unity
            OFUCUSWF.placeNewSWFFile(file, unityRoot);
        }
    }
}
