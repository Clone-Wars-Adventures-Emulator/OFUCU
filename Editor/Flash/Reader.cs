using CWAEmu.OFUCU.Flash.Tags;
using CWAEmu.Ionic.Zlib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;

namespace CWAEmu.OFUCU.Flash {
    public class Reader {
        private const float FIXED_16_CONVERT = 0x10000;
        private const float FIXED_8_CONVERT = 0x100;

        private readonly byte[] data;
        private readonly byte flashVersion;
        private readonly SWFFile file;
        private int index;
        private int bitOffset;
        private readonly bool skipImageData;

        public Reader(byte[] data, SWFFile file, bool skipImageData = false) {
            this.data = data;
            flashVersion = file.Version;
            this.file = file;
            this.skipImageData = skipImageData;
            index = 0;
            bitOffset = 0;
        }

        public int Version => flashVersion;
        public bool SkipImageData => skipImageData;
        public bool ReachedEnd => index == data.Length;
        public int Remaining => data.Length - index;
        public int Index => index;
        public SWFFile File => file;

        public void skip(int bytes) => index += bytes;

        public byte readByte() {
            endBitRead();

            return data[index++];
        }

        public sbyte readSByte() {
            endBitRead();

            return (sbyte) data[index++];
        }

        public char readChar() {
            endBitRead();
            return (char) data[index++];
        }

        public byte readUInt8() {
            endBitRead();
            return readByte();
        }

        public sbyte readInt8() {
            endBitRead();
            return readSByte();
        }

        public byte[] readBytes(int count) {
            endBitRead();

            byte[] bytes = new byte[count];
            for (int i = 0; i < count; i++) {
                bytes[i] = data[index++];
            }

            return bytes;
        }

        public byte[] readBytes(uint count) {
            endBitRead();

            byte[] bytes = new byte[count];
            for (int i = 0; i < count; i++) {
                bytes[i] = data[index++];
            }

            return bytes;
        }

        public char[] readChars(int count) {
            endBitRead();

            char[] chars = new char[count];
            for (int i = 0; i < count; i++) {
                chars[i] = (char) data[index++];
            }

            return chars;
        }

        public short readInt16() {
            byte lower = readByte();
            byte upper = readByte();

            return (short) ((upper << 8) | lower);
        }

        public ushort readUInt16() {
            byte lower = readByte();
            byte upper = readByte();

            return (ushort) ((upper << 8) | lower);
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

            return (uint) ((upperHig << 24) | (upperLow << 16) | (lowerHig << 8) | lowerLow);
        }

        public float readFixed16() {
            int raw = readInt32();

            return raw / FIXED_16_CONVERT;
        }

        public float readFixed8() {
            int raw = readInt16();

            return raw / FIXED_8_CONVERT;
        }

        public float readSingle() {
            byte[] bytes = readBytes(4);

            if (!BitConverter.IsLittleEndian) {
                Array.Reverse(bytes);
            }

            return BitConverter.ToSingle(bytes);
        }

        public FlashTagHeader readFlashTagHeader() {
            ushort header = readUInt16();
            ushort tag = (ushort) ((header & 0b1111_1111_1100_0000) >> 6);
            int length = (header & 0b0011_1111);

            if (length == 0x3f) {
                length = readInt32();
            }

            FlashTagHeader tHeader = new() {
                TagType = (EnumTagType) tag,
                TagLength = length
            };

            return tHeader;
        }

        public uint readUBits(uint numBits) => readUBits((int) numBits);

        public uint readUBits(int numBits) {
            uint result = 0;

            if (numBits == 0) {
                return result;
            }

            int skippedBits = bitOffset % 8;

            int readBit = 7 - skippedBits;
            while (numBits > 0) {
                // move a 1 into the bit we want to read
                int bitMask = 0b1 << readBit;

                // shift result left to make space for the new bit
                result <<= 1;

                // read the bit
                byte @byte = data[index];
                uint bit = (uint) ((@byte & bitMask) >> readBit);

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

        public int readBits(uint numBits) => readBits((int) numBits);

        public int readBits(int numBits) {
            return signExtend(readUBits(numBits), numBits);
        }

        public float readFixedBits(int numBits) {
            int raw = readBits(numBits);

            return raw / FIXED_16_CONVERT;
        }

        public bool readBitFlag() {
            return readUBits(1) == 1;
        }

        private int signExtend(uint source, int numBits) {
            int shiftAmount = 32 - numBits;

            return ((int) (source << shiftAmount)) >> shiftAmount;
        }

        // consume all remaining padding bits and move the index to the next whole byte
        public void endBitRead() {
            if (bitOffset == 0) {
                return;
            }

            bitOffset = 0;
            index++;
        }

        public string readString() {
            List<byte> bytes = new();

            byte read = readByte();
            while (read != 0) {
                bytes.Add(read);
                read = readByte();
            }

            if (flashVersion >= 6) {
                return Encoding.UTF8.GetString(bytes.ToArray());
            }

            return Encoding.Default.GetString(bytes.ToArray());
        }

        public string readLengthEncodedString() {
            byte size = readByte();

            byte[] bytes = new byte[size];
            for (int i = 0; i < size; i++) {
                bytes[i] = readByte();
            }

            if (flashVersion >= 6) {
                return Encoding.UTF8.GetString(bytes.ToArray());
            }

            return Encoding.Default.GetString(bytes.ToArray());
        }

        public Reader readZLibBytes(int numBytes) {
            byte[] bytes = readBytes(numBytes);

            using var targetStream = new MemoryStream();

            using var compressedStream = new MemoryStream(bytes);
            using var decompressStream = new ZlibStream(compressedStream, Ionic.Zlib.CompressionMode.Decompress);

            decompressStream.CopyTo(targetStream);

            return new Reader(targetStream.ToArray(), file);
        }

        internal Reader readZLibBytes(uint numBytes) {
            byte[] bytes = readBytes(numBytes);

            using var targetStream = new MemoryStream();

            using var compressedStream = new MemoryStream(bytes);
            using var decompressStream = new ZlibStream(compressedStream, Ionic.Zlib.CompressionMode.Decompress);

            decompressStream.CopyTo(targetStream);

            return new Reader(targetStream.ToArray(), file);
        }
    }
}
