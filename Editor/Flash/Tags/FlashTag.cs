using UnityEngine;

namespace CWAEmu.OFUCU.Flash.Tags {
    [System.Serializable]
    public class FlashTag {
        public FlashTagHeader Header { get { return header; } set { header = value; } }
        [SerializeField] private FlashTagHeader header;

        private protected FlashTag() { }

        public virtual void read(Reader reader) { }
    }
}
