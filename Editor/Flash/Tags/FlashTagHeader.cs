using UnityEngine;

namespace CWAEmu.OFUCU.Flash.Tags {
    [System.Serializable]
    public class FlashTagHeader {
        public EnumTagType TagType { get { return tagType; } set { tagType = value; } }
        public int TagLength { get { return tagLength; } set { tagLength = value; } }

        [SerializeField] private EnumTagType tagType;
        [SerializeField] private int tagLength;
    }
}
