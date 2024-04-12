using CWAEmu.OFUCU.Data;
using CWAEmu.OFUCU.Flash.Tags;
using CWAEmu.OFUCU.Runtime;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace CWAEmu.OFUCU {
    [RequireComponent(typeof(RectTransform), typeof(RuntimeSprite))]
    public class OFUCUSprite : AbstractOFUCUObject {
        // inited
        private OFUCUSWF swf;
        private DefineSprite sprite;

        // generated
        private string prefabSaveDir;
        private string prefabAssetPath;
        public bool Cloned => cloned;
        private bool cloned;
        public bool Filled => filled;
        private bool filled;

        private readonly HashSet<int> dependencies = new();

        private AbstractOFUCUObject[] children = new AbstractOFUCUObject[0];

        private void Awake() {
            loadChildren();
        }

        public void init(OFUCUSWF swf, DefineSprite sprite, string prefabSaveDir) {
            this.swf = swf;
            this.sprite = sprite;
            this.prefabSaveDir = prefabSaveDir;

            foreach (var f in sprite.Frames) {
                foreach (var t in f.Tags) {
                    if (t is PlaceObject2 po2 && po2.HasCharacter) {
                        dependencies.Add(po2.CharacterId);
                    }
                }
            }
        }

        public void place(bool forceDeps = false) {
            if (forceDeps) {
                swf.placeFrames(transform as RectTransform, sprite.Frames, dependencies);

                return;
            }

            // check if dependencies are filled, if not, dont do this
            var dep = swf.allSpritesFilled(dependencies);
            if (dep != 0) {
                Debug.LogError($"Not placing {sprite.CharacterId}, sprite {dep} is not filled.");
                return;
            }

            swf.placeFrames(transform as RectTransform, sprite.Frames);
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

        public void saveAsPrefab() {
            if (cloned) {
                Debug.LogError("Cannot save a clone as a prefab");
                return;
            }

            if (prefabAssetPath == null) {
                if (!Directory.Exists(prefabSaveDir)) {
                    Directory.CreateDirectory(prefabSaveDir);
                }
                prefabAssetPath = $"{prefabSaveDir}/{name}.prefab";
                PrefabUtility.SaveAsPrefabAsset(gameObject, prefabAssetPath);
            } else {
                PrefabUtility.SavePrefabAsset(gameObject);
            }

            // TODO: reload all copies
        }

        public GameObject getCopy() {
            if (prefabAssetPath != null) {
                GameObject pgo = AssetDatabase.LoadAssetAtPath<GameObject>(prefabAssetPath);
                GameObject rgo = (GameObject)PrefabUtility.InstantiatePrefab(pgo);
                rgo.GetComponent<OFUCUSprite>().cloned = true;

                return rgo;
            }

            GameObject go = Instantiate(gameObject);
            go.GetComponent<OFUCUSprite>().cloned = true;

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
        public override void setBlendMode(EnumFlashBlendMode mode) {
            foreach (var obj in children) {
                obj.setBlendMode(mode);
            }
        }
    }
}
