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
                    DisplayObject state;
                    if (previous != null && previous.states.TryGetValue(po2.Depth, out var prev)) {
                        // we got a previous state object from the dictionary, clone it so we dont corrupt it
                        state = DisplayObject.Clone(prev);
                    } else {
                        state = new();
                    }
                    DisplayObject delt = new();

                    bool isDelta = false;
                    if (po2.HasCharacter && !po2.Move) {
                        df.objectsAdded.Add(po2.Depth);

                        // check to see if there was a RemoveObject tag for this object this frame and that both this object and the previous object were masks.
                        // If so, treat this as a delta with a state reset instead of delete and create (basically, we need to fix what ever flash export / artist
                        // made this be 2 tags instead of just Move=true, which is annoyingly common in a lot of cases, but messes with masks)
                        if (df.objectsRemoved.Contains(po2.Depth) && state.charId == po2.CharacterId && state.hasClipDepth && po2.HasClipDepth) {
                            df.objectsRemoved.Remove(po2.Depth);
                            df.objectsAdded.Remove(po2.Depth);
                            state = new();
                            isDelta = true;
                        }
                    }
                    if (!po2.HasCharacter && po2.Move) {
                        isDelta = true;
                    }
                    if (po2.HasCharacter && po2.Move) {
                        df.objectsAdded.Add(po2.Depth);
                        df.objectsRemoved.Add(po2.Depth);
                        // force prev to be new obj
                        state = new();
                    }

                    state.depth = delt.depth = po2.Depth;

                    if (po2.HasCharacter) {
                        state.charId = delt.charId = po2.CharacterId;
                        state.hasCharId = delt.hasCharId = true;
                    }

                    if (po2.HasMatrix) {
                        state.matrix = delt.matrix = Matrix2x3.FromFlash(po2.Matrix);
                        state.hasMatrixChange = delt.hasMatrixChange = true;
                    } else {
                        // only assign a new matrix if the old one was null
                        state.matrix ??= new();
                    }

                    if (po2.HasColorTransform) {
                        state.color = delt.color = ColorTransform.FromFlash(po2.ColorTransform);
                        state.hasColor = delt.hasColor = true;
                    }

                    if (po2.HasName) {
                        state.name = delt.name = po2.Name;
                        state.hasName = delt.hasName = true;
                    }

                    if (po2.HasClipDepth) {
                        state.clipDepth = delt.clipDepth = po2.ClipDepth;
                        state.hasClipDepth = delt.hasClipDepth = true;
                    }

                    if (po2 is PlaceObject3 po3) {
                        if (po3.HasBlendMode) {
                            state.blendMode = delt.blendMode = po3.BlendMode;
                            state.hasBlendMode = delt.hasBlendMode = true;
                        }
                    }

                    df.states.Add(po2.Depth, state);
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
