using CWAEmu.OFUCU.Flash.Records;
using CWAEmu.OFUCU.Flash.Tags;
using UnityEngine;

namespace CWAEmu.OFUCU.Flash {
    public class DefineBits : CharacterTag {
        public override void read(Reader reader) {
            throw new System.NotImplementedException();
        }
    }

    public class DefineBitsJPEG2 : CharacterTag {
        public FlashImage Image { get; private set; }

        public override void read(Reader reader) {
            CharacterId = reader.readUInt16();

            Image = JPEG2Image.readJpeg2(reader, Header.TagLength - 2);
        }
    }

    public class DefineBitsJPEG3 : CharacterTag {
        public FlashImage Image { get; private set; }

        public override void read(Reader reader) {
            CharacterId = reader.readUInt16();

            // This is also called JpegDataLen
            uint alphaDataOffset = reader.readUInt32();
            uint remaingData = (uint)(Header.TagLength - alphaDataOffset - 6);

            Image = JPEG3Image.readJpeg3(reader, alphaDataOffset, remaingData);
        }
    }
}
