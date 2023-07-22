using System;

namespace CWAEmu.OFUCU.Flash.Records {
    public class Rect {
        private int nBits;
        public int ByteLength => (5 + nBits * 4) / 8 + 1;
        public float X => XMinTwips / 20.0f;
        public float Y => YMinTwips / 20.0f;
        public float Width => Math.Abs(XMaxTwips - XMinTwips) / 20.0f;
        public float Height => Math.Abs(YMaxTwips - YMinTwips) / 20.0f;
        public int XMinTwips { get; private set; }
        public int YMinTwips { get; private set; }
        public int XMaxTwips { get; private set; }
        public int YMaxTwips { get; private set; }

        public static Rect readRect(Reader reader) {
            Rect rect = new();

            rect.nBits = (int) reader.readUBits(5);
            rect.XMinTwips = reader.readBits(rect.nBits);
            rect.XMaxTwips = reader.readBits(rect.nBits);
            rect.YMinTwips = reader.readBits(rect.nBits);
            rect.YMaxTwips = reader.readBits(rect.nBits);

            reader.endBitRead();

            return rect;
        }
    }
}
