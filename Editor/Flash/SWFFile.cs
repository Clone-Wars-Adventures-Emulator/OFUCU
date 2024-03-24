using CWAEmu.OFUCU.Flash.Records;
using CWAEmu.OFUCU.Flash.Tags;
using CWAEmu.Ionic.Zlib;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Rect = CWAEmu.OFUCU.Flash.Records.Rect;
using CWAEmu.OFUCU.Data;

namespace CWAEmu.OFUCU.Flash {
    // G:\Programming\CWAEmu\OldCWAData\____.swf
    // G:\Programming\CWAEmu\OldCWAData\StuntGungan.swf
    // G:\Programming\CWAEmu\OldCWAData\StarTyper.swf
    // G:\Programming\CWAEmu\OldCWAData\DailyTrivia.swf
    // G:\Programming\CWAEmu\OldCWAData\FleetCommander.swf
    [System.Serializable]
    public class SWFFile {
        public char Signature1 => sig1;
        private char sig1;
        public char Signature2 => sig2;
        private char sig2;
        public char Signature3 => sig3;
        private char sig3;
        public byte Version => version;
        private byte version;
        public string Name => name;
        private string name;
        public string FullName => fullName;
        private string fullName;
        public Rect FrameSize => frameSize;
        private Rect frameSize;
        public float FrameRate => frameRate;
        private float frameRate;
        public ushort FrameCount => frameCount;
        private ushort frameCount;
        public FileAttributesTag AttributesTag => attributesTag;
        private FileAttributesTag attributesTag;
        public List<FlashTagHeader> TagHeaders => tagHeaders;
        private readonly List<FlashTagHeader> tagHeaders = new();
        public Dictionary<int, CharacterTag> CharacterTags => charTags;
        private readonly Dictionary<int, CharacterTag> charTags = new();
        public Dictionary<int, DefineShape> Shapes => shapes;
        private readonly Dictionary<int, DefineShape> shapes = new();
        public Dictionary<int, ImageCharacterTag> Images => images;
        private readonly Dictionary<int, ImageCharacterTag> images = new();
        public Dictionary<int, DefineSprite> Sprites => sprites;
        private readonly Dictionary<int, DefineSprite> sprites = new();
        public List<Frame> Frames => frames;
        private readonly List<Frame> frames = new();
        public List<DefineScalingGrid> ScalingGrids => scalingGrids;
        private readonly List<DefineScalingGrid> scalingGrids = new();
        public JPEGTable JPEGTable => jpegTable;
        private JPEGTable jpegTable;

        private SWFFile(string name) {
            fullName = name;
            this.name = name[0..name.LastIndexOf('.')];
        }

