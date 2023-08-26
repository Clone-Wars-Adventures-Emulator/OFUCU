using CWAEmu.OFUCU.Flash.Tags;
using System.Collections.Generic;

namespace CWAEmu.OFUCU.Flash {
    public class Frame {
        // Flash treats frame indexes as 1 based, so start at 1
        public int FrameIndex { get; set; } = 1;
        public List<FlashTag> Tags { get; private set; } = new();
        public string Label { get; private set; } = null;

        public void addTag(FlashTag tag) {
            Tags.Add(tag);

            if (tag is FrameLabel) {
                Label = ((FrameLabel)tag).Label;
            }
        }

        public UFrame asUFrame() {
            UFrame frame = new() {
                label = Label,
                frameIndex = FrameIndex - 1
            };

            foreach (var tag in Tags) {
                UFrameObject frameObj = new();
                if (tag is PlaceObject2 po2) {
                    if (po2.HasCharacter && !po2.Move) {
                        frameObj.type = EnumUFrameObjectType.Place;
                    }
                    if (!po2.HasCharacter && po2.Move) {
                        frameObj.type = EnumUFrameObjectType.Modify;
                    }
                    if (po2.HasCharacter && po2.Move) {
                        frameObj.type = EnumUFrameObjectType.PlaceRemove;
                    }

                    frameObj.depth = po2.Depth;
                    
                    if (po2.HasCharacter) {
                        frameObj.charId = po2.CharacterId;
                    }

                    if (po2.HasMatrix) {
                        frameObj.matrix = UMatrix.fromFlashMatrix(po2.Matrix);
                    }

                    if (po2.HasColorTransform) {
                        frameObj.colorTransform = UColorTransform.fromFlashMatrix(po2.ColorTransform);
                    }

                    if (po2.HasName) {
                        frameObj.name = po2.Name;
                    }

                    if (po2.HasClipDepth) {
                        frameObj.clipDepth = po2.ClipDepth;
                    }

                    if (po2 is PlaceObject3 po3) {
                        if (po3.HasBlendMode) {
                            frameObj.blendMode = po3.BlendMode;
                        }
                    }

                    frame.displayList.Add(po2.Depth, frameObj);
                }

                if (tag is RemoveObject ro) {
                    frameObj.type = EnumUFrameObjectType.Remove;
                    frameObj.depth = ro.Depth;
                    frameObj.charId = ro.CharacterId;
                    frame.removeDisplayList.Add(ro.Depth, frameObj);
                }

                if (tag is RemoveObject2 ro2) {
                    frameObj.type = EnumUFrameObjectType.Remove;
                    frameObj.depth = ro2.Depth;
                    frame.removeDisplayList.Add(ro2.Depth, frameObj);
                }
            }

            return frame;
        }
    }
}
