using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace CWAEmu.OFUCU {
    public class AssetCleaner : AssetPostprocessor {
        [PostProcessScene]
        public static void PostProcessScene() {
            var thing = Object.FindObjectsOfType<AbstractOFUCUObject>();
            foreach (var obj in thing) {
                Object.DestroyImmediate(obj);
            }
        }

        private static bool enableDeletion;

        [MenuItem("OFUCU/Clean Scripts from Prefabs")]
        public static void deleteAbstractOFUCUScripts() {
            enableDeletion = true;
            reimportAllPrefabs();
            enableDeletion = false;
        }

        [MenuItem("OFUCU/Restore Scripts to Prefabs")]
        public static void reimportAllPrefabs() {
            try {
                AssetDatabase.StartAssetEditing();

                var guids = AssetDatabase.FindAssets("t:prefab");
                foreach (var guid in guids) {
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                }
            } catch (System.Exception e) {
                Debug.LogException(e);
            } finally {
                AssetDatabase.StopAssetEditing();
            }
        }

        private void OnPostprocessPrefab(GameObject gameObject) {
            if (!enableDeletion) {
                return;
            }

            var swfs = gameObject.GetComponents<OFUCUSWF>();
            foreach (var swf in swfs) {
                Object.DestroyImmediate(swf, true);
            }

            var comps = gameObject.GetComponents<AbstractOFUCUObject>();
            foreach (var comp in comps) {
                Object.DestroyImmediate(comp, true);
            }

            var absObjs = gameObject.GetComponentsInChildren<AbstractOFUCUObject>(true);
            foreach (var abs in absObjs) {
                Object.DestroyImmediate(abs, true);
            }
        }
    }
}
