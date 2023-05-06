using System.Collections.Generic;

namespace CWAEmu.FlashConverter.Flash.Records {
    public class Gradient {
        public byte SpreadMode { get; private set; }
        public byte InterpolationMode { get; private set; }
        public byte NumGradients { get; private set; }
        public List<GradientRecord> GradientRecords { get; private set; }

        public static Gradient readGradient(Reader reader, int shapeTagType) {
            Gradient grad = new() {
                SpreadMode = (byte)reader.readBits(2),
                InterpolationMode = (byte)reader.readBits(2),
                NumGradients = (byte)reader.readUBits(4)
            };

            reader.endBitRead();

            for (int i = 0; i < grad.NumGradients; i++) {
                grad.GradientRecords.Add(GradientRecord.readGradientRecord(reader, shapeTagType));
            }

            return grad;
        }
    }

    public class FocalGradient {
        public byte SpreadMode { get; private set; }
        public byte InterpolationMode { get; private set; }
        public byte NumGradients { get; private set; }
        public List<GradientRecord> GradientRecords { get; private set; }
        public float FocalPoint { get; private set; }

        public static FocalGradient readGradient(Reader reader, int shapeTagType) {
            FocalGradient grad = new() {
                SpreadMode = (byte)reader.readBits(2),
                InterpolationMode = (byte)reader.readBits(2),
                NumGradients = (byte)reader.readUBits(4)
            };

            reader.endBitRead();

            for (int i = 0; i < grad.NumGradients; i++) {
                grad.GradientRecords.Add(GradientRecord.readGradientRecord(reader, shapeTagType));
            }

            grad.FocalPoint = reader.readFixed8();

            return grad;
        }
    }

    public class GradientRecord {
        public byte Ratio { get; private set; }
        public RGBA Color { get; private set; }

        public static GradientRecord readGradientRecord(Reader reader, int shapeTagType) {
            GradientRecord gradientRecord = new() {
                Ratio = reader.readByte()
            };

            if (shapeTagType >= 3) {
                gradientRecord.Color = RGBA.readRGBA(reader);
            } else {
                gradientRecord.Color = RGBA.readRGBasRGBA(reader);
            }

            return gradientRecord;
        }
    }
}
