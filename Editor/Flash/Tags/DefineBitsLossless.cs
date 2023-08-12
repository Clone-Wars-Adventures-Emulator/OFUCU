using CWAEmu.OFUCU.Flash.Records;

namespace CWAEmu.OFUCU.Flash.Tags {
    public class DefineBitsLossless : ImageCharacterTag {
        public int BitsLosslessType { get; set; }
        public byte BitmapFormat { get; private set; }
        public ushort BitmapWidth { get; private set; }
        public ushort BitmapHeight { get; private set; }
        public int BitmapColorTableSize { get; private set; }

        public override void read(Reader reader) {
            CharacterId = reader.readUInt16();

            BitmapFormat = reader.readByte();

            BitmapWidth = reader.readUInt16();
            BitmapHeight = reader.readUInt16();
            
            if (reader.SkipImageData) {
                // skip (taglength - 7) bytes, as 7 bytes were already read from the tag.
                reader.readBytes(Header.TagLength - 7);
                Image = FlashImage.createBlankImage(BitmapWidth, BitmapHeight);
                return;
            }

            int bytesOfTagread = 7;

            int padding = calculatePadding(BitmapFormat, BitmapWidth, BitmapHeight);

            if (BitmapFormat == 3) {
                BitmapColorTableSize = reader.readByte() + 1;
                bytesOfTagread++;

                Reader decompressed = reader.readZLibBytes(Header.TagLength - bytesOfTagread);

                Image = ColorMapData.readColorMapData(decompressed, BitmapColorTableSize, BitsLosslessType, BitmapWidth, BitmapHeight, padding);
            } else if (BitmapFormat == 4 || BitmapFormat == 5) {
                Reader decompressed = reader.readZLibBytes(Header.TagLength - bytesOfTagread);
                Image = BitMapData.readBitMapData(decompressed, BitsLosslessType, BitmapFormat, BitmapWidth, BitmapHeight, padding);
            }
        }

        private static int calculatePadding(byte bitmapFormat, ushort width, ushort height) {
            int bytesPerPixel = 0;

            if (bitmapFormat == 3) {
                bytesPerPixel = 1;
            } else if (bitmapFormat == 4) {
                bytesPerPixel = 2;
            } else if (bitmapFormat == 5) {
                bytesPerPixel = 4;
            }

            int rowBytes = width * bytesPerPixel;
            int padding = 0;
            if (rowBytes % 4 != 0) {
                padding = 4 - (rowBytes % 4);
            }
            return padding / bytesPerPixel;
        }
    }
}
