namespace CWAEmu.FlashConverter.Flash.Records {
    public class CXForm {
        private int nBits;

        public float RMult { get; private set; } = 1;
        public float GMult { get; private set; } = 1;
        public float BMult { get; private set; } = 1;
        public float RAdd { get; private set; }
        public float GAdd { get; private set; }
        public float BAdd { get; private set; }

        public static CXForm readCXForm(Reader reader) {
            CXForm cxform = new();

            bool hasAdd = reader.readBits(1) == 1;
            bool hasMult = reader.readBits(1) == 1;
            cxform.nBits = reader.readBits(4);

            if (hasMult) {
                cxform.RMult = reader.readBits(cxform.nBits);
                cxform.GMult = reader.readBits(cxform.nBits);
                cxform.BMult = reader.readBits(cxform.nBits);
            }

            if (hasAdd) {
                cxform.RAdd = reader.readBits(cxform.nBits);
                cxform.GAdd = reader.readBits(cxform.nBits);
                cxform.BAdd = reader.readBits(cxform.nBits);
            }

            return cxform;
        }
    }
}
