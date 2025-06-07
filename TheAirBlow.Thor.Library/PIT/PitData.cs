namespace TheAirBlow.Thor.Library.PIT; 

public class PitData {
    public readonly List<PitEntry> Entries = [];
    public FieldMapper.Mapper Mapper { get; set; } = null!;
    public bool IsNewVersion { get; set; }
    public string Unknown { get; set; } = "";
    public string Project { get; set; } = "";
    public int Reserved { get; set; }

    public PitData(byte[] content)
        => Parse(new MemoryStream(content));
    
    public PitData(string path)
        => Parse(new FileStream(path, FileMode.Open, FileAccess.Read));
    
    public PitData() { }

    private void Parse(Stream stream) {
        using var reader = new BinaryReader(stream);
        if (reader.ReadInt32() != 0x12349876)
            throw new InvalidDataException("Magic number mismatch!");
        var entries = reader.ReadInt32();
        Unknown = reader.ReadString(8);
        Project = reader.ReadString(8);
        Reserved = reader.ReadInt32();
        var lastBlockSize = 0;
        for (var i = 0; i < entries; i++) {
            var entry = new PitEntry {
                BinaryType = reader.ReadInt32(),
                DeviceType = reader.ReadInt32(),
                PartitionId = reader.ReadInt32(),
                Attributes = reader.ReadInt32(),
                UpdateAttributes = reader.ReadInt32(),
                BlockSize = reader.ReadInt32(),
                BlockCount = reader.ReadInt32(),
                FileOffset = reader.ReadInt32(),
                FileSize = reader.ReadInt32(),
                Partition = reader.ReadString(32),
                FileName = reader.ReadString(32),
                DeltaName = reader.ReadString(32)
            };
            if (i > 0 && lastBlockSize != entry.BlockSize)
                IsNewVersion = true;
            lastBlockSize = entry.BlockSize;
            Entries.Add(entry);
        }

        // Field mappers
        Mapper = IsNewVersion 
            ? FieldMapper.NewPitMapper 
            : FieldMapper.OldPitMapper;
        stream.Dispose();
    }
}