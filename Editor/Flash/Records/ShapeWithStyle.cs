using System.Collections.Generic;

namespace CWAEmu.OFUCU.Flash.Records {
    public class ShapeWithStyle {
        public FillStyleArray FillStyles { get; private set; }
        public LineStyleArray LineStyles { get; private set; }
        public uint NumFillBits { get; private set; }
        public uint NumLineBits { get; private set; }
        public List<ShapeRecord> ShapeRecords { get; private set; } = new();

        private FillStyleArray curFillStyle;
        private LineStyleArray curLineStyle;
        private uint curNumFillBits;
        private uint curNumLineBits;

        public static ShapeWithStyle readShapeWithStyle(Reader reader, int shapeTagType) {
            ShapeWithStyle style = new();

            style.FillStyles = FillStyleArray.readFillStyleArray(reader, shapeTagType);
            style.LineStyles = LineStyleArray.readLineStyleArray(reader, shapeTagType);
            style.NumFillBits = reader.readUBits(4);
            style.NumLineBits = reader.readUBits(4);

            style.curFillStyle = style.FillStyles;
            style.curLineStyle = style.LineStyles;
            style.curNumFillBits = style.NumFillBits;
            style.curNumLineBits = style.NumLineBits;

            bool readEndFlag = false;
            while (!readEndFlag) {
                bool typeFlag = reader.readBitFlag();

                if (!typeFlag) {
                    bool stateNewStyles = reader.readBitFlag();
                    bool stateLineStyle = reader.readBitFlag();
                    bool stateFillStyle1 = reader.readBitFlag();
                    bool stateFillStyle0 = reader.readBitFlag();
                    bool stateMoveTo = reader.readBitFlag();

                    // if all 0, end record
                    if (!(stateNewStyles || stateLineStyle || stateFillStyle1 || stateFillStyle0 || stateMoveTo)) {
                        style.ShapeRecords.Add(new EndShapeRecord());
                        readEndFlag = true;
                        break;
                    }

                    StyleChangeRecord scr = new();
                    scr.StateNewStyles = stateNewStyles;
                    scr.StateLineStyle = stateLineStyle;
                    scr.StateFillStyle1 = stateFillStyle1;
                    scr.StateFillStyle0 = stateFillStyle0;
                    scr.StateMoveTo = stateMoveTo;

                    if (stateMoveTo) {
                        scr.MoveBits = reader.readUBits(5);
                        scr.MoveDeltaXTwips = reader.readBits(scr.MoveBits);
                        scr.MoveDeltaYTwips = reader.readBits(scr.MoveBits);
                    }

                    if (stateFillStyle0) {
                        scr.FillStyle0 = reader.readUBits(style.curNumFillBits);
                    }

                    if (stateFillStyle1) {
                        scr.FillStyle1 = reader.readUBits(style.curNumFillBits);
                    }

                    if (stateLineStyle) {
                        scr.LineStyle = reader.readUBits(style.curNumLineBits);
                    }

                    if (stateNewStyles) {
                        scr.FillStyles = FillStyleArray.readFillStyleArray(reader, shapeTagType);
                        scr.LineStyles = LineStyleArray.readLineStyleArray(reader, shapeTagType);
                        scr.NumFillBits = reader.readUBits(4);
                        scr.NumLineBits = reader.readUBits(4);

                        style.curFillStyle = scr.FillStyles;
                        style.curLineStyle = scr.LineStyles;
                        style.curNumFillBits = scr.NumFillBits;
                        style.curNumLineBits = scr.NumLineBits;
                    }

                    style.ShapeRecords.Add(scr);
                } else {
                    bool straightFlag = reader.readBitFlag();

                    if (straightFlag) {
                        StraightEdgeRecord ser = new();
                        ser.NumBits = reader.readUBits(4);
                        ser.GeneralLineFlag = reader.readBitFlag();

                        if (!ser.GeneralLineFlag) {
                            ser.VertLineFlag = reader.readBitFlag();
                        }

                        if (ser.GeneralLineFlag || !ser.VertLineFlag) {
                            ser.DeltaXTwips = reader.readBits(ser.NumBits + 2);
                        }

                        if (ser.GeneralLineFlag || ser.VertLineFlag) {
                            ser.DeltaYTwips = reader.readBits(ser.NumBits + 2);
                        }

                        style.ShapeRecords.Add(ser);
                    } else {
                        CurvedEdgeRecord cer = new() {
                            NumBits = reader.readUBits(4),
                        };

                        cer.ControlDeltaX = reader.readBits(cer.NumBits + 2);
                        cer.ControlDeltaY = reader.readBits(cer.NumBits + 2);
                        cer.AnchorDeltaX = reader.readBits(cer.NumBits + 2);
                        cer.AnchorDeltaY = reader.readBits(cer.NumBits + 2);

                        style.ShapeRecords.Add(cer);
                    }
                }
            }

            reader.endBitRead();

            return style;
        }
    }

