using System;
using System.Collections.Generic;
using UnityEngine;

namespace CWAEmu.OFUCU.Flash.Records {
    public class Matrix {
        public int NScaleBits { get; private set; } = 0;
        public int NRotateBits { get; private set; } = 0;
        public int NTranslateBits { get; private set; } = 0;

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
                matrix.NScaleBits = (int) reader.readUBits(5);
                matrix.ScaleX = reader.readFixedBits(matrix.NScaleBits);
                matrix.ScaleY = reader.readFixedBits(matrix.NScaleBits);
            }

            bool hasRotate = reader.readBitFlag();
            if (hasRotate) {
                matrix.NRotateBits = (int) reader.readUBits(5);
                matrix.RotateSkew0 = reader.readFixedBits(matrix.NRotateBits);
                matrix.RotateSkew1 = reader.readFixedBits(matrix.NRotateBits);
            }

            matrix.NTranslateBits = (int) reader.readUBits(5);
            matrix.TranslateXTwips = reader.readBits(matrix.NTranslateBits);
            matrix.TranslateYTwips = reader.readBits(matrix.NTranslateBits);

            reader.endBitRead();

            return matrix;
        }

        public bool hasT() {
            return NTranslateBits > 0;
        }

        public bool hasS() {
            return NScaleBits > 0;
        }

        public bool hasR() {
            return NRotateBits > 0;
        }
    }
}
