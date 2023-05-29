namespace CWAEmu.FlashConverter.Flash.Records {
    public class ActionRecord  {
        public byte ActionCode { get; set; }
        public byte[] AdditionalData { get; set; } = null;

        public static ActionRecord readActionRecord(Reader reader) {
            ActionRecord actionRecord = new();

            actionRecord.ActionCode = reader.readByte();
            if (actionRecord.ActionCode >= 0x80) {
                var additionalLength = reader.readUInt16();
                actionRecord.AdditionalData = reader.readBytes(additionalLength);
            }

            return actionRecord;
        }

        public uint getSize() {
            uint bytes = 1;
            if (AdditionalData != null) {
                bytes += 2;
                bytes += (uint)AdditionalData.Length;
            }
            return bytes;
        }
    }
}
