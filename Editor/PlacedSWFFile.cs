using CWAEmu.OFUCU.Flash;
using CWAEmu.OFUCU.Flash.Tags;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using URect = UnityEngine.Rect;

namespace CWAEmu.OFUCU {
    [ExecuteInEditMode]
    public class PlacedSWFFile : MonoBehaviour {
        public SWFFile File;

        private RectTransform vfswfhT;
        private RectTransform dictonaryT;

        public Dictionary<int, DictonaryEntry> dictonary = new();
        private Dictionary<int, int> usageCount = new();

        public static void placeNewSWFFile(SWFFile File) {
            GameObject go = new($"SWF Root: {File.Name}");
            PlacedSWFFile swf = go.AddComponent<PlacedSWFFile>();
            swf.File = File;
            swf.initialPlace();
        }

        private void initialPlace() {
            Canvas canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler cScaler = gameObject.AddComponent<CanvasScaler>();
            cScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            cScaler.referenceResolution = new Vector2(File.FrameSize.Width, File.FrameSize.Height);
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
            vfswfhT.sizeDelta = new Vector2(File.FrameSize.Width, File.FrameSize.Height);

            GameObject dictonaryRoot = new($"Dictonary", typeof(RectTransform));
            dictonaryT = dictonaryRoot.transform as RectTransform;
            dictonaryT.SetParent(canvas.transform, false);

            foreach (var image in File.Images) {
                usageCount.Add(image.Key, 0);
                createImageObject(image.Key, image.Value);
            }

            foreach (var shape in File.Shapes) {
                usageCount.Add(shape.Value.CharacterId, 0);
                createShapeObject(shape.Value);
            }

            foreach (var sprite in File.Sprites) {
                usageCount.Add(sprite.Value.CharacterId, 0);
                createSpriteObject(sprite.Value);
            }
        }

        private void createImageObject(int charId, ImageCharacterTag image) {
            var (rt, de) = createDictonaryEntry(image, $"Image {charId}");

            rt.SetParent(dictonaryT, false);

            rt.pivot = new Vector2(0, 1);
            rt.sizeDelta = new Vector2(image.Image.Width, image.Image.Height);

            dictonary.Add(charId, de);
        }

        private void createShapeObject(DefineShape shape) {
            var (rootRt, de) = createDictonaryEntry(shape, $"Shape {shape.CharacterId}");
            rootRt.SetParent(dictonaryT, false);
            rootRt.pivot = new Vector2(0, 1);
            rootRt.sizeDelta = new Vector2(shape.ShapeBounds.Width, shape.ShapeBounds.Height);

            void onBitmapFill(URect extends, ushort bitmapId, bool smooth, bool clipped) {
                if (!dictonary.ContainsKey(bitmapId)) {
                    Debug.LogError($"Attempting to render a shape ({shape.CharacterId}) with fill image ({bitmapId}) that is not a in the dictonary.");
                    return;
                }

                // duplicate the existing image object so we can possibly modify it for this sprite
                usageCount[bitmapId]++;
                var (rt, placed) = createDictonaryReference(dictonary[bitmapId]);
                de.addDependency(bitmapId);

                rt.SetParent(rootRt, false);
                rt.pivot = new Vector2(0, 1);
                rt.anchorMin = new Vector2(0, 1);
                rt.anchorMax = new Vector2(0, 1);
                rt.anchoredPosition = new Vector2(extends.xMin, -extends.yMin);
                rt.sizeDelta = new Vector2(Mathf.Abs(extends.xMax - extends.xMin), Mathf.Abs(extends.yMax - extends.yMin));
            }

            shape.iterateOnShapeFill(onBitmapFill, null, null);

            dictonary.Add(shape.CharacterId, de);
        }

        private void createSpriteObject(DefineSprite sprite) {
            // TODO: uncomment when names are added
            // string nameSuffix = sprite.name;
            string spriteName = $"Sprite {sprite.CharacterId}";
            if (sprite.Frames.Count == 0) {
                Debug.Log($"Trimming {spriteName} with no frames (assumed to be script only).");
                return;
            }

            var (rt, de) = createDictonaryEntry(sprite, spriteName);
            rt.SetParent(dictonaryT, false);
            rt.pivot = new Vector2(0, 1);


            foreach (Frame frame in sprite.Frames) {
                foreach (FlashTag tag in frame.Tags) {
                    if (tag is PlaceObject2 po2 && po2.HasCharacter) {
                        if (dictonary.TryGetValue(po2.CharacterId, out var dictE) && dictE.CharacterType == DictonaryEntry.EnumDictonaryCharacterType.Image) {
                            // this should never happen, but just in case
                            Debug.LogWarning($"Sprite {spriteName} directly relys on image {po2.CharacterId}. This is not good.");
                        }

                        de.addDependency(po2.CharacterId);
                    }
                }
            }

            dictonary.Add(sprite.CharacterId, de);
        }

        private void createEditTextObject(DefineEditText det) {

        }

        public (GameObject, RectTransform) createUIObj(string name) {
            GameObject go = new(name, typeof(RectTransform));
            RectTransform rt = go.transform as RectTransform;

            return (go, rt);
        }

