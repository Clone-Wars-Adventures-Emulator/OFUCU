using CWAEmu.OFUCU.Flash.Tags;

namespace CWAEmu.OFUCU.Flash {
    public class JPEGTable : FlashTag {
        public byte[] TableData { get; private set; }
        public override void read(Reader reader) {
            TableData = reader.readBytes(Header.TagLength);
        }
    }
}
