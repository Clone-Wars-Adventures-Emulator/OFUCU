using CWAEmu.OFUCU.Flash.Records;
using System;
using System.Collections.Generic;
using UnityEngine;
using Rect = CWAEmu.OFUCU.Flash.Records.Rect;
using URect = UnityEngine.Rect;
using UColor = UnityEngine.Color;

namespace CWAEmu.OFUCU.Flash.Tags {
    public class DefineShape : CharacterTag {
        public delegate void OnShapeClosed(FillStyleArray fsa, LineStyleArray lsa, int fill0, int fill1, int line, List<Vector2> boxPoints, int shapeId);
        public delegate void OnFillBitmap(URect extends, ushort bitmapId, bool smooth, bool cropped);
        public delegate void OnFillSolid(URect extends, UColor color);
        public delegate void OnFillGradient(URect extends);

        public int ShapeType { get; set; }
        public Rect ShapeBounds { get; private set; }
        public Rect EdgeBounds { get; private set; }
        public bool UsesNonScalingStrokes { get; private set; }
        public bool UsesScalingStrokes { get; private set; }
        public ShapeWithStyle Shapes { get; private set; }

        public override void read(Reader reader) {
            CharacterId = reader.readUInt16();

            ShapeBounds = Rect.readRect(reader);

            if (ShapeType == 4) {
                EdgeBounds = Rect.readRect(reader);

                reader.readBits(6);

                UsesNonScalingStrokes = reader.readBitFlag();
                UsesScalingStrokes = reader.readBitFlag();
                reader.endBitRead();
            }

            Shapes = ShapeWithStyle.readShapeWithStyle(reader, ShapeType);
        }

        public void iterateOnShape(OnShapeClosed onClosed) {
            // parse the child placed objects
            Vector2 cursorPos = new(0, 0);
            List<Vector2> boxPoints = new();

            FillStyleArray fsa = Shapes.FillStyles;
            LineStyleArray lsa = Shapes.LineStyles;
            int fill0Idx = -1;
            int fill1Idx = -1;
            int lineIdx = -1;
            bool dontEndOnSCR = true;

            foreach (var record in Shapes.ShapeRecords) {
                if (record is StyleChangeRecord) {
                    var scr = record as StyleChangeRecord;

                    // case signifying end of shape
                    if (!dontEndOnSCR) {
                        onClosed(fsa, lsa, fill0Idx, fill1Idx, lineIdx, boxPoints, CharacterId);
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
                        fill0Idx = (int) scr.FillStyle0 - 1;
                    }

                    if (scr.StateFillStyle1) {
                        fill1Idx = (int) scr.FillStyle1 - 1;
                    }

                    if (scr.StateLineStyle) {
                        lineIdx = (int) scr.LineStyle - 1;
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
                    Debug.LogError($"Curved edge record in shape {CharacterId} definition, how do i handle this");
                }

                if (record is EndShapeRecord) {
                    onClosed(fsa, lsa, fill0Idx, fill1Idx, lineIdx, boxPoints, CharacterId);
                }
            }
        }

        public void iterateOnShapeFill(OnFillBitmap onBitmap, OnFillSolid onSolid, OnFillGradient onGradient) {
            iterateOnShape((fsa, lsa, fill0, fill1, line, boxPoints, shapeId) => {
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

                URect extends = new() {
                    xMin = minX,
                    yMin = minY,
                    xMax = maxX,
                    yMax = maxY
                };

                // if solid, fill solid
                if (singleStyle.Type == FillStyle.EnumFillStyleType.Solid) {
                    UColor fillColor = singleStyle.Color.asUnityColor();
                    onSolid?.Invoke(extends, fillColor);
                    return;
                }

                byte fillTypeAsByte = ((byte) singleStyle.Type);
                if ((fillTypeAsByte & 0x10) == 0x10) {
                    // TODO: what all properties will i need for this??
                    Debug.LogWarning("Gradient fill. Unknown how to properly handle.");

                    onGradient?.Invoke(extends);

                    return;
                }

                bool smoothed = (fillTypeAsByte & 0x02) == 0x02;
                bool clipped = (fillTypeAsByte & 0x01) == 0x01;

                onBitmap?.Invoke(extends, singleStyle.BitmapId, smoothed, clipped);
            });
        }
    }
}
