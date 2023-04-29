using CWAEmu.FlashConverter.Flash.Records;
using CWAEmu.FlashConverter.Flash.Tags;
using Ionic.Zlib;
using System.IO;
using UnityEngine;
using Rect = CWAEmu.FlashConverter.Flash.Records.Rect;

namespace CWAEmu.FlashConverter.Flash {
    public class SWFFile {
        public char Signature1 { get; private set; }
        public char Signature2 { get; private set; }
        public char Signature3 { get; private set; }
        public byte Version { get; private set; }
        public string Name { get; private set; }
        public Rect FrameSize { get; private set; }
        public float FrameRate { get; private set; }
        public ushort FrameCount { get; private set; }
        public FileAttributesTag AttributesTag { get; private set; }


        private SWFFile(string name) {
            Name = name;
        }

        private void parseFull(Reader reader) {
            if (Version >= 8) {
                FlashTagHeader header = reader.readFlashTagHeader();
                if (header.TagType != FileAttributesTag.TAG_TYPE) {
                    // error
                    return;
                }

                AttributesTag = FileAttributesTag.readTag(header, reader);
            }

            // read all tags until completion
            while (!reader.ReachedEnd) {
                FlashTagHeader header = reader.readFlashTagHeader();

                // if end tag, stop parsing
                if (header.TagType == 0) {
                    break;
                }

                switch (header.TagType) {

                    default:
                        reader.skip(header.TagLength);
                        break;
                }
            }
        }

        public static SWFFile readFull(string path) {
            if (!File.Exists(path)) {
                Debug.LogError($"File `{path}` does not exist!");
                return null;
            }

            string name = Path.GetFileName(path);
            SWFFile file = new(name);

            var stream = File.OpenRead(path);
            var binReader = new BinaryReader(stream);

            // Read SWF magic and version
            file.Signature1 = binReader.ReadChar();
            file.Signature2 = binReader.ReadChar();
            file.Signature3 = binReader.ReadChar();
            file.Version = binReader.ReadByte();

            if ((file.Signature1 != 'C' && file.Signature1 != 'F') || file.Signature2 != 'W' || file.Signature3 != 'S') {
                Debug.LogError("Invalid file signature.");
                return null;
            }

            if (file.Version > 9) {
                Debug.LogError("File version must be 9 or older.");
                return null;
            }

            uint uncompressedLen = binReader.ReadUInt32();
            // may read less than uncompressedLen, this is okay as long as it reads all of the remaining data
            byte[] rawBytes = binReader.ReadBytes((int)uncompressedLen);
            byte[] data = rawBytes;

            binReader.Close();
            stream.Close();

            if (file.Signature1 == 'C') {
                using var targetStream = new MemoryStream();

                using var compressedStream = new MemoryStream(rawBytes);
                using var decompressStream = new ZlibStream(compressedStream, Ionic.Zlib.CompressionMode.Decompress);

                decompressStream.CopyTo(targetStream);

                data = targetStream.ToArray();
            }

            Reader reader = new(data, file.Version);
            file.FrameSize = Rect.readRect(reader);
            file.FrameRate = reader.readUInt16() / 256.0f;
            file.FrameCount = reader.readUInt16();

            Debug.Log($"{file.FrameSize.X},{file.FrameSize.Y} X {file.FrameSize.Width},{file.FrameSize.Height} @ {file.FrameRate}fps with {file.FrameCount} frames");

            file.parseFull(reader);

            return file;
        }
    }
}
