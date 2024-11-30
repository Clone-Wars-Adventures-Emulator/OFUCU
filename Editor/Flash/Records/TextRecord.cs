using CWAEmu.OFUCU.Flash.Records;

namespace CWAEmu.OFUCU.Flash {
    public class TextRecord {
        public byte Type { get; private set; }
        public bool HasFont { get; private set; }
        public bool HasColor { get; private set; }
        public bool HasYOffset { get; private set; }
        public bool HasXOffset { get; private set; }
        public ushort FontID { get; private set; }
        public Color Color { get; private set; }
        public ushort XOffset { get; private set; }
        public ushort YOffset { get; private set; }
        public ushort Height { get; private set; }
        public byte GlyphCount { get; private set; }
        public GlyphEntry[] Glyphs { get; private set; }

        public static TextRecord readTextRecord(Reader reader, byte firstByte, byte glyphBits, byte advanceBits, int type) {
            TextRecord record = new() {
                HasFont =    ((firstByte & 0b1000) >> 3) == 1,
                HasColor =   ((firstByte & 0b0100) >> 2) == 1,
                HasYOffset = ((firstByte & 0b0010) >> 1) == 1,
                HasXOffset =  (firstByte & 0b0001) == 1
            };

            if (record.HasFont) {
                record.FontID = reader.readUInt16();
            }

            if (record.HasColor) {
                if (type == 2) {
                    record.Color = Color.readRGBA(reader);
                } else {
                    record.Color = Color.readRGB(reader);
                }
            }

            if (record.HasXOffset) {
                record.XOffset = reader.readUInt16();
            }

            if (record.HasYOffset) {
                record.YOffset = reader.readUInt16();
            }

            record.Height = reader.readUInt16();
            record.GlyphCount = reader.readByte();

            var glyphs = new GlyphEntry[record.GlyphCount];
            for (int i = 0; i < glyphs.Length; i++) {
                glyphs[i] = GlyphEntry.readGliphEntry(reader, glyphBits, advanceBits);
            }
            reader.endBitRead();
            record.Glyphs = glyphs;

            return record;
        }
    }

    public class GlyphEntry {
        public uint GlyphIndex { get; private set; }
        public int GlyphAdvance { get; private set; }

        public static GlyphEntry readGliphEntry(Reader reader, byte indexBits, byte advanceBits) {
            GlyphEntry entry = new();
            entry.GlyphIndex = reader.readUBits(indexBits);
            entry.GlyphAdvance = reader.readBits(advanceBits);
            return entry;
        }
    }
}
