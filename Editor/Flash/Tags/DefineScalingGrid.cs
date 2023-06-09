using CWAEmu.FlashConverter.Flash.Records;

namespace CWAEmu.FlashConverter.Flash.Tags {
    public class DefineScalingGrid : FlashTag {
        public ushort CharacterId { get; private set; }
        public Rect Splitter { get; private set; }

        public override void read(Reader reader) {
            CharacterId = reader.readUInt16();

            Splitter = Rect.readRect(reader);
        }
    }
}
