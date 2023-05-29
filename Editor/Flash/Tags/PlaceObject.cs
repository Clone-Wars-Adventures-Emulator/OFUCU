using CWAEmu.FlashConverter.Flash.Records;

namespace CWAEmu.FlashConverter.Flash.Tags {
    public class PlaceObject : FlashTag {
        public override void read(Reader reader) {
            throw new System.NotImplementedException("PlaceObject tag is unsupported.");
        }
    }

    public class PlaceObject2 : FlashTag {
        public bool HasClipActions { get; protected set; }
        public bool HasClipDepth { get; protected set; }
        public bool HasName { get; protected set; }
        public bool HasRatio { get; protected set; }
        public bool HasColorTransform { get; protected set; }
        public bool HasMatrix { get; protected set; }
        public bool HasCharacter { get; protected set; }
        public bool Move { get; protected set; }
        public ushort Depth { get; protected set; }
        public ushort CharacterId { get; protected set; }
        public Matrix Matrix { get; protected set; }
        public CXFormWithAlpha ColorTransform { get; protected set; }
        public ushort Ratio { get; protected set; }
        public string Name { get; protected set; }
        public ushort ClipDepth { get; protected set; }
        public ClipActions ClipActions { get; protected set; }

        public override void read(Reader reader) {
            HasClipActions = reader.readBitFlag();
            HasClipDepth = reader.readBitFlag();
            HasName = reader.readBitFlag();
            HasRatio = reader.readBitFlag();
            HasColorTransform = reader.readBitFlag();
            HasMatrix = reader.readBitFlag();
            HasCharacter = reader.readBitFlag();
            Move = reader.readBitFlag();

            Depth = reader.readUInt16();

            if (HasCharacter) {
                CharacterId = reader.readUInt16();
            }

            if (HasMatrix) {
                Matrix = Matrix.readMatrix(reader);
            }

            if (HasColorTransform) {
                ColorTransform = CXFormWithAlpha.readCXForm(reader);
            }

            if (HasRatio) {
                Ratio = reader.readUInt16();
            }

            if (HasName) {
                Name = reader.readString();
            }

            if (HasClipDepth) {
                ClipDepth = reader.readUInt16();
            }

            if (HasClipActions) {
                ClipActions = ClipActions.readClipActions(reader);
            }
        }
    }

    public class PlaceObject3 : PlaceObject2 {
        public bool HasImage { get; protected set; }
        public bool HasClassName { get; protected set; }
        public bool HasCacheAsBitmap { get; protected set; }
        public bool HasBlendMode { get; protected set; }
        public bool HasFilterList { get; protected set; }

        public string ClassName { get; protected set; }
        public byte BlendMode { get; protected set; }
        public FilterList SurfaceFilterList { get; protected set; }

        public override void read(Reader reader) {
            HasClipActions = reader.readBitFlag();
            HasClipDepth = reader.readBitFlag();
            HasName = reader.readBitFlag();
            HasRatio = reader.readBitFlag();
            HasColorTransform = reader.readBitFlag();
            HasMatrix = reader.readBitFlag();
            HasCharacter = reader.readBitFlag();
            Move = reader.readBitFlag();

            // reserved
            reader.readUBits(3);

            HasImage = reader.readBitFlag();
            HasClassName = reader.readBitFlag();
            HasCacheAsBitmap = reader.readBitFlag();
            HasBlendMode = reader.readBitFlag();
            HasFilterList = reader.readBitFlag();

            Depth = reader.readUInt16();

            if (HasClassName || (HasImage && HasCharacter)) {
                ClassName = reader.readString();
            }

            if (HasCharacter) {
                CharacterId = reader.readUInt16();
            }

            if (HasMatrix) {
                Matrix = Matrix.readMatrix(reader);
            }

            if (HasColorTransform) {
                ColorTransform = CXFormWithAlpha.readCXForm(reader);
            }

            if (HasRatio) {
                Ratio = reader.readUInt16();
            }

            if (HasName) {
                Name = reader.readString();
            }

            if (HasClipDepth) {
                ClipDepth = reader.readUInt16();
            }

            if (HasFilterList) {
                SurfaceFilterList = FilterList.readFilterList(reader);
            }

            if (HasClipActions) {
                ClipActions = ClipActions.readClipActions(reader);
            }
        }
    }
}
