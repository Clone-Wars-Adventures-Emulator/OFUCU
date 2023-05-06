namespace CWAEmu.FlashConverter.Flash.Records {
    public class CXFormWithAlpha {
        private int nBits;

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
            cxform.nBits = reader.readBits(4);

            if (hasMult) {
                cxform.RMult = reader.readBits(cxform.nBits);
                cxform.GMult = reader.readBits(cxform.nBits);
                cxform.BMult = reader.readBits(cxform.nBits);
                cxform.AMult = reader.readBits(cxform.nBits);
            }

            if (hasAdd) {
                cxform.RAdd = reader.readBits(cxform.nBits);
                cxform.GAdd = reader.readBits(cxform.nBits);
                cxform.BAdd = reader.readBits(cxform.nBits);
                cxform.AAdd = reader.readBits(cxform.nBits);
            }

            return cxform;
        }
    }
}
