namespace CWAEmu.FlashConverter.Flash.Tags {
    public abstract class FlashTag {
        public FlashTagHeader Header { get; set; }

        public abstract void read(Reader reader);
    }
}
