using CWAEmu.OFUCU.Flash.Records;
using CWAEmu.OFUCU.Flash.Tags;
using System.Collections.Generic;
using System.Linq;
using UColor32 = UnityEngine.Color32;

namespace CWAEmu.OFUCU.Flash {
    [System.Serializable]
    public class UFrameList {
        public int frameCount;
        public List<Dictionary<int, UFrameObject>> frames = new();
        public List<Dictionary<int, UFrameObject>> deltaFrames = new();
        public Dictionary<int, string> frameLabels = new();

        public static UFrameList fromList(List<UFrame> list) {
            UFrameList res = new();

            Dictionary<int, UFrameObject> prevFrame = null;
            foreach (UFrame frame in list) {
                Dictionary<int, UFrameObject> displayList = new();
                Dictionary<int, UFrameObject> deltaList = new();
                res.frames.Add(displayList);
                res.deltaFrames.Add(deltaList);

                if (frame.label != null) {
                    res.frameLabels.Add(frame.frameIndex, frame.label);
                }

                List<int> depths = frame.displayList.Keys.Union(frame.removeDisplayList.Keys).ToList();
                foreach (int depth in depths) {
                    bool inDisp = frame.displayList.ContainsKey(depth);
                    bool inRem = frame.removeDisplayList.ContainsKey(depth);

                    UFrameObject frameObj = null;
                    if (inRem && inDisp) {
                        // Special but rare case. Modify the one in display to be a remove add
                        frameObj = frame.displayList[depth];
                        frameObj.type = EnumUFrameObjectType.PlaceRemove;
                    } else if (inRem && !inDisp) {
                        frameObj = frame.removeDisplayList[depth];
                    } else if (!inRem && inDisp) {
                        frameObj = frame.displayList[depth];
                    }

                    if (frameObj != null) {
                        displayList.Add(depth, frameObj);

                        if (prevFrame != null && prevFrame.ContainsKey(depth)) {
                            var delta = prevFrame[depth].delta(frameObj);
                            if (delta != null) {
                                deltaList.Add(depth, delta);
                            }
                        }
                    }
                }

                prevFrame = deltaList;
            }

            res.frameCount = list.Count;

            return res;
        }
    }

    [System.Serializable]
    public class UFrame {
        public int frameIndex;
        public string label;
        public Dictionary<int, UFrameObject> displayList = new();
        public Dictionary<int, UFrameObject> removeDisplayList = new();

        public static List<UFrame> toDiscreteList(List<Frame> frames) {
            List<UFrame> discrete = new();
            foreach (Frame frame in frames) {
                discrete.Add(frame.asUFrame());
            }

            return discrete;
        }

        public static UFrameList toDeltaList(List<Frame> frames) {
            List<UFrame> discrete = toDiscreteList(frames);

            return UFrameList.fromList(discrete);
        }

        public static UFrameList toDeltaList(List<UFrame> frames) {
            return UFrameList.fromList(frames);
        }
    }

    public enum EnumUFrameObjectType {
        Place,
        Modify,
        Remove,
        PlaceRemove
    }

    [System.Serializable]
    public class UFrameObject {
        public EnumUFrameObjectType type;
        public ushort charId;
        public ushort depth;

        // PlaceObject2 properties
        public UMatrix matrix;
        public UColorTransform colorTransform;
        // Ratio unsupported
        public string name;
        public ushort clipDepth;
        // ClipActions unsupported

        // PlaceObject3 properties
        // className unsupported
        public EnumFlashBlendMode blendMode;
        // TODO: FilterList is important
        // rest of po3 properties are unsupported

        // I want an object that has all of the properties of the current but with the modifications of the after
        public UFrameObject delta(UFrameObject after) {
            if (after == null) {
                return null;
            }

            // TODO: implement and find a good way to track the delta
            return null;
        }
    }

    [System.Serializable]
    public class UMatrix {
        public float scaleX;
        public float scaleY;
        public float rotateSkew0;
        public float rotateSkew1;
        public float translateX;
        public float translateY;

        public static UMatrix fromFlashMatrix(Matrix matrix) {
            UMatrix mat = new();

            mat.scaleX = matrix.ScaleX;
            mat.scaleY = matrix.ScaleY;
            mat.rotateSkew0 = matrix.RotateSkew0;
            mat.rotateSkew1 = matrix.RotateSkew1;
            mat.translateX = matrix.TranslateX;
            mat.translateY = -matrix.TranslateY;

            return mat;
        }
    }

    [System.Serializable]
    public class UColorTransform {
        public UColor32 mult;
        public UColor32 add;

        public static UColorTransform fromFlashMatrix(CXFormWithAlpha color) {
            UColorTransform col = new();

            if (color.HasMult) {
                col.mult = new UColor32(color.RMult, color.GMult, color.BMult, color.AMult);
            }

            if (color.HasAdd) {
                col.add = new UColor32(color.RAdd, color.GAdd, color.BAdd, color.AAdd);
            }

            return col;
        }
    }
}
