using UnityEngine.TextCore.Text;

namespace CWAEmu.FlashConverter.Flash.Tags {
    public class RemoveObject : FlashTag {
        public ushort CharacterId { get; private set; }
        public ushort Depth { get; private set; }

        public override void read(Reader reader) {
            CharacterId = reader.readUInt16();
            Depth = reader.readUInt16();
        }
    }

    public class RemoveObject2 : FlashTag {
        public ushort Depth { get; private set; }

        public override void read(Reader reader) {
            Depth = reader.readUInt16();
        }
    }
}