        public (RectTransform, DictonaryEntry) createDictonaryEntry(CharacterTag tag, string name) {
            GameObject go = new(name, typeof(RectTransform), typeof(DictonaryEntry));
            RectTransform rt = go.transform as RectTransform;
            DictonaryEntry entry = go.GetComponent<DictonaryEntry>();

            entry.containingFile = this;

            entry.rt = rt;
            entry.charTag = tag;
            DictonaryEntry.EnumDictonaryCharacterType type = DictonaryEntry.EnumDictonaryCharacterType.Shape;
            if (tag is DefineSprite) {
                type = DictonaryEntry.EnumDictonaryCharacterType.Sprite;
            }
            if (tag is ImageCharacterTag) {
                type = DictonaryEntry.EnumDictonaryCharacterType.Image;
                entry.image = (tag as ImageCharacterTag).Image;
            }
            entry.CharacterType = type;

            return (rt, entry);
        }

        public (RectTransform, PlacedObject) createDictonaryReference(DictonaryEntry entry, bool fillIn = false) {
            Type type = entry.CharacterType switch {
                DictonaryEntry.EnumDictonaryCharacterType.Image => typeof(PlacedImage),
                DictonaryEntry.EnumDictonaryCharacterType.Shape => typeof(PlacedShape),
                DictonaryEntry.EnumDictonaryCharacterType.Sprite => typeof(PlacedSprite),
                _ => typeof(PlacedObject)
            };

            if (fillIn && entry.CharacterType != DictonaryEntry.EnumDictonaryCharacterType.Image) {
                return copyDictEntryAsReference(entry, type);
            }

            string name = $"Placed {entry.name}";
            GameObject go = new(name, typeof(RectTransform), type);
            RectTransform rt = go.transform as RectTransform;
            PlacedObject placed = go.GetComponent<PlacedObject>();
            placed.placedEntry = entry;

            return (rt, placed);
        }

        private (RectTransform, PlacedObject) copyDictEntryAsReference(DictonaryEntry entry, Type type) {
            GameObject go = Instantiate(entry.gameObject);
            go.name = $"Placed {go.name}";
            if (go.TryGetComponent(out DictonaryEntry de)) {
                DestroyImmediate(de);
            }
            PlacedObject po = go.AddComponent(type) as PlacedObject;
            po.placedEntry = entry;

            if (!go.TryGetComponent(out RectTransform rt)) {
                rt = go.AddComponent<RectTransform>();
            }

            if (po is not PlacedShape) {
                rt.anchorMin = new Vector2(0, 1);
                rt.anchorMax = new Vector2(0, 1);
                rt.pivot = new Vector2(0, 1);
            }

            return (rt, po);
        }

        public void runOnAllOfType(Action<DictonaryEntry> action, DictonaryEntry.EnumDictonaryCharacterType type) {
            foreach (var pair in dictonary) {
                if (pair.Value.CharacterType == type) {
                    action(pair.Value);
                }
            }
        }

        public void placeFrames(RectTransform rt, List<Frame> frames, bool translateFrames = false) {
            // nuke the child frames
            foreach (Transform t in rt) {
                DestroyImmediate(t.gameObject);
            }

            UFrameList frameList = UFrame.toDeltaList(frames);

            Dictionary<int, GameObject> previousObject = new();
            for (int i = 0; i < frameList.frameCount; i++) {
                var (_, frameRt) = createUIObj($"Frame {i}");
                frameRt.SetParent(rt, false);

                if (translateFrames) {
                    frameRt.anchorMin = new Vector2(0, 1);
                    frameRt.anchorMax = new Vector2(0, 1);
                    frameRt.anchoredPosition = new Vector2();
                }

                Dictionary<int, UFrameObject> displaylist = frameList.frames[i];
                List<int> depths = displaylist.Keys.ToList();
                depths.Sort();

                foreach (int depth in depths) {
                    UFrameObject obj = displaylist[depth];

                    switch (obj.type) {
                        case EnumUFrameObjectType.PlaceRemove:
                            if (previousObject.ContainsKey(depth)) {
                                previousObject.Remove(depth);
                            }
                            // META: i really hate how you have to explicitly say fall through like this
                            goto case EnumUFrameObjectType.Place;
                        case EnumUFrameObjectType.Place:
                            GameObject pfo = placeFrameObject(frameRt, obj);
                            previousObject.Add(depth, pfo);


                            break;
                        case EnumUFrameObjectType.Modify:

                            break;
                        case EnumUFrameObjectType.Remove:
                            if (previousObject.ContainsKey(depth)) {
                                previousObject.Remove(depth);
                            }
                            break;
                    }
                }
            }
        }

        public void animateFrames(RectTransform rt, List<Frame> frames) {

        }

        public void placeSWFFrames() {
            placeFrames(vfswfhT, File.Frames, true);
        }

        public void animateSWFFrames() {
            animateFrames(vfswfhT, File.Frames);
        }

        public GameObject placeFrameObject(RectTransform parent, UFrameObject obj) {
            if (!dictonary.ContainsKey(obj.charId)) {
                Debug.LogError($"Character {obj.charId} is not in the dictonary, skipping reference.");
                return null;
            }

            var (drRt, po) = createDictonaryReference(dictonary[obj.charId], true);

            drRt.SetParent(parent, false);
            if (obj.name != null) {
                drRt.name = obj.name;
            }

            UMatrix mat = obj.matrix;
            if (mat != null) {
                var (pos, scale, rot) = mat.getTransformation();

                drRt.anchoredPosition = pos;
                drRt.localScale = new Vector3(scale.x, scale.y, 1);
                drRt.rotation = Quaternion.Euler(0, 0, rot);
            }


            // TODO: handle the following properties
            // color transform
            // clip depth
            // blend mode

            return drRt.gameObject;
        }

        public GameObject modifyFrameObject(GameObject obj, UFrameObject delta) {
            // TODO: 
            return null;
        }
    }
}
