using System.Collections.Generic;

namespace CWAEmu.FlashConverter.Flash.Records {
    public class ClipActions {
        public ClipEventFlags AllEventFlags { get; private set; }
        public List<ClipActionRecord> Records { get; private set; } = new();

        public static ClipActions readClipActions(Reader reader) {
            ClipActions ca = new ClipActions();

            // reserved
            reader.readUInt16();

            ca.AllEventFlags = ClipEventFlags.readClipEventFlags(reader);

            ClipEventFlags lastFlags = ClipEventFlags.readClipEventFlags(reader);
            while (!lastFlags.allZero()) {
                ClipActionRecord car = ClipActionRecord.readClipActionRecord(reader, lastFlags);
                ca.Records.Add(car);

                lastFlags = ClipEventFlags.readClipEventFlags(reader);
            }

            return ca;
        }
    }

    public class ClipActionRecord {
        public ClipEventFlags EventFlags { get; private set; }
        public uint RecordSize { get; private set; }
        public byte KeyCode { get; private set; }
        public List<ActionRecord> Actions { get; private set; }

        public static ClipActionRecord readClipActionRecord(Reader reader, ClipEventFlags flags) {
            ClipActionRecord car = new();
            car.EventFlags = flags;
            car.RecordSize = reader.readUInt32();

            uint bytesToRead = car.RecordSize;
            
            if (flags.KeyPress) {
                car.KeyCode = reader.readByte();
                bytesToRead--;
            }

            while (bytesToRead > 0) {
                ActionRecord ar = ActionRecord.readActionRecord(reader);
                car.Actions.Add(ar);
                bytesToRead -= ar.getSize();
            }

            return car;
        }
    }

    public class ClipEventFlags {
        public bool KeyUp { get; private set; }
        public bool KeyDown { get; private set; }
        public bool MouseUp { get; private set; }
        public bool MouseDown { get; private set; }
        public bool MouseMove { get; private set; }
        public bool Unload { get; private set; }
        public bool EnterFrame { get; private set; }
        public bool Load { get; private set; }
        public bool DragOver { get; private set; }
        public bool RollOut { get; private set; }
        public bool RollOver { get; private set; }
        public bool ReleaseOutside { get; private set; }
        public bool Release { get; private set; }
        public bool Press { get; private set; }
        public bool Initialize { get; private set; }
        public bool Data { get; private set; }
        public byte Reserved1 { get; private set; }
        public bool Construct { get; private set; }
        public bool KeyPress { get; private set; }
        public bool DragOut { get; private set; }
        public byte Reserved2 { get; private set; }

        public bool allZero() {
            return KeyUp || KeyDown || MouseUp || MouseDown || MouseMove || Unload || EnterFrame || Load 
                || DragOver || RollOut || RollOver || ReleaseOutside || Release || Press || Initialize 
                || Data || Reserved1 == 0 || Construct || KeyPress || DragOut || Reserved2 == 0;
        }

        public static ClipEventFlags readClipEventFlags(Reader reader) {
            ClipEventFlags cef = new();

            cef.KeyUp = reader.readBitFlag();
            cef.KeyDown = reader.readBitFlag();
            cef.MouseUp = reader.readBitFlag();
            cef.MouseDown = reader.readBitFlag();
            cef.MouseMove = reader.readBitFlag();
            cef.Unload = reader.readBitFlag();
            cef.EnterFrame = reader.readBitFlag();
            cef.Load = reader.readBitFlag();
            cef.DragOver = reader.readBitFlag();
            cef.RollOut = reader.readBitFlag();
            cef.RollOver = reader.readBitFlag();
            cef.ReleaseOutside = reader.readBitFlag();
            cef.Release = reader.readBitFlag();
            cef.Press = reader.readBitFlag();
            cef.Initialize = reader.readBitFlag();
            cef.Data = reader.readBitFlag();

            if (reader.Version >= 6) {
                cef.Reserved1 = (byte)reader.readUBits(5);

                cef.Construct = reader.readBitFlag();
                cef.KeyPress = reader.readBitFlag();
                cef.DragOut = reader.readBitFlag();

                cef.Reserved2 = (byte)reader.readUBits(8);
            }

            reader.endBitRead();

            return cef;
        }
    }
}
