namespace TheAirBlow.Thor.Library.PIT; 

public class PitEntry {
    public int BinaryType;
    public int DeviceType;
    public int PartitionID;
    public int Attributes;       // Or Partition Type
    public int UpdateAttributes; // Or File System
    public int BlockSize;
    public int BlockCount;
    public int FileOffset;
    public int FileSize;
    public string Partition;
    public string FileName;
    public string DeltaName;
}