using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace CWAEmu.OFUCU.Data {
    public class SettingsEditor {
        private static Vector2 scrollPos = Vector2.zero;
        [SettingsProvider]
        public static SettingsProvider CreateSettingsProvider() {
            Settings.Instance.load();

            var provider = new SettingsProvider("Project/OFUCU Settings", SettingsScope.Project) {
                label = "OFUCU Settings",
                guiHandler = context => {
                    SerializedObject settings = new(Settings.Instance);

                    // TODO: fix up styling (possibly switch to UXML instead of IMGUI?)
                    scrollPos = GUILayout.BeginScrollView(scrollPos);
                    GUILayout.BeginVertical();

                    EditorGUILayout.PropertyField(settings.FindProperty("defaultExportDir"));
                    EditorGUILayout.PropertyField(settings.FindProperty("defaultPrefabDir"));
                    EditorGUILayout.PropertyField(settings.FindProperty("inDepthLogging"));

                    GUILayout.EndVertical();
                    GUILayout.EndScrollView();

                    settings.ApplyModifiedPropertiesWithoutUndo();
                },
                keywords = new HashSet<string> { "OFUCU", "Flash" }
            };

            return provider;
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
