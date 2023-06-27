using Serilog;

namespace ThorRewrite.Shell.Commands; 

public class DevParse : ICommand {
    public FailInfo RunCommand(State state, List<string> args) {
        if (args.Count < 1)
            return new FailInfo("Not enough arguments!", 1);
        if (args.Count > 1)
            return new FailInfo("Too many arguments!", 1);
        if (!Directory.Exists(Path.GetDirectoryName(args[0])))
            return new FailInfo("Directory in the path does not exist!", 1);
        if (Path.GetFileName(args[0]) == string.Empty)
            return new FailInfo("Path must contain a filename!", 1);
        try {
            state.Handler.Initialize(null, File.ReadAllBytes(args[0]));
        } catch (Exception e) {
            Log.Debug("Full exception: {0}", e);
            return new FailInfo("This is not a valid /dev/bus/usb file!", 0);
        }
        
        return new FailInfo();
    }

    public string GetDescription()
        => "devParse <path> - Parse a /dev/bus/usb file for debug purposes";
}