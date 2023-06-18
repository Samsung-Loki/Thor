namespace TheAirBlow.Thor.Library; 

public static class Lookup {
    private static string[]? _split;

    public enum InitState {
        Failed, Downloaded, Cache
    }
    
    public static async Task<InitState> Initialize() {
        try {
            if (!File.Exists("usb.ids")) {
                using var client = new HttpClient();
                var usbIds1 = await client
                    .GetStringAsync("http://www.linux-usb.org/usb.ids");
                await File.WriteAllTextAsync("usb.ids", usbIds1);
                _split = usbIds1.Split("\n");
                return InitState.Downloaded;
            }

            var usbIds2 = await File.ReadAllTextAsync("usb.ids");
            await File.WriteAllTextAsync("usb.ids", usbIds2);
            _split = usbIds2.Split("\n");
            return InitState.Cache;
        } catch (Exception e) {
            return InitState.Failed;
        }
    }
    
    public static async Task<string> GetDisplayName(int vendorId, int productId) {
        if (_split == null)
            return "Failed to load device name database";
        
        var str = ""; var found = false;
        foreach (var line in _split) {
            if (!found) {
                if (line.StartsWith(vendorId.ToString("x4"))) found = true;
            } else if (line.StartsWith($"\t{productId:x4}")) {
                str += $"{line[7..]}";
            }
        }

        if (!found)
            return "Unable to find device in database";
        return str;
    }
}