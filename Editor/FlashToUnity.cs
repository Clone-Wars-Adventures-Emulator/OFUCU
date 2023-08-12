using CWAEmu.OFUCU.Flash;
using CWAEmu.OFUCU.Flash.Records;
using CWAEmu.OFUCU.Flash.Tags;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Color = CWAEmu.OFUCU.Flash.Records.Color;
using UObject = UnityEngine.Object;

namespace CWAEmu.OFUCU {
    public class FlashToUnityOneShot {
        private SWFFile file;

        private GameObject rootObj;
        private RectTransform vfswfhT;
        private RectTransform dictonaryT;
        private PlacedSWFFile placedSWF;

        private Dictionary<int, DictonaryEntry> dictonary = new();
        private Dictionary<int, int> usageCount = new();

        public FlashToUnityOneShot(SWFFile file, bool trimFile = false) {
            this.file = file;

            if (trimFile) {
                file.destructivelyTrimUnused();
            }

            loadDictonaryToScene();
        }

        private void loadDictonaryToScene() {
            rootObj = new($"SWF Root: {file.Name}");
            placedSWF = rootObj.AddComponent<PlacedSWFFile>();
            placedSWF.File = file;
            Canvas canvas = rootObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler cScaler = rootObj.AddComponent<CanvasScaler>();
            cScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            cScaler.referenceResolution = new Vector2(file.FrameSize.Width, file.FrameSize.Height);
            cScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            cScaler.matchWidthOrHeight = 1.0f;
            cScaler.referencePixelsPerUnit = 100.0f;

            GraphicRaycaster raycaster = rootObj.AddComponent<GraphicRaycaster>();
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

            foreach (var image in file.Images) {
                usageCount.Add(image.Key, 0);
                createImageObject(image.Key, image.Value);
            }

            foreach (var shape in file.Shapes) {
                usageCount.Add(shape.Value.CharacterId, 0);
                createShapeObject(shape.Value);
            }

            foreach (var sprite in file.Sprites) {
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
            var (rt, de) = createDictonaryEntry(shape, $"Shape {shape.CharacterId}");
            rt.SetParent(dictonaryT, false);
            rt.pivot = new Vector2(0, 1);
            rt.sizeDelta = new Vector2(shape.ShapeBounds.Width, shape.ShapeBounds.Height);

            var (_, absZeroTrans) = createUIObj($"ShapeAbsZero");
            absZeroTrans.SetParent(rt, false);
            absZeroTrans.anchorMin = new Vector2(0, 1);
            absZeroTrans.anchorMax = new Vector2(0, 1);
            absZeroTrans.pivot = new Vector2(0, 1);
            absZeroTrans.anchoredPosition = new Vector2(-shape.ShapeBounds.X, shape.ShapeBounds.Y);

            shape.iterateOnShape((fsa, lsa, fill0, fill1, line, boxPoints, shapeId) => {
                fillInShape(fsa, lsa, fill0, fill1, line, boxPoints, absZeroTrans, shapeId, de);
            });

            dictonary.Add(shape.CharacterId, de);
        }

        private void fillInShape(FillStyleArray fsa, LineStyleArray lsa, int fill0, int fill1, int line, List<Vector2> boxPoints, RectTransform absZeroTrans, int shapeId, DictonaryEntry entry) {
            // TODO: proper pathing instead of just bounds checking
            float minX = float.MaxValue;
            float minY = float.MaxValue;
            float maxX = float.MinValue;
            float maxY = float.MinValue;

            foreach (var point in boxPoints) {
                minX = Math.Min(point.x, minX);
                minY = Math.Min(point.y, minY);
                maxX = Math.Max(point.x, maxX);
                maxY = Math.Max(point.y, maxY);
            }

            // TODO: handle lines rendering lines
            if (line != -1) {
                Debug.LogError($"Attempting to render a shape ({shapeId}) with a line style specified. Unknown how to handle.");
                return;
            }

            // TODO: handle multiple fill styles being set
            if (fill0 != -1 && fill1 != -1) {
                Debug.LogError($"Attempting to render a shape ({shapeId}) with two fill styles specified. Unknown how to handle.");
                return;
            }

            FillStyle singleStyle = null;
            if (fill0 != -1) {
                singleStyle = fsa[fill0];
            }

            if (fill1 != -1) {
                singleStyle = fsa[fill1];
            }

            if (singleStyle == null) {
                Debug.LogError($"Attempting to render a shape ({shapeId}) with no fill styles specified. Unknown how to handle.");
                return;
            }

            if (singleStyle.Type == FillStyle.EnumFillStyleType.Solid) {
                Color fillColor = singleStyle.Color;
                // TODO: this fill will be using an image component and: 
                //   Leaving the sprite field blank
                //   setting the color field to the above fill color
                return;
            }

            byte fillTypeAsByte = ((byte)singleStyle.Type);
            if ((fillTypeAsByte & 0x40) != 0x40) {
                // Type is not Bitmap or some variation on that
                // TODO: handle other types of fills
                Debug.LogError($"Attempting to render a shape ({shapeId}) with fill style type ({singleStyle.Type}) that is not a bitmap. Unknown how to handle.");
                return;
            }

            if (!dictonary.ContainsKey(singleStyle.BitmapId)) {
                Debug.LogError($"Attempting to render a shape ({shapeId}) with fill image ({singleStyle.BitmapId}) that is not a in the dictonary. Parse JPEGs to fix.");
                return;
            }

            bool smoothed = (fillTypeAsByte & 0x02) == 0x02;
            bool clipped = (fillTypeAsByte & 0x01) == 0x01;

            Debug.LogWarning($"Shape rendering a fill style ({shapeId}) with bitmap types smooth:{smoothed} clipped:{clipped}");

            // duplicate the existing image object so we can possibly modify it for this sprite
            usageCount[singleStyle.BitmapId]++;
            var (rt, placed) = createDictonaryReference(dictonary[singleStyle.BitmapId]);
            entry.addDependency(singleStyle.BitmapId);
            if (rt == null) {
                Debug.LogWarning("Failure in duplication of object");
                return;
            }

            rt.SetParent(absZeroTrans, false);
            // TODO: set the smoothed and clipped props somehow
            rt.anchoredPosition = new Vector2(minX, -minY);
            rt.sizeDelta = new Vector2(Mathf.Abs(maxX - minX), Mathf.Abs(maxY - minY));
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

        private (GameObject, RectTransform) createUIObj(string name) {
            GameObject go = new(name, typeof(RectTransform));
            RectTransform rt = go.transform as RectTransform;

            return (go, rt);
        }

        public (RectTransform, DictonaryEntry) createDictonaryEntry(CharacterTag tag, string name) {
            GameObject go = new(name, typeof(RectTransform), typeof(DictonaryEntry));
            RectTransform rt = go.transform as RectTransform;
            DictonaryEntry entry = go.GetComponent<DictonaryEntry>();

            entry.containingFile = placedSWF;

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

        public (RectTransform, PlacedObject) createDictonaryReference(DictonaryEntry entry) {
            Type type = entry.CharacterType switch {
                DictonaryEntry.EnumDictonaryCharacterType.Image => typeof(PlacedImage),
                DictonaryEntry.EnumDictonaryCharacterType.Shape => typeof(PlacedShape),
                DictonaryEntry.EnumDictonaryCharacterType.Sprite => typeof(PlacedSprite),
                _ => typeof(PlacedObject)
            };

            string name = $"Placed {entry.name}";
            GameObject go = new(name, typeof(RectTransform), type);
            RectTransform rt = go.transform as RectTransform;
            PlacedObject placed = go.GetComponent<PlacedObject>();
            placed.placedEntry = entry;


            return (rt, placed);
        }
    }
}
