using System;
using System.Linq;
using UnityEngine;
using UColor32 = UnityEngine.Color32;

namespace CWAEmu.OFUCU.Flash.Records {
    [System.Serializable]
    public class FlashImage {
        public int Width { get { return width; } protected set { width = value; } }
        public int Height { get { return height; } protected set { height = value; } }
        [SerializeField] private int width;
        [SerializeField] private int height;

        public static FlashImage createBlankImage(int width, int height) {
            FlashImage fi = new();
            fi.Width = width;
            fi.Height = height;

            return fi;
        }

        public virtual Color readPixelAt(int x, int y) {
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
            return ColorTableRGB[ImgData[y, x]];
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
            return ImgData[y, x];
        }
    }

    public class Bits1Iamge : FlashImage {
        public Color[,] ImgData { get; private set; }

        public static Bits1Iamge readBits(Reader reader, int len) {
            Bits1Iamge img = new();

            byte[] imageData = reader.readBytes(len);

            // TODO: verify a dum concat works
            byte[] jpegData = reader.File.JPEGTable.TableData.Concat(imageData).ToArray();

            Texture2D tex = new(2, 2);
            ImageConversion.LoadImage(tex, jpegData);

            img.Width = tex.width;
            img.Height = tex.height;

            img.ImgData = new Color[img.Height, img.Width];

            var unityColors = tex.GetPixels32();
            for (int i = 0; i < unityColors.Length; i++) {
                int y = i / img.Width;
                int x = i % img.Width;
                UColor32 color = unityColors[i];

                img.ImgData[y, x] = Color.fromUnityColor(color);
            }

            Texture2D.DestroyImmediate(tex);


            return img;
        }

        public override Color readPixelAt(int x, int y) {
            return ImgData[y, x];
        }
    }

    public class JPEG2Image : FlashImage {
        public Color[,] ImgData { get; private set; }

        public static JPEG2Image readJpeg2(Reader reader, int jpegLen) {
            JPEG2Image img = new();

            if (reader.Version < 8) {
                // TODO: handle stupid erroneous header of 0xFF, 0xD9, 0xFF, 0xD8 before the JPEG SOI marker
            }

            byte[] bytes = reader.readBytes(jpegLen);
            Texture2D tex = new(2, 2);
            ImageConversion.LoadImage(tex, bytes);

            img.Width = tex.width;
            img.Height = tex.height;

            img.ImgData = new Color[img.Height, img.Width];

            var unityColors = tex.GetPixels32();
            for (int i = 0; i < unityColors.Length; i++) {
                int y = i / img.Width;
                int x = i % img.Width;
                UColor32 color = unityColors[i];

                img.ImgData[y, x] = Color.fromUnityColor(color);
            }

            Texture2D.DestroyImmediate(tex);

            return img;
        }

        public override Color readPixelAt(int x, int y) {
            return ImgData[y, x];
        }
    }

    public class JPEG3Image : FlashImage {
        public Color[,] ImgData { get; private set; }

        public static JPEG3Image readJpeg3(Reader reader, uint jpegLen, uint compressedAlphaLen) {
            JPEG3Image img = new();

            if (reader.Version < 8) {
                // TODO: handle stupid erroneous header of 0xFF, 0xD9, 0xFF, 0xD8 before the JPEG SOI marker
            }

            byte[] bytes = reader.readBytes(jpegLen);
            Texture2D tex = new(2, 2);
            ImageConversion.LoadImage(tex, bytes);

            img.Width = tex.width;
            img.Height = tex.height;

            img.ImgData = new Color[img.Height, img.Width];

            var unityColors = tex.GetPixels32();
            Reader zlibed = reader.readZLibBytes(compressedAlphaLen);
            byte[] alphaData = zlibed.readBytes(zlibed.Remaining);
            for (int i = 0; i < unityColors.Length; i++) {
                int y = i / img.Width;
                int x = i % img.Width;
                UColor32 color = unityColors[i];

                img.ImgData[y, x] = Color.fromUnityColor(color, alphaData[i]);
            }

            Texture2D.DestroyImmediate(tex);

            return img;
        }

        public override Color readPixelAt(int x, int y) {
            return ImgData[y, x];
        }
    }
}
