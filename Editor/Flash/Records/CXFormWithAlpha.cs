namespace CWAEmu.OFUCU.Flash.Records {
    public class CXFormWithAlpha {
        private uint nBits;

        public ushort RMult { get; private set; } = 256;
        public ushort GMult { get; private set; } = 256;
        public ushort BMult { get; private set; } = 256;
        public ushort AMult { get; private set; } = 256;
        public ushort RAdd { get; private set; }
        public ushort GAdd { get; private set; }
        public ushort BAdd { get; private set; }
        public ushort AAdd { get; private set; }

        public bool HasAdd { get; private set; }
        public bool HasMult { get; private set; }

        public static CXFormWithAlpha readCXForm(Reader reader) {
            CXFormWithAlpha cxform = new();

            cxform.HasAdd = reader.readBitFlag();
            cxform.HasMult = reader.readBitFlag();
            cxform.nBits = reader.readUBits(4);

            if (cxform.HasMult) {
                cxform.RMult = (ushort)reader.readBits(cxform.nBits);
                cxform.GMult = (ushort)reader.readBits(cxform.nBits);
                cxform.BMult = (ushort)reader.readBits(cxform.nBits);
                cxform.AMult = (ushort)reader.readBits(cxform.nBits);
            }

            if (cxform.HasAdd) {
                cxform.RAdd = (ushort)reader.readBits(cxform.nBits);
                cxform.GAdd = (ushort)reader.readBits(cxform.nBits);
                cxform.BAdd = (ushort)reader.readBits(cxform.nBits);
                cxform.AAdd = (ushort)reader.readBits(cxform.nBits);
            }

            reader.endBitRead();

            return cxform;
        }
    }
}
