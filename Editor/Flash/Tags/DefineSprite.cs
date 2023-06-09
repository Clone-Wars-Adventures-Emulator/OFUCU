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
                    if (SWFFile.IndepthLogging) {
                        Debug.Log($"Sprite End Tag found");
                    }
                    break;
                }

                switch (header.TagType) {
                    // = = = = = = = = = = Need to parse = = = = = = = = = =
                    case 1:  // ShowFrame
                        Frames.Add(curFrame);
                        curFrame = new();
                        break;

                    case 43: // FrameLabel
                        FrameLabel fl = new();
                        fl.Header = header;
                        fl.read(reader);

                        curFrame.addTag(fl);
                        break;

                    case 26: // PlaceObject2
                        PlaceObject2 po2 = new();
                        po2.Header = header;
                        po2.read(reader);

                        curFrame.addTag(po2);
                        break;

                    case 70: // RemoveObject3
                        PlaceObject3 po3 = new();
                        po3.Header = header;
                        po3.read(reader);

                        curFrame.addTag(po3);
                        break;

                    case 5: // RemoveObject
                        RemoveObject ro = new();
                        ro.Header = header;
                        ro.read(reader);

                        curFrame.addTag(ro);
                        break;

                    case 28: // RemoveObject2
                        RemoveObject2 ro2 = new();
                        ro2.Header = header;
                        ro2.read(reader);

                        curFrame.addTag(ro2);
                        break;

                    //  = = = = = = = = = = No need to parse = = = = = = = = = =
                    case 12: // DoAction
                        reader.skip(header.TagLength);
                        break;

                    default:
                        Debug.LogWarning($"Sprite {CharacterId} Skipping {header.TagLength} bytes for tag {header.TagType}");
                        reader.skip(header.TagLength);
                        break;
                }
            }
        }
    }
}
