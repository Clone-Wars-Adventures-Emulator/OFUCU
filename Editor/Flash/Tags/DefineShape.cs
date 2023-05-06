using CWAEmu.FlashConverter.Flash.Records;

namespace CWAEmu.FlashConverter.Flash.Tags {
    public class DefineShape : CharacterTag {
        public Rect ShapeBounds { get; private set; }

        public override void read(Reader reader) {
            CharacterId = reader.readUInt16();

            ShapeBounds = Rect.readRect(reader);

            // TODO: rest of the shape with style stuffs
            reader.skip(Header.TagLength - ShapeBounds.ByteLength - 2);
        }
    }
}
