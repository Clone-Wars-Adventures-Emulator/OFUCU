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
    }
}
