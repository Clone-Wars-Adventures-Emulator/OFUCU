using CWAEmu.OFUCU.Flash.Tags;
using CWAEmu.OFUCU.Runtime;
using System.IO;
using Unity.VectorGraphics;
using UnityEditor;
using UnityEngine;

namespace CWAEmu.OFUCU {
    [RequireComponent(typeof(RuntimeShape))]
    public class OFUCUShape : AbstractOFUCUObject {
        public override void setBlendMode(EnumFlashBlendMode blendMode, string saveFolder, string path) {
            var shaderName = blendMode switch {
                EnumFlashBlendMode.Normal => "Unlit/VectorGradientUI",
                EnumFlashBlendMode.Add => "Unlit/VectorGradientUIAdditive",
                _ => null
            };

            if (shaderName == null) {
                Debug.LogError($"BlendMode {blendMode} is not valid for SVG shapes.");
            }

            var svgi = gameObject.GetComponent<SVGImage>();
            if (svgi.material.shader.name == shaderName) {
                // NO-OP, already this shader
                return;
            }

            var mat = new Material(svgi.material) {
                shader = Shader.Find(shaderName)
            };
            svgi.material = mat;

            if (!Directory.Exists(saveFolder)) {
                Directory.CreateDirectory(saveFolder);
            }
            AssetDatabase.CreateAsset(mat, $"{saveFolder}/{path}.mat");
        }
    }
}