    public class ShapeRecord { }

    public class EndShapeRecord : ShapeRecord { }

    public class StyleChangeRecord : ShapeRecord {
        public bool StateNewStyles { get; set; }
        public bool StateLineStyle { get; set; }
        public bool StateFillStyle1 { get; set; }
        public bool StateFillStyle0 { get; set; }
        public bool StateMoveTo { get; set; }
        public uint MoveBits { get; set; }
        public int MoveDeltaXTwips { get; set; }
        public int MoveDeltaYTwips { get; set; }
        public float MoveDeltaX => MoveDeltaXTwips / 20.0f;
        public float MoveDeltaY => MoveDeltaYTwips / 20.0f;
        public uint FillStyle0 { get; set; }
        public uint FillStyle1 { get; set; }
        public uint LineStyle { get; set; }
        public FillStyleArray FillStyles { get; set; }
        public LineStyleArray LineStyles { get; set; }
        public uint NumFillBits { get; set; }
        public uint NumLineBits { get; set; }
    }

    public class StraightEdgeRecord : ShapeRecord {
        public uint NumBits { get; set; }
        public bool GeneralLineFlag { get; set; }
        public bool VertLineFlag { get; set; }
        public int DeltaXTwips { get; set; }
        public float DeltaX => DeltaXTwips / 20.0f;
        public int DeltaYTwips { get; set; }
        public float DeltaY => DeltaYTwips / 20.0f;
    }

    public class CurvedEdgeRecord : ShapeRecord {
        public uint NumBits { get; set; }
        public int ControlDeltaX { get; set; }
        public int ControlDeltaY { get; set; }
        public int AnchorDeltaX { get; set; }
        public int AnchorDeltaY { get; set; }
    }


    public class FillStyleArray {
        public List<FillStyle> Array { get; private set; } = new();
        public FillStyle this[int index] => Array[index];

        public static FillStyleArray readFillStyleArray(Reader reader, int shapeTagType) {
            FillStyleArray fsa = new();

            byte bCount = reader.readByte();
            int count = bCount;
            if (bCount == 0xff) {
                count = reader.readUInt16();
            }

            for (int i = 0; i < count; i++) {
                fsa.Array.Add(FillStyle.readFillStyle(reader, shapeTagType));
            }

            return fsa;
        }
    }

    public class FillStyle {
        public enum EnumFillStyleType : byte {
            Solid = 0x00,
            LinearGradientFill = 0x10,
            RadialGradientFill = 0x12,
            FocalRadialGradientFill = 0x13,
            RepeatingBitmapFill = 0x40,
            ClippedBitmapFill = 0x41,
            NonSmoothedRepeatingBitmap = 0x42,
            NonSmoothedClippedBitmap = 0x43,
        }

        public EnumFillStyleType Type { get; private set; }

        public Color Color { get; private set; }
        public Matrix GradientMatrix { get; private set; }
        public Gradient Gradient { get; private set; }
        public FocalGradient FocalGradient { get; private set; }
        public ushort BitmapId { get; private set; }
        public Matrix BitmapMatrix { get; private set; }

