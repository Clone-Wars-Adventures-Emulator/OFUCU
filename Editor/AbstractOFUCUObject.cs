using CWAEmu.OFUCU.Flash.Tags;
using UnityEngine;

namespace CWAEmu.OFUCU {
    public abstract class AbstractOFUCUObject : MonoBehaviour {
        public abstract void setBlendMode(EnumFlashBlendMode blendMode);
        public abstract void setMultColor(Color color);
        public abstract void setAddColor(Color color);
        public abstract void setParentMultColor(Color color);
        public abstract void setParentAddColor(Color color);
    }
}
