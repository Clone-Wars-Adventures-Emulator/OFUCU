using CWAEmu.OFUCU.Flash;
using CWAEmu.OFUCU.Flash.Records;
using CWAEmu.OFUCU.Flash.Tags;
using CWAEmu.OFUCU.Runtime;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace CWAEmu.OFUCU {
    [RequireComponent(typeof(RectTransform), typeof(RuntimeButton2))]
    public class OFUCUButton2 : AbstractOFUCUObject {
        // based off of the bool states in BUTTONRECORD
        private enum EnumButtonState {
            Up,
            Over,
            Down
        }

        // inited
        [SerializeField]
        private OFUCUSWF swf;
        [SerializeField]
        private DefineButton2 button;
        [SerializeField]
        private string prefabSaveDir;
        [SerializeField]
        private string matSaveDir;

        // generated
        [SerializeField]
        private string prefabAssetPath;
        public bool HasPrefab => prefabAssetPath != null;

        public void init(OFUCUSWF swf, DefineButton2 button, string prefabSaveDir, string matSaveDir) {
            this.swf = swf;
            this.button = button;
            this.prefabSaveDir = prefabSaveDir;
            this.matSaveDir = matSaveDir;

            Dictionary<EnumButtonState, List<ButtonRecord>> typeDict = new() {
                { EnumButtonState.Up, new() },
                { EnumButtonState.Over, new() },
                { EnumButtonState.Down, new() },
            };

            foreach (var record in button.ButtonRecords) {
                if (record.StateUp) {
                    typeDict[EnumButtonState.Up].Add(record);
                }
                if (record.StateOver) {
                    typeDict[EnumButtonState.Over].Add(record);
                }
                if (record.StateDown) {
                    typeDict[EnumButtonState.Down].Add(record);
                }
            }

            foreach (var pair in typeDict) {
                var stateGo = new GameObject(pair.Key.ToString(), typeof(RectTransform));
                stateGo.transform.SetParent(transform, false);

                var arr = pair.Value.ToArray();
                Array.Sort(arr, (a, b) => a.PlaceDepth.CompareTo(b.PlaceDepth));

                foreach (var record in arr) {
                    var (go, _, ro) = swf.createObjectReference(stateGo.transform as RectTransform, record.CharacterId);
                    var goRt = go.transform as RectTransform;

                    var (translate, scale, rotz) = Matrix2x3.FromFlash(record.Matrix).getTransformation();
                    goRt.anchoredPosition = translate;
                    goRt.localScale = scale.ToVector3(1);
                    goRt.rotation = Quaternion.Euler(0, 0, rotz);

                    var cxform = ColorTransform.FromFlash(record.ColorTransform);
                    // not the biggest fan of having to do this i dont think? may its fine
                    ro.initReferences();
                    if (record.ColorTransform.HasAdd) {
                        ro.setAddColor(cxform.add);
                    }
                    if (record.ColorTransform.HasMult) {
                        ro.setMultColor(cxform.mult);
                    }
                }
            }
            // TODO: HIT BOXES?
        }

        public override void setBlendMode(EnumFlashBlendMode blendMode, string saveFolder, string path) {
            Debug.LogError("Unimplemented for buttons");
        }

        public void saveAsPrefab() {
            // TODO: the implementations of these methods across all instances are wrong. They should check for the asset and use that as the condition
            // This would prevent needing to reload all the time....
            if (string.IsNullOrEmpty(prefabAssetPath)) {
                if (!Directory.Exists(prefabSaveDir)) {
                    Directory.CreateDirectory(prefabSaveDir);
                }
                prefabAssetPath = $"{prefabSaveDir}/{name}.prefab";
                PrefabUtility.SaveAsPrefabAssetAndConnect(gameObject, prefabAssetPath, InteractionMode.AutomatedAction);
            } else {
                Debug.LogWarning($"{name} already has a prefab at path {prefabAssetPath}, modify that directly");
                // PrefabUtility.SavePrefabAsset(gameObject);
            }
        }

        public GameObject getCopy() {
            if (prefabAssetPath != null) {
                GameObject pgo = AssetDatabase.LoadAssetAtPath<GameObject>(prefabAssetPath);
                GameObject rgo = (GameObject) PrefabUtility.InstantiatePrefab(pgo);

                return rgo;
            }

            GameObject go = Instantiate(gameObject);

            return go;
        }
    }
}
