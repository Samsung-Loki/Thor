using TheAirBlow.Thor.Library.Platform;

namespace TheAirBlow.Thor.Library.Communication; 

public static class USB {
    public const int Vendor = 0x04E8;
    
    private static Dictionary<PlatformID, IHandler> _handlers = new() {
        { PlatformID.Unix, new Linux() }
    };

    public static bool TryGetHandler(out IHandler handler) {
        var platform = Environment.OSVersion.Platform;
        return _handlers.TryGetValue(platform, out handler!);
    }

    public static string GetSupported() {
        var list = _handlers.Keys.Select(i => i.ToString()).ToList();
        return string.Join(", ", list);
    }
}