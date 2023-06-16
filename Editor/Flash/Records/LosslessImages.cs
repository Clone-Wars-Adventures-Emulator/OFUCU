using System;

namespace CWAEmu.FlashConverter.Flash.Records {
    public abstract class FlashImage {
        public int Width { get; protected set; }
        public int Height { get; protected set; }
        public abstract Color readPixelAt(int x, int y);
    }
    
    public class DummyImage : FlashImage {
        public static DummyImage createDummyImage(int width, int height) {
            DummyImage di = new();
            di.Width = width;
            di.Height = height;
            
            return di;
        }
        
        public override Color readPixelAt(int x, int y) {
            return Color.Black;
        }
    }

    public class ColorMapData : FlashImage {
        public Color[] ColorTableRGB { get; private set; }
        public int[,] ImgData { get; private set; }

        public static ColorMapData readColorMapData(Reader reader, int colorTableSize, int losslessType, int width, int height, int widthPadding) {
            ColorMapData cmd = new();
            cmd.Width = width;
            cmd.Height = height;

            Func<Reader, Color> generator = Color.readRGBA;
            if (losslessType == 1) {
                generator = Color.readRGB;
            }

            cmd.ColorTableRGB = new Color[colorTableSize];
            for (int i = 0; i < colorTableSize; i++) {
                cmd.ColorTableRGB[i] = generator(reader);
            }

            cmd.ImgData = new int[height, width];
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

        public override Color readPixelAt(int x, int y) {
            return ColorTableRGB[ImgData[x, y]];
        }
    }

    public class BitMapData : FlashImage {
        public Color[,] ImgData { get; private set; }

        public static BitMapData readBitMapData(Reader reader, int losslessType, int bitmapFormat, int width, int height, int widthPadding) {
            BitMapData bmd = new();
            bmd.Width = width;
            bmd.Height = height;

            Func<Reader, Color> generator = Color.readARGB;
            if (losslessType == 1) {
                if (bitmapFormat == 4) {
                    generator = Color.readPIX15;
                } else {
                    generator = Color.readPIX24;
                }
            }

            bmd.ImgData = new Color[height, width];
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

        public override Color readPixelAt(int x, int y) {
            return ImgData[x, y];
        }
    }
}
