using System;
using UnityEngine;
using Utils.Conversion;

namespace CWAEmu.FlashConverter.Flash {
    public class Reader  {
        private byte[] data;
        private int index;
        private int bitOffset;
        private bool readingBits;

        public Reader(byte[] data) {
            this.data = data;
            index = 0;
            readingBits = false;
            bitOffset = 0;
        }

        public bool ReachedEnd => index == data.Length;
        public int Remaining => data.Length - index;

        public byte readByte() => data[index++];
        public char readChar() => (char)data[index++];

        // TODO: error checking, not mixing bit reading with byte reading
        public byte[] readBytes(int count) {
            byte[] bytes = new byte[count];
            for (int i = 0; i < count; i++) {
                bytes[i] = data[index++];
            }

            return bytes;
        }

        public char[] readChars(int count) {
            char[] chars = new char[count];
            for (int i = 0; i < count; i++) {
                chars[i] = (char)data[index++];
            }

            return chars;
        }

        public short readInt16() {
            byte lower = readByte();
            byte upper = readByte();

            return (short)((upper << 8) | lower);
        }

        public ushort readUInt16() {
            byte lower = readByte();
            byte upper = readByte();

            return (ushort)((upper << 8) | lower);
        }

        public int readInt32() {
            byte lowerLow = readByte();
            byte lowerHig = readByte();
            byte upperLow = readByte();
            byte upperHig = readByte();

            return (upperHig << 24) | (upperLow << 16) | (lowerHig << 8) | lowerLow;
        }

        public uint readUInt32() {
            byte lowerLow = readByte();
            byte lowerHig = readByte();
            byte upperLow = readByte();
            byte upperHig = readByte();

            return (uint)((upperHig << 24) | (upperLow << 16) | (lowerHig << 8) | lowerLow);
        }

        public float readSingle() {
            byte[] bytes = readBytes(4);

            if (!BitConverter.IsLittleEndian) {
                Array.Reverse(bytes);
            }

            return BitConverter.ToSingle(bytes);
        }

        public (ushort tag, uint length) readFlashTagHeader() {
            ushort header = readUInt16();
            ushort tag = (ushort)((header & 0b1111_1111_1100_0000) >> 6);
            uint length = (uint)(header & 0b0011_1111);

            if (length == 0x3f) {
                length = readUInt32();
            }

            return (tag, length);
        }

        public uint readUBits(int numBits) {
            uint result = 0;

            int skippedBits = bitOffset % 8;

            int readBit = 7 - skippedBits;
            while (numBits > 0) {
                // move a 1 into the bit we want to read
                int bitMask = 0b1 << readBit;

                // shift result left to make space for the new bit
                result <<= 1;

                // read the bit
                byte @byte = data[index];
                uint bit = (uint)((@byte & bitMask) >> readBit);

                // add the bit to the result
                result |= bit;

                // subtract 1 from the read bit and wrap it around to the start if it falls off the end of the bit
                readBit--;
                if (readBit < 0) {
                    index++;
                    readBit = 7;
                }

                bitOffset++;
                if (bitOffset == 8) {
                    bitOffset = 0;
                }

                // we successfully read a bit
                numBits--;
            }

            return result;
        }

        public int readBits(int numBits) {
            return signExtend(readUBits(numBits), numBits);
        }

        private int signExtend(uint source, int numBits) {
            int shiftAmount = 32 - numBits;

            return ((int)(source << shiftAmount)) >> shiftAmount;
        }

        // consume all remaining padding bits and move the index to the next whole byte
        public void endBitRead() {
            if (bitOffset == 0) {
                return;
            }

            bitOffset = 0;
            index++;
        }
    }
}
