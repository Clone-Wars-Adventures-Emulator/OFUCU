using CWAEmu.OFUCU.Data;
using CWAEmu.OFUCU.Flash.Tags;
using System.Collections.Generic;
using System.IO;
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

        public void place() {
            // check if dependencies are filled, if not, dont do this
            var dep = swf.allSpritesFilled(dependencies);
            if (dep != 0) {
                Debug.LogError($"Not placing {sprite.CharacterId}, sprite {dep} is not filled.");
                return;
            }

            swf.placeFrames(transform as RectTransform, sprite.Frames);
            sprite = null;
            filled = true;
        }

        // TODO: popup window that takes in what ever params i want (this is future)
        public void animate() {
            // check if dependencies are filled, if not, dont do this
            var dep = swf.allSpritesFilled(dependencies);
            if (dep != 0) {
                Debug.LogError($"Not animating {sprite.CharacterId}, sprite {dep} is not filled.");
                return;
            }

            swf.animateFrames(transform as RectTransform, sprite.Frames);
            filled = true;
        }

        public void saveAsPrefab() {
            if (prefabAssetPath == null) {
                if (!Directory.Exists(Settings.Instance.DefaultPrefabDir)) {
                    Directory.CreateDirectory(Settings.Instance.DefaultPrefabDir);
                }
                prefabAssetPath = $"{Settings.Instance.DefaultPrefabDir}/{name}.prefab";
                PrefabUtility.SaveAsPrefabAsset(gameObject, prefabAssetPath);
            } else {
                PrefabUtility.SavePrefabAsset(gameObject);
            }
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

        public override void setBlendMode(EnumFlashBlendMode mode) {
            // TODO: 
        }

        public override void setMultColor(Color color) {
            // TODO: 
        }

        public override void setAddColor(Color color) {
            // TODO: 
        }
    }
}
