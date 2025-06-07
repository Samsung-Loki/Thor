using System.Text;
using Serilog;
using TheAirBlow.Thor.Library.Communication;
using TheAirBlow.Thor.Library.PIT;

namespace TheAirBlow.Thor.Library.Protocols; 

public class Odin {
    public struct VersionStruct {
        public byte Unknown1;
        public byte Unknown2;
        public short Version;
    }
    
    private IHandler _handler;
    private int FlashTimeout;
    private int FlashPacketSize;
    private int FlashSequence;
    public bool TFlashEnabled { get; private set; }
    public bool ResetFlashCount = true;
    public VersionStruct Version;
    public bool BootloaderUpdate;
    public bool EfsClear;

    public Odin(IHandler handler)
        => _handler = handler;

    public void Handshake() {
        _handler.BulkWrite(Encoding.ASCII.GetBytes("ODIN"));
        var buf = _handler.BulkRead(4, out var read);
        if (read != 4) throw new InvalidDataException(
            $"Received {read} bytes instead of 4!");
        var str = Encoding.ASCII.GetString(buf);
        if (str != "LOKE") throw new InvalidDataException(
            $"Received {str} instead of LOKE!");
    }

    // Begin session region, 0x64
    public void BeginSession() {
        var buf = new byte[1024];
        buf.WriteInt(0x64, 0);
        buf.WriteInt(0x00, 4);
        // Write the proto version to be the max value
        // So it would basically catch-all BL versions
        buf.WriteInt(int.MaxValue, 8);
        _handler.BulkWrite(buf);
        buf = _handler.BulkRead(8, out var read);
        if (read != 8) throw new InvalidDataException(
            $"Received {read} bytes instead of 8!");
        buf.OdinFailCheck("BeginSession");
        Version = new VersionStruct {
            Unknown1 = buf[4],
            Unknown2 = buf[5],
            Version = BitConverter.ToInt16(new[] {
                buf[6], buf[7]
            })
        };
        var version = buf.ReadInt(4);
        Log.Debug("Bootloader version integer 0x{0:X8}", version);
        Log.Debug("Unknown1: {0}, Unknown2: {1}, Version: {2}",
            Version.Unknown1, Version.Unknown2, Version.Version);
        switch (Version.Version) {
            case 0 or 1:
                FlashTimeout = 30000;      // 30 seconds
                FlashPacketSize = 131072;  // 128 KiB
                FlashSequence = 240;       // 30 MB
                break;
            case >= 2:
                FlashTimeout = 120000;     // 2 minutes
                FlashPacketSize = 1048576; // 1 MiB
                FlashSequence = 30;        // 30 MiB
                break;
        }

        if (Version.Unknown1 != 0) {
            Log.Information("Unknown1 is not zero: {0:x2}", Version.Unknown1);
            Log.Information("Please contact me (TheAirBlow) about this in XDA DMs!");
            Log.Information("If you would cooperate, we could uncover hidden features!");
        }
        
        if (Version.Unknown2 != 0) {
            Log.Information("Unknown2 is not zero: {0:x2}", Version.Unknown2);
            Log.Information("Please contact me (TheAirBlow) about this in XDA DMs!");
            Log.Information("If you would cooperate, we could uncover hidden features!");
        }

        if (Version.Version > 1) {
            Log.Debug("Sending file part size of {0}", FlashPacketSize);
            buf = new byte[1024];
            buf.WriteInt(0x64, 0);
            buf.WriteInt(0x05, 4);
            buf.WriteInt(FlashPacketSize, 8);
            _handler.BulkWrite(buf);
            buf = _handler.BulkRead(8, out read);
            if (read != 8) throw new InvalidDataException(
                $"Received {read} bytes instead of 8!");
            buf.OdinFailCheck("SendFilePartSize");
        }
    }

