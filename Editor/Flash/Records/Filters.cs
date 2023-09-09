using System.Collections.Generic;

namespace CWAEmu.OFUCU.Flash.Records {
    public class FilterList {
        public byte NumFilters { get; private set; }
        public List<Filter> Filters { get; private set; } = new();

        public static FilterList readFilterList(Reader reader) {
            FilterList fl = new();
            fl.NumFilters = reader.readByte();

            for (int i = 0; i < fl.NumFilters; i++) {
                Filter f = Filter.readFilter(reader);
                fl.Filters.Add(f);
            }

            return fl;
        }
    }

    public class Filter {
        public byte FilterId { get; private set; }
        public FilterData FilterData { get; private set; }

        public static Filter readFilter(Reader reader) {
            Filter f = new();

            f.FilterId = reader.readByte();
            switch (f.FilterId) {
                case 0:
                    f.FilterData = DropShadowFitler.readFilter(reader);
                    break;
                case 1:
                    f.FilterData = BlurFilter.readFilter(reader);
                    break;
                case 2:
                    f.FilterData = GlowFilter.readFilter(reader);
                    break;
                case 3:
                    f.FilterData = BevelFilter.readFilter(reader);
                    break;
                case 4:
                    f.FilterData = GradientGlowFilter.readFilter(reader);
                    break;
                case 5:
                    f.FilterData = ConvolutionFilter.readFilter(reader);
                    break;
                case 6:
                    f.FilterData = ColorMatrixFilter.readFilter(reader);
                    break;
                case 7:
                    f.FilterData = GradientBevelFilter.readFilter(reader);
                    break;
            }

            return f;
        }
    }

    public class FilterData { }

    public class DropShadowFitler : FilterData {
        public Color DropShadowColor { get; private set; }
        public float BlurX { get; private set; }
        public float BlurY { get; private set; }
        public float Angle { get; private set; }
        public float Distance { get; private set; }
        public float Strength { get; private set; }
        public bool InnerShadow { get; private set; }
        public bool Knockout { get; private set; }
        public bool CompositeSource { get; private set; }
        public byte Passes { get; private set; }

        public static DropShadowFitler readFilter(Reader reader) {
            DropShadowFitler dsf = new();

            dsf.DropShadowColor = Color.readRGBA(reader);
            dsf.BlurX = reader.readFixed16();
            dsf.BlurY = reader.readFixed16();
            dsf.Angle = reader.readFixed16();
            dsf.Distance = reader.readFixed16();
            dsf.Strength = reader.readFixed8();

            dsf.InnerShadow = reader.readBitFlag();
            dsf.Knockout = reader.readBitFlag();
            dsf.CompositeSource = reader.readBitFlag();
            dsf.Passes = (byte)reader.readUBits(5);

            return dsf;
        }
    }

    public class BlurFilter : FilterData {
        public float BlurX { get; private set; }
        public float BlurY { get; private set; }
        public byte Passes { get; private set; }

        public static BlurFilter readFilter(Reader reader) {
            BlurFilter bf = new();

            bf.BlurX = reader.readFixed16();
            bf.BlurY = reader.readFixed16();

            bf.Passes = (byte)reader.readUBits(5);

            // reserved
            reader.readUBits(3);

            return bf;
        }
    }

    public class GlowFilter : FilterData {
        public Color GlowColor { get; private set; }
        public float BlurX { get; private set; }
        public float BlurY { get; private set; }
        public float Strength { get; private set; }
        public bool InnerGlow { get; private set; }
        public bool Knockout { get; private set; }
        public bool CompositeSource { get; private set; }
        public byte Passes { get; private set; }

        public static GlowFilter readFilter(Reader reader) {
            GlowFilter gf = new();

            gf.GlowColor = Color.readRGBA(reader);
            gf.BlurX = reader.readFixed16();
            gf.BlurY = reader.readFixed16();
            gf.Strength = reader.readFixed8();

            gf.InnerGlow = reader.readBitFlag();
            gf.Knockout = reader.readBitFlag();
            gf.CompositeSource = reader.readBitFlag();
            gf.Passes = (byte)reader.readUBits(5);

            return gf;
        }
    }

    public class BevelFilter : FilterData {
        public Color ShadowColor { get; private set; }
        public Color HighlightColor { get; private set; }
        public float BlurX { get; private set; }
        public float BlurY { get; private set; }
        public float Angle { get; private set; }
        public float Distance { get; private set; }
        public float Strength { get; private set; }
        public bool InnerShadow { get; private set; }
        public bool Knockout { get; private set; }
        public bool CompositeSource { get; private set; }
        public bool OnTop { get; private set; }
        public byte Passes { get; private set; }

        public static BevelFilter readFilter(Reader reader) {
            BevelFilter bf = new();

            bf.ShadowColor = Color.readRGBA(reader);
            bf.HighlightColor = Color.readRGBA(reader);
            bf.BlurX = reader.readFixed16();
            bf.BlurY = reader.readFixed16();
            bf.Angle = reader.readFixed16();
            bf.Distance = reader.readFixed16();
            bf.Strength = reader.readFixed8();

            bf.InnerShadow = reader.readBitFlag();
            bf.Knockout = reader.readBitFlag();
            bf.CompositeSource = reader.readBitFlag();
            bf.OnTop = reader.readBitFlag();
            bf.Passes = (byte)reader.readUBits(4);

            return bf;
        }
    }

