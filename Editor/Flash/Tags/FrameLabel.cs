namespace CWAEmu.FlashConverter.Flash.Tags {
    public class FrameLabel : FlashTag {
        public string Label { get; private set; }

        public override void read(Reader reader) {
            Label = reader.readString();
        }
    }
}