    public void SetTotalBytes(long total) {
        var buf = new byte[1024];
        buf.WriteInt(0x64, 0);
        buf.WriteInt(0x02, 4);
        buf.WriteLong(total, 8);
        _handler.BulkWrite(buf);
        buf = _handler.BulkRead(8, out var read);
        if (read != 8) throw new InvalidDataException(
            $"Received {read} bytes instead of 8!");
        buf.OdinFailCheck("SetTotalBytes");
    }

    public void EraseUserData() {
        var buf = new byte[1024];
        buf.WriteInt(0x64, 0);
        buf.WriteInt(0x07, 4);
        _handler.BulkWrite(buf);
        buf = _handler.BulkRead(8, out var read, 600000);
        if (read != 8) throw new InvalidDataException(
            $"Received {read} bytes instead of 8!");
        buf.OdinFailCheck("EraseUserData");
    }
    
    public void EnableTFlash() {
        if (TFlashEnabled) throw new InvalidOperationException(
            "T-Flash mode was already enabled!");
        var buf = new byte[1024];
        buf.WriteInt(0x64, 0);
        buf.WriteInt(0x08, 4);
        _handler.BulkWrite(buf);
        buf = _handler.BulkRead(8, out var read, 600000);
        if (read != 8) throw new InvalidDataException(
            $"Received {read} bytes instead of 8!");
        buf.OdinFailCheck("EnableTFlash");
        TFlashEnabled = true;
    }
    
    public void SetRegionCode(string code) {
        if (code.Length != 3)
            throw new InvalidDataException(
                "Region code should be length of 3!");
        var buf = new byte[1024];
        buf.WriteInt(0x64, 0);
        buf.WriteInt(0x08, 4);
        buf.WriteString(code, 8);
        _handler.BulkWrite(buf);
        buf = _handler.BulkRead(8, out var read, 600000);
        if (read != 8) throw new InvalidDataException(
            $"Received {read} bytes instead of 8!");
        buf.OdinFailCheck("SetRegionCode");
    }

    // End session region, 0x67
    public void EndSession() {
        var buf = new byte[1024];
        buf.WriteInt(0x67, 0);
        buf.WriteInt(0x00, 4);
        _handler.BulkWrite(buf);
        buf = _handler.BulkRead(8, out var read);
        if (read != 8) throw new InvalidDataException(
            $"Received {read} bytes instead of 8!");
        buf.OdinFailCheck("EndSession");
    }
    
    public void Reboot() {
        var buf = new byte[1024];
        buf.WriteInt(0x67, 0);
        buf.WriteInt(0x01, 4);
        _handler.BulkWrite(buf);
        buf = _handler.BulkRead(8, out var read);
        if (read != 8) throw new InvalidDataException(
            $"Received {read} bytes instead of 8!");
        buf.OdinFailCheck("Reboot");
    }
    
    public void RebootToOdin() {
        var buf = new byte[1024];
        buf.WriteInt(0x67, 0);
        buf.WriteInt(0x02, 4);
        _handler.BulkWrite(buf);
        buf = _handler.BulkRead(8, out var read);
        if (read != 8) throw new InvalidDataException(
            $"Received {read} bytes instead of 8!");
        buf.OdinFailCheck("RebootToOdin");
    }
    
    public void Shutdown() {
        var buf = new byte[1024];
        buf.WriteInt(0x67, 0);
        buf.WriteInt(0x03, 4);
        _handler.BulkWrite(buf);
        buf = _handler.BulkRead(8, out var read);
        if (read != 8) throw new InvalidDataException(
            $"Received {read} bytes instead of 8!");
        buf.OdinFailCheck("Shutdown");
    }

