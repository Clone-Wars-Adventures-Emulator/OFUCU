using CWAEmu.OFUCU.Flash.Records;
using CWAEmu.OFUCU.Flash.Tags;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using URect = UnityEngine.Rect;
using UColor = UnityEngine.Color;
using CWAEmu.OFUCU.Data;

namespace CWAEmu.OFUCU {
    public class DictonaryEntry : MonoBehaviour {
        public enum EnumDictonaryCharacterType {
            Image,
            Shape,
            Sprite
        }

        // Common
        public PlacedSWFFile containingFile;
        public CharacterTag charTag;
        public EnumDictonaryCharacterType CharacterType;
        public RectTransform rt;
        public List<int> neededCharacters = new();
        public List<PlaceObject> dependentObjects = new();

        // Image
        public FlashImage image; // TODO: remove in favor of casting charTag???

        public string AssetPath {
            get {
                if (assetPath == null) {
                    string path = PersistentData.Instance.getSwfExportDir(containingFile.File.Name);
                    path = $"{path}/{name}.{getFileExtension(CharacterType)}";

                    if (!File.Exists(path)) {
                        return null;
                    }

                    if (AssetDatabase.LoadAssetAtPath<Sprite>(path) == null) {
                        return null;
                    }

                    return path;
                }

                return assetPath;
            }
        }
        private string assetPath;

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

            if (assetPath != null) {
                // TODO: prevent this?
                Debug.LogWarning($"Image {name} already has a saved asset at {assetPath}.");
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

            assetPath = path;

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

        public void fillShape() {
            List<PlacedImage> images = gameObject.GetComponentsInChildren<PlacedImage>().ToList();

            void onBitmapFill(URect extends, ushort bitmapId, bool smooth, bool clipped) {
                PlacedImage workingImage = null;
                foreach (PlacedImage image in images) {
                    // It is possible that there is more than one of the same image placed, avoid anyone that has already been placed
                    if (image.gameObject.GetComponent<Image>() != null) {
                        continue;
                    }

                    if (image.placedEntry.charTag.CharacterId == bitmapId) {
                        workingImage = image;
                        break;
                    }
                }

                if (workingImage == null) {
                    // BAD USER! do not delete my objects!
                    // TODO: fix this for the user
                }

                string filePath = workingImage.placedEntry.AssetPath;
                if (filePath == null) {
                    Debug.LogError($"Asset file for {workingImage.placedEntry.name} does not exist.");
                    return;
                }

                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(filePath);
                if (sprite == null) {
                    Debug.LogError($"Failed to load sprite at {filePath}.");
                }
                Image img = workingImage.gameObject.AddComponent<Image>();
                img.sprite = sprite;
            }

            void onSolidFill(URect extends, UColor color) {
                var (go, rt) = containingFile.createUIObj("Solid Fill");
                rt.SetParent(transform, false);
                rt.pivot = new Vector2(0, 1);
                rt.anchorMin = new Vector2(0, 1);
                rt.anchorMax = new Vector2(0, 1);
                rt.anchoredPosition = new Vector2(extends.xMin, -extends.yMin);
                rt.sizeDelta = new Vector2(Mathf.Abs(extends.xMax - extends.xMin), Mathf.Abs(extends.yMax - extends.yMin));

                Image img = go.AddComponent<Image>();
                img.color = color;
            }

            void onGradientFill(URect extends) {
                Debug.LogError($"Gradient fill not supported. Cannot fill shape {charTag.CharacterId}");
            }

            (charTag as DefineShape).iterateOnShapeFill(onBitmapFill, onSolidFill, onGradientFill);
        }

        public void flattenShape() {
            // TODO: will this ever have functionality??
            // Also this would need to ensure a filled shape first before flattening
        }

        public void placeFrames() {
            containingFile.placeFrames(rt, (charTag as DefineSprite).Frames);
        }

        public void animateFrames() {
            containingFile.animateFrames(rt, (charTag as DefineSprite).Frames);
        }

        private string getFileExtension(EnumDictonaryCharacterType type) {
            return type switch {
                EnumDictonaryCharacterType.Image => "png",
                _ => "",
            };
        }
    }
}
