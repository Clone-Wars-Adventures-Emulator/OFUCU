using CWAEmu.OFUCU.Data;
using System.Collections.Generic;
using UnityEngine;

namespace CWAEmu.OFUCU.Flash.Tags {
    [System.Serializable]
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
                    if (Settings.Instance.EnhancedLogging) {
                        Debug.Log($"Sprite End Tag found");
                    }
                    break;
                }

                switch (header.TagType) {
                    // = = = = = = = = = = Need to parse = = = = = = = = = =
                    case EnumTagType.ShowFrame:
                        Frames.Add(curFrame);
                        int nextIdx = curFrame.FrameIndex + 1;
                        curFrame = new() {
                            FrameIndex = nextIdx
                        };
                        break;

                    case EnumTagType.FrameLabel:
                        FrameLabel fl = new() {
                            Header = header
                        };
                        fl.read(reader);

                        curFrame.addTag(fl);
                        break;

                    case EnumTagType.PlaceObject2:
                        PlaceObject2 po2 = new() {
                            Header = header
                        };
                        po2.read(reader);

                        curFrame.addTag(po2);
                        break;

                    case EnumTagType.PlaceObject3:
                        PlaceObject3 po3 = new() {
                            Header = header
                        };
                        po3.read(reader);

                        curFrame.addTag(po3);
                        break;

                    case EnumTagType.RemoveObject:
                        RemoveObject ro = new() {
                            Header = header
                        };
                        ro.read(reader);

                        curFrame.addTag(ro);
                        break;

                    case EnumTagType.RemoveObject2:
                        RemoveObject2 ro2 = new() {
                            Header = header
                        };
                        ro2.read(reader);

                        curFrame.addTag(ro2);
                        break;

                    //  = = = = = = = = = = No need to parse = = = = = = = = = =
                    case EnumTagType.DoAction:
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
