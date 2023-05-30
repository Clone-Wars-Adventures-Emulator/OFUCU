using System.Collections.Generic;
using UnityEngine;

namespace CWAEmu.FlashConverter.Flash.Tags {
    public class DefineSprite : CharacterTag {
        public ushort NumFrames { get; private set; }
        public List<Frame> Frames { get; private set; } = new();

        public override void read(Reader reader) {
            CharacterId = reader.readUInt16();

            NumFrames = reader.readUInt16();

            Frame curFrame = new();

            // read all tags until completion
            while (!reader.ReachedEnd) {
                FlashTagHeader header = reader.readFlashTagHeader();

                // if end tag, stop parsing
                if (header.TagType == 0) {
                    if (curFrame.Tags.Count > 0) {
                        Debug.LogError("Sprite end tag reached with a frame having contents");
                        Frames.Add(curFrame);
                    }
                    Debug.Log($"Sprite End Tag found");
                    break;
                }

                switch (header.TagType) {
                    // = = = = = = = = = = Need to parse = = = = = = = = = =
                    case 1:  // ShowFrame
                        Frames.Add(curFrame);
                        curFrame = new();
                        break;


                    case 26: // PlaceObject2
                        PlaceObject2 po2 = new();
                        po2.Header = header;
                        po2.read(reader);

                        curFrame.addTag(po2);
                        break;

                        //reader.skip(header.TagLength);
                        //break;
                    default:
                        Debug.Log($"Sprite {CharacterId} Skipping {header.TagLength} bytes for tag {header.TagType}");
                        reader.skip(header.TagLength);
                        break;
                }
            }
        }
    }
}
