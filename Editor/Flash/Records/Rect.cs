namespace CWAEmu.FlashConverter.Flash.Records {
    public class Rect {
        private int nBits;
        public float X => XTwips / 20;
        public float Y => YTwips / 20;
        public float Width => WidthTwips / 20;
        public float Height => HeightTwips / 20;
        public int XTwips { get; private set; }
        public int YTwips { get; private set; }
        public int WidthTwips { get; private set; }
        public int HeightTwips { get; private set; }

        public static Rect readRect(Reader reader) {
            Rect rect = new();

            rect.nBits = (int) reader.readUBits(5);
            rect.XTwips = reader.readBits(rect.nBits);
            rect.WidthTwips = reader.readBits(rect.nBits);
            rect.YTwips = reader.readBits(rect.nBits);
            rect.HeightTwips = reader.readBits(rect.nBits);

            reader.endBitRead();

            return rect;
        }
    }
}
