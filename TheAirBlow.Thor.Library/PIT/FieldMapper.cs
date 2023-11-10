namespace TheAirBlow.Thor.Library.PIT; 

public static class FieldMapper {
    public class Mapper {
        public string[] BinaryType;
        public string[] DeviceType;
        public string[] Attributes;
        public string[] UpdateAttributes;
        public string BlockSize;
        public string BlockCount;

        // Note: first element is name of the field
        // Only after that come value -> description
        public Mapper(string[] binaryType, string[] deviceType, 
            string[] attributes, string[] updateAttributes,
            string blockSize, string blockCount) {
            BinaryType = binaryType; DeviceType = deviceType;
            Attributes = attributes; UpdateAttributes = updateAttributes;
            BlockSize = blockSize; BlockCount = blockCount;
        }
    }

    public static Mapper NewPitMapper = new(
        new[] {
            "Binary Type",
            "Phone / AP",
            "Modem / CP"
        }, new[] {
            "Device Type",
            "OneNAND", "NAND",
            "EMMC", "SPI", "IDE",
            "NAND X16"
        }, new[] { 
            "Partition Type",
            "None", "BCT",
            "Bootloader",
            "Partition Table",
            "NV-Data", "Data",
            "MBR", "EBR",
            "GP1", "GP1"
        }, new[] { 
            "Filesystem",
            "None", "Basic",
            "Enhanced", "EXT2",
            "YAFFS2", "EXT4"
        }, "Start Block",
        "Block Count");
    
    public static Mapper OldPitMapper = new(
        new[] { 
            "Binary Type",
            "Phone / AP",
            "Modem / CP"
        }, new[] { 
            "Device Type",
            "OneNAND", "NAND",
            "MoviNAND",
        }, new[] { 
            "Attributes",
            "Read-only",
            "Read-write",
            "STL"
        }, new[] { 
            "Update Attributes",
            "None", "FOTA", 
            "Secure",
            "Secure FOTA"
        }, "Block Size",
        "Block Count");
    
    /// <summary>
    /// Get mapped string with fallback
    /// </summary>
    /// <param name="array">Array</param>
    /// <param name="index">Index</param>
    /// <returns>Mapped string</returns>
    public static string GetMapping(this string[] array, int index)
        => index > array.Length ? "Unknown" : array[index];
}