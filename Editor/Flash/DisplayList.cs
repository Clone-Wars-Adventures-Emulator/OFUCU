using CWAEmu.OFUCU.Flash.Records;
using CWAEmu.OFUCU.Flash.Tags;
using System;
using System.Collections.Generic;
using UnityEngine;
using Color = UnityEngine.Color;

namespace CWAEmu.OFUCU.Flash {
    [Serializable]
    public class DisplayList {
        public readonly DisplayFrame[] frames;
        public DisplayList(List<Frame> frames) {
            if (frames == null || frames.Count == 0) {
                this.frames = new DisplayFrame[0];
                return;
            }

            this.frames = new DisplayFrame[frames.Count];

            DisplayFrame df = frames[0].asDisplayFrame();
            this.frames[0] = df;

            for (int i = 1; i < frames.Count; i++) {
                var prev = this.frames[i - 1];
                this.frames[i] = frames[i].asDisplayFrame(prev);
            }
        }
    }

    [Serializable]
    public class DisplayFrame {
        public int frameIndex;
        public string label;
        public readonly HashSet<int> objectsRemoved = new();
        public readonly HashSet<int> objectsAdded = new();
        public readonly Dictionary<int, DisplayObject> states = new();
        public readonly Dictionary<int, DisplayObject> changes = new();
    }

    [Serializable]
    public class DisplayObject {
        // TODO: props? would mean i dont have to individually set has
        public int charId;
        public int depth;

        public bool hasMatrixChange;
        public bool hasCharId;
        public bool hasColor;
        public bool hasName;
        public bool hasClipDepth;
        public bool hasBlendMode;

        // PlaceObject2 Data
        public Matrix2x3 matrix; // ENSURE NON NULL
        public ColorTransform color;
        // Ratio unsupported
        public string name = "";
        public ushort clipDepth;
        // ClipActions unsupported

        // PlaceObject3 Data
        // className unsupported
        public EnumFlashBlendMode blendMode;
        // TODO: filter list
    }

    [Serializable]
    public class Matrix2x3 {
        public float scaleX = 1;
        public float scaleY = 1;
        public float rotateSkew0 = 0;
        public float rotateSkew1 = 0;
        public float translateX = 0;
        public float translateY = 0;

        private bool hadR;
        private bool hadS;

        public static Matrix2x3 fromFlash(Matrix matrix) {
            Matrix2x3 mat = new();

            mat.scaleX = matrix.ScaleX;
            mat.scaleY = matrix.ScaleY;
            mat.rotateSkew0 = matrix.RotateSkew0;
            mat.rotateSkew1 = matrix.RotateSkew1;
            mat.translateX = matrix.TranslateX;
            mat.translateY = -matrix.TranslateY;

            mat.hadR = matrix.hasR();
            mat.hadS = matrix.hasS();

            return mat;
        }

        // TODO: is this correct??
        public (Vector2 translate, Vector2 scale, float rotz) getTransformation() {
            Vector2 translate = new(translateX, translateY);
            Vector2 scale;
            float rotz;

            if (hadR && hadS) {
                (scale, rotz) = scaleAndRotate();
            } else if (hadR && !hadS) {
                scale = new(1, 1);
                rotz = rotateOnly();
            } else if (hadS && !hadR) {
                scale = scaleOnly();
                rotz = 0;
            } else {
                scale = new(1, 1);
                rotz = 0;
            }

            return (translate, scale, rotz);
        }

        private Vector2 scaleOnly() {
            return new(scaleX, scaleY);
        }

        private float rotateOnly() {
            // TODO: HUH????
            Debug.LogError($"Rotate only with rotateSkew values {rotateSkew0} {rotateSkew1}");
            return 0;
        }

        private (Vector2, float) scaleAndRotate() {
            // based on the algo described in https://math.stackexchange.com/questions/612006/decomposing-an-affine-transformation, but adapted to not use sheer (unsupported in Unity)
            var rot = Mathf.Atan2(rotateSkew0, scaleX);
            var rotDeg = rot * Mathf.Rad2Deg;

            var sx = Mathf.Sqrt(scaleX * scaleX + rotateSkew0 * rotateSkew0);
            var sy = Mathf.Sqrt(scaleY * scaleY + rotateSkew1 * rotateSkew1);

            return (new(sx, sy), rotDeg);
        }
    }

    public class ColorTransform {
        public Color mult;
        public Color add;

        public bool hasMult;
        public bool hasAdd;

        public static ColorTransform frameFlash(CXFormWithAlpha cx) {
            var ting =  new ColorTransform {
                hasMult = cx.HasMult,
                hasAdd = cx.HasAdd,
            };

            if (cx.HasMult) {
                ting.mult = new Color(cx.RMult / 256.0f, cx.GMult / 256.0f, cx.BMult / 256.0f, cx.AMult / 256.0f);
            }

            if (cx.HasAdd) {
                ting.add = new Color(cx.RAdd / 256.0f, cx.GAdd / 256.0f, cx.BAdd / 256.0f, cx.AAdd / 256.0f);
            }

            return ting;
        }
    }
}
