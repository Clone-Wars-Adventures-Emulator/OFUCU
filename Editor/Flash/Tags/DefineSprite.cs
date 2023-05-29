using UnityEngine;

namespace CWAEmu.FlashConverter.Flash.Tags {
    public class DefineSprite : CharacterTag {
        public ushort NumFrames { get; private set; }

        public override void read(Reader reader) {
            CharacterId = reader.readUInt16();

            NumFrames = reader.readUInt16();

            // read all tags until completion
            while (!reader.ReachedEnd) {
                FlashTagHeader header = reader.readFlashTagHeader();

                // if end tag, stop parsing
                if (header.TagType == 0) {
                    Debug.Log($"Sprite End Tag found");
                    break;
                }

                switch (header.TagType) {
                    // = = = = = = = = = = Need to parse = = = = = = = = = =

                    case 1:  // ShowFrame
                    case 26: // PlaceObject2

                        //reader.skip(header.TagLength);
                        //break;
                    default:
                        Debug.Log($"Sprite Skipping {header.TagLength} bytes for tag {header.TagType}");
                        reader.skip(header.TagLength);
                        break;
                }
            }
        }
    }
}
