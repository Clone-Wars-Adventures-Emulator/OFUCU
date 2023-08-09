using CWAEmu.OFUCU.Flash.Records;
using CWAEmu.OFUCU.Flash.Tags;
using System.Collections.Generic;
using UnityEngine;

namespace CWAEmu.OFUCU {
    public class DictonaryEntry : MonoBehaviour {
        public enum EnumDictonaryCharacterType {
            Image,
            Shape,
            Sprite
        }

        public CharacterTag charTag;
        public FlashImage image;
        public EnumDictonaryCharacterType CharacterType;
        public RectTransform rt;
        public List<int> neededCharacters = new();

        public void addDependency(int charId) {
            if (!neededCharacters.Contains(charId)) {
                neededCharacters.Add(charId);
            }
        }
    }
}
