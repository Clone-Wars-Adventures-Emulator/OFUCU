using CWAEmu.FlashConverter.Flash.Records;

namespace CWAEmu.FlashConverter.Flash.Tags {
    public class DefineShape : CharacterTag {
        public int ShapeType { get; set; }
        public Rect ShapeBounds { get; private set; }
        public Rect EdgeBounds { get; private set; }
        public bool UsesNonScalingStrokes { get; private set; }
        public bool UsesScalingStrokes { get; private set; }
        public ShapeWithStyle Shapes { get; private set; }

        public override void read(Reader reader) {
            CharacterId = reader.readUInt16();

            ShapeBounds = Rect.readRect(reader);

            if (ShapeType == 4) {
                EdgeBounds = Rect.readRect(reader);

                reader.readBits(6);

                UsesNonScalingStrokes = reader.readBitFlag();
                UsesScalingStrokes = reader.readBitFlag();
                reader.endBitRead();
            }

            Shapes = ShapeWithStyle.readShapeWithStyle(reader, ShapeType);
        }
    }
}
