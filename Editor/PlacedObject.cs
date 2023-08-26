using System.Collections.Generic;
using UnityEngine;

namespace CWAEmu.OFUCU {
    public class PlacedObject : MonoBehaviour {
        public DictonaryEntry placedEntry;

        public void fillIn() {
            // This should only be handled by sprites
            if (this is PlacedImage) {
                return;
            }

            // TODO: this should copy the entry and place it as 
            // - A child of this object? (dont necessarily like cause more objects)
            // - As this object (in place)
        }
    }
}
