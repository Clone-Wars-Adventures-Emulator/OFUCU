using CWAEmu.OFUCU.Data;
using CWAEmu.OFUCU.Flash.Tags;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace CWAEmu.OFUCU {
    [RequireComponent(typeof(RectTransform))]
    public class OFUCUSprite : AbstractOFUCUObject {
        // inited
        private OFUCUSWF swf;
        private DefineSprite sprite;

        // generated
        private string prefabAssetPath;
        public bool Cloned => cloned;
        private bool cloned;
        public bool Filled => filled;
        private bool filled;

        private readonly HashSet<int> dependencies = new();

        // TODO: Alpha handling
        private Color parentMult = new(1, 1, 1, 1);
        private Color parentAdd = new(0, 0, 0, 0);
        private Color selfMult = new(1, 1, 1, 1);
        private Color selfAdd = new(0, 0, 0, 0);

        private AbstractOFUCUObject[] children = new AbstractOFUCUObject[0];

        private void Awake() {
            loadChildren();
        }

        public void init(OFUCUSWF swf, DefineSprite sprite) {
            this.swf = swf;
            this.sprite = sprite;

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
                if (!Directory.Exists(Settings.Instance.DefaultPrefabDir)) {
                    Directory.CreateDirectory(Settings.Instance.DefaultPrefabDir);
                }
                prefabAssetPath = $"{Settings.Instance.DefaultPrefabDir}/{name}.prefab";
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

        public override void setBlendMode(EnumFlashBlendMode mode) {
            foreach (var obj in children) {
                obj.setBlendMode(mode);
            }
        }

        public override void setMultColor(Color color) {
            selfMult = color;
            applyMultToChildren();
        }

        public override void setAddColor(Color color) {
            selfAdd = color;
            applyAddToChildren();
        }

        public override void setParentMultColor(Color color) {
            parentMult = color;
            applyMultToChildren();
        }

        public override void setParentAddColor(Color color) {
            parentAdd = color;
            applyAddToChildren();
        }

        private void applyMultToChildren() {
            var r = Mathf.Clamp(parentMult.r * selfMult.r, 0, 1);
            var g = Mathf.Clamp(parentMult.g * selfMult.g, 0, 1);
            var b = Mathf.Clamp(parentMult.b * selfMult.b, 0, 1);
            var a = Mathf.Clamp(parentMult.a * selfMult.a, 0, 1);
            var res = new Color(r, g, b, a);

            foreach (var obj in children) {
                obj.setParentMultColor(res);
            }
        }

        private void applyAddToChildren() {
            var r = Mathf.Clamp(parentAdd.r + selfAdd.r, 0, 1);
            var g = Mathf.Clamp(parentAdd.g + selfAdd.g, 0, 1);
            var b = Mathf.Clamp(parentAdd.b + selfAdd.b, 0, 1);
            var a = Mathf.Clamp(parentAdd.a + selfAdd.a, 0, 1);
            var res = new Color(r, g, b, a);

            foreach (var obj in children) {
                obj.setParentAddColor(res);
            }
        }
    }
}
