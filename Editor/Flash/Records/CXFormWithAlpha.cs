namespace CWAEmu.OFUCU.Flash.Records {
    public class CXFormWithAlpha {
        private uint nBits;

        public short RMult { get; private set; } = 256;
        public short GMult { get; private set; } = 256;
        public short BMult { get; private set; } = 256;
        public short AMult { get; private set; } = 256;
        public short RAdd { get; private set; }
        public short GAdd { get; private set; }
        public short BAdd { get; private set; }
        public short AAdd { get; private set; }

        public bool HasAdd { get; private set; }
        public bool HasMult { get; private set; }

        public static CXFormWithAlpha readCXForm(Reader reader) {
            CXFormWithAlpha cxform = new() {
                HasAdd = reader.readBitFlag(),
                HasMult = reader.readBitFlag(),
                nBits = reader.readUBits(4)
            };

            if (cxform.HasMult) {
                cxform.RMult = (short)reader.readBits(cxform.nBits);
                cxform.GMult = (short)reader.readBits(cxform.nBits);
                cxform.BMult = (short)reader.readBits(cxform.nBits);
                cxform.AMult = (short)reader.readBits(cxform.nBits);
            }

            if (cxform.HasAdd) {
                cxform.RAdd = (short)reader.readBits(cxform.nBits);
                cxform.GAdd = (short)reader.readBits(cxform.nBits);
                cxform.BAdd = (short)reader.readBits(cxform.nBits);
                cxform.AAdd = (short)reader.readBits(cxform.nBits);
            }

            reader.endBitRead();

            return cxform;
        }
    }
}
