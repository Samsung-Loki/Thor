namespace TheAirBlow.Thor.Library.PIT; 

public class PitEntry {
    public int BinaryType { get; set; }
    public int DeviceType { get; set; }
    public int PartitionId { get; set; }
    public int Attributes { get; set; }
    public int UpdateAttributes { get; set; }
    public int BlockSize { get; set; }
    public int BlockCount { get; set; }
    public int FileOffset { get; set; }
    public int FileSize { get; set; }
    public string Partition { get; set; } = "";
    public string FileName { get; set; } = "";
    public string DeltaName { get; set; } = "";
}