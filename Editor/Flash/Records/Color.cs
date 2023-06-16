namespace CWAEmu.FlashConverter.Flash.Records {
    public class Color {
        public byte R { get; private set; }
        public byte G { get; private set; }
        public byte B { get; private set; }
        public byte A { get; private set; }
        
        public Color Black => new Color() { r = 0, b = 0, g = 0, a = 255 };
        public Color White => new Color() { r = 255, b = 255, g = 255, a = 255 };

        public static Color readARGB(Reader reader) {
            Color argb = new() {
                A = reader.readUInt8(),
                R = reader.readUInt8(),
                G = reader.readUInt8(),
                B = reader.readUInt8()
            };

            return argb;
        }

        public static Color readRGBA(Reader reader) {
            Color rgba = new() {
                R = reader.readUInt8(),
                G = reader.readUInt8(),
                B = reader.readUInt8(),
                A = reader.readUInt8()
            };

            return rgba;
        }

        public static Color readRGB(Reader reader) {
            Color rgba = new() {
                R = reader.readUInt8(),
                G = reader.readUInt8(),
                B = reader.readUInt8(),
                A = 255
            };

            return rgba;
        }

        public static Color readPIX24(Reader reader) {
            reader.readUInt8();
            Color rgba = new() {
                R = reader.readUInt8(),
                G = reader.readUInt8(),
                B = reader.readUInt8(),
                A = 255
            };

            return rgba;
        }

        public static Color readPIX15(Reader reader) {
            reader.readBits(1);
            Color rgba = new() {
                R = (byte)reader.readUBits(5),
                G = (byte)reader.readUBits(5),
                B = (byte)reader.readUBits(5),
                A = 255
            };

            reader.endBitRead();

            return rgba;
        }

    }
}