        public static FillStyle readFillStyle(Reader reader, int shapeTagType) {
            FillStyle fs = new() {
                Type = (EnumFillStyleType)reader.readByte()
            };

            if (fs.Type == EnumFillStyleType.Solid) {
                if (shapeTagType >= 3) {
                    fs.Color = Color.readRGBA(reader);
                } else {
                    fs.Color = Color.readRGB(reader);
                }
            }

            if (fs.Type == EnumFillStyleType.LinearGradientFill || fs.Type == EnumFillStyleType.RadialGradientFill) {
                fs.GradientMatrix = Matrix.readMatrix(reader);
                fs.Gradient = Gradient.readGradient(reader, shapeTagType);
            }

            if (fs.Type == EnumFillStyleType.FocalRadialGradientFill) {
                fs.FocalGradient = FocalGradient.readGradient(reader, shapeTagType);
            }

            if (((byte) fs.Type & 0x40) == 0x40) {
                fs.BitmapId = reader.readUInt16();
                fs.BitmapMatrix = Matrix.readMatrix(reader);
            }

            return fs;
        }

    }
    public class LineStyleArray {
        public List<LineStyle> Array { get; private set; } = new();
        public LineStyle this[int index] => Array[index];

        public static LineStyleArray readLineStyleArray(Reader reader, int shapeTagType) {
            LineStyleArray lsa = new();

            byte bCount = reader.readByte();
            int count = bCount;
            if (bCount == 0xff) {
                count = reader.readUInt16();
            }

            for (int i = 0; i < count; i++) {
                if (shapeTagType == 4) {
                    lsa.Array.Add(LineStyle2.readLineStyle2(reader, shapeTagType));
                } else {
                    lsa.Array.Add(LineStyle.readLineStyle(reader, shapeTagType));
                }
            }

            return lsa;
        }
    }

    public class LineStyle {
        public ushort Width { get; protected set; }
        public Color Color { get; protected set; }

        public static LineStyle readLineStyle(Reader reader, int shapeTagType) {
            LineStyle lineStyle = new();

            lineStyle.Width = reader.readUInt16();
                
            if (shapeTagType == 3) {
                lineStyle.Color = Color.readRGBA(reader);
            } else {
                lineStyle.Color = Color.readRGB(reader);
            }

            return lineStyle;
        }
    }

    public class LineStyle2 : LineStyle {
        public byte StartCapStyle { get; private set; }
        public byte JoinStyle { get; private set; }
        public bool HasFillFlag { get; private set; }
        public bool NoHScaleFlag { get; private set; }
        public bool NoVScaleFlag { get; private set; }
        public bool PixelHintingFlag { get; private set; }
        public bool NoClose { get; private set; }
        public byte EndCapStyle { get; private set; }
        public ushort MiterLimitFactor { get; private set; }
        public FillStyle FillType { get; private set; }

        public static LineStyle2 readLineStyle2(Reader reader, int shapeTagType) {
            LineStyle2 lineStyle = new();

            lineStyle.Width = reader.readUInt16();
            lineStyle.StartCapStyle = (byte)reader.readUBits(2);
            lineStyle.JoinStyle = (byte)reader.readUBits(2);
            lineStyle.HasFillFlag = reader.readBitFlag();
            lineStyle.NoHScaleFlag = reader.readBitFlag();
            lineStyle.NoVScaleFlag = reader.readBitFlag();
            lineStyle.PixelHintingFlag = reader.readBitFlag();
            // reserved
            reader.readUBits(5);
            lineStyle.NoClose = reader.readBitFlag();
            lineStyle.EndCapStyle = (byte)reader.readUBits(2);

            if (lineStyle.JoinStyle == 2) {
                lineStyle.MiterLimitFactor = reader.readUInt16();
            }
                
            if (!lineStyle.HasFillFlag) {
                lineStyle.Color = Color.readRGBA(reader);
            } else {
                lineStyle.FillType = FillStyle.readFillStyle(reader, shapeTagType);
            }

            return lineStyle;
        }
    }
}
