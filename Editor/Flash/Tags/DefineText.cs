using CWAEmu.OFUCU.Flash.Records;
using System.Collections.Generic;

namespace CWAEmu.OFUCU.Flash.Tags {
    public class DefineText : CharacterTag {
        public int Type { get; set; }
        public Rect TextBounds { get; private set; }
        public Matrix TextMatrix { get; private set; }
        public byte GlyphBits { get; private set; }
        public byte AdvanceBits { get; private set; }
        public TextRecord[] Records { get; private set; }

        public override void read(Reader reader) {
            CharacterId = reader.readUInt16();

            TextBounds = Rect.readRect(reader);
            TextMatrix = Matrix.readMatrix(reader);
            GlyphBits = reader.readByte();
            AdvanceBits = reader.readByte();

            List<TextRecord> records = new();
            byte firstByte = reader.readByte();
            while (firstByte != 0) {
                var record = TextRecord.readTextRecord(reader, firstByte, GlyphBits, AdvanceBits, Type);
                records.Add(record);
                firstByte = reader.readByte();
            }
            Records = records.ToArray();
        }
    }
}
