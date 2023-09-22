using System.IO;
using System;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace CWAEmu.OFUCU.Data {
    [Serializable]
    public class PersistentData : ScriptableObject {
        private const string FilePath = "Assets/OFUCU/Persistent.asset";

        private static PersistentData instance;
        public static PersistentData Instance {
            get {
                if (instance == null) {
                    instance = CreateInstance<PersistentData>();
                    instance.load();
                }
                return instance;
            }
        }

        #region DataFields

        public HashSet<string> exportDontAskSwfs = new();
        public Dictionary<string, string> swfToExportDir = new();
        public Dictionary<string, string> swfToPrefabDir = new();

        // TODO: track each indivudal export and prefab or dynamically generate that list

        #endregion DataFields

        #region Accessors

        public string getSwfExportDir(string fileName) {
            if (swfToExportDir.TryGetValue(fileName, out string dir)) {
                return dir;
            }

            return Settings.Instance.defaultExportDir + "/" + fileName;
        }

        public void setSwfExportDir(string fileName, string path, bool save = false) {
            swfToExportDir[fileName] = path;
            if (save) {
                this.save();
            }
        }

        public string getSwfPrefabDir(string fileName) {
            if (swfToPrefabDir.TryGetValue(fileName, out string dir)) {
                return dir;
            }

            return Settings.Instance.defaultPrefabDir + "/" + fileName;
        }

        public void setSwfPrefabDir(string fileName, string path, bool save = false) {
            swfToPrefabDir[fileName] = path;
            if (save) {
                this.save();
            }
        }

        public bool askExportForSwf(string fileName) {
            return exportDontAskSwfs.Contains(fileName);
        }

        public void setDontAskForSwf(string fileName) {
            exportDontAskSwfs.Add(fileName);
            save();
        }

        #endregion Accessors

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
            exportDontAskSwfs = new();
            swfToExportDir = new();
            swfToPrefabDir = new();
        }

        public void save() {
            string dirName = Path.GetDirectoryName(FilePath);
            if (!Directory.Exists(dirName)) {
                Directory.CreateDirectory(dirName);
            }
            File.WriteAllText(FilePath, EditorJsonUtility.ToJson(this, true));
        }
    }
}
