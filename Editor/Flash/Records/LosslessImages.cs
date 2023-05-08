using System;

namespace CWAEmu.FlashConverter.Flash.Records {
    public abstract class FlashImage {
        public int Width { get; protected set; }
        public int Height { get; protected set; }
        public abstract RGBA readPixelAt(int x, int y);
    }

    public class ColorMapData : FlashImage {
        public RGBA[] ColorTableRGB { get; private set; }
        public int[,] ImgData { get; private set; }

        public static ColorMapData readColorMapData(Reader reader, int colorTableSize, int losslessType, int width, int height, int widthPadding) {
            ColorMapData cmd = new();
            cmd.Width = width;
            cmd.Height = height;

            Func<Reader, RGBA> generator = RGBA.readRGBA;
            if (losslessType == 1) {
                generator = RGBA.readRGBasRGBA;
            }

            cmd.ColorTableRGB = new RGBA[colorTableSize];
            for (int i = 0; i < colorTableSize; i++) {
                cmd.ColorTableRGB[i] = generator(reader);
            }

            cmd.ImgData = new int[width, height];
            for (int r = 0; r < height; r++) {
                for (int c = 0; c < width; c++) {
                    cmd.ImgData[r, c] = reader.readByte();
                }

                for (int i = 0; i < widthPadding; i++) {
                    reader.readByte();
                }
            }

            return cmd;
        }

        public override RGBA readPixelAt(int x, int y) {
            return ColorTableRGB[ImgData[x, y]];
        }
    }

    public class BitMapData : FlashImage {
        public RGBA[,] ImgData { get; private set; }

        public static BitMapData readBitMapData(Reader reader, int losslessType, int bitmapFormat, int width, int height, int widthPadding) {
            BitMapData bmd = new();
            bmd.Width = width;
            bmd.Height = height;

            Func<Reader, RGBA> generator = RGBA.readARGBasRGBA;
            if (losslessType == 1) {
                if (bitmapFormat == 4) {
                    generator = RGBA.readPIX15asRGBA;
                } else {
                    generator = RGBA.readPIX24asRGBA;
                }
            }

            bmd.ImgData = new RGBA[width, height];
            for (int r = 0; r < height; r++) {
                for (int c = 0; c < width; c++) {
                    bmd.ImgData[r, c] = generator(reader);
                }

                for (int i = 0; i < widthPadding; i++) {
                    generator(reader);
                }
            }

            return bmd;
        }

        public override RGBA readPixelAt(int x, int y) {
            return ImgData[x, y];
        }
    }
}