    public class GradientGlowFilter : FilterData {
        public byte NumColors { get; private set; }
        public Color[] GradientColors { get; private set; }
        public byte[] GradientRatio { get; private set; }
        public float BlurX { get; private set; }
        public float BlurY { get; private set; }
        public float Angle { get; private set; }
        public float Distance { get; private set; }
        public float Strength { get; private set; }
        public bool InnerShadow { get; private set; }
        public bool Knockout { get; private set; }
        public bool CompositeSource { get; private set; }
        public bool OnTop { get; private set; }
        public byte Passes { get; private set; }

        public static GradientGlowFilter readFilter(Reader reader) {
            GradientGlowFilter ggf = new();

            ggf.NumColors = reader.readByte();
            ggf.GradientColors = new Color[ggf.NumColors];
            ggf.GradientRatio = new byte[ggf.NumColors];

            for (int i = 0; i < ggf.NumColors; i++) {
                ggf.GradientColors[i] = Color.readRGBA(reader);
            }

            for (int i = 0; i < ggf.NumColors; i++) {
                ggf.GradientRatio[i] = reader.readByte();
            }

            ggf.BlurX = reader.readFixed16();
            ggf.BlurY = reader.readFixed16();
            ggf.Angle = reader.readFixed16();
            ggf.Distance = reader.readFixed16();
            ggf.Strength = reader.readFixed8();

            ggf.InnerShadow = reader.readBitFlag();
            ggf.Knockout = reader.readBitFlag();
            ggf.CompositeSource = reader.readBitFlag();
            ggf.OnTop = reader.readBitFlag();
            ggf.Passes = (byte)reader.readUBits(4);

            return ggf;
        }
    }

    public class ConvolutionFilter : FilterData {
        public byte MatrixX { get; private set; }
        public byte MatrixY { get; private set; }
        public float Divisor { get; private set; }
        public float Bias { get; private set; }
        public float[] Matrix { get; private set; }
        public Color DefaultColor { get; private set; }
        public bool Clamp { get; private set; }
        public bool PreserveAlpha { get; private set; }

        public static ConvolutionFilter readFilter(Reader reader) {
            ConvolutionFilter cf = new();

            cf.MatrixX = reader.readByte();
            cf.MatrixY = reader.readByte();
            cf.Divisor = reader.readSingle();
            cf.Bias = reader.readSingle();

            cf.Matrix = new float[cf.MatrixX * cf.MatrixY];
            for (int i  = 0; i < cf.Matrix.Length; i++) {
                cf.Matrix[i] = reader.readSingle();
            }

            cf.DefaultColor = Color.readRGBA(reader);

            // reserved
            reader.readUBits(6);

            cf.Clamp = reader.readBitFlag();
            cf.PreserveAlpha = reader.readBitFlag();
            return cf;
        }
    }

    public class ColorMatrixFilter : FilterData {
        public float[] Matrix { get; private set; } = new float[20];

        public static ColorMatrixFilter readFilter(Reader reader) {
            ColorMatrixFilter cmf = new();

            for (int i = 0; i < 20; i++) {
                cmf.Matrix[i] = reader.readSingle();
            }

            return cmf;
        }
    }

    public class GradientBevelFilter : FilterData {
        public byte NumColors { get; private set; }
        public Color[] GradientColors { get; private set; }
        public byte[] GradientRatio { get; private set; }
        public float BlurX { get; private set; }
        public float BlurY { get; private set; }
        public float Angle { get; private set; }
        public float Distance { get; private set; }
        public float Strength { get; private set; }
        public bool InnerShadow { get; private set; }
        public bool Knockout { get; private set; }
        public bool CompositeSource { get; private set; }
        public bool OnTop { get; private set; }
        public byte Passes { get; private set; }

        public static GradientBevelFilter readFilter(Reader reader) {
            GradientBevelFilter gbf = new();

            gbf.NumColors = reader.readByte();
            gbf.GradientColors = new Color[gbf.NumColors];
            gbf.GradientRatio = new byte[gbf.NumColors];

            for (int i = 0; i < gbf.NumColors; i++) {
                gbf.GradientColors[i] = Color.readRGBA(reader);
            }

            for (int i = 0; i < gbf.NumColors; i++) {
                gbf.GradientRatio[i] = reader.readByte();
            }

            gbf.BlurX = reader.readFixed16();
            gbf.BlurY = reader.readFixed16();
            gbf.Angle = reader.readFixed16();
            gbf.Distance = reader.readFixed16();
            gbf.Strength = reader.readFixed8();

            gbf.InnerShadow = reader.readBitFlag();
            gbf.Knockout = reader.readBitFlag();
            gbf.CompositeSource = reader.readBitFlag();
            gbf.OnTop = reader.readBitFlag();
            gbf.Passes = (byte)reader.readUBits(4);

            return gbf;
        }
    }
}
