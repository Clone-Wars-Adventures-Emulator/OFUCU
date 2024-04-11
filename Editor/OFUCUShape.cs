using CWAEmu.OFUCU.Flash.Tags;
using CWAEmu.OFUCU.Runtime;
using Unity.VectorGraphics;
using UnityEngine;

namespace CWAEmu.OFUCU {
    [RequireComponent(typeof(RuntimeShape))]
    public class OFUCUShape : AbstractOFUCUObject {
        public override void setBlendMode(EnumFlashBlendMode blendMode) {
            var svgi = gameObject.GetComponent<SVGImage>();
            var mat = svgi.material;

            switch (blendMode) {
                case EnumFlashBlendMode.Normal:
                    mat.shader = Shader.Find("Unlit/VectorGradientUI");
                    break;
                case EnumFlashBlendMode.Add:
                    mat.shader = Shader.Find("Unlit/VectorGradientUIAdditive");
                    break;
                default:
                    Debug.LogError($"BlendMode {blendMode} is not valid for SVG shapes.");
                    break;
            }
        }
    }
}
