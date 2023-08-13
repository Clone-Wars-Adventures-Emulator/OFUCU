using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace CWAEmu.OFUCU {
    public class AssetPathModal : EditorWindow {
        private const string BaseAssetPath = "Assets/SWFExport";
        private static Dictionary<string, string> fileToPath = new();

        private string curPath;
        private string swfName;
        private Action<string> callback;

        public static void ShowModal(string swfName, Action<string> onClose) {
            AssetPathModal window = CreateInstance<AssetPathModal>();
            window.position = new Rect(Screen.width / 2, Screen.height / 2, 250, 150);
            window.swfName = swfName;
            window.curPath = $"{BaseAssetPath}/{Path.GetFileName(swfName)}";
            window.callback = onClose;
            window.Show();
        }

        public void OnGUI() {
            // TODO: update syling of this
            EditorGUILayout.LabelField("Image export path: ");

            curPath = EditorGUILayout.TextField(curPath);

            // TODO: not working as intended, find out why and fix
            /*
            bool last = fileToPath.ContainsKey(curPath);
            bool toggled = GUILayout.Toggle(last, $"Dont ask again for {swfName}.");
            if (last != toggled) {
                if (toggled && !last) {
                    fileToPath.Add(swfName, curPath);
                } else {
                    fileToPath.Remove(swfName);
                }
            }
            */

            if (GUILayout.Button("Confirm")) {
                Close();
                callback(curPath);
            }
        }
    }
}
