using System.Collections.Generic;

namespace CWAEmu.OFUCU.Flash.Records {
    public class ActionRecord  {
        public byte ActionCode { get; set; }
        public byte[] AdditionalData { get; set; } = null;

        public static ActionRecord readActionRecord(Reader reader) {
            return readImpl(reader, reader.readByte());
        }

        public static List<ActionRecord> readActionRecordList(Reader reader) {
            List<ActionRecord> arl = new();

            byte first = reader.readByte();
            while (first != 0) {
                arl.Add(readImpl(reader, first));

                first = reader.readByte();
            }

            return arl;
        }

        private static ActionRecord readImpl(Reader reader, byte first) {
            ActionRecord actionRecord = new();

            actionRecord.ActionCode = first;
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
