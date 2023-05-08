using CWAEmu.FlashConverter.Flash.Records;

namespace CWAEmu.FlashConverter.Flash.Tags {
    public class DefineBitsLossless : CharacterTag {
        public int BitsLosslessType { get; set; }
        public byte BitmapFormat { get; private set; }
        public ushort BitmapWidth { get; private set; }
        public ushort BitmapHeight { get; private set; }
        public int BitmapColorTableSize { get; private set; }
        public FlashImage ImageData { get; private set; }  

        public override void read(Reader reader) {
            CharacterId = reader.readUInt16();

            BitmapFormat = reader.readByte();

            BitmapWidth = reader.readUInt16();
            BitmapHeight = reader.readUInt16();

            int numBytes = calculateNumBytes(BitmapFormat, BitmapWidth, BitmapHeight);

            if (BitmapFormat == 3) {
                BitmapColorTableSize = reader.readByte() + 1;

                int colorTableBytes;
                if (BitsLosslessType == 1) {
                    colorTableBytes = 3 * BitmapColorTableSize;
                } else {
                    colorTableBytes = 4 * BitmapColorTableSize;
                }

                numBytes += colorTableBytes;
                Reader decompressed = reader.readZLibBytes(numBytes);

                ImageData = ColorMapData.readColorMapData(decompressed, BitmapColorTableSize, BitsLosslessType, BitmapWidth, BitmapHeight);
            } else if (BitmapFormat == 4 || BitmapFormat == 5) {
                Reader decompressed = reader.readZLibBytes(numBytes);
                ImageData = BitMapData.readBitMapData(decompressed, BitsLosslessType, BitmapFormat, BitmapWidth, BitmapHeight);
            }
        }

        private static int calculateNumBytes(byte bitmapFormat, ushort width, ushort height) {
            int bytesPerPixel = 0;

            if (bitmapFormat == 3) {
                bytesPerPixel = 1;
            } else if (bitmapFormat == 4) {
                bytesPerPixel = 2;
            } else if (bitmapFormat == 5) {
                bytesPerPixel = 4;
            }

            int rowBytes = width * bytesPerPixel;
            if (rowBytes % 4 != 0) {
                rowBytes += 4 - (rowBytes % 4);
            }

            int imagePixelBytes = rowBytes * height;

            return imagePixelBytes;
        }
    }
}
