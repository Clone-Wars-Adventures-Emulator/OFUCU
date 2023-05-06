using CWAEmu.FlashConverter.Flash.Records;
using CWAEmu.FlashConverter.Flash.Tags;
using Ionic.Zlib;
using System;
using System.Collections.Generic;
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
        public Dictionary<int, CharacterTag> CharacterTags { get; private set; }
        public Dictionary<int, DefineShape> Shapes { get; private set; }


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
                    Debug.Log($"End Tag found");
                    break;
                }

                switch (header.TagType) {
                    // = = = = = = = = = = Need to parse = = = = = = = = = =
                    case 2:  // DefineShape
                        readShape(1, header, reader);
                        break;
                    case 22: // DefineShape2
                        readShape(2, header, reader);
                        break;
                    case 32: // DefineShape3
                        readShape(3, header, reader);
                        break;
                    case 83: // DefineShape4
                        readShape(4, header, reader);
                        break;

                    case 39: // DefineSprite

                    case 26: // PlaceObject2

                    case 20: // DefineBitsLossless
                    case 36: // DefineBitsLossless2
                    case 6:  // DefineBits
                    case 8:  // JPEGTables
                    case 21: // DefineBitsJPEG2
                    case 35: // DefineBitsJPEG3

                    case 34: // DefineButton2

                    case 37: // DefineEditText
                    case 74: // CSMTextSettings           IMPORTANT

                    case 78: // DefineScalingGrid

                    //  = = = = = = = = = = Unknown how to handle = = = = = = = = = =
                    case 1:  // ShowFrame
                    case 71: // ImportAssets2
                    case 75: // DefineFont3
                    case 73: // DefineFontAlignZones
                    case 77: // Metadata
                    case 88: // DefineFontName

                    //  = = = = = = = = = = No need to parse = = = = = = = = = =
                    case 9:  // SetBackgroundColor
                    case 24: // Protect
                    case 56: // ExportAssets
                    case 59: // DoInitAction
                        reader.skip(header.TagLength);
                        break;
                    default:
                        Debug.Log($"Skipping {header.TagLength} bytes for tag {header.TagType}");
                        reader.skip(header.TagLength);
                        break;
                }
            }
        }

        private void readShape(int shapeType, FlashTagHeader header, Reader reader) {
            DefineShape ds1 = new();
            ds1.Header = header;
            ds1.ShapeType = shapeType;
            ds1.read(reader);

            CharacterTags.Add(ds1.CharacterId, ds1);
            Shapes.Add(ds1.CharacterId, ds1);
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
