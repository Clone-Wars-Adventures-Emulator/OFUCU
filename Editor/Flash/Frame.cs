using CWAEmu.OFUCU.Flash.Tags;
using System.Collections.Generic;

namespace CWAEmu.OFUCU.Flash {
    public class Frame {
        // Flash treats frame indexes as 1 based, so start at 1
        public int FrameIndex { get; set; } = 1;
        public List<FlashTag> Tags { get; private set; } = new();
        public string Label { get; private set; } = null;

        public void addTag(FlashTag tag) {
            Tags.Add(tag);

            if (tag is FrameLabel) {
                Label = ((FrameLabel)tag).Label;
            }
        }
    }
}
