using CWAEmu.OFUCU.Flash.Records;

namespace CWAEmu.OFUCU.Flash.Tags {
    // TODO: support for DefineFont2 and other font tags?

    public class DefineFont3 : CharacterTag {
        public bool HasLayout { get; private set; }
        public bool ShiftJIS { get; private set; }
        public bool SmallText { get; private set; }
        public bool ANSI { get; private set; }
        public bool WideOffsets { get; private set; }
        public bool Italic { get; private set; }
        public bool Bold { get; private set; }
        public byte Langcode { get; private set; }
        public string FontName { get; private set; }
        public ushort NumGlyphs { get; private set; }
        public uint[] OffsetTable { get; private set; }
        public uint CodeTableOffset { get; private set; }
        // You wanna render you own shapes????  (・_・)
        public ShapeRecord[] GlyphShapeTable { get; private set; }
        public ushort[] CodeTable { get; private set; }
        public short Ascent { get; private set; }
        public short Descent { get; private set; }
        public short Leading { get; private set; }
        public short[] AdvanceTable { get; private set; }
        public Rect[] BoundsTable { get; private set; }
        public ushort KerningCount { get; private set; }
        public KerningRecord[] KerningTable { get; private set; }

        public override void read(Reader reader) {
            CharacterId = reader.readUInt16();

            HasLayout = reader.readBitFlag();
            ShiftJIS = reader.readBitFlag();
            SmallText = reader.readBitFlag();
            ANSI = reader.readBitFlag();
            WideOffsets = reader.readBitFlag();
            // TODO: validate that this is always one...
            var wideCodes = reader.readBitFlag();
            Italic = reader.readBitFlag();
            Bold = reader.readBitFlag();

            Langcode = reader.readByte();

            FontName = reader.readLengthEncodedString();

            NumGlyphs = reader.readUInt16();

            uint offsetTableBytes = 0;
            OffsetTable = new uint[NumGlyphs];
            for (int i = 0; i < NumGlyphs; i++) {
                if (WideOffsets) {
                    OffsetTable[i] = reader.readUInt32();
                    offsetTableBytes += 4;
                } else {
                    OffsetTable[i] = reader.readUInt16();
                    offsetTableBytes += 2;
                }
            }

            if (WideOffsets) {
                CodeTableOffset = reader.readUInt32();
                offsetTableBytes += 4;
            } else {
                CodeTableOffset = reader.readUInt16();
                offsetTableBytes += 2;
            }

            GlyphShapeTable = new ShapeRecord[NumGlyphs];
            // TODO: read the shape table
            // For now, skip the shape table (save sanity and its not used yet)

            uint bytesToSkip = CodeTableOffset - offsetTableBytes;
            reader.skip((int) bytesToSkip);

            CodeTable = new ushort[NumGlyphs];
            for (int i = 0; i < NumGlyphs; i++) {
                CodeTable[i] = reader.readUInt16();
            }

            if (HasLayout) {
                Ascent = reader.readInt16();
                Descent = reader.readInt16();
                Leading = reader.readInt16();

                AdvanceTable = new short[NumGlyphs];
                for (int i = 0; i < NumGlyphs; i++) {
                    AdvanceTable[i] = reader.readInt16();
                }

                BoundsTable = new Rect[NumGlyphs];
                for (int i = 0; i < NumGlyphs; i++) {
                    BoundsTable[i] = Rect.readRect(reader);
                }

                KerningCount = reader.readUInt16();
                KerningTable = new KerningRecord[KerningCount];
                for (int i = 0; i < KerningCount; i++) {
                    KerningTable[i] = KerningRecord.readRecord(reader, WideOffsets);
                }
            }
        }
    }
}
