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

            if (tag is FrameLabel label) {
                Label = label.Label;
            }
        }

        public DisplayFrame asDisplayFrame(DisplayFrame previous = null) {
            DisplayFrame df = new() {
                label = Label,
                frameIndex = FrameIndex,
            };

            foreach (var tag in Tags) {
                if (tag is PlaceObject2 po2) {
                    if (previous != null && previous.states.TryGetValue(po2.Depth, out var prev)) {
                        // we got a previous state object from the dictionary, clone it so we dont corrupt it
                        prev = DisplayObject.Clone(prev);
                    } else {
                        prev = new();
                    }
                    DisplayObject delt = new();

                    bool isDelta = false;
                    if (po2.HasCharacter && !po2.Move) {
                        df.objectsAdded.Add(po2.Depth);
                    }
                    if (!po2.HasCharacter && po2.Move) {
                        isDelta = true;
                    }
                    if (po2.HasCharacter && po2.Move) {
                        df.objectsAdded.Add(po2.Depth);
                        df.objectsRemoved.Add(po2.Depth);
                        // force prev to be new obj
                        prev = new();
                    }

                    prev.depth = delt.depth = po2.Depth;

                    if (po2.HasCharacter) {
                        prev.charId = delt.charId = po2.CharacterId;
                        prev.hasCharId = delt.hasCharId = true;
                    }

                    if (po2.HasMatrix) {
                        prev.matrix = delt.matrix = Matrix2x3.FromFlash(po2.Matrix);
                        prev.hasMatrixChange = delt.hasMatrixChange = true;
                    } else {
                        // TODO: does this work the way you expected?
                        prev.matrix = new();
                    }

                    if (po2.HasColorTransform) {
                        prev.color = delt.color = ColorTransform.FromFlash(po2.ColorTransform);
                        prev.hasColor = delt.hasColor = true;
                    }

                    if (po2.HasName) {
                        prev.name = delt.name = po2.Name;
                        prev.hasName = delt.hasName = true;
                    }

                    if (po2.HasClipDepth) {
                        prev.clipDepth = delt.clipDepth = po2.ClipDepth;
                        prev.hasClipDepth = delt.hasClipDepth = true;
                    }

                    if (po2 is PlaceObject3 po3) {
                        if (po3.HasBlendMode) {
                            prev.blendMode = delt.blendMode = po3.BlendMode;
                            prev.hasBlendMode = delt.hasBlendMode = true;
                        }
                    }

                    df.states.Add(po2.Depth, prev);
                    if (isDelta) {
                        df.changes.Add(po2.Depth, delt);
                    }
                }

                if (tag is RemoveObject ro) {
                    df.objectsRemoved.Add(ro.Depth);
                }

                if (tag is RemoveObject2 ro2) {
                    df.objectsRemoved.Add(ro2.Depth);
                }
            }

            // for each object in the previous frame's states, check if it should be carried over to this frame
            // this should happen if the current state does not contain the object and the object at that depth was not removed
            if (previous != null) {
                foreach (var pair in previous.states) {
                    if (df.states.ContainsKey(pair.Key) || df.objectsRemoved.Contains(pair.Key)) {
                        continue;
                    }

                    df.states.Add(pair.Key, pair.Value);
                }
            }

            return df;
        }
    }
}
