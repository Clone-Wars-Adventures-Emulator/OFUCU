namespace CWAEmu.FlashConverter.Flash.Records {
    public class RGBA {
        public byte R { get; private set; }
        public byte G { get; private set; }
        public byte B { get; private set; }
        public byte A { get; private set; }

        public static RGBA readRGBA(Reader reader) {
            RGBA rgba = new() {
                R = reader.readUInt8(),
                G = reader.readUInt8(),
                B = reader.readUInt8(),
                A = reader.readUInt8()
            };

            return rgba;
        }

        public static RGBA readRGBasRGBA(Reader reader) {
            RGBA rgba = new() {
                R = reader.readUInt8(),
                G = reader.readUInt8(),
                B = reader.readUInt8(),
                A = 255
            };

            return rgba;
        }

        public static RGBA readARGBasRGBA(Reader reader) {
            RGBA rgba = new() {
                A = reader.readUInt8(),
                R = reader.readUInt8(),
                G = reader.readUInt8(),
                B = reader.readUInt8()
            };

            return rgba;
        }

        public static RGBA readPIX24asRGBA(Reader reader) {
            reader.readUInt8();
            RGBA rgba = new() {
                R = reader.readUInt8(),
                G = reader.readUInt8(),
                B = reader.readUInt8(),
                A = 255
            };

            return rgba;
        }

        public static RGBA readPIX15asRGBA(Reader reader) {
            reader.readBits(1);
            RGBA rgba = new() {
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
