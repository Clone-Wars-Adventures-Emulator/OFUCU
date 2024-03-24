using CWAEmu.OFUCU.Flash;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace CWAEmu.OFUCU {
    public class OFUCUSWF : MonoBehaviour {
        private RectTransform vfswfhT;
        private RectTransform dictonaryT;

        private SWFFile file;
        private string svgRoot;

        public readonly Dictionary<int, OFUCUSprite> neoSprites = new();

        public static void placeNewSWFFile(SWFFile file, string svgRoot) {
            // TODO: validate svg root

            GameObject go = new($"SWF Root: {file.Name}");
            OFUCUSWF swf = go.AddComponent<OFUCUSWF>();
            swf.svgRoot = svgRoot;
            swf.file = file;
            swf.init();



            // Debug stuff, removable
            Debug.Log($"Generated {swf.dictonaryT.childCount} objects as a child of {swf.dictonaryT.name}");
            int dt = swf.dictonaryT.childCount;
            int total = 0;
            for (int i = 0; i < dt; i++) {
                var t = swf.dictonaryT.GetChild(i);
                Debug.Log($"{t.name} generated {t.childCount} direct child objects and {t.FullChildCount()} total child objects");
                total += t.FullChildCount();
            }
            Debug.Log($"Total children of dictionary objects: {total}");
        }

        private void init() {
            Canvas canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.additionalShaderChannels = AdditionalCanvasShaderChannels.TexCoord1 | AdditionalCanvasShaderChannels.TexCoord2 | AdditionalCanvasShaderChannels.TexCoord3;

            CanvasScaler cScaler = gameObject.AddComponent<CanvasScaler>();
            cScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            cScaler.referenceResolution = new Vector2(file.FrameSize.Width, file.FrameSize.Height);
            cScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            cScaler.matchWidthOrHeight = 1.0f;
            cScaler.referencePixelsPerUnit = 100.0f;

            GraphicRaycaster raycaster = gameObject.AddComponent<GraphicRaycaster>();
            raycaster.ignoreReversedGraphics = true;
            raycaster.blockingObjects = GraphicRaycaster.BlockingObjects.None;
            // TODO: blocking mask
            // raycaster.blockingMask = 

            GameObject vfswfh = new($"VirtualFlashSWFHolder", typeof(RectTransform));
            vfswfhT = vfswfh.transform as RectTransform;

            vfswfhT.SetParent(canvas.transform, false);
            vfswfhT.sizeDelta = new Vector2(file.FrameSize.Width, file.FrameSize.Height);

            GameObject dictonaryRoot = new($"Dictonary", typeof(RectTransform));
            dictonaryT = dictonaryRoot.transform as RectTransform;
            dictonaryT.SetParent(canvas.transform, false);

            // find shape SVGs from specified output foler
            // create dictionary of all sprites and svg shapes
            // sprites need custom inspector that allows them to be made into prefabs (with further changes to the sprite by code modifying the prefab), and placed manually or animated
            
            foreach (var pair in file.Sprites) {
                GameObject go = new($"{file.Name}.Sprite.{pair.Value.CharacterId}", typeof(OFUCUSprite), typeof(RectTransform));
                RectTransform rt = go.transform as RectTransform;
                rt.SetParent(dictonaryT, false);
                var dict = go.GetComponent<OFUCUSprite>();
                dict.init(this, pair.Value);
                neoSprites.Add(pair.Key, dict);
            }
        }

        public void placeFrames(RectTransform root, List<Frame> frames) {
            DisplayList dl = new(frames);

            for (int i = 0; i < dl.frames.Length; i++) {
                DisplayFrame df = dl.frames[i];
                string name = df.label ?? $"Frame {df.frameIndex}";

                // create frame object
                GameObject frameGo = new(name, typeof(RectTransform));
                RectTransform frameRt = (RectTransform)frameGo.transform;
                frameRt.SetParent(root, false);

                RectTransform maskTrans = null;
                int maskDepth = -1;
                int curDepth = 0;

                foreach (var obj in df.states.Values.OrderBy(o => o.depth)) {
                    curDepth = obj.depth;
                    if (maskDepth > curDepth && maskTrans != null) {
                        maskTrans = null;
                    }

                    // create object (can be extracted)
                    var (go, aoo) = createObjectReference(frameRt, obj);

                    // handle all the funnies
                    RectTransform goRt = (RectTransform)go.transform;
                    goRt.SetParent(maskTrans ?? frameRt, false);

                    // handle matrix (can be extracted?? (prob not, considering anim needs to do its own thing))
                    var transform = obj.matrix.getTransformation();
                    goRt.anchoredPosition = transform.translate;
                    goRt.localScale = transform.scale.ToVector3(1);
                    goRt.rotation = Quaternion.Euler(0, 0, transform.rotz);

                    if (obj.hasClipDepth) {
                        maskDepth = obj.clipDepth;
                        maskTrans = goRt;
                        var mask = go.AddComponent<Mask>();
                        mask.showMaskGraphic = false;
                    }

                    if (obj.hasColor) {
                        var col = obj.color;
                        if (col.hasMult) {
                            aoo.setMultColor(col.mult);
                        }
                        if (col.hasAdd) {
                            aoo.setAddColor(col.add);
                        }
                    }

                    if (obj.hasName) {
                        // TODO: decide if this is an override or just an additional
                        go.name = obj.name;
                    }

                    if (obj.hasBlendMode) {
                        aoo.setBlendMode(obj.blendMode);
                    }

                    if (obj.depth < maskDepth) {
                        go.AddComponent<OFUCUAnchor>();
                    }
                }
            }
        }

        public void animateFrames(RectTransform root, List<Frame> frames) {
            // TODO: load all objects into one list, load as tuples, tuples are (int depth, TYPE initialObjectInfo, int firstFrame, int lastFrame)
            // TODO: setup the structures for the objects (eg parent the masks correctly), place all objects with their initial info, and disable them

            // TODO: create an animation clip on this object, iterate though the frames and apply delats to the clip, enable objects as they are added, disable them as they are removed

            // TODO: animate is going to be the tricky one as it needs to keep all objects around but enable/disable them when they get added/removed.
            // this will require persisting a seperate display list? and checking the frame objectAdd / objectRemove lists to know what depths changed
        }

        public int allSpritesFilled(HashSet<int> spriteIds) {
            foreach (int id in spriteIds) {
                if (!neoSprites.TryGetValue(id, out var sprite) || !sprite.Filled) {
                    return id;
                }
            }

            return 0;
        }

        private (GameObject go, AbstractOFUCUObject aoo) createObjectReference(RectTransform parent, DisplayObject obj) {
            GameObject go = null;
            AbstractOFUCUObject aoo = null;
            if (neoSprites.TryGetValue(obj.charId, out var sprite)) {
                go = sprite.getCopy();
                aoo = go.GetComponent<AbstractOFUCUObject>();
                Debug.Log($"Found {obj.charId} as sprite");
            }

            if (aoo == null) {
                string svg = $"{svgRoot}/{obj.charId}.svg";
                Debug.Log($"Looking for {obj.charId} as shape at {svg}");
                GameObject prefabGo = AssetDatabase.LoadAssetAtPath<GameObject>(svg);
                if (prefabGo == null) {
                    Debug.LogError("Failed to find svg file");
                }

                go = (GameObject)PrefabUtility.InstantiatePrefab(prefabGo, parent);
                aoo = go.AddComponent<OFUCUShape>();
            }

            // TODO: handle text objects
            if (aoo == null) {
                Debug.Log($"Not placing {obj.charId}, not shape or sprite");
            }

            return (go, aoo);
        }
    }
}
