namespace CWAEmu.FlashConverter.Flash.Records {
    public class ARGB {
        public byte A { get; private set; }
        public byte R { get; private set; }
        public byte G { get; private set; }
        public byte B { get; private set; }

        public static ARGB readARGB(Reader reader) {
            ARGB argb = new() {
                A = reader.readUInt8(),
                R = reader.readUInt8(),
                G = reader.readUInt8(),
                B = reader.readUInt8()
            };

            return argb;
        }
    }
}
