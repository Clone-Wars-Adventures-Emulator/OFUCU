using CWAEmu.OFUCU.Flash.Tags;
using UnityEngine;

namespace CWAEmu.OFUCU {
    public abstract class AbstractOFUCUObject : MonoBehaviour {
        public abstract void setBlendMode(EnumFlashBlendMode blendMode);
    }
}
