using System.Collections.Generic;

namespace CWAEmu.OFUCU.Flash.Records {
    public class ButtonRecord {
        public byte Reserved { get; private set; }
        public bool HasBlendMode { get; private set; }
        public bool HasFilterList { get; private set; }
        public bool StateHitTest { get; private set; }
        public bool StateDown { get; private set; }
        public bool StateOver { get; private set; }
        public bool StateUp { get; private set; }
        public ushort CharacterId { get; private set; }
        public ushort PlaceDepth { get; private set; }
        public Matrix Matrix { get; private set; }
        public CXFormWithAlpha ColorTransform { get; private set; }
        public FilterList FilterList { get; private set; }
        public byte BlendMode { get; private set; }

        public static ButtonRecord ReadButtonRecord(Reader reader, byte firstByte, int buttonType) {
            ButtonRecord br = new();

            br.Reserved = (byte)((firstByte & 0b1100_0000) >> 6);
            br.HasBlendMode = (firstByte & 0b0010_0000) == 0b0010_0000;
            br.HasFilterList = (firstByte & 0b0001_0000) == 0b0001_0000;
            br.StateHitTest = (firstByte & 0b0000_1000) == 0b0000_1000;
            br.StateDown = (firstByte & 0b0000_0100) == 0b0000_0100;
            br.StateOver = (firstByte & 0b0000_0010) == 0b0000_0010;
            br.StateUp = (firstByte & 0b0000_0001) == 0b0000_0001;

            br.CharacterId = reader.readUInt16();
            br.PlaceDepth = reader.readUInt16();
            br.Matrix = Matrix.readMatrix(reader);

            if (buttonType == 2) {
                br.ColorTransform = CXFormWithAlpha.readCXForm(reader);
            
                if (br.HasFilterList) {
                    br.FilterList = FilterList.readFilterList(reader);
                }

                if (br.HasBlendMode) {
                    br.BlendMode = reader.readByte();
                }
            }

            return br;
        }
    }

    public class ButtonCondAction {
        public ushort CondActionSize { get; private set; }
        public bool IdleToOverDown { get; private set; }
        public bool OutDownToIdle { get; private set; }
        public bool OutDownToOverDown { get; private set; }
        public bool OverDownToOutDown { get; private set; }
        public bool OverDownToOverUp { get; private set; }
        public bool OverUpToOverDown { get; private set; }
        public bool OverUpToIdle { get; private set; }
        public bool IdleToOverUp { get; private set; }
        public byte KeyPress { get; private set; }
        public bool OverDownToIdle { get; private set; }
        public List<ActionRecord> Actions { get; private set; } = new();

        public static ButtonCondAction readButtonCondAction(Reader reader, ushort condActionSize) {
            ButtonCondAction bca = new();

            bca.CondActionSize = condActionSize;
            bca.IdleToOverDown = reader.readBitFlag();
            bca.OutDownToIdle = reader.readBitFlag();
            bca.OutDownToOverDown = reader.readBitFlag();
            bca.OverDownToOutDown = reader.readBitFlag();
            bca.OverDownToOverUp = reader.readBitFlag();
            bca.OverUpToOverDown = reader.readBitFlag();
            bca.OverUpToIdle = reader.readBitFlag();
            bca.IdleToOverUp = reader.readBitFlag();
            bca.KeyPress = (byte)reader.readUBits(7);
            bca.OverDownToIdle = reader.readBitFlag();
            bca.Actions = ActionRecord.readActionRecordList(reader);

            return bca;
        }
    }
}
