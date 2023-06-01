using CWAEmu.FlashConverter.Flash.Records;
using CWAEmu.FlashConverter.Flash.Tags;
using CWAEmu.Ionic.Zlib;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Rect = CWAEmu.FlashConverter.Flash.Records.Rect;

namespace CWAEmu.FlashConverter.Flash {
    // G:\Programming\CWAEmu\OldCWAData\____.swf
    // G:\Programming\CWAEmu\OldCWAData\StuntGungan.swf
    public class SWFFile {
        // TODO make this a config value of the plugin
        public static readonly bool IndepthLogging = false;

        public char Signature1 { get; private set; }
        public char Signature2 { get; private set; }
        public char Signature3 { get; private set; }
        public byte Version { get; private set; }
        public string Name { get; private set; }
        public Rect FrameSize { get; private set; }
        public float FrameRate { get; private set; }
        public ushort FrameCount { get; private set; }
        public FileAttributesTag AttributesTag { get; private set; }
        public List<FlashTagHeader> TagHeaders { get; private set; } = new();
        public Dictionary<int, CharacterTag> CharacterTags { get; private set; } = new();
        public Dictionary<int, DefineShape> Shapes { get; private set; } = new();
        public Dictionary<int, FlashImage> Images { get; private set; } = new();
        public Dictionary<int, DefineSprite> Sprites { get; private set; } = new();
        public List<Frame> Frames { get; private set; } = new();

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

            Frame curFrame = new();

            // read all tags until completion
            while (!reader.ReachedEnd) {
                FlashTagHeader header = reader.readFlashTagHeader();
                TagHeaders.Add(header);

                // if end tag, stop parsing
                if (header.TagType == 0) {
                    if (IndepthLogging) {
                        Debug.Log($"================ End Tag found ================");
                    }
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

                    case 20: // DefineBitsLossless
                        readBitsLossless(1, header, reader);
                        break;
                    case 36: // DefineBitsLossless2
                        readBitsLossless(2, header, reader);
                        break;

                    case 39: // DefineSprite
                        readSprite(header, reader);
                        break;

                    case 26: // PlaceObject2
                        PlaceObject2 po2 = new();
                        po2.Header = header;
                        po2.read(reader);

                        curFrame.addTag(po2);
                        break;

                    case 1:  // ShowFrame
                        Frames.Add(curFrame);
                        curFrame = new();
                        break;

                    case 34: // DefineButton2

                    case 37: // DefineEditText
                    case 74: // CSMTextSettings           IMPORTANT

                    //  = = = = = = = = = = Unknown how to handle = = = = = = = = = =
                    case 71: // ImportAssets2
                    case 75: // DefineFont3
                    case 73: // DefineFontAlignZones
                    case 77: // Metadata
                    case 88: // DefineFontName

                    //  = = = = = = = = = = No need to parse = = = = = = = = = =
                    // SKIPPING THESE, JPEG ALGO is GARBAGE
                    case 6:  // DefineBits
                    case 8:  // JPEGTables
                    case 21: // DefineBitsJPEG2
                    case 35: // DefineBitsJPEG3

                    case 9:  // SetBackgroundColor
                    case 24: // Protect
                    case 56: // ExportAssets
                    case 59: // DoInitAction
                        reader.skip(header.TagLength);
                        break;
                    default:
                        Debug.LogWarning($"Skipping {header.TagLength} bytes for tag {header.TagType}");
                        reader.skip(header.TagLength);
                        break;
                }
            }

            Dictionary<int, int> count = new();
            foreach (var header in TagHeaders) {
                if (count.ContainsKey(header.TagType)) {
                    count[header.TagType]++;
                } else {
                    count.Add(header.TagType, 1);
                }
            }

            if (IndepthLogging) {
                foreach (var tagType in count.Keys) {
                    Debug.Log($"There are {count[tagType]} tags of type {tagType}");
                }
            }
        }

        private void readShape(int shapeType, FlashTagHeader header, Reader reader) {
            DefineShape ds = new();
            ds.Header = header;
            ds.ShapeType = shapeType;
            ds.read(reader);

            CharacterTags.Add(ds.CharacterId, ds);
            Shapes.Add(ds.CharacterId, ds);
        }

        private void readBitsLossless(int type, FlashTagHeader header, Reader reader) {
            DefineBitsLossless bits = new();
            bits.Header = header;
            bits.BitsLosslessType = type;
            bits.read(reader);

            CharacterTags.Add(bits.CharacterId, bits);
            Images.Add(bits.CharacterId, bits.ImageData);
        }

        private void readSprite(FlashTagHeader header, Reader reader) {
            DefineSprite ds = new();
            ds.Header = header;
            ds.read(reader);

            CharacterTags.Add(ds.CharacterId, ds);
            Sprites.Add(ds.CharacterId, ds);
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
                Debug.LogError($"File version {file.Version} is too new! Must be 9 or older.");
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
                using var decompressStream = new ZlibStream(compressedStream, CompressionMode.Decompress);

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
