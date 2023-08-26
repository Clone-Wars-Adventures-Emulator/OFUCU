namespace CWAEmu.OFUCU.Flash.Records {
    public class CXFormWithAlpha {
        private uint nBits;

        public byte RMult { get; private set; } = 1;
        public byte GMult { get; private set; } = 1;
        public byte BMult { get; private set; } = 1;
        public byte AMult { get; private set; } = 1;
        public byte RAdd { get; private set; }
        public byte GAdd { get; private set; }
        public byte BAdd { get; private set; }
        public byte AAdd { get; private set; }

        public bool HasAdd { get; private set; }
        public bool HasMult { get; private set; }

        public static CXFormWithAlpha readCXForm(Reader reader) {
            CXFormWithAlpha cxform = new();

            cxform.HasAdd = reader.readBitFlag();
            cxform.HasMult = reader.readBitFlag();
            cxform.nBits = reader.readUBits(4);

            if (cxform.HasMult) {
                cxform.RMult = (byte)reader.readBits(cxform.nBits);
                cxform.GMult = (byte)reader.readBits(cxform.nBits);
                cxform.BMult = (byte)reader.readBits(cxform.nBits);
                cxform.AMult = (byte)reader.readBits(cxform.nBits);
            }

            if (cxform.HasAdd) {
                cxform.RAdd = (byte)reader.readBits(cxform.nBits);
                cxform.GAdd = (byte)reader.readBits(cxform.nBits);
                cxform.BAdd = (byte)reader.readBits(cxform.nBits);
                cxform.AAdd = (byte)reader.readBits(cxform.nBits);
            }

            reader.endBitRead();

            return cxform;
        }
    }
}
