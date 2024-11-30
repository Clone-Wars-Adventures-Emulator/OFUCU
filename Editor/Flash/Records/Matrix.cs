using System;
using System.Collections.Generic;
using UnityEngine;

namespace CWAEmu.OFUCU.Flash.Records {
    public class Matrix {
        // TODO: remove debug
        public static List<Matrix> All = new();

        private int nScaleBits = 0;
        private int nRotateBits = 0;
        private int nTranslateBits = 0;

        public float ScaleX { get; private set; } = 1;
        public float ScaleY { get; private set; } = 1;
        public float RotateSkew0 { get; private set; } = 0;
        public float RotateSkew1 { get; private set; } = 0;
        public int TranslateXTwips { get; private set; }
        public int TranslateYTwips { get; private set; }
        public float TranslateX => TranslateXTwips / 20.0f;
        public float TranslateY => TranslateYTwips / 20.0f;

        public static Matrix readMatrix(Reader reader) {
            Matrix matrix = new();

            bool hasScale = reader.readBitFlag();
            if (hasScale) {
                matrix.nScaleBits = (int) reader.readUBits(5);
                matrix.ScaleX = reader.readFixedBits(matrix.nScaleBits);
                matrix.ScaleY = reader.readFixedBits(matrix.nScaleBits);
            }

            bool hasRotate = reader.readBitFlag();
            if (hasRotate) {
                matrix.nRotateBits = (int) reader.readUBits(5);
                matrix.RotateSkew0 = reader.readFixedBits(matrix.nRotateBits);
                matrix.RotateSkew1 = reader.readFixedBits(matrix.nRotateBits);
            }

            matrix.nTranslateBits = (int) reader.readUBits(5);
            matrix.TranslateXTwips = reader.readBits(matrix.nTranslateBits);
            matrix.TranslateYTwips = reader.readBits(matrix.nTranslateBits);

            reader.endBitRead();

            // TODO: remove debug
            All.Add(matrix);

            return matrix;
        }

        public bool hasT() {
            return nTranslateBits > 0;
        }

        public bool hasS() {
            return nScaleBits > 0;
        }

        public bool hasR() {
            return nRotateBits > 0;
        }
    }
}
