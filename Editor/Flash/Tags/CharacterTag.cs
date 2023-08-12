using CWAEmu.OFUCU.Flash.Records;
using UnityEngine;

namespace CWAEmu.OFUCU.Flash.Tags {
    [System.Serializable]
    public class CharacterTag : FlashTag {
        private protected CharacterTag() { }
        
        public ushort CharacterId { get { return charId; } protected set { charId = value; } }
        [SerializeField] private ushort charId;
    }

    public class ImageCharacterTag : CharacterTag {
        public FlashImage Image { get; private protected set; }
    }
}
