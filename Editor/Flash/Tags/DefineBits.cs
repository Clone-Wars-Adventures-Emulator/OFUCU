using CWAEmu.OFUCU.Flash.Records;

namespace CWAEmu.OFUCU.Flash.Tags {
    public class DefineBits : ImageCharacterTag {
        public override void read(Reader reader) {
            CharacterId = reader.readUInt16();

            if (reader.SkipImageData) {
                // skip (taglength - 2) bytes, as 2 bytes were already read from the tag.
                reader.readBytes(Header.TagLength - 2);
                Image = FlashImage.createBlankImage(1, 1);
                return;
            }

            Image = Bits1Iamge.readBits(reader, Header.TagLength - 2);
        }
    }

    public class DefineBitsJPEG2 : ImageCharacterTag {
        public override void read(Reader reader) {
            CharacterId = reader.readUInt16();

            if (reader.SkipImageData) {
                // skip (taglength - 2) bytes, as 2 bytes were already read from the tag.
                reader.readBytes(Header.TagLength - 2);
                Image = FlashImage.createBlankImage(1, 1);
                return;
            }

            Image = JPEG2Image.readJpeg2(reader, Header.TagLength - 2);
        }
    }

    public class DefineBitsJPEG3 : ImageCharacterTag {
        public override void read(Reader reader) {
            CharacterId = reader.readUInt16();

            if (reader.SkipImageData) {
                // skip (taglength - 2) bytes, as 2 bytes were already read from the tag.
                reader.readBytes(Header.TagLength - 2);
                Image = FlashImage.createBlankImage(1, 1);
                return;
            }

            // This is also called JpegDataLen
            uint alphaDataOffset = reader.readUInt32();
            uint remaingData = (uint) (Header.TagLength - alphaDataOffset - 6);

            Image = JPEG3Image.readJpeg3(reader, alphaDataOffset, remaingData);
        }
    }
}
