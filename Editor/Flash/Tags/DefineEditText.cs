using CWAEmu.OFUCU.Flash.Records;

namespace CWAEmu.OFUCU.Flash.Tags {
    public class DefineEditText : CharacterTag {
        public Rect Bounds { get; private set; }
        public bool HasText { get; private set; }
        public bool WordWrap { get; private set; }
        public bool Multiline { get; private set; }
        public bool Password { get; private set; }
        public bool ReadOnly { get; private set; }
        public bool HasTextColor { get; private set; }
        public bool HasMaxLength { get; private set; }
        public bool HasFont { get; private set; }
        public bool HasFontClass { get; private set; }
        public bool AutoSize { get; private set; }
        public bool HasLayout { get; private set; }
        public bool NoSelect { get; private set; }
        public bool Border { get; private set; }
        public bool WasStatic { get; private set; }
        public bool HTML { get; private set; }
        public bool UseOutlines { get; private set; }
        public ushort FontId { get; private set; }
        public string FontClass { get; private set; }
        public float FontHeight => FontHeightTwips / 20.0f;
        public ushort FontHeightTwips { get; private set; }
        public Color TextColor { get; private set; }
        public ushort MaxLength { get; private set; }
        public byte Align { get; private set; }
        public ushort LeftMargin { get; private set; }
        public ushort RightMargin { get; private set; }
        public ushort Indent { get; private set; }
        public short Leading { get; private set; }
        public string VariableName { get; private set; }
        public string InitialText { get; private set; }

        public override void read(Reader reader) {
            CharacterId = reader.readUInt16();

            Bounds = Rect.readRect(reader);

            HasText = reader.readBitFlag();
            WordWrap = reader.readBitFlag();
            Multiline = reader.readBitFlag();
            Password = reader.readBitFlag();
            ReadOnly = reader.readBitFlag();
            HasTextColor = reader.readBitFlag();
            HasMaxLength = reader.readBitFlag();
            HasFont = reader.readBitFlag();
            HasFontClass = reader.readBitFlag();
            AutoSize = reader.readBitFlag();
            HasLayout = reader.readBitFlag();
            NoSelect = reader.readBitFlag();
            Border = reader.readBitFlag();
            WasStatic = reader.readBitFlag();
            HTML = reader.readBitFlag();
            UseOutlines = reader.readBitFlag();

            if (HasFont) {
                FontId = reader.readUInt16();
            }

            if (HasFontClass) {
                FontClass = reader.readString();
            }

            if (HasFont) {
                FontHeightTwips = reader.readUInt16();
            }

            if (HasTextColor) {
                TextColor = Color.readRGBA(reader);
            }

            if (HasMaxLength) {
                MaxLength = reader.readUInt16();
            }

            if (HasLayout) {
                Align = reader.readByte();
                LeftMargin = reader.readUInt16();
                RightMargin = reader.readUInt16();
                Indent = reader.readUInt16();
                Leading = reader.readInt16();
            }

            VariableName = reader.readString();

            if (HasText) {
                InitialText = reader.readString();
            }
        }
    }
}
