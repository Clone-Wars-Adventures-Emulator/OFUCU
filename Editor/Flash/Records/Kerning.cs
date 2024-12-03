namespace CWAEmu.OFUCU.Flash.Records {
    public class KerningRecord {
        public ushort KerningCode1 { get; private set; }
        public ushort KerningCode2 { get; private set; }
        public short KerningAdjustment { get; private set; }

        public static KerningRecord readRecord(Reader reader, bool wide) {
            return new KerningRecord {
                KerningCode1 = wide ? reader.readUInt16() : reader.readByte(),
                KerningCode2 = wide ? reader.readUInt16() : reader.readByte(),
                KerningAdjustment = reader.readInt16()
            };
        }
    }
}
