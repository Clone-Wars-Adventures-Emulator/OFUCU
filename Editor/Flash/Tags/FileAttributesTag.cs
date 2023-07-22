namespace CWAEmu.OFUCU.Flash.Tags {
    public class FileAttributesTag : FlashTag {
        public const ushort TAG_TYPE = 69;

        public bool HasMetadata { get; private set; }
        public bool ActionScript3 { get; private set; }
        public bool UseNetwork { get; private set; }

        public override void read(Reader reader) {
            // reserved
            reader.readBits(3);

            HasMetadata = reader.readBitFlag();

            ActionScript3 = reader.readBitFlag();

            // reserved
            reader.readBits(2);

            UseNetwork = reader.readBitFlag();

            // reserved
            reader.readBits(24);

            reader.endBitRead();
        }

        public static FileAttributesTag readTag(FlashTagHeader header, Reader reader) {
            FileAttributesTag tag = new() {
                Header = header
            };
            tag.read(reader);

            return tag;
        }
    }
}