    // PIT region, 0x65
    public byte[] DumpPIT() {
        // Request PIT dump
        var buf = new byte[1024];
        buf.WriteInt(0x65, 0);
        buf.WriteInt(0x01, 4);
        _handler.BulkWrite(buf);
        buf = _handler.BulkRead(8, out var read);
        if (read != 8) throw new InvalidDataException(
            $"Received {read} bytes instead of 8!");
        buf.OdinFailCheck("RequestPitDump");
        var size = buf.ReadInt(4);
        var blocks = (int)Math.Ceiling((decimal)size / 500);
        Log.Debug("PIT size is {0:X4} ({0}), {1} total blocks", 
            size, blocks);

        // Dump PIT blocks
        var pitBuf = new byte[size];
        for (var i = 0; i < blocks; i++) {
            try {
                buf = new byte[1024];
                buf.WriteInt(0x65, 0);
                buf.WriteInt(0x02, 4);
                buf.WriteInt(i, 8);
                _handler.BulkWrite(buf);
                buf = _handler.BulkRead(500, out _);
                Array.Copy(buf, 0, pitBuf, 
                    i*500, buf.Length);
            } catch {
                Log.Debug("Failed to read block {0}", i);
                throw;
            }
        }

        try {
            _handler.ReadZLP();
        } catch { /* L + ratio */ }

        // End PIT dump
        buf = new byte[1024];
        buf.WriteInt(0x65, 0);
        buf.WriteInt(0x03, 4);
        _handler.BulkWrite(buf);
        buf = _handler.BulkRead(8, out read);
        if (read != 8) throw new InvalidDataException(
            $"Received {read} bytes instead of 8!");
        buf.OdinFailCheck("EndPitDump");
        return pitBuf;
    }
    
    public void FlashPIT(byte[] content) {
        // Request PIT flash
        var buf = new byte[1024];
        buf.WriteInt(0x65, 0);
        buf.WriteInt(0x00, 4);
        _handler.BulkWrite(buf);
        buf = _handler.BulkRead(8, out var read);
        if (read != 8) throw new InvalidDataException(
            $"Received {read} bytes instead of 8!");
        buf.OdinFailCheck("RequestPitFlash");
        
        // Begin PIT flash
        buf = new byte[1024];
        buf.WriteInt(0x65, 0);
        buf.WriteInt(0x02, 4);
        buf.WriteInt(content.Length, 8);
        _handler.BulkWrite(buf);
        buf = _handler.BulkRead(8, out read);
        if (read != 8) throw new InvalidDataException(
            $"Received {read} bytes instead of 8!");
        buf.OdinFailCheck("BeginPitFlash");
        
        // Send the PIT file
        _handler.BulkWrite(content);
        buf = _handler.BulkRead(8, out read, 120000);
        if (read != 8) throw new InvalidDataException(
            $"Received {read} bytes instead of 8!");
        buf.OdinFailCheck("SendPitFile");
        
        // End PIT flash
        buf = new byte[1024];
        buf.WriteInt(0x65, 0);
        buf.WriteInt(0x03, 4);
        _handler.BulkWrite(buf);
        buf = _handler.BulkRead(8, out read);
        if (read != 8) throw new InvalidDataException(
            $"Received {read} bytes instead of 8!");
        buf.OdinFailCheck("EndPitFlash");
    }

    public class FlashProgressInfo {
        public enum StateEnum {
            Sending, Flashing
        }
        
        public int SequenceIndex;
        public int TotalSequences;
        public StateEnum State;
        public long TotalBytes;
        public long SentBytes;

        public FlashProgressInfo(int index, int sequences, 
            long sent, long total, StateEnum state) {
            SequenceIndex = index; TotalSequences = sequences;
            SentBytes = sent; TotalBytes = total; State = state;
        }
    }
    
