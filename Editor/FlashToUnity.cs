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

        private Dictionary<int, (GameObject go, RectTransform rt)> dictonary = new();
        private Dictionary<int, int> usageCount = new();

        public FlashToUnityOneShot(SWFFile file, bool trimFile = false) {
            this.file = file;

            if (trimFile) {
                file.destructivelyTrimUnused();
            }

            loadSwfFileInScene();
        }

        private void loadSwfFileInScene() {
            rootObj = new($"SWF Root: {file.Name}");
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

            if (file.FrameCount != 1) {
                // TODO: unsupported
                Debug.LogError($"SWFFile {file.Name} has more than one frame. This is unsupported at this time.");
                return;
            }

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

            // Clean up so i dont have so many objects
            foreach (var pair in usageCount) {
                if (pair.Value > 0) {
                    UObject.DestroyImmediate(dictonary[pair.Key].go);
                }
            }
        }

        private void createImageObject(int charId, FlashImage image) {
            var (obj, transform) = createUIObj($"Image {charId}");

            transform.SetParent(vfswfhT, false);

            transform.pivot = new Vector2(0, 1);
            transform.sizeDelta = new Vector2(image.Width, image.Height);

            dictonary.Add(charId, (obj, transform));
        }

        private void createShapeObject(DefineShape shape) {
            var (rootObj, rootTransform) = createUIObj($"Shape {shape.CharacterId}");
            rootTransform.SetParent(vfswfhT, false);
            rootTransform.pivot = new Vector2(0, 1);
            rootTransform.sizeDelta = new Vector2(shape.ShapeBounds.Width, shape.ShapeBounds.Height);

            var (_, absZeroTrans) = createUIObj($"ShapeAbsZero");
            absZeroTrans.SetParent(rootTransform, false);
            absZeroTrans.anchorMin = new Vector2(0, 1);
            absZeroTrans.anchorMax = new Vector2(0, 1);
            absZeroTrans.pivot = new Vector2(0, 1);
            absZeroTrans.anchoredPosition = new Vector2(-shape.ShapeBounds.X, shape.ShapeBounds.Y);

            // parse the child placed shapes
            Vector2 cursorPos = new(0, 0);
            List<Vector2> boxPoints = new();

            FillStyleArray fsa = shape.Shapes.FillStyles;
            LineStyleArray lsa = shape.Shapes.LineStyles;
            int fill0Idx = -1;
            int fill1Idx = -1;
            int lineIdx = -1;
            bool dontEndOnSCR = true;

            foreach (var record in shape.Shapes.ShapeRecords) {
                if (record is StyleChangeRecord) {
                    var scr = record as StyleChangeRecord;

                    // case signifying end of shape
                    if (!dontEndOnSCR) {
                        fillInShape(fsa, lsa, fill0Idx, fill1Idx, lineIdx, boxPoints, absZeroTrans, shape.CharacterId);
                    }
                    dontEndOnSCR = true;

                    if (scr.StateNewStyles) {
                        fsa = scr.FillStyles;
                        lsa = scr.LineStyles;
                    }

                    if (scr.StateMoveTo) {
                        boxPoints.Clear();
                        cursorPos = new Vector2(scr.MoveDeltaX, scr.MoveDeltaY);
                        boxPoints.Add(cursorPos);
                    }
                    
                    if (scr.StateFillStyle0) {
                        fill0Idx = (int)scr.FillStyle0 - 1;
                    }

                    if (scr.StateFillStyle1) {
                        fill1Idx = (int)scr.FillStyle1 - 1;
                    }

                    if (scr.StateLineStyle) {
                        lineIdx = (int)scr.LineStyle - 1;
                    }
                }

                if (record is StraightEdgeRecord) {
                    dontEndOnSCR = false;
                    var ser = record as StraightEdgeRecord;

                    float dx = 0;
                    float dy = 0;

                    if (ser.GeneralLineFlag || !ser.VertLineFlag) {
                        dx = ser.DeltaX;
                    }

                    if (ser.GeneralLineFlag || ser.VertLineFlag) {
                        dy = ser.DeltaY;
                    }

                    cursorPos = new(cursorPos.x + dx, cursorPos.y + dy);
                    boxPoints.Add(cursorPos);
                }

                // TODO: curve edge record
                if (record is CurvedEdgeRecord) {
                    dontEndOnSCR = false;
                    Debug.LogError($"Curved edge record in shape {shape.CharacterId} definition, how do i handle this");
                }

                if (record is EndShapeRecord) {
                    fillInShape(fsa, lsa, fill0Idx, fill1Idx, lineIdx, boxPoints, absZeroTrans, shape.CharacterId);
                }
            }

            dictonary.Add(shape.CharacterId, (rootObj, rootTransform));
        }

        private void fillInShape(FillStyleArray fsa, LineStyleArray lsa, int fill0, int fill1, int line, List<Vector2> boxPoints, RectTransform absZeroTrans, int shapeId) {
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
                // TODO: allow this type of fill
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
            GameObject imageObj = dictonary[singleStyle.BitmapId].go;
            usageCount[singleStyle.BitmapId]++;
            var (obj, rt) = duplicateObject(imageObj, absZeroTrans);
            if (rt == null) {
                Debug.LogWarning("Failure in duplication of object");
                return;
            }

            // TODO: set the smoothed and clipped props somehow
            rt.anchoredPosition = new Vector2(minX, -minY);
            rt.sizeDelta = new Vector2(Mathf.Abs(maxX - minX), Mathf.Abs(maxY - minY));
        }

        private void createSpriteObject(DefineSprite sprite) {
            var (rootObj, rootTransform) = createUIObj($"Sprite {sprite.CharacterId}");
            rootTransform.SetParent(vfswfhT, false);
            rootTransform.pivot = new Vector2(0, 1);

            var frames = generateFramesAsObjects(sprite.Frames);

            foreach (var framePair in frames) {
                RectTransform rt = framePair.rt;
                rt.SetParent(rootTransform, false);
                rt.anchoredPosition = new Vector2();
            }

            dictonary.Add(sprite.CharacterId, (rootObj, rootTransform));
        }

        private List<(GameObject go, RectTransform rt)> generateFramesAsObjects(List<Frame> frames) {
            List<(GameObject go, RectTransform rt)> frameObjs = new();

            foreach (Frame frame in frames) {
                // TODO: iterate over frames and create each object individually. Also create an intermediate representation to track deltas so we dont have 
                // to duplicate everything all the time. 
            }

            return frameObjs;
        }

        private (GameObject, RectTransform) createUIObj(string name) {
            GameObject go = new(name, typeof(RectTransform));
            RectTransform rt = go.transform as RectTransform;

            return (go, rt);
        }

        private (GameObject, RectTransform) duplicateObject(GameObject go, RectTransform parent) {
            // TODO: this doesnt work the way i would have hoped
            RectTransform rt = go.transform as RectTransform;
            GameObject newGo = UObject.Instantiate(go, parent, false);
            RectTransform newRt = newGo.transform as RectTransform;

            newRt.pivot = rt.pivot;
            newRt.anchorMin = new Vector2(0, 1);
            newRt.anchorMax = new Vector2(0, 1);

            return (newGo, newRt);
        }
    }
}
