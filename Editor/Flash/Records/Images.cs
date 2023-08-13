using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UColor32 = UnityEngine.Color32;

namespace CWAEmu.OFUCU.Flash.Records {
    [Serializable]
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
            jpegData = JPEGFixer.fixJpeg(jpegData);

            Texture2D tex = new(2, 2);
            bool success = ImageConversion.LoadImage(tex, jpegData);
            if (!success) {
                Debug.LogError("Failed to parse JPG Bits Image data.");
            }

            img.Width = tex.width;
            img.Height = tex.height;

            img.ImgData = new Color[img.Height, img.Width];

            var unityColors = tex.GetPixels32();

            // Unity likes trolling people and treating the bottom left of an image as 0,0 instead of the top left, so we have to accout for that here
            for (int y = 0; y < img.Height; y++) {
                for (int x = 0; x < img.Width; x++) {
                    int unityY = img.Height - y - 1;
                    UColor32 color = unityColors[unityY * img.Width + x];

                    img.ImgData[y, x] = Color.fromUnityColor(color);
                }
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
            bytes = JPEGFixer.fixJpeg(bytes);
            Texture2D tex = new(2, 2);
            bool success = ImageConversion.LoadImage(tex, bytes);
            if (!success) {
                Debug.LogError("Failed to parse JPEG2 Image data.");
            }

            img.Width = tex.width;
            img.Height = tex.height;

            img.ImgData = new Color[img.Height, img.Width];

            var unityColors = tex.GetPixels32();

            // Unity likes trolling people and treating the bottom left of an image as 0,0 instead of the top left, so we have to accout for that here
            for (int y = 0; y < img.Height; y++) {
                for (int x = 0; x < img.Width; x++) {
                    int unityY = img.Height - y - 1;
                    UColor32 color = unityColors[unityY * img.Width + x];

                    img.ImgData[y, x] = Color.fromUnityColor(color);
                }
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
            bytes = JPEGFixer.fixJpeg(bytes);
            Texture2D tex = new(2, 2);
            bool success = ImageConversion.LoadImage(tex, bytes);
            if (!success) {
                Debug.LogError("Failed to parse JPEG3 Image data.");
            }

            img.Width = tex.width;
            img.Height = tex.height;

            img.ImgData = new Color[img.Height, img.Width];

            var unityColors = tex.GetPixels32();
            Reader zlibed = reader.readZLibBytes(compressedAlphaLen);
            byte[] alphaData = zlibed.readBytes(zlibed.Remaining);

            // Unity likes trolling people and treating the bottom left of an image as 0,0 instead of the top left, so we have to accout for that here
            for (int y = 0; y < img.Height; y++) {
                for (int x = 0; x < img.Width; x++) {
                    int unityY = img.Height - y - 1;
                    UColor32 color = unityColors[unityY * img.Width + x];
                    byte alpha = alphaData[y * img.Width + x];

                    img.ImgData[y, x] = Color.fromUnityColor(color, alpha);
                }
            }

            Texture2D.DestroyImmediate(tex);

            return img;
        }

        public override Color readPixelAt(int x, int y) {
            return ImgData[y, x];
        }
    }

    static class JPEGFixer {
        private const byte SOI_MARKER = 0xD8;
        private const byte EOI_MARKER = 0xD9;
        private const byte CONTROL_MARKER = 0xff;
        private static List<byte> markersWithoutLength = new() { 0, SOI_MARKER, EOI_MARKER, 0xD0, 0xD1, 0xD2, 0xD3, 0xD4, 0xD5, 0xD6, 0xD7 };

        public static byte[] fixJpeg(byte[] jpegData) {
            // init the list with length equal to JPEG data, allows us to not have to grow the list as much
            List<byte> data = new(jpegData.Length);

            // No data, return empty array
            if (jpegData.Length == 0) {
                Debug.Log("Returning 1");
                return jpegData;
            }

            int idx = 0;

            // Invalid, give back the original data and let the caller suffer
            if (jpegData[idx] != CONTROL_MARKER) {
                Debug.Log("Returning 2");
                return jpegData;
            }

            idx++;

            // Not a jpeg, return original data
            byte byte2 = jpegData[idx];
            if (byte2 != SOI_MARKER && byte2 != EOI_MARKER) {
                Debug.Log("Returning 3");
                return jpegData;
            }

            // check for that possible bad header
            if (byte2 == EOI_MARKER) {
                // check that the next 2 bytes are valid SOI header bytes
                if (jpegData[idx + 1] != CONTROL_MARKER || jpegData[idx + 2] != SOI_MARKER) {
                    // Not valid, return original data
                    Debug.Log("Returning 4");
                    return jpegData;
                }
                idx += 2;

                // check that the NEXT next 2 bytes are valid SOI header bytes
                if (jpegData[idx + 1] != CONTROL_MARKER || jpegData[idx + 2] != SOI_MARKER) {
                    // Not valid, return original data
                    Debug.Log("Returning 5");
                    return jpegData;
                }
                idx += 2;
            }

            data.Add(CONTROL_MARKER);
            data.Add(SOI_MARKER);

            bool lastEOI = false;

            // remove any EOI+SOI combinations
            while (idx < jpegData.Length) {
                byte b = jpegData[idx++];
                if (b != CONTROL_MARKER) {
                    data.Add(b);
                    lastEOI = false;
                    continue;
                }

                // invalid, return original data
                if (idx >= jpegData.Length) {
                    Debug.Log("Returning 6");
                    return jpegData;
                }

                // b is 0xff, which means some marker is next (except if the next value is 0)
                b = jpegData[idx++];

                // 0xff followed by 0x00 is data and not a marker
                if (b == 0) {
                    data.Add(CONTROL_MARKER);
                    data.Add(b);
                    lastEOI = false;
                    continue;
                }

                if (b == SOI_MARKER && lastEOI) {
                    // if we are at an SOI marker, and it is immeaditely following an EOI marker, ignore
                } else if (b == SOI_MARKER) {
                    // also remove any duplicate SOI markers
                } else if (lastEOI) {
                    // if last marker was EOI, write the EOI marker and what ever this marker is
                    data.Add(CONTROL_MARKER);
                    data.Add(EOI_MARKER);
                    data.Add(CONTROL_MARKER);
                    data.Add(b);
                } else if (b != EOI_MARKER) {
                    // If not EOI marker, add the marker
                    data.Add(CONTROL_MARKER);
                    data.Add(b);
                }

                // check if the marker is a marker that defines a length
                if (markerDefinesLength(b)) {
                    // invalid, return original data
                    if (idx + 2 >= jpegData.Length) {
                        Debug.Log("Returning 7");
                        return jpegData;
                    }

                    byte lenUpper = jpegData[idx++];
                    byte lenLower = jpegData[idx++];
                    data.Add(lenUpper);
                    data.Add(lenLower);
                    int len = (lenUpper << 8) + lenLower;

                    // invalid, return original data
                    if (idx + len >= jpegData.Length) {
                        Debug.Log("Returning 8");
                        return jpegData;
                    }

                    for (int i = 0; i < len - 2; i++) {
                        data.Add(jpegData[idx++]);
                    }
                }

                // reset the EOI flag
                lastEOI = b == EOI_MARKER;
            }

            return data.ToArray();
        }

        private static bool markerDefinesLength(byte marker) {
            return !markersWithoutLength.Contains(marker);
        }
    }
}