    // Flashing region, 0x66
    public void FlashPartition(Stream? stream, PitEntry entry, Action<FlashProgressInfo> progress, long length = -1) {
        if (stream != null) length = stream.Length;
        
        // Initialize progress bar
        progress(new FlashProgressInfo(0, 0, 
            0, 0, FlashProgressInfo.StateEnum.Sending));

        // Request file flash
        var buf = new byte[1024];
        buf.WriteInt(0x66, 0);
        buf.WriteInt(0x00, 4);
        _handler.BulkWrite(buf);
        buf = _handler.BulkRead(8, out var read);
        if (read != 8) throw new InvalidDataException(
            $"Received {read} bytes instead of 8!");
        buf.OdinFailCheck("RequestFileFlash");

        var totalBytes = 0;
        var sequence = FlashPacketSize * FlashSequence;
        var sequences = (int)(length / sequence);
        var lastSequence = (int)(length % sequence);
        if (lastSequence != 0) sequences++;
        else lastSequence = sequence;
        for (var i = 0; i < sequences; i++) {
            var last = i + 1 == sequences;
            var realSize = last ? lastSequence : sequence;
            var alignedSize = realSize;
            if (realSize % FlashPacketSize != 0)
                alignedSize += FlashPacketSize - realSize % FlashPacketSize;
            progress(new FlashProgressInfo(i, sequences, 
                totalBytes, length, FlashProgressInfo.StateEnum.Sending));

            // Request sequence flash
            buf = new byte[1024];
            buf.WriteInt(0x66, 0);
            buf.WriteInt(0x02, 4);
            buf.WriteInt(alignedSize, 8);
            _handler.BulkWrite(buf);
            buf = _handler.BulkRead(8, out read);
            if (read != 8) throw new InvalidDataException(
                $"Received {read} bytes instead of 8!");
            buf.OdinFailCheck($"RequestSequenceFlash/{i}");

            // Send file part
            var parts = alignedSize / FlashPacketSize;
            for (var j = 0; j < parts; j++) {
                buf = new byte[FlashPacketSize];
                stream?.Read(buf, 0, FlashPacketSize);
                _handler.BulkWrite(buf);
                buf = _handler.BulkRead(8, out read);
                if (read != 8) throw new InvalidDataException(
                    $"Received {read} bytes instead of 8!");
                buf.OdinFailCheck($"SendFilePart/{i}");
                var index = buf.ReadInt(4);
                if (index != j) throw new InvalidOperationException(
                    $"Expected index to be {j} but bootloader sent {index}!");
                totalBytes += FlashPacketSize;
                progress(new FlashProgressInfo(i, sequences, 
                    totalBytes, length, FlashProgressInfo.StateEnum.Sending));
            }
            
            progress(new FlashProgressInfo(i, sequences,
                totalBytes, length, FlashProgressInfo.StateEnum.Flashing));

            // End file sequence flash
            if (entry.BinaryType == 1) {
                // Flash modem firmware
                buf = new byte[1024];
                buf.WriteInt(0x66, 0);
                buf.WriteInt(0x03, 4);
                buf.WriteInt(0x01, 8);
                buf.WriteInt(realSize, 12);
                buf.WriteInt(entry.BinaryType, 16);
                buf.WriteInt(entry.DeviceType, 20);
                buf.WriteInt(last ? 1 : 0, 24);
                _handler.BulkWrite(buf);
            } else {
                // Flash phone firmware
                buf = new byte[1024];
                buf.WriteInt(0x66, 0);
                buf.WriteInt(0x03, 4);
                buf.WriteInt(0x00, 8);
                buf.WriteInt(realSize, 12);
                buf.WriteInt(entry.BinaryType, 16);
                buf.WriteInt(entry.DeviceType, 20);
                buf.WriteInt(entry.PartitionId, 24);
                buf.WriteInt(last ? 1 : 0, 28);
                buf.WriteInt(EfsClear ? 1 : 0, 32);
                buf.WriteInt(BootloaderUpdate ? 1 : 0, 36);
                _handler.BulkWrite(buf);
            }
            
            buf = _handler.BulkRead(8, out read, FlashTimeout);
            if (read != 8) throw new InvalidDataException(
                $"Received {read} bytes instead of 8!");
            buf.OdinFailCheck($"EndSequenceFlash/{i}", true);
        }

        // Reset flash count
        if (ResetFlashCount) {
            buf = new byte[1024];
            buf.WriteInt(0x64, 0);
            buf.WriteInt(0x01, 4);
            _handler.BulkWrite(buf);
            buf = _handler.BulkRead(8, out read);
            if (read != 8) throw new InvalidDataException(
                $"Received {read} bytes instead of 8!");
            buf.OdinFailCheck($"ResetFlashCount");
        }
    }
}