namespace CWAEmu.FlashConverter.Flash.Records {
    public class Matrix {
        private int nScaleBits = 0;
        private int nRotateBits = 0;
        private int nTranslateBits = 0;

        public float ScaleX { get; private set; } = 1;
        public float ScaleY { get; private set; } = 1;
        public float RotateSkew0 { get; private set; } = 0;
        public float RotateSkew1 { get; private set; } = 0;
        public int TranslateX { get; private set; }
        public int TranslateY { get; private set; }

        public static Matrix readMatrix(Reader reader) {
            Matrix matrix = new();

            bool hasScale = reader.readBits(1) == 1;
            if (hasScale) {
                matrix.nScaleBits = (int)reader.readUBits(5);
                matrix.ScaleX = reader.readFixedBits(matrix.nScaleBits);
                matrix.ScaleY = reader.readFixedBits(matrix.nScaleBits);
            }

            bool hasRotate = reader.readBits(1) == 1;
            if (hasRotate) {
                matrix.nRotateBits = (int)reader.readUBits(5);
                matrix.RotateSkew0 = reader.readFixedBits(matrix.nRotateBits);
                matrix.RotateSkew1 = reader.readFixedBits(matrix.nRotateBits);
            }

            matrix.nTranslateBits = (int)reader.readUBits(5);
            matrix.TranslateX = reader.readBits(matrix.nTranslateBits);
            matrix.TranslateY = reader.readBits(matrix.nTranslateBits);

            reader.endBitRead();

            return matrix;
        }
    }
}
