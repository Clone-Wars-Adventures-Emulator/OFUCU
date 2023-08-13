using CWAEmu.OFUCU.Flash.Records;
using CWAEmu.OFUCU.Flash.Tags;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace CWAEmu.OFUCU {
    public class DictonaryEntry : MonoBehaviour {
        public enum EnumDictonaryCharacterType {
            Image,
            Shape,
            Sprite
        }

        public PlacedSWFFile containingFile;
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

        public void saveImageToAsset(string path) {
            if (!path.StartsWith("Assets/")) {
                Debug.LogError($"File path {path} is invalid. Path must start with Assets/");
                return;
            }

            // get the image into an array of unity colors and load it into a Texture2D
            Color32[] colors = new Color32[image.Width * image.Height];
            for (int y = 0; y < image.Height; y++) {
                for (int x = 0; x < image.Width; x++) {
                    colors[y * image.Width + x] = image.readPixelAt(x, image.Height - y - 1).asUnityColor();
                }
            }

            Texture2D tex = new(image.Width, image.Height);
            tex.SetPixels32(colors, 0);
            tex.Apply();

            // make sure where we are about to save the file to exists
            if (!Directory.Exists(path)) {
                Directory.CreateDirectory(path);
            }

            // write file
            path = $"{path}/{name}.png";
            byte[] png = tex.EncodeToPNG();
            File.WriteAllBytes(path, png);
            AssetDatabase.Refresh();

            // Force opinionated settings (other users can overwrite these if they want of course)
            TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path);
            if (importer == null) {
                // this should not happen, but it is a saftey catch just in case
                Debug.LogError("No importer for path " + path);
                return;
            }

            importer.isReadable = true;

            TextureImporterSettings settings = new();
            importer.ReadTextureSettings(settings);
            // TODO: set settings as desired
            importer.SetTextureSettings(settings);

            importer.mipmapEnabled = false;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.alphaSource = TextureImporterAlphaSource.FromInput;
            // TODO: modify settings as desired

            EditorUtility.SetDirty(importer);
            importer.SaveAndReimport();
        }
    }
}