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

                    EditorGUILayout.PropertyField(settings.FindProperty("defaultExportDir"), new GUIContent("Default image Export dir [DEPRECATED]"));
                    EditorGUILayout.PropertyField(settings.FindProperty("defaultPrefabDir"), new GUIContent("Default prefab Export dir [DEPRECATED]"));
                    EditorGUILayout.PropertyField(settings.FindProperty("inDepthLogging"), new GUIContent("Enhanced Plugin Logging"));

                    GUILayout.EndVertical();
                    GUILayout.EndScrollView();

                    if (settings.hasModifiedProperties) {
                        settings.ApplyModifiedPropertiesWithoutUndo();
                        Debug.Log("Modifying saved settings object");
                        Settings.Instance.save();
                    }
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

        public string DefaultExportDir => defaultExportDir;
        [SerializeField]
        private string defaultExportDir;

        public string DefaultPrefabDir => defaultPrefabDir;
        [SerializeField]
        private string defaultPrefabDir;

        public bool EnhancedLogging => inDepthLogging;
        [SerializeField]
        private bool inDepthLogging;

        // = = = = = = = = = = END Settings Fields = = = = = = = = = = 
        private void initDefaults() {
            defaultExportDir = "Assets/SWFExport";
            defaultPrefabDir = "Assets/SWFPrefab";
            inDepthLogging = false;
        }

        private void initNullable() {

        }

        public void load() {
            if (!File.Exists(FilePath)) {
                initDefaults();
                return;
            }

            try {
                string jsonText = File.ReadAllText(FilePath);
                EditorJsonUtility.FromJsonOverwrite(jsonText, this);
            } catch (Exception e) {
                Debug.LogException(e);
                initDefaults();
            }

            initNullable();
        }

        internal void save() {
            string dirName = Path.GetDirectoryName(FilePath);
            if (!Directory.Exists(dirName)) {
                Directory.CreateDirectory(dirName);
            }
            File.WriteAllText(FilePath, EditorJsonUtility.ToJson(this, true));
        }
    }
}
