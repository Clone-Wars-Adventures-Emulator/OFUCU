using CWAEmu.FlashConverter.Flash.Records;
using System.Collections.Generic;

namespace CWAEmu.FlashConverter.Flash.Tags {
    public class DefineButton : CharacterTag {
        public List<ButtonRecord> ButtonRecords { get; private set; } = new();
        public List<ActionRecord> Actions { get; private set; } = new();

        public override void read(Reader reader) {
            CharacterId = reader.readUInt16();

            byte first = reader.readByte();
            while (first != 0) {
                ButtonRecords.Add(ButtonRecord.ReadButtonRecord(reader, first, 1));

                first = reader.readByte();
            }

            Actions = ActionRecord.readActionRecordList(reader);
        }
    }

    public class DefineButton2 : CharacterTag {
        public byte Reserved { get; private set; }
        public bool TrackAsMenu { get; private set; }
        public List<ButtonRecord> ButtonRecords { get; private set; } = new();
        public List<ButtonCondAction> Actions { get; private set; } = new();

        public override void read(Reader reader) {
            CharacterId = reader.readUInt16();

            Reserved = (byte)reader.readUBits(7);

            TrackAsMenu = reader.readBitFlag();

            ushort actionOffset = reader.readUInt16();

            byte first = reader.readByte();
            while (first != 0) {
                ButtonRecords.Add(ButtonRecord.ReadButtonRecord(reader, first, 2));

                first = reader.readByte();
            }

            if (actionOffset != 0) {
                ushort condActionSize = reader.readUInt16();
                while (condActionSize != 0) {
                    Actions.Add(ButtonCondAction.readButtonCondAction(reader, condActionSize));

                    condActionSize = reader.readUInt16();
                }
            }
        }
    }
}
