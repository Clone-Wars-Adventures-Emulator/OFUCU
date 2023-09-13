using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace CWAEmu.OFUCU.Data {
    [CustomEditor(typeof(Settings))]
    public class SettingsEditor : Editor {

        public override void OnInspectorGUI() {
            // TODO: use inspector gui or use the UXML stuff
        }

        public static SettingsProvider CreateSettingsProvider() {
            Settings.Instance.load();

            // TODO: settings provider

            return null;
        }
    }

    public class Settings : ScriptableObject {
        private const string FilePath = "ProjectSettings/OFUCUSettings.asset";

        private static Settings instance;
        public static Settings Instance {
            get {
                if (instance == null) {
                    instance = CreateInstance<Settings>();
                    instance.load();
                }
                return instance;
            }
        }

        // = = = = = = = = = = Settings Fields = = = = = = = = = = 

        public string defaultExportDir;
        public string defaultPrefabDir;
        public bool inDepthLogging;

        // = = = = = = = = = = END Settings Fields = = = = = = = = = = 

        public void load() {
            if (!File.Exists(FilePath)) {
                loadDefaults();
                return;
            }

            try {
                string jsonText = File.ReadAllText(FilePath);
                EditorJsonUtility.FromJsonOverwrite(jsonText, this);
            } catch (Exception e) {
                Debug.LogException(e);
                loadDefaults();
            }
        }

        private void loadDefaults() {
            defaultExportDir = "Assets/SWFExport";
            defaultPrefabDir = "Assets/SWFPrefab";
            inDepthLogging = false;
        }

        private void save() {
            string dirName = Path.GetDirectoryName(FilePath);
            if (!Directory.Exists(dirName)) {
                Directory.CreateDirectory(dirName);
            }
            File.WriteAllText(FilePath, EditorJsonUtility.ToJson(this, true));
        }
    }
}
