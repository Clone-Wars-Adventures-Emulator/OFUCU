using CWAEmu.OFUCU.Data;
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace CWAEmu.OFUCU {
    public class AssetPathModal : EditorWindow {
        private string curPath;
        private string swfName;
        private bool dontAsk;
        private Action<string> callback;

        public static void ShowModal(string swfName, Action<string> onClose) {
            string shortSwfName = Path.GetFileName(swfName);
            if (PersistentData.Instance.askExportForSwf(shortSwfName)) {
                onClose(PersistentData.Instance.getSwfExportDir(shortSwfName));
                return;
            }

            AssetPathModal window = CreateInstance<AssetPathModal>();
            window.position = new Rect(Screen.width / 2, Screen.height / 2, 250, 150);
            window.swfName = shortSwfName;
            window.curPath = PersistentData.Instance.getSwfExportDir(shortSwfName);
            window.callback = onClose;
            window.Show();
        }

        public void OnGUI() {
            // TODO: update syling of this
            EditorGUILayout.LabelField("Image export path: ");

            curPath = EditorGUILayout.TextField(curPath);

            dontAsk = GUILayout.Toggle(dontAsk, $"Dont ask again for {swfName}.");

            if (GUILayout.Button("Confirm")) {
                Close();

                PersistentData.Instance.setSwfExportDir(swfName, curPath, true);
                if (dontAsk) {
                    PersistentData.Instance.setDontAskForSwf(swfName);
                }

                callback(curPath);
            }
        }
    }
}