        private void parseFull(Reader reader) {
            if (Version >= 8) {
                FlashTagHeader header = reader.readFlashTagHeader();
                if (header.TagType != FileAttributesTag.TAG_TYPE) {
                    // error
                    return;
                }

                attributesTag = FileAttributesTag.readTag(header, reader);
            }

            Frame curFrame = new();

            // read all tags until completion
            while (!reader.ReachedEnd) {
                FlashTagHeader header = reader.readFlashTagHeader();
                TagHeaders.Add(header);

                if (Settings.Instance.EnhancedLogging) {
                    Debug.Log($"Tag: {header.TagType} {header.TagLength}");
                }

                // if end tag, stop parsing
                if (header.TagType == 0) {
                    if (Settings.Instance.EnhancedLogging) {
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

                    case 6:  // DefineBits
                        DefineBits defBits = new();
                        defBits.Header = header;
                        defBits.read(reader);

                        CharacterTags.Add(defBits.CharacterId, defBits);
                        Images.Add(defBits.CharacterId, defBits);
                        break;
                    case 8:  // JPEGTables
                        if (JPEGTable != null) {
                            Debug.LogError("There cannot be more than one JPEGTable tag in a flash file");
                            break;
                        }
                        
                        JPEGTable jTable = new();
                        jTable.Header = header;
                        jTable.read(reader);

                        jpegTable = jTable;
                        break;
                    case 21: // DefineBitsJPEG2
                        DefineBitsJPEG2 jpg2 = new();
                        jpg2.Header = header;
                        jpg2.read(reader);

                        CharacterTags.Add(jpg2.CharacterId, jpg2);
                        Images.Add(jpg2.CharacterId, jpg2);
                        break;
                    case 35: // DefineBitsJPEG3
                        DefineBitsJPEG3 jpg3 = new();
                        jpg3.Header = header;
                        jpg3.read(reader);

                        CharacterTags.Add(jpg3.CharacterId, jpg3);
                        Images.Add(jpg3.CharacterId, jpg3);
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
                        int nextIdx = curFrame.FrameIndex + 1;
                        curFrame = new();
                        curFrame.FrameIndex = nextIdx;
                        break;

                    case 7: // DefineButton
                        DefineButton db = new();
                        db.Header = header;
                        db.read(reader);

                        CharacterTags.Add(db.CharacterId, db);
                        break;

                    case 34: // DefineButton2
                        DefineButton2 db2 = new();
                        db2.Header = header;
                        db2.read(reader);

                        CharacterTags.Add(db2.CharacterId, db2);
                        break;

                    case 37: // DefineEditText
                        DefineEditText det = new();
                        det.Header = header;
                        det.read(reader);

                        CharacterTags.Add(det.CharacterId, det);
                        break;

                    case 78: // DefineScalingGrid
                        DefineScalingGrid dsg = new();
                        dsg.Header = header;
                        dsg.read(reader);

                        ScalingGrids.Add(dsg);
                        break;

                    case 74: // CSMTextSettings           IMPORTANT
                    case 56: // ExportAssets              THIS NAMES SPRITES IT NEEDS TO BE PARSED

                    //  = = = = = = = = = = Unknown how to handle = = = = = = = = = =
                    case 71: // ImportAssets2
                    case 75: // DefineFont3
                    case 73: // DefineFontAlignZones
                    case 77: // Metadata
                    case 88: // DefineFontName

                    //  = = = = = = = = = = No need to parse = = = = = = = = = =

                    case 9:  // SetBackgroundColor
                    case 24: // Protect
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

            if (Settings.Instance.EnhancedLogging) {
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
            Images.Add(bits.CharacterId, bits);
        }

        private void readSprite(FlashTagHeader header, Reader reader) {
            DefineSprite ds = new();
            ds.Header = header;
            ds.read(reader);

            CharacterTags.Add(ds.CharacterId, ds);
            Sprites.Add(ds.CharacterId, ds);
        }

        public static SWFFile readFull(string path, bool parseImages = true) {
            if (!File.Exists(path)) {
                Debug.LogError($"File `{path}` does not exist!");
                return null;
            }

            string name = Path.GetFileName(path);
            SWFFile file = new(name);

            var stream = File.OpenRead(path);
            var binReader = new BinaryReader(stream);

            // Read SWF magic and version
            file.sig1 = binReader.ReadChar();
            file.sig2 = binReader.ReadChar();
            file.sig3 = binReader.ReadChar();
            file.version = binReader.ReadByte();

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

            Reader reader = new(data, file, parseImages);
            file.frameSize = Rect.readRect(reader);
            file.frameRate = reader.readUInt16() / 256.0f;
            file.frameCount = reader.readUInt16();

            if (Settings.Instance.EnhancedLogging) {
                Debug.Log($"{file.FrameSize.X},{file.FrameSize.Y} X {file.FrameSize.Width},{file.FrameSize.Height} @ {file.FrameRate}fps with {file.FrameCount} frames");
            }

            file.parseFull(reader);

            return file;
        }

        // NOTE: this has ZERO knowledge of any usage in actionscript
        public void destructivelyTrimUnused() {
            Dictionary<int, int> usageCount = new();

            foreach (int id in CharacterTags.Keys) {
                usageCount.Add(id, 0);
            }

            foreach (var shape in Shapes.Values) {
                FillStyleArray fsa = shape.Shapes.FillStyles;
                LineStyleArray lsa = shape.Shapes.LineStyles;
                int fill0Idx = -1;
                int fill1Idx = -1;
                int lineIdx = -1;
                bool dontEndOnSCR = true;

                Action onEndShape = () => {
                    if (fill0Idx != -1 && fill1Idx != -1) {
                        // ignore
                        return;
                    }

                    FillStyle singleStyle = null;
                    if (fill0Idx != -1) {
                        singleStyle = fsa[fill0Idx];
                    }

                    if (fill1Idx != -1) {
                        singleStyle = fsa[fill1Idx];
                    }

                    if (singleStyle == null) {
                        return;
                    }

                    byte fillTypeAsByte = ((byte)singleStyle.Type);
                    if ((fillTypeAsByte & 0x40) != 0x40) {
                        // ignore
                        return;
                    }

                    // incase the image didnt get parsed or something
                    if (usageCount.ContainsKey(singleStyle.BitmapId)) {
                        usageCount[singleStyle.BitmapId]++;
                    }
                };

                foreach (var record in shape.Shapes.ShapeRecords) {
                    if (record is StyleChangeRecord) {
                        var scr = record as StyleChangeRecord;

                        // case signifying end of shape
                        if (!dontEndOnSCR) {
                            onEndShape();
                        }
                        dontEndOnSCR = true;

                        if (scr.StateNewStyles) {
                            fsa = scr.FillStyles;
                            lsa = scr.LineStyles;
                        }

                        if (scr.StateFillStyle0) {
                            fill0Idx = (int)scr.FillStyle0 - 1;
                        }

                        if (scr.StateFillStyle1) {
                            fill1Idx = (int)scr.FillStyle1 - 1;
                        }

                        if (scr.StateLineStyle) {
                            lineIdx = (int)scr.LineStyle - 1;
                        }
                    }

                    if (record is EndShapeRecord) {
                        onEndShape();
                    }
                }
            }

            foreach (var sprite in Sprites.Values) {
                foreach (var frame in sprite.Frames) {
                    foreach (var tag in frame.Tags) {
                        // also handles place object 3
                        if (tag is PlaceObject2) {
                            PlaceObject2 po2 = tag as PlaceObject2;

                            if (po2.HasCharacter) {
                                if (usageCount.ContainsKey(po2.CharacterId)) {
                                    usageCount[po2.CharacterId]++;
                                }
                            }
                        }
                    }
                }
            }

            foreach (var frame in Frames) {
                foreach (var tag in frame.Tags) {
                    // also handles place object 3
                    if (tag is PlaceObject2) {
                        PlaceObject2 po2 = tag as PlaceObject2;

                        if (po2.HasCharacter) {
                            if (usageCount.ContainsKey(po2.CharacterId)) {
                                usageCount[po2.CharacterId]++;
                            }
                        }
                    }
                }
            }

            // remove and log which are being removed
            foreach (var pair in usageCount) {
                if (pair.Value == 0) {
                    int id = pair.Key;
                    CharacterTags.Remove(id);

                    if (Images.ContainsKey(id)) {
                        Images.Remove(id);
                    }

                    if (Shapes.ContainsKey(id)) {
                        Shapes.Remove(id);
                    }

                    if (Sprites.ContainsKey(id)) {
                        Sprites.Remove(id);
                    }
                }
            }
        }
    }
}
