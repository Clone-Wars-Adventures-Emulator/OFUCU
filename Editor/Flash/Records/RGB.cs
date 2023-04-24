namespace CWAEmu.FlashConverter.Flash.Records {
    public class RGB {
        public byte R { get; private set; }
        public byte G { get; private set; }
        public byte B { get; private set; }

        public static RGB readRGB(Reader reader) {
            RGB rgb = new() {
                R = reader.readUInt8(),
                G = reader.readUInt8(),
                B = reader.readUInt8(),
            };

            return rgb;
        }
    }
}
