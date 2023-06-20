using System.Text;

namespace TheAirBlow.Thor.Library; 

public static class Extensions {
    public static string Ansify(this string input)
        => input.Replace("[", "[[").Replace("]", "]]");
    
    public static byte[] OdinAlign(this byte[] buf) {
        Array.Resize(ref buf, 1024);
        return buf;
    }

    public static void OdinFailCheck(this byte[] buf, string id, bool end = false) {
        if (buf[0] != 0xFF) return;
        var error = buf.ReadInt(4);
        var str = $"{id} received 0xFF (code 0x{error:X4})";
        if (end)
            str = error switch {
                -7 => $"{id} received 0xFF (code 0x{error:X4}, Ext4)",
                -6 => $"{id} received 0xFF (code 0x{error:X4}, Size)",
                -5 => $"{id} received 0xFF (code 0x{error:X4}, Auth)",
                -4 => $"{id} received 0xFF (code 0x{error:X4}, Write)",
                -3 => $"{id} received 0xFF (code 0x{error:X4}, Erase)",
                -2 => $"{id} received 0xFF (code 0x{error:X4}, WP)",
                _ => str
            };
        throw new InvalidDataException(str);
    }

    public static string ReadString(this BinaryReader reader, int count)
        => Encoding.ASCII.GetString(reader.ReadBytes(count)).TrimEnd('\0');

    public static void WriteInt(this byte[] buf, int integer, int offset) {
        var b = BitConverter.GetBytes(integer);
        if (!BitConverter.IsLittleEndian)
            b = b.Reverse().ToArray();
        buf[offset  ] = b[0];
        buf[offset+1] = b[1];
        buf[offset+2] = b[2];
        buf[offset+3] = b[3];
    }
    
    public static void WriteString(this byte[] buf, string str, int offset) {
        var b = Encoding.ASCII.GetBytes(str);
        for (var i = 0; i < b.Length; i++)
            buf[offset + i] = b[i];
    }
    
    public static void WriteLong(this byte[] buf, long integer, int offset) {
        var b = BitConverter.GetBytes(integer);
        if (!BitConverter.IsLittleEndian)
            b = b.Reverse().ToArray();
        buf[offset  ] = b[0];
        buf[offset+1] = b[1];
        buf[offset+2] = b[2];
        buf[offset+3] = b[3];
        buf[offset+4] = b[4];
        buf[offset+5] = b[5];
        buf[offset+6] = b[6];
        buf[offset+7] = b[7];
    }

    public static int ReadInt(this byte[] buf, int offset) {
        var intBuf = new[] {
            buf[offset], buf[offset + 1], 
            buf[offset + 2], buf[offset + 3]
        };
        if (!BitConverter.IsLittleEndian)
            intBuf = intBuf.Reverse().ToArray();
        return BitConverter.ToInt32(intBuf);
    }
}