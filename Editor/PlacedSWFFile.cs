using CWAEmu.OFUCU.Flash;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CWAEmu.OFUCU {
    public class PlacedSWFFile : MonoBehaviour {
        public SWFFile File;
        public Dictionary<int, DictonaryEntry> dictonary;

        public void runOnAllOfType(Action<DictonaryEntry> action, DictonaryEntry.EnumDictonaryCharacterType type) {
            foreach (var pair in dictonary) {
                if (pair.Value.CharacterType == type) {
                    action(pair.Value);
                }
            }
        }
    }
}
