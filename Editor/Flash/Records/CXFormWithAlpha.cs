namespace CWAEmu.OFUCU.Flash.Records {
    public class CXFormWithAlpha {
        private uint nBits;

        public float RMult { get; private set; } = 1;
        public float GMult { get; private set; } = 1;
        public float BMult { get; private set; } = 1;
        public float AMult { get; private set; } = 1;
        public float RAdd { get; private set; }
        public float GAdd { get; private set; }
        public float BAdd { get; private set; }
        public float AAdd { get; private set; }

        public static CXFormWithAlpha readCXForm(Reader reader) {
            CXFormWithAlpha cxform = new();

            bool hasAdd = reader.readBitFlag();
            bool hasMult = reader.readBitFlag();
            cxform.nBits = reader.readUBits(4);

            if (hasMult) {
                cxform.RMult = reader.readUBits(cxform.nBits);
                cxform.GMult = reader.readUBits(cxform.nBits);
                cxform.BMult = reader.readUBits(cxform.nBits);
                cxform.AMult = reader.readUBits(cxform.nBits);
            }

            if (hasAdd) {
                cxform.RAdd = reader.readUBits(cxform.nBits);
                cxform.GAdd = reader.readUBits(cxform.nBits);
                cxform.BAdd = reader.readUBits(cxform.nBits);
                cxform.AAdd = reader.readUBits(cxform.nBits);
            }

            reader.endBitRead();

            return cxform;
        }
    }
}
