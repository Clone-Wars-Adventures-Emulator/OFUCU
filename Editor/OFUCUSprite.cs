using CWAEmu.OFUCU.Flash.Tags;
using CWAEmu.OFUCU.Runtime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.VectorGraphics;
using UnityEditor;
using UnityEngine;

namespace CWAEmu.OFUCU {
    [RequireComponent(typeof(RectTransform), typeof(RuntimeSprite))]
    public class OFUCUSprite : AbstractOFUCUObject {
        // inited
        [SerializeField]
        private OFUCUSWF swf;
        [SerializeField]
        private DefineSprite sprite;
        [SerializeField]
        private string prefabSaveDir;
        [SerializeField]
        private string matSaveDir;

        // generated
        [SerializeField]
        private string prefabAssetPath;
        public bool HasPrefab => prefabAssetPath != null;

        public bool Filled => filled;
        [SerializeField]
        private bool filled;

        [SerializeField]
        private HashSet<int> dependencies = new();

        private AbstractOFUCUObject[] children = new AbstractOFUCUObject[0];

        private void Awake() {
            loadChildren();
        }

        public void init(OFUCUSWF swf, DefineSprite sprite, string prefabSaveDir, string matSaveDir) {
            this.swf = swf;
            this.sprite = sprite;
            this.prefabSaveDir = prefabSaveDir;
            this.matSaveDir = matSaveDir;

            foreach (var f in sprite.Frames) {
                foreach (var t in f.Tags) {
                    if (t is PlaceObject2 po2 && po2.HasCharacter) {
                        dependencies.Add(po2.CharacterId);
                    }
                }
            }
        }

        public void place(bool forceDeps = false, bool ignoreMissing = false) {
            if (forceDeps) {
                swf.placeFrames(transform as RectTransform, sprite.Frames, dependencies, missingIsError: !ignoreMissing);

                return;
            }

            // check if dependencies are filled, if not, dont do this
            var dep = swf.allSpritesFilled(dependencies);
            if (dep != 0 && !ignoreMissing) {
                Debug.LogError($"Not placing {sprite.CharacterId}, sprite {dep} is not filled.");
                return;
            }

            swf.placeFrames(transform as RectTransform, sprite.Frames, missingIsError: !ignoreMissing);
            filled = true;

            loadChildren();
        }

        public void animate() {
            // check if dependencies are filled, if not, dont do this
            var dep = swf.allSpritesFilled(dependencies);
            if (dep != 0) {
                Debug.LogError($"Not animating {sprite.CharacterId}, sprite {dep} is not filled.");
                return;
            }

            swf.animateFrames(transform as RectTransform, sprite.Frames);
            filled = true;

            loadChildren();
        }

        public void uniquifyMaterials() {
            try {
                AssetDatabase.StartAssetEditing();
                recurseMats(transform, name);
                if (prefabAssetPath != null) {
                    Debug.Log($"{name} thinks it has PAP at {prefabAssetPath}");
                    PrefabUtility.SavePrefabAsset(gameObject);
                }
            } catch (Exception e) {
                Debug.LogException(e);
            } finally {
                AssetDatabase.StopAssetEditing();
            }
        }

        private void recurseMats(Transform cur, string path) {
            if (cur.TryGetComponent<SVGImage>(out var img)) {
                var mat = new Material(img.material);
                img.material = mat;

                if (!Directory.Exists(matSaveDir)) {
                    Directory.CreateDirectory(matSaveDir);
                }
                AssetDatabase.CreateAsset(mat, $"{matSaveDir}/{path}.mat");
            }

            for (int i = 0; i < cur.childCount; i++) {
                var child = cur.GetChild(i);

                recurseMats(child, $"{path}~{child.name}");
            }
        }

        public void saveAsPrefab() {
            if (prefabAssetPath == null) {
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
                GameObject rgo = (GameObject)PrefabUtility.InstantiatePrefab(pgo);

                return rgo;
            }

            GameObject go = Instantiate(gameObject);

            return go;
        }

        private void loadChildren() {
            var objects = gameObject.GetComponentsInChildren<AbstractOFUCUObject>();
            HashSet<AbstractOFUCUObject> objs = new();
            foreach (var obj in objects) {
                if (obj != this) {
                    objs.Add(obj);
                }
            }
            children = objs.ToArray();
        }

        // This stays here because this isnt runtime
        public override void setBlendMode(EnumFlashBlendMode mode, string saveFolder, string path) {
            loadChildren();
            foreach (var obj in children) {
                obj.setBlendMode(mode, saveFolder, $"{path}~{name}");
            }
        }
    }
}
