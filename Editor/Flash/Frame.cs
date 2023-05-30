using CWAEmu.FlashConverter.Flash.Tags;
using System.Collections.Generic;

namespace CWAEmu.FlashConverter.Flash {
    public class Frame {
        public List<FlashTag> Tags { get; private set; } = new();

        public void addTag(FlashTag tag) {
            Tags.Add(tag);
        }
    }
}
